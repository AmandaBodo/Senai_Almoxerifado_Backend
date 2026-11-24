
using Microsoft.EntityFrameworkCore;
using InventarioSENAIBackend.Models;

namespace InventarioSENAIBackend.Repository
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("Usuarios");

                entity.HasKey(u => u.IdUsuario);

                entity.Property(u => u.IdUsuario)
                    .ValueGeneratedOnAdd();

                entity.HasIndex(u => u.Matricula)
                    .IsUnique();
            });
        }
    }
}
