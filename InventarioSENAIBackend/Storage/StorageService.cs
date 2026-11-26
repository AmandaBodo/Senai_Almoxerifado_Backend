
using System.Text.Json;

namespace InventarioSENAIBackend.Storage
{
    public class StorageService
    {
        private readonly string _storagePath;

        private readonly JsonSerializerOptions _jsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        public StorageService(IWebHostEnvironment environment)
        {
            _storagePath = Path.Combine(
                environment.ContentRootPath,
                "Storage",
                "Data"
            );

            Directory.CreateDirectory(_storagePath);
        }

        public async Task<List<T>> LerAsync<T>(string arquivo)
        {
            string caminho = ObterCaminho(arquivo);

            if (!File.Exists(caminho))
                return new List<T>();

            string json = await File.ReadAllTextAsync(caminho);

            return JsonSerializer.Deserialize<List<T>>(
                json,
                _jsonOptions
            ) ?? new List<T>();
        }

        public async Task SalvarAsync<T>(
            string arquivo,
            List<T> dados)
        {
            string caminho = ObterCaminho(arquivo);

            string json = JsonSerializer.Serialize(
                dados,
                _jsonOptions
            );

            await File.WriteAllTextAsync(caminho, json);
        }

        private string ObterCaminho(string arquivo)
        {
            if (string.IsNullOrWhiteSpace(arquivo) ||
                Path.GetFileName(arquivo) != arquivo ||
                !arquivo.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Nome de arquivo inválido.");
            }

            return Path.Combine(_storagePath, arquivo);
        }
    }
}
