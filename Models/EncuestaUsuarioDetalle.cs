using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoDeGradoFundacion.Models
{
    [Table("encuesta_usuario_detalle")]
    public class EncuestaUsuarioDetalle
    {
        [Key]
        [Column("detalle_id")]
        public int DetalleId { get; set; }

        [Column("encuesta_usuario_id")]
        public int EncuestaUsuarioId { get; set; }

        [Column("pregunta_id")]
        public int PreguntaId { get; set; }

        [Column("opcion_id")]
        public int? OpcionId { get; set; }

        [Column("texto_respuesta")]
        [MaxLength(1000)]
        public string? TextoRespuesta { get; set; }

        [ForeignKey(nameof(EncuestaUsuarioId))]
        public EncuestaUsuario EncuestaUsuario { get; set; } = null!;

        [ForeignKey(nameof(PreguntaId))]
        public Pregunta Pregunta { get; set; } = null!;

        [ForeignKey(nameof(OpcionId))]
        public OpcionRespuesta? OpcionRespuesta { get; set; }
    }
}