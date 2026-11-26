
namespace InventarioSENAIBackend.Models
{
    public class Emprestimo
    {
        public int IdEmprestimo { get; set; }

        public int IdUsuario { get; set; }

        public int IdProduto { get; set; }

        public int IdSalaOrigem { get; set; }

        public int IdSalaDestino { get; set; }

        public int Quantidade { get; set; }

        public int QuantidadeDevolvida { get; set; }

        public int QuantidadePendente =>
            Quantidade - QuantidadeDevolvida;

        public DateTime DataEmprestimo { get; set; }

        public DateTime DataPrevistaDevolucao { get; set; }

        public string Status { get; set; } = "Ativo";

        public Usuario? Usuario { get; set; }

        public Produto? Produto { get; set; }

        public Sala? SalaOrigem { get; set; }

        public Sala? SalaDestino { get; set; }
    }
}
