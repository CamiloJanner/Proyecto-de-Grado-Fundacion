using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ProyectoDeGradoFundacion.Pages.Analysis
{
    public class registroModel : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                ErrorMessage = "Por favor, complete correctamente todos los campos requeridos.";
                return Page();
            }

            if (Input.Contrasena != Input.ConfirmarContrasena)
            {
                ErrorMessage = "La contraseña y la confirmación no coinciden.";
                return Page();
            }

            // TODO:
            // Aquí debes guardar el usuario en tu base de datos real.
            // Se deja solo la estructura lista sin romper el flujo.

            return RedirectToPage("/inicio_sesion", new { registered = true });
        }

        public class InputModel
        {
            [Required(ErrorMessage = "Los nombres son obligatorios.")]
            public string NombresCompletos { get; set; } = string.Empty;

            [Required(ErrorMessage = "Los apellidos son obligatorios.")]
            public string Apellidos { get; set; } = string.Empty;

            [Required(ErrorMessage = "El correo es obligatorio.")]
            [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
            public string CorreoElectronico { get; set; } = string.Empty;

            [Required(ErrorMessage = "El teléfono es obligatorio.")]
            public string Telefono { get; set; } = string.Empty;

            [Required(ErrorMessage = "Seleccione el tipo de documento.")]
            public string TipoDocumento { get; set; } = string.Empty;

            [Required(ErrorMessage = "El número de identificación es obligatorio.")]
            public string NumeroIdentificacion { get; set; } = string.Empty;

            [Required(ErrorMessage = "La contraseña es obligatoria.")]
            [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
            [DataType(DataType.Password)]
            public string Contrasena { get; set; } = string.Empty;

            [Required(ErrorMessage = "Debe confirmar la contraseña.")]
            [DataType(DataType.Password)]
            public string ConfirmarContrasena { get; set; } = string.Empty;
        }
    }
}