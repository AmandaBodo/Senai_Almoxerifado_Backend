
namespace InventarioSENAIBackend.Dtos
{
    public class DevolucaoRequest
    {
        public int IdEmprestimo { get; set; }

        public int IdUsuarioConferente { get; set; }

        public string EstadoDevolucao { get; set; } = "Completa";

        public bool ComponentesConferidos { get; set; }

        public string Observacoes { get; set; } = string.Empty;
    }
}
