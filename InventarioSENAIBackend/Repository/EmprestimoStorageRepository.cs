
using InventarioSENAIBackend.Models;
using InventarioSENAIBackend.Storage;

namespace InventarioSENAIBackend.Repository
{
    public class EmprestimoStorageRepository
    {
        private readonly StorageService _storage;
        private const string Arquivo = "emprestimos.json";

        public EmprestimoStorageRepository(StorageService storage)
        {
            _storage = storage;
        }

        public async Task<List<Emprestimo>> ObterTodosAsync()
        {
            return await _storage.LerAsync<Emprestimo>(Arquivo);
        }

        public async Task<Emprestimo?> ObterPorIdAsync(int id)
        {
            var emprestimos = await ObterTodosAsync();

            return emprestimos.FirstOrDefault(
                e => e.IdEmprestimo == id
            );
        }

        public async Task<Emprestimo> AdicionarAsync(Emprestimo emprestimo)
        {
            var emprestimos = await ObterTodosAsync();

            emprestimo.IdEmprestimo = emprestimos.Count == 0
                ? 1
                : emprestimos.Max(e => e.IdEmprestimo) + 1;

            emprestimo.Usuario = null;
            emprestimo.Produto = null;
            emprestimo.SalaOrigem = null;
            emprestimo.SalaDestino = null;

            emprestimos.Add(emprestimo);

            await _storage.SalvarAsync(Arquivo, emprestimos);

            return emprestimo;
        }

        public async Task<bool> AtualizarAsync(Emprestimo emprestimo)
        {
            var emprestimos = await ObterTodosAsync();

            int indice = emprestimos.FindIndex(
                e => e.IdEmprestimo == emprestimo.IdEmprestimo
            );

            if (indice == -1)
                return false;

            emprestimo.Usuario = null;
            emprestimo.Produto = null;
            emprestimo.SalaOrigem = null;
            emprestimo.SalaDestino = null;

            emprestimos[indice] = emprestimo;

            await _storage.SalvarAsync(Arquivo, emprestimos);

            return true;
        }
    }
}
