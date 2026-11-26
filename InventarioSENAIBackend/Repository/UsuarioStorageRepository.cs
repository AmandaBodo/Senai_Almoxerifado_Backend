
using InventarioSENAIBackend.Models;
using InventarioSENAIBackend.Storage;

namespace InventarioSENAIBackend.Repository
{
    public class UsuarioStorageRepository
    {
        private readonly StorageService _storage;
        private const string Arquivo = "usuarios.json";

        public UsuarioStorageRepository(StorageService storage)
        {
            _storage = storage;
        }

        public async Task<List<Usuario>> ObterTodosAsync()
        {
            return await _storage.LerAsync<Usuario>(Arquivo);
        }

        public async Task<Usuario?> ObterPorIdAsync(int id)
        {
            var usuarios = await ObterTodosAsync();

            return usuarios.FirstOrDefault(u => u.IdUsuario == id);
        }

        public async Task<Usuario> AdicionarAsync(Usuario usuario)
        {
            var usuarios = await ObterTodosAsync();

            usuario.IdUsuario = usuarios.Count == 0
                ? 1
                : usuarios.Max(u => u.IdUsuario) + 1;

            usuarios.Add(usuario);

            await _storage.SalvarAsync(Arquivo, usuarios);

            return usuario;
        }

        public async Task<bool> AtualizarAsync(Usuario usuario)
        {
            var usuarios = await ObterTodosAsync();

            int indice = usuarios.FindIndex(
                u => u.IdUsuario == usuario.IdUsuario
            );

            if (indice == -1)
                return false;

            usuarios[indice] = usuario;

            await _storage.SalvarAsync(Arquivo, usuarios);

            return true;
        }
    }
}
