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

        // ============================
        // 🔥 RESULTADOS IA
        // ============================

        [Column("resultado_ojo_izquierdo")]
        [MaxLength(200)]
        public string? ResultadoOjoIzquierdo { get; set; }

        [Column("resultado_ojo_derecho")]
        [MaxLength(200)]
        public string? ResultadoOjoDerecho { get; set; }

        [Column("confianza_ojo_izquierdo")]
        public float? ConfianzaOjoIzquierdo { get; set; }

        [Column("confianza_ojo_derecho")]
        public float? ConfianzaOjoDerecho { get; set; }

        // ============================
        // 🔥 VALIDACIÓN MÉDICA
        // ============================

        [Column("recomendacion_modelo")]
        [MaxLength(200)]
        public string? RecomendacionModelo { get; set; }

        [Column("tipo_retinopatia")]
        [MaxLength(100)]
        public string? TipoRetinopatia { get; set; }

        [Column("grado_retinopatia")]
        [MaxLength(50)]
        public string? GradoRetinopatia { get; set; }

        [Column("observaciones_ojo_izquierdo")]
        [MaxLength(1000)]
        public string? ObservacionesOjoIzquierdo { get; set; }

        [Column("observaciones_ojo_derecho")]
        [MaxLength(1000)]
        public string? ObservacionesOjoDerecho { get; set; }

        // ============================
        // 🔥 CONTROL
        // ============================

        [Column("estado_cita")]
        [MaxLength(30)]
        public string EstadoCita { get; set; } = "pendiente";

        [Column("fecha_creado")]
        public DateTime FechaCreado { get; set; } = DateTime.UtcNow;

        [Column("fecha_analisis")]
        public DateTime? FechaAnalisis { get; set; }

        // ============================
        // 🔗 RELACIÓN
        // ============================

        [ForeignKey(nameof(UsuarioId))]
        public Usuario Usuario { get; set; } = null!;
    }
}