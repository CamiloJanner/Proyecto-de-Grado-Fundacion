using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoDeGradoFundacion.Models
{
    [Table("usuarios")]
    public class Usuario
    {
        [Key]
        [Column("usuario_id")]
        public int UsuarioId { get; set; }

        [Column("rol_id")]
        public int RolId { get; set; }

        [Required]
        [Column("nombre_usuario")]
        [MaxLength(100)]
        public string NombreUsuario { get; set; } = string.Empty;

        [Required]
        [Column("apellido_usuario")]
        [MaxLength(100)]
        public string ApellidoUsuario { get; set; } = string.Empty;

        [Required]
        [Column("email")]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Column("telefono")]
        [MaxLength(30)]
        public string? Telefono { get; set; }

        [Required]
        [Column("tipo_identificacion")]
        [MaxLength(30)]
        public string TipoIdentificacion { get; set; } = string.Empty;

        [Required]
        [Column("numero_identificacion")]
        [MaxLength(50)]
        public string NumeroIdentificacion { get; set; } = string.Empty;

        [Required]
        [Column("password_hash")]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("estado_usuario_id")]
        public int EstadoUsuarioId { get; set; }

        [Column("fecha_creado")]
        public DateTime FechaCreado { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(RolId))]
        public Rol Rol { get; set; } = null!;

        public ICollection<EncuestaUsuario> EncuestasUsuario { get; set; } = new List<EncuestaUsuario>();
        public ICollection<Cita> Citas { get; set; } = new List<Cita>();
    }
}