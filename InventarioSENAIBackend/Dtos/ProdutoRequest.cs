namespace InventarioSENAIBackend.Dtos
{
    public class ProdutoRequest
    {
        public int IdUsuario { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Descricao { get; set; } = string.Empty;

        public string Categoria { get; set; } = string.Empty;

        public int QuantidadeTotal { get; set; }

        public int QuantidadeDisponivel { get; set; }

        public string EstadoConservacao { get; set; } = string.Empty;

        public string CodigoQR { get; set; } = string.Empty;

        public string CodigoSala { get; set; } = string.Empty;
    }
}