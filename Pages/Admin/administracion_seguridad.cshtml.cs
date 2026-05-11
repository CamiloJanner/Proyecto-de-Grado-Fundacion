using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ProyectoDeGradoFundacion.Data;
using ProyectoDeGradoFundacion.Models;
using ProyectoDeGradoFundacion.Services;

namespace ProyectoDeGradoFundacion.Pages.Admin
{
    public class administracion_seguridadModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ClinicOnlineUsuariosService _clinicOnlineUsuariosService;

        public administracion_seguridadModel(
            ApplicationDbContext context,
            ClinicOnlineUsuariosService clinicOnlineUsuariosService)
        {
            _context = context;
            _clinicOnlineUsuariosService = clinicOnlineUsuariosService;
        }

        public List<UsuarioClinicAdminViewModel> UsuariosClinic { get; set; } = new();
        public List<Rol> Roles { get; set; } = new();
        public List<Cita> Reportes { get; set; } = new();
        public List<Cita> Observaciones { get; set; } = new();
        public List<AuditoriaLog> Logs { get; set; } = new();

        public string? Error { get; set; }
        public string? Mensaje { get; set; }

        [BindProperty]
        public List<UsuarioRolClinicInputModel> Asignaciones { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? FiltroUsuario { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroReporte { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroObservacion { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroTipoLog { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? FiltroUsuarioLog { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FechaInicio { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime? FechaFin { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!await EsAdministradorAsync())
            {
                return RedirectToPage("/Login/inicio");
            }

            await CargarDatosAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostGuardarRolesClinicAsync()
        {
            if (!await EsAdministradorAsync())
            {
                return RedirectToPage("/Login/inicio");
            }

            var rolesValidos = await _context.Roles
    .Where(r =>
        !r.Nombre.ToLower().Contains("paciente") &&
        !r.Nombre.ToLower().Contains("cliente"))
    .Select(r => r.RolId)
    .ToListAsync();

            foreach (var item in Asignaciones)
            {
                if (string.IsNullOrWhiteSpace(item.Codempleados) ||
                    string.IsNullOrWhiteSpace(item.Login))
                {
                    continue;
                }

                if (!item.RolId.HasValue)
                {
                    continue;
                }

                if (!rolesValidos.Contains(item.RolId.Value))
                {
                    continue;
                }

                var asignacionExistente = await _context.UsuariosRolesClinic
                    .FirstOrDefaultAsync(x => x.Codempleados == item.Codempleados);

                if (asignacionExistente == null)
                {
                    var nuevaAsignacion = new UsuarioRolClinic
                    {
                        Codempleados = item.Codempleados.Trim(),
                        Login = item.Login.Trim(),
                        RolId = item.RolId.Value,
                        Estado = item.Estado ? true : false,
                        FechaCreado = DateTime.UtcNow
                    };

                    _context.UsuariosRolesClinic.Add(nuevaAsignacion);
                }
                else
                {
                    asignacionExistente.Login = item.Login.Trim();
                    asignacionExistente.RolId = item.RolId.Value;
                    asignacionExistente.Estado = item.Estado ? true : false;
                    asignacionExistente.FechaActualizado = DateTime.UtcNow;
                }
            }

            _context.AuditoriaLogs.Add(new AuditoriaLog
            {
                TipoRegistro = "sesion",
                Usuario = User.FindFirstValue(ClaimTypes.Name) ?? "Administrador",
                Rol = User.FindFirstValue(ClaimTypes.Role) ?? "Administrador",
                Descripcion = "Administrador actualizó asignaciones de roles de usuarios Clinic Online.",
                FechaCreado = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();

            Mensaje = "Roles y estados actualizados correctamente.";

            await CargarDatosAsync();

            return Page();
        }

       

        private async Task CargarDatosAsync()
        {
            Roles = await _context.Roles
    .Where(r =>
        !r.Nombre.ToLower().Contains("paciente") &&
        !r.Nombre.ToLower().Contains("cliente"))
    .OrderBy(r => r.RolId)
    .ToListAsync();

            await CargarUsuariosClinicAsync();
            await CargarReportesAsync();
            await CargarObservacionesAsync();
            await CargarLogsAsync();
        }

        private async Task CargarUsuariosClinicAsync()
        {
            try
            {
                var usuariosClinic = await _clinicOnlineUsuariosService.ObtenerUsuariosClinicAsync();

                var asignaciones = await _context.UsuariosRolesClinic
                    .Include(x => x.Rol)
                    .ToListAsync();

                var lista = usuariosClinic
                    .Select(u =>
                    {
                        var asignacion = asignaciones
                            .FirstOrDefault(a => a.Codempleados == u.CodigoEmpleado);

                        return new UsuarioClinicAdminViewModel
                        {
                            Codempleados = u.CodigoEmpleado,
                            Login = u.Login,
                            RolId = asignacion?.RolId,
                            RolNombre = asignacion?.Rol.Nombre,
                            Estado = asignacion?.Estado ?? false,
                            TieneAsignacion = asignacion != null
                        };
                    })
                    .ToList();

                if (!string.IsNullOrWhiteSpace(FiltroUsuario))
                {
                    lista = lista
                        .Where(u =>
                            u.Codempleados.Contains(FiltroUsuario, StringComparison.OrdinalIgnoreCase) ||
                            u.Login.Contains(FiltroUsuario, StringComparison.OrdinalIgnoreCase) ||
                            (!string.IsNullOrWhiteSpace(u.RolNombre) &&
                             u.RolNombre.Contains(FiltroUsuario, StringComparison.OrdinalIgnoreCase)))
                        .ToList();
                }

                UsuariosClinic = lista
                    .OrderBy(u => int.TryParse(u.Codempleados, out var numero) ? numero : int.MaxValue)
                    .ThenBy(u => u.Login)
                    .ToList();
            }
            catch
            {
                UsuariosClinic = new List<UsuarioClinicAdminViewModel>();

                if (string.IsNullOrWhiteSpace(Error))
                {
                    Error = "No fue posible consultar los usuarios de Clinic Online.";
                }
            }
        }

        private async Task CargarReportesAsync()
        {
            var query = _context.Citas
                .Include(c => c.Usuario)
                .Where(c =>
                    c.EstadoCita == "finalizado" &&
                    !string.IsNullOrEmpty(c.ReportePdf))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(FiltroReporte))
            {
                query = query.Where(c =>
                    (c.CitaExternaId != null && c.CitaExternaId.Contains(FiltroReporte)) ||
                    (c.Usuario.NombreUsuario + " " + c.Usuario.ApellidoUsuario).Contains(FiltroReporte) ||
                    (c.MedicoResponsable != null && c.MedicoResponsable.Contains(FiltroReporte)) ||
                    (c.ResultadoOjoIzquierdo != null && c.ResultadoOjoIzquierdo.Contains(FiltroReporte)) ||
                    (c.ResultadoOjoDerecho != null && c.ResultadoOjoDerecho.Contains(FiltroReporte)));
            }

            Reportes = await query
                .OrderByDescending(c => c.FechaAnalisis)
                .ToListAsync();
        }

        private async Task CargarObservacionesAsync()
        {
            var query = _context.Citas
                .Include(c => c.Usuario)
                .Where(c =>
                    c.EstadoCita == "finalizado" &&
                    (
                        !string.IsNullOrEmpty(c.ObservacionesOjoIzquierdo) ||
                        !string.IsNullOrEmpty(c.ObservacionesOjoDerecho)
                    ))
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(FiltroObservacion))
            {
                query = query.Where(c =>
                    (c.CitaExternaId != null && c.CitaExternaId.Contains(FiltroObservacion)) ||
                    (c.Usuario.NombreUsuario + " " + c.Usuario.ApellidoUsuario).Contains(FiltroObservacion) ||
                    (c.ObservacionesOjoIzquierdo != null && c.ObservacionesOjoIzquierdo.Contains(FiltroObservacion)) ||
                    (c.ObservacionesOjoDerecho != null && c.ObservacionesOjoDerecho.Contains(FiltroObservacion)));
            }

            Observaciones = await query
                .OrderByDescending(c => c.FechaAnalisis)
                .ToListAsync();
        }

        private async Task CargarLogsAsync()
        {
            var query = _context.AuditoriaLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(FiltroTipoLog))
            {
                query = query.Where(l => l.TipoRegistro == FiltroTipoLog);
            }

            if (!string.IsNullOrWhiteSpace(FiltroUsuarioLog))
            {
                query = query.Where(l =>
                    (l.Usuario != null && l.Usuario.Contains(FiltroUsuarioLog)) ||
                    (l.Paciente != null && l.Paciente.Contains(FiltroUsuarioLog)) ||
                    (l.MedicoResponsable != null && l.MedicoResponsable.Contains(FiltroUsuarioLog)));
            }

            if (FechaInicio.HasValue)
            {
                var inicio = FechaInicio.Value.Date;

                query = query.Where(l =>
                    l.FechaCreado >= inicio ||
                    l.FechaLogin >= inicio ||
                    l.FechaAnalisis >= inicio);
            }

            if (FechaFin.HasValue)
            {
                var fin = FechaFin.Value.Date.AddDays(1);

                query = query.Where(l =>
                    l.FechaCreado < fin ||
                    l.FechaLogin < fin ||
                    l.FechaAnalisis < fin);
            }

            Logs = await query
                .OrderByDescending(l => l.FechaCreado)
                .Take(300)
                .ToListAsync();
        }

        private async Task<bool> EsAdministradorAsync()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
                return false;

            var rolClaim = User.FindFirstValue(ClaimTypes.Role) ?? "";

            if (rolClaim.ToLower().Contains("admin") ||
                rolClaim.ToLower().Contains("administrador"))
            {
                return true;
            }

            var codigoEmpleado = User.FindFirstValue("CodigoEmpleadoClinicOnline");

            if (string.IsNullOrWhiteSpace(codigoEmpleado))
                return false;

            var asignacion = await _context.UsuariosRolesClinic
                .Include(x => x.Rol)
                .FirstOrDefaultAsync(x =>
                    x.Codempleados == codigoEmpleado &&
                    x.Estado);

            if (asignacion == null)
                return false;

            var rol = asignacion.Rol.Nombre.ToLower();

            return rol.Contains("admin") || rol.Contains("administrador");
        }

        public string ObtenerDescripcionRol(string nombreRol)
        {
            var rol = nombreRol.ToLower();

            if (rol.Contains("administrador") || rol.Contains("admin"))
                return "Acceso completo al sistema, incluyendo gestión de usuarios, perfiles, reportes y logs de auditoría.";

            if (rol.Contains("médico") || rol.Contains("medico"))
                return "Acceso a funcionalidades clínicas: consulta de citas, carga de imágenes, análisis IA, validación médica, reportes e historial.";

            if (rol.Contains("administrativo") || rol.Contains("operativo"))
                return "Acceso a funcionalidades administrativas y de gestión operativa.";

            if (rol.Contains("cliente") || rol.Contains("paciente"))
                return "Acceso a funcionalidades de encuesta, cotización y consulta de servicios.";

            return "Perfil configurado en el sistema.";
        }

        public class UsuarioClinicAdminViewModel
        {
            public string Codempleados { get; set; } = "";
            public string Login { get; set; } = "";
            public int? RolId { get; set; }
            public string? RolNombre { get; set; }
            public bool Estado { get; set; }
            public bool TieneAsignacion { get; set; }
        }

        public class UsuarioRolClinicInputModel
        {
            public string Codempleados { get; set; } = "";
            public string Login { get; set; } = "";
            public int? RolId { get; set; }
            public bool Estado { get; set; }
        }
    }
}