using InventarioSENAIBackend.Models;
using InventarioSENAIBackend.Storage;

namespace InventarioSENAIBackend.Repository
{
    public class DevolucaoStorageRepository
    {
        private readonly StorageService _storage;
        private const string Arquivo = "devolucoes.json";

        public DevolucaoStorageRepository(StorageService storage)
        {
            _storage = storage;
        }

        public async Task<List<Devolucao>> ObterTodasAsync()
        {
            return await _storage.LerAsync<Devolucao>(Arquivo);
        }

        public async Task<Devolucao?> ObterPorIdAsync(int id)
        {
            var devolucoes = await ObterTodasAsync();

            return devolucoes.FirstOrDefault(
                d => d.IdDevolucao == id
            );
        }

        public async Task<Devolucao> AdicionarAsync(Devolucao devolucao)
        {
            var devolucoes = await ObterTodasAsync();

            devolucao.IdDevolucao = devolucoes.Count == 0
                ? 1
                : devolucoes.Max(d => d.IdDevolucao) + 1;

            devolucao.Emprestimo = null;
            devolucao.UsuarioConferente = null;

            devolucoes.Add(devolucao);

            await _storage.SalvarAsync(Arquivo, devolucoes);

            return devolucao;
        }

        public async Task<bool> AtualizarAsync(Devolucao devolucao)
        {
            var devolucoes = await ObterTodasAsync();

            int indice = devolucoes.FindIndex(
                d => d.IdDevolucao == devolucao.IdDevolucao
            );

            if (indice == -1)
                return false;

            devolucao.Emprestimo = null;
            devolucao.UsuarioConferente = null;

            devolucoes[indice] = devolucao;

            await _storage.SalvarAsync(Arquivo, devolucoes);

            return true;
        }
    }
}
