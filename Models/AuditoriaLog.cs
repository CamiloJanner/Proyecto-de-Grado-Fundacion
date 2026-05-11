using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoDeGradoFundacion.Models
{
    [Table("auditoria_logs")]
    public class AuditoriaLog
    {
        [Key]
        [Column("auditoria_log_id")]
        public int AuditoriaLogId { get; set; }

        [Column("tipo_registro")]
        [MaxLength(30)]
        public string TipoRegistro { get; set; } = string.Empty;
        // sesion / analisis

        [Column("usuario_id")]
        public int? UsuarioId { get; set; }

        [Column("usuario")]
        [MaxLength(180)]
        public string? Usuario { get; set; }

        [Column("rol")]
        [MaxLength(100)]
        public string? Rol { get; set; }

        [Column("fecha_login")]
        public DateTime? FechaLogin { get; set; }

        [Column("fecha_logout")]
        public DateTime? FechaLogout { get; set; }

        [Column("tiempo_sesion_minutos")]
        public int? TiempoSesionMinutos { get; set; }

        [Column("cita_externa_id")]
        [MaxLength(100)]
        public string? CitaExternaId { get; set; }

        [Column("paciente")]
        [MaxLength(220)]
        public string? Paciente { get; set; }

        [Column("medico_responsable")]
        [MaxLength(180)]
        public string? MedicoResponsable { get; set; }

        [Column("fecha_analisis")]
        public DateTime? FechaAnalisis { get; set; }

        [Column("descripcion")]
        [MaxLength(500)]
        public string? Descripcion { get; set; }

        [Column("fecha_creado")]
        public DateTime FechaCreado { get; set; } = DateTime.UtcNow;
    }
}