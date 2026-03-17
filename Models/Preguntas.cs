using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoDeGradoFundacion.Models
{
    [Table("preguntas")]
    public class Pregunta
    {
        [Key]
        [Column("pregunta_id")]
        public int PreguntaId { get; set; }

        [Required]
        [Column("texto")]
        [MaxLength(500)]
        public string Texto { get; set; } = string.Empty;

        [Required]
        [Column("tipo")]
        [MaxLength(50)]
        public string Tipo { get; set; } = string.Empty;

        [Column("condicion_genero")]
        [MaxLength(20)]
        public string? CondicionGenero { get; set; }

        [Column("activa")]
        public bool Activa { get; set; } = true;

        [Column("orden")]
        public int Orden { get; set; }

        public ICollection<OpcionRespuesta> OpcionesRespuesta { get; set; } = new List<OpcionRespuesta>();
        public ICollection<EncuestaUsuarioDetalle> EncuestaUsuarioDetalles { get; set; } = new List<EncuestaUsuarioDetalle>();
    }
}