using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoDeGradoFundacion.Models
{
    [Table("roles_permisos_opciones")]
    public class RolPermisoOpcion
    {
        [Column("rol_id")]
        public int RolId { get; set; }

        [Column("permiso_id")]
        public int PermisoId { get; set; }

        [Column("opcion_id")]
        public int OpcionId { get; set; }

        public Rol Rol { get; set; } = null!;
        public Permiso Permiso { get; set; } = null!;
        public Opcion Opcion { get; set; } = null!;
    }
}