
using InventarioSENAIBackend.Models;
using InventarioSENAIBackend.Storage;

namespace InventarioSENAIBackend.Repository
{
    public class MovimentacaoStorageRepository
    {
        private readonly StorageService _storage;
        private const string Arquivo = "movimentacoes.json";

        public MovimentacaoStorageRepository(StorageService storage)
        {
            _storage = storage;
        }

        public async Task<List<Movimentacao>> ObterTodasAsync()
        {
            return await _storage.LerAsync<Movimentacao>(Arquivo);
        }

        public async Task<Movimentacao?> ObterPorIdAsync(int id)
        {
            var movimentacoes = await ObterTodasAsync();

            return movimentacoes.FirstOrDefault(
                m => m.IdMovimentacao == id
            );
        }

        public async Task<List<Movimentacao>> ObterPorProdutoAsync(int idProduto)
        {
            var movimentacoes = await ObterTodasAsync();

            return movimentacoes
                .Where(m => m.IdProduto == idProduto)
                .OrderByDescending(m => m.DataMovimentacao)
                .ToList();
        }

        public async Task<List<Movimentacao>> ObterPorUsuarioAsync(int idUsuario)
        {
            var movimentacoes = await ObterTodasAsync();

            return movimentacoes
                .Where(m => m.IdUsuario == idUsuario)
                .OrderByDescending(m => m.DataMovimentacao)
                .ToList();
        }

        public async Task<List<Movimentacao>> ObterPorSalaAsync(int idSala)
        {
            var movimentacoes = await ObterTodasAsync();

            return movimentacoes
                .Where(m =>
                    m.IdSalaOrigem == idSala ||
                    m.IdSalaDestino == idSala
                )
                .OrderByDescending(m => m.DataMovimentacao)
                .ToList();
        }

        public async Task<Movimentacao> AdicionarAsync(
            Movimentacao movimentacao)
        {
            var movimentacoes = await ObterTodasAsync();

            movimentacao.IdMovimentacao = movimentacoes.Count == 0
                ? 1
                : movimentacoes.Max(m => m.IdMovimentacao) + 1;

            movimentacao.Produto = null;
            movimentacao.Usuario = null;
            movimentacao.Emprestimo = null;
            movimentacao.SalaOrigem = null;
            movimentacao.SalaDestino = null;

            movimentacoes.Add(movimentacao);

            await _storage.SalvarAsync(Arquivo, movimentacoes);

            return movimentacao;
        }

        public async Task<bool> AtualizarAsync(
            Movimentacao movimentacao)
        {
            var movimentacoes = await ObterTodasAsync();

            int indice = movimentacoes.FindIndex(
                m => m.IdMovimentacao == movimentacao.IdMovimentacao
            );

            if (indice == -1)
                return false;

            movimentacao.Produto = null;
            movimentacao.Usuario = null;
            movimentacao.Emprestimo = null;
            movimentacao.SalaOrigem = null;
            movimentacao.SalaDestino = null;

            movimentacoes[indice] = movimentacao;

            await _storage.SalvarAsync(Arquivo, movimentacoes);

            return true;
        }
    }
}
