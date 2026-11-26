
namespace InventarioSENAIBackend.Dtos
{
    public class EntradaProdutoRequest
    {
        public int IdUsuario { get; set; }

        public int Quantidade { get; set; }

        public string Descricao { get; set; } = string.Empty;
    }
}
