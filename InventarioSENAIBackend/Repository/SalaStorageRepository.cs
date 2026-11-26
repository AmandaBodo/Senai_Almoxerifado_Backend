
using InventarioSENAIBackend.Models;
using InventarioSENAIBackend.Storage;

namespace InventarioSENAIBackend.Repository
{
    public class SalaStorageRepository
    {
        private readonly StorageService _storage;
        private const string Arquivo = "salas.json";

        public SalaStorageRepository(StorageService storage)
        {
            _storage = storage;
        }

        public async Task<List<Sala>> ObterTodasAsync()
        {
            return await _storage.LerAsync<Sala>(Arquivo);
        }

        public async Task<Sala?> ObterPorIdAsync(int id)
        {
            var salas = await ObterTodasAsync();

            return salas.FirstOrDefault(s => s.IdSala == id);
        }

        public async Task<Sala> AdicionarAsync(Sala sala)
        {
            var salas = await ObterTodasAsync();

            sala.IdSala = salas.Count == 0
                ? 1
                : salas.Max(s => s.IdSala) + 1;

            salas.Add(sala);

            await _storage.SalvarAsync(Arquivo, salas);

            return sala;
        }

        public async Task<bool> AtualizarAsync(Sala sala)
        {
            var salas = await ObterTodasAsync();

            int indice = salas.FindIndex(
                s => s.IdSala == sala.IdSala
            );

            if (indice == -1)
                return false;

            salas[indice] = sala;

            await _storage.SalvarAsync(Arquivo, salas);

            return true;
        }
    }
}
