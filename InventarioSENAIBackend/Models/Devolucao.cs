
namespace InventarioSENAIBackend.Models
{
    public class Devolucao
    {
        public int IdDevolucao { get; set; }

        public int IdEmprestimo { get; set; }

        public int IdUsuarioConferente { get; set; }

        public DateTime DataDevolucao { get; set; }

        public int QuantidadeBoa { get; set; }

        public int QuantidadeDanificada { get; set; }

        public bool ComponentesConferidos { get; set; }

        public string Observacoes { get; set; } = string.Empty;

        public Emprestimo? Emprestimo { get; set; }

        public Usuario? UsuarioConferente { get; set; }
    }
}
