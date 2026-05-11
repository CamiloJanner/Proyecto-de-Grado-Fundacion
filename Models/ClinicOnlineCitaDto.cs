namespace ProyectoDeGradoFundacion.Models
{
    public class ClinicOnlineCitaDto
    {
        public string CodigoCita { get; set; } = string.Empty;

        public string Nombre1 { get; set; } = string.Empty;

        public string Nombre2 { get; set; } = string.Empty;

        public string Apellido1 { get; set; } = string.Empty;

        public string Apellido2 { get; set; } = string.Empty;

        public DateTime? FechaCita { get; set; }

        public string Documento { get; set; } = string.Empty;

        public string TipoDocumento { get; set; } = string.Empty;

        public string NombreCompleto
        {
            get
            {
                var partes = new[]
                {
                    Nombre1,
                    Nombre2,
                    Apellido1,
                    Apellido2
                };

                return string.Join(" ", partes.Where(p => !string.IsNullOrWhiteSpace(p)));
            }
        }

        public bool EsCitaDeHoy
        {
            get
            {
                return FechaCita.HasValue &&
                       FechaCita.Value.Date == DateTime.Today;
            }
        }
    }
}
