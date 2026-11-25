
namespace InventarioSENAIBackend.Models
{
    public class Movimentacao
    {
        public int IdMovimentacao { get; set; }

        public int IdProduto { get; set; }

        public int IdUsuario { get; set; }

        public int? IdEmprestimo { get; set; }

        public int? IdSalaOrigem { get; set; }

        public int? IdSalaDestino { get; set; }

        public string TipoMovimentacao { get; set; } = string.Empty;

        public int Quantidade { get; set; }

        public DateTime DataMovimentacao { get; set; }

        public string Descricao { get; set; } = string.Empty;

        public Produto? Produto { get; set; }

        public Usuario? Usuario { get; set; }

        public Emprestimo? Emprestimo { get; set; }

        public Sala? SalaOrigem { get; set; }

        public Sala? SalaDestino { get; set; }
    }
}
