
namespace InventarioSENAIBackend.Dtos
{
    public class EmprestimoRequest
    {
        public int IdUsuario { get; set; }

        public int IdProduto { get; set; }

        public int IdSalaOrigem { get; set; }

        public int IdSalaDestino { get; set; }

        public int Quantidade { get; set; }

        public DateTime DataPrevistaDevolucao { get; set; }
    }
}
