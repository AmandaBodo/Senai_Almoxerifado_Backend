
namespace InventarioSENAIBackend.Dtos
{
    public class DevolucaoRequest
    {
        public int IdEmprestimo { get; set; }

        public int IdUsuarioConferente { get; set; }

        public int QuantidadeBoa { get; set; }

        public int QuantidadeDanificada { get; set; }

        public bool ComponentesConferidos { get; set; }

        public string Observacoes { get; set; } = string.Empty;
    }
}
