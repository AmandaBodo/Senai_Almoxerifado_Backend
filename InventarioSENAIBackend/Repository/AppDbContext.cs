
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

        public DbSet<Sala> Salas { get; set; }

        public DbSet<Produto> Produtos { get; set; }

        public DbSet<Emprestimo> Emprestimos { get; set; }

        public DbSet<Devolucao> Devolucoes { get; set; }

        public DbSet<Movimentacao> Movimentacoes { get; set; }

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

            modelBuilder.Entity<Sala>(entity =>
            {
                entity.ToTable("Salas");

                entity.HasKey(s => s.IdSala);

                entity.Property(s => s.IdSala)
                    .ValueGeneratedOnAdd();

                entity.HasIndex(s => s.CodigoSala)
                    .IsUnique();
            });

            modelBuilder.Entity<Produto>(entity =>
            {
                entity.ToTable("Produtos");

                entity.HasKey(p => p.IdProduto);

                entity.Property(p => p.IdProduto)
                    .ValueGeneratedOnAdd();

                entity.HasIndex(p => p.CodigoQR)
                    .IsUnique();

                entity.HasOne(p => p.Sala)
                    .WithMany()
                    .HasForeignKey(p => p.IdSala);
            });

            modelBuilder.Entity<Emprestimo>(entity =>
            {
                entity.ToTable("Emprestimos");

                entity.HasKey(e => e.IdEmprestimo);

                entity.Property(e => e.IdEmprestimo)
                    .ValueGeneratedOnAdd();

                entity.HasOne(e => e.Usuario)
                    .WithMany()
                    .HasForeignKey(e => e.IdUsuario);

                entity.HasOne(e => e.Produto)
                    .WithMany()
                    .HasForeignKey(e => e.IdProduto);

                entity.HasOne(e => e.SalaOrigem)
                    .WithMany()
                    .HasForeignKey(e => e.IdSalaOrigem);

                entity.HasOne(e => e.SalaDestino)
                    .WithMany()
                    .HasForeignKey(e => e.IdSalaDestino);
            });

            modelBuilder.Entity<Devolucao>(entity =>
            {
                entity.ToTable("Devolucoes");

                entity.HasKey(d => d.IdDevolucao);

                entity.Property(d => d.IdDevolucao)
                    .ValueGeneratedOnAdd();

                entity.HasIndex(d => d.IdEmprestimo)
                    .IsUnique();

                entity.HasOne(d => d.Emprestimo)
                    .WithOne()
                    .HasForeignKey<Devolucao>(d => d.IdEmprestimo);

                entity.HasOne(d => d.UsuarioConferente)
                    .WithMany()
                    .HasForeignKey(d => d.IdUsuarioConferente);
            });

            modelBuilder.Entity<Movimentacao>(entity =>
            {
                entity.ToTable("Movimentacoes");

                entity.HasKey(m => m.IdMovimentacao);

                entity.Property(m => m.IdMovimentacao)
                    .ValueGeneratedOnAdd();

                entity.HasOne(m => m.Produto)
                    .WithMany()
                    .HasForeignKey(m => m.IdProduto);

                entity.HasOne(m => m.Usuario)
                    .WithMany()
                    .HasForeignKey(m => m.IdUsuario);

                entity.HasOne(m => m.Emprestimo)
                    .WithMany()
                    .HasForeignKey(m => m.IdEmprestimo);

                entity.HasOne(m => m.SalaOrigem)
                    .WithMany()
                    .HasForeignKey(m => m.IdSalaOrigem);

                entity.HasOne(m => m.SalaDestino)
                    .WithMany()
                    .HasForeignKey(m => m.IdSalaDestino);
            });
        }
    }
}
