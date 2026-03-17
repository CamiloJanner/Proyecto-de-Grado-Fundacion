using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoDeGradoFundacion.Models
{
    [Table("permisos")]
    public class Permiso
    {
        [Key]
        [Column("permiso_id")]
        public int PermisoId { get; set; }

        [Required]
        [Column("nombre")]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        public ICollection<RolPermisoOpcion> RolesPermisosOpciones { get; set; } = new List<RolPermisoOpcion>();
    }
}