using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoDeGradoFundacion.Models
{
    [Table("citas")]
    public class Cita
    {
        [Key]
        [Column("cita_id")]
        public int CitaId { get; set; }

        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        [Column("paciente_externo_id")]
        [MaxLength(100)]
        public string? PacienteExternoId { get; set; }

        [Column("cita_externa_id")]
        [MaxLength(100)]
        public string? CitaExternaId { get; set; }

        [Column("reporte_pdf")]
        [MaxLength(255)]
        public string? ReportePdf { get; set; }

        [Column("imagen_ojo_izquierdo")]
        [MaxLength(255)]
        public string? ImagenOjoIzquierdo { get; set; }

        [Column("imagen_ojo_derecho")]
        [MaxLength(255)]
        public string? ImagenOjoDerecho { get; set; }

        [Column("fecha_creado")]
        public DateTime FechaCreado { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UsuarioId))]
        public Usuario Usuario { get; set; } = null!;
    }
}