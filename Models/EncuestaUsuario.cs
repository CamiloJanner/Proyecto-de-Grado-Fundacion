using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoDeGradoFundacion.Models
{
    [Table("encuesta_usuario")]
    public class EncuestaUsuario
    {
        [Key]
        [Column("encuesta_usuario_id")]
        public int EncuestaUsuarioId { get; set; }

        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        [Column("fecha")]
        public DateTime Fecha { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UsuarioId))]
        public Usuario Usuario { get; set; } = null!;

        public ICollection<EncuestaUsuarioDetalle> Detalles { get; set; } = new List<EncuestaUsuarioDetalle>();
    }
}