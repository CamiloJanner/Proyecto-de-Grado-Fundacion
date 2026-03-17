using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoDeGradoFundacion.Models
{
    [Table("opcion_respuesta")]
    public class OpcionRespuesta
    {
        [Key]
        [Column("opcion_id")]
        public int OpcionId { get; set; }

        [Column("pregunta_id")]
        public int PreguntaId { get; set; }

        [Required]
        [Column("texto")]
        [MaxLength(255)]
        public string Texto { get; set; } = string.Empty;

        [Column("valor_logico")]
        public bool? ValorLogico { get; set; }

        [ForeignKey(nameof(PreguntaId))]
        public Pregunta Pregunta { get; set; } = null!;

        public ICollection<EncuestaUsuarioDetalle> EncuestaUsuarioDetalles { get; set; } = new List<EncuestaUsuarioDetalle>();
    }
}