using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoDeGradoFundacion.Models
{
    [Table("opciones")]
    public class Opcion
    {
        [Key]
        [Column("opcion_id")]
        public int OpcionId { get; set; }

        [Required]
        [Column("nombre_opcion")]
        [MaxLength(100)]
        public string NombreOpcion { get; set; } = string.Empty;

        public ICollection<RolPermisoOpcion> RolesPermisosOpciones { get; set; } = new List<RolPermisoOpcion>();
    }
}