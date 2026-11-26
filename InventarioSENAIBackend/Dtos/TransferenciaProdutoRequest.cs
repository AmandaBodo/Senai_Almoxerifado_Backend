
namespace InventarioSENAIBackend.Dtos
{
    public class TransferenciaProdutoRequest
    {
        public int IdUsuario { get; set; }

        public string CodigoSalaDestino { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;
    }
}
