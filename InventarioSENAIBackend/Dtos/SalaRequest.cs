
namespace InventarioSENAIBackend.Dtos
{
    public class SalaRequest
    {
        public string NomeSala { get; set; } = string.Empty;

        public string CodigoSala { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        public string Localizacao { get; set; } = string.Empty;

        public string Status { get; set; } = "Ativo";
    }
}
