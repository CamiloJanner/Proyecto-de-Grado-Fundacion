using Microsoft.EntityFrameworkCore;
using ProyectoDeGradoFundacion.Models;

namespace ProyectoDeGradoFundacion.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Pregunta> Preguntas => Set<Pregunta>();
        public DbSet<OpcionRespuesta> OpcionesRespuesta => Set<OpcionRespuesta>();
        public DbSet<EncuestaUsuario> EncuestasUsuario => Set<EncuestaUsuario>();
        public DbSet<EncuestaUsuarioDetalle> EncuestasUsuarioDetalles => Set<EncuestaUsuarioDetalle>();
        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Rol> Roles => Set<Rol>();
        public DbSet<RolPermisoOpcion> RolesPermisosOpciones => Set<RolPermisoOpcion>();
        public DbSet<Permiso> Permisos => Set<Permiso>();
        public DbSet<Opcion> Opciones => Set<Opcion>();
        public DbSet<Cita> Citas => Set<Cita>();
        public DbSet<AuditoriaLog> AuditoriaLogs { get; set; }
        public DbSet<UsuarioRolClinic> UsuariosRolesClinic => Set<UsuarioRolClinic>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<RolPermisoOpcion>()
                .HasKey(x => new { x.RolId, x.PermisoId, x.OpcionId });

            modelBuilder.Entity<Usuario>()
                .HasIndex(x => x.Email)
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasIndex(x => x.NumeroIdentificacion)
                .IsUnique();

            modelBuilder.Entity<RolPermisoOpcion>()
                .HasOne(x => x.Rol)
                .WithMany(x => x.RolesPermisosOpciones)
                .HasForeignKey(x => x.RolId);

            modelBuilder.Entity<RolPermisoOpcion>()
                .HasOne(x => x.Permiso)
                .WithMany(x => x.RolesPermisosOpciones)
                .HasForeignKey(x => x.PermisoId);

            modelBuilder.Entity<RolPermisoOpcion>()
                .HasOne(x => x.Opcion)
                .WithMany(x => x.RolesPermisosOpciones)
                .HasForeignKey(x => x.OpcionId);

            modelBuilder.Entity<Usuario>()
                .HasOne(x => x.Rol)
                .WithMany(x => x.Usuarios)
                .HasForeignKey(x => x.RolId);

            modelBuilder.Entity<OpcionRespuesta>()
                .HasOne(x => x.Pregunta)
                .WithMany(x => x.OpcionesRespuesta)
                .HasForeignKey(x => x.PreguntaId);

            modelBuilder.Entity<EncuestaUsuario>()
                .HasOne(x => x.Usuario)
                .WithMany(x => x.EncuestasUsuario)
                .HasForeignKey(x => x.UsuarioId);

            modelBuilder.Entity<EncuestaUsuarioDetalle>()
                .HasOne(x => x.EncuestaUsuario)
                .WithMany(x => x.Detalles)
                .HasForeignKey(x => x.EncuestaUsuarioId);

            modelBuilder.Entity<EncuestaUsuarioDetalle>()
                .HasOne(x => x.Pregunta)
                .WithMany(x => x.EncuestaUsuarioDetalles)
                .HasForeignKey(x => x.PreguntaId);

            modelBuilder.Entity<EncuestaUsuarioDetalle>()
                .HasOne(x => x.OpcionRespuesta)
                .WithMany(x => x.EncuestaUsuarioDetalles)
                .HasForeignKey(x => x.OpcionId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Cita>()
                .HasOne(x => x.Usuario)
                .WithMany(x => x.Citas)
                .HasForeignKey(x => x.UsuarioId);

            modelBuilder.Entity<UsuarioRolClinic>()
    .HasIndex(x => x.Codempleados)
    .IsUnique();

            modelBuilder.Entity<UsuarioRolClinic>()
                .HasOne(x => x.Rol)
                .WithMany()
                .HasForeignKey(x => x.RolId);
        }
    }
}