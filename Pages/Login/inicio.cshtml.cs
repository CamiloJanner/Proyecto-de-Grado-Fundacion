using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProyectoDeGradoFundacion.Data;
using ProyectoDeGradoFundacion.Models;
using ProyectoDeGradoFundacion.Services;

namespace ProyectoDeGradoFundacion.Pages.Login
{
    public class inicioModel : PageModel
    {
        private readonly ClinicOnlineAuthService _clinicOnlineAuthService;
        private readonly ApplicationDbContext _context;

        public inicioModel(
            ClinicOnlineAuthService clinicOnlineAuthService,
            ApplicationDbContext context)
        {
            _clinicOnlineAuthService = clinicOnlineAuthService;
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public void OnGet(bool registered = false, bool logout = false)
        {
            if (registered)
            {
                SuccessMessage = "Registro realizado correctamente. Ahora puedes iniciar sesión.";
            }

            if (logout)
            {
                SuccessMessage = "Sesión cerrada correctamente.";
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Por favor, complete correctamente los campos requeridos.";
                return Page();
            }

            try
            {
                var usuarioClinic = await _clinicOnlineAuthService.ValidarUsuarioAsync(
                    Input.Identificador,
                    Input.Contrasena
                );

                if (usuarioClinic == null)
                {
                    await RegistrarLoginAsync(
                        usuario: Input.Identificador,
                        rol: "No autenticado",
                        fechaLogin: DateTime.UtcNow,
                        descripcion: "Intento de login fallido. Credenciales incorrectas."
                    );

                    ErrorMessage = "Usuario o contraseña incorrectos.";
                    return Page();
                }

                var asignacionRol = await _context.UsuariosRolesClinic
                    .Include(x => x.Rol)
                    .FirstOrDefaultAsync(x =>
                        x.Codempleados == usuarioClinic.CodigoEmpleado ||
                        x.Login == usuarioClinic.Login);

                if (asignacionRol == null)
                {
                    await RegistrarLoginAsync(
                        usuario: usuarioClinic.Login,
                        rol: "Sin rol asignado",
                        fechaLogin: DateTime.UtcNow,
                        descripcion: "Usuario válido en Clinic Online, pero sin rol asignado en Aiven."
                    );

                    ErrorMessage = "El usuario existe en Clinic Online, pero todavía no tiene un rol asignado en el sistema.";
                    return Page();
                }

                if (!asignacionRol.Estado)
                {
                    await RegistrarLoginAsync(
                        usuario: usuarioClinic.Login,
                        rol: asignacionRol.Rol.Nombre,
                        fechaLogin: DateTime.UtcNow,
                        descripcion: "Intento de login fallido. Usuario inactivo."
                    );

                    ErrorMessage = "El usuario está inactivo. Comuníquese con el administrador.";
                    return Page();
                }

                var rolNombre = asignacionRol.Rol.Nombre;
                var nombreUsuario = usuarioClinic.Login;

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuarioClinic.CodigoEmpleado),
                    new Claim(ClaimTypes.Name, nombreUsuario),
                    new Claim(ClaimTypes.Role, rolNombre),

                    new Claim("CodigoEmpleadoClinicOnline", usuarioClinic.CodigoEmpleado),
                    new Claim("LoginClinicOnline", usuarioClinic.Login),
                    new Claim("RolNombre", rolNombre)
                };

                var identity = new ClaimsIdentity(
                    claims,
                    CookieAuthenticationDefaults.AuthenticationScheme
                );

                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal
                );

                await RegistrarLoginAsync(
                    usuario: nombreUsuario,
                    rol: rolNombre,
                    fechaLogin: DateTime.UtcNow,
                    descripcion: "Login exitoso."
                );

                var rol = rolNombre.ToLower().Trim();

                if (rol.Contains("admin") || rol.Contains("administrador"))
                {
                    return RedirectToPage("/Admin/administracion_seguridad");
                }

                if (rol.Contains("medico") || rol.Contains("médico"))
                {
                    return RedirectToPage("/Analysis/consultar_citas");
                }

                if (rol.Contains("operativo") || rol.Contains("administrativo"))
                {
                    return RedirectToPage("/Analysis/consultar_citas");
                }

                if (rol.Contains("paciente") || rol.Contains("cliente"))
                {
                    return RedirectToPage("/Analysis/consultar_citas");
                }

                ErrorMessage = $"El rol '{rolNombre}' no tiene una página asignada.";
                return Page();
            }
            catch
            {
                ErrorMessage = "Ocurrió un error al validar el usuario. Revise la conexión con Clinic Online.";
                return Page();
            }
        }

        public async Task<IActionResult> OnPostLogoutAsync()
        {
            var usuario = User.FindFirstValue(ClaimTypes.Name) ?? "Usuario";
            var rol = User.FindFirstValue(ClaimTypes.Role) ?? "Sin rol";

            var ultimoLogin = await _context.AuditoriaLogs
                .Where(x =>
                    x.TipoRegistro == "sesion" &&
                    x.Usuario == usuario &&
                    x.Rol == rol &&
                    x.FechaLogout == null)
                .OrderByDescending(x => x.FechaLogin)
                .FirstOrDefaultAsync();

            if (ultimoLogin != null)
            {
                ultimoLogin.FechaLogout = DateTime.UtcNow;

                if (ultimoLogin.FechaLogin.HasValue)
                {
                    ultimoLogin.TiempoSesionMinutos = (int)Math.Round(
                        (ultimoLogin.FechaLogout.Value - ultimoLogin.FechaLogin.Value).TotalMinutes
                    );
                }

                ultimoLogin.Descripcion = "Logout manual.";
                await _context.SaveChangesAsync();
            }
            else
            {
                _context.AuditoriaLogs.Add(new AuditoriaLog
                {
                    TipoRegistro = "sesion",
                    Usuario = usuario,
                    Rol = rol,
                    FechaLogout = DateTime.UtcNow,
                    Descripcion = "Logout manual sin login abierto encontrado.",
                    FechaCreado = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToPage("/Login/inicio", new { logout = true });
        }

        private async Task RegistrarLoginAsync(
            string usuario,
            string rol,
            DateTime fechaLogin,
            string descripcion)
        {
            _context.AuditoriaLogs.Add(new AuditoriaLog
            {
                TipoRegistro = "sesion",
                Usuario = usuario,
                Rol = rol,
                FechaLogin = fechaLogin,
                Descripcion = descripcion,
                FechaCreado = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
        }

        public class InputModel
        {
            [Required(ErrorMessage = "El usuario o documento es obligatorio.")]
            public string Identificador { get; set; } = string.Empty;

            [Required(ErrorMessage = "La contraseña es obligatoria.")]
            [DataType(DataType.Password)]
            public string Contrasena { get; set; } = string.Empty;
        }
    }
}