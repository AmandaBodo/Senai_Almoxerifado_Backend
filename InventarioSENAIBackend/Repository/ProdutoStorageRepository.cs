
using InventarioSENAIBackend.Models;
using InventarioSENAIBackend.Storage;

namespace InventarioSENAIBackend.Repository
{
    public class ProdutoStorageRepository
    {
        private readonly StorageService _storage;
        private const string Arquivo = "produtos.json";

        public ProdutoStorageRepository(StorageService storage)
        {
            _storage = storage;
        }

        public async Task<List<Produto>> ObterTodosAsync()
        {
            return await _storage.LerAsync<Produto>(Arquivo);
        }

        public async Task<Produto?> ObterPorIdAsync(int id)
        {
            var produtos = await ObterTodosAsync();

            return produtos.FirstOrDefault(p => p.IdProduto == id);
        }

        public async Task<Produto> AdicionarAsync(Produto produto)
        {
            var produtos = await ObterTodosAsync();

            produto.IdProduto = produtos.Count == 0
                ? 1
                : produtos.Max(p => p.IdProduto) + 1;

            produtos.Add(produto);

            await _storage.SalvarAsync(Arquivo, produtos);

            return produto;
        }

        public async Task<bool> AtualizarAsync(Produto produto)
        {
            var produtos = await ObterTodosAsync();

            int indice = produtos.FindIndex(
                p => p.IdProduto == produto.IdProduto
            );

            if (indice == -1)
                return false;

            produtos[indice] = produto;

            await _storage.SalvarAsync(Arquivo, produtos);

            return true;
        }
    }
}
