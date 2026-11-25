
namespace InventarioSENAIBackend.Dtos
{
    public class MovimentacaoRequest
    {
        public int IdProduto { get; set; }

        public int IdUsuario { get; set; }

        public int? IdEmprestimo { get; set; }

        public int? IdSalaOrigem { get; set; }

        public int? IdSalaDestino { get; set; }

        public string TipoMovimentacao { get; set; } = string.Empty;

        public int Quantidade { get; set; }

        public string Descricao { get; set; } = string.Empty;
    }
}
