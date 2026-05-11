using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoDeGradoFundacion.Models
{
    [Table("usuarios_roles_clinic")]
    public class UsuarioRolClinic
    {
        [Key]
        [Column("usuario_rol_clinic_id")]
        public int UsuarioRolClinicId { get; set; }

        [Required]
        [Column("codempleados")]
        [MaxLength(50)]
        public string Codempleados { get; set; } = string.Empty;

        [Required]
        [Column("login")]
        [MaxLength(100)]
        public string Login { get; set; } = string.Empty;

        [Column("rol_id")]
        public int RolId { get; set; }

        [Column("estado")]
        public bool Estado { get; set; } = true;

        [Column("fecha_creado")]
        public DateTime FechaCreado { get; set; } = DateTime.UtcNow;

        [Column("fecha_actualizado")]
        public DateTime? FechaActualizado { get; set; }

        [ForeignKey(nameof(RolId))]
        public Rol Rol { get; set; } = null!;
    }
}