
using Microsoft.EntityFrameworkCore;
using InventarioSENAIBackend.Models;

namespace InventarioSENAIBackend.Repository
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Sala> Salas { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<Emprestimo> Emprestimos { get; set; }
        public DbSet<Devolucao> Devolucoes { get; set; }
        public DbSet<Movimentacao> Movimentacoes { get; set; }
    }
}
