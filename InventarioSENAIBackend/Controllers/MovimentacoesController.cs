using Microsoft.AspNetCore.Mvc;
using InventarioSENAIBackend.Models;
using InventarioSENAIBackend.Repository;

namespace InventarioSENAIBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovimentacoesController : ControllerBase
    {
        private readonly MovimentacaoStorageRepository _movimentacaoRepository;
        private readonly ProdutoStorageRepository _produtoRepository;
        private readonly UsuarioStorageRepository _usuarioRepository;
        private readonly EmprestimoStorageRepository _emprestimoRepository;
        private readonly SalaStorageRepository _salaRepository;

        public MovimentacoesController(
            MovimentacaoStorageRepository movimentacaoRepository,
            ProdutoStorageRepository produtoRepository,
            UsuarioStorageRepository usuarioRepository,
            EmprestimoStorageRepository emprestimoRepository,
            SalaStorageRepository salaRepository)
        {
            _movimentacaoRepository = movimentacaoRepository;
            _produtoRepository = produtoRepository;
            _usuarioRepository = usuarioRepository;
            _emprestimoRepository = emprestimoRepository;
            _salaRepository = salaRepository;
        }

        private async Task<List<object>> MontarResultadoAsync(
            IEnumerable<Movimentacao> movimentacoes)
        {
            var produtos = await _produtoRepository.ObterTodosAsync();
            var usuarios = await _usuarioRepository.ObterTodosAsync();
            var emprestimos = await _emprestimoRepository.ObterTodosAsync();
            var salas = await _salaRepository.ObterTodasAsync();

            var resultado = new List<object>();

            foreach (var m in movimentacoes.OrderByDescending(
                m => m.DataMovimentacao))
            {
                var usuario = usuarios.FirstOrDefault(
                    u => u.IdUsuario == m.IdUsuario
                );

                resultado.Add(new
                {
                    m.IdMovimentacao,
                    m.IdProduto,
                    Produto = produtos.FirstOrDefault(
                        p => p.IdProduto == m.IdProduto
                    ),

                    m.IdUsuario,
                    Usuario = usuario == null ? null : new
                    {
                        usuario.IdUsuario,
                        usuario.Nome,
                        usuario.Matricula,
                        usuario.Perfil,
                        usuario.Status
                    },

                    m.IdEmprestimo,
                    Emprestimo = m.IdEmprestimo.HasValue
                        ? emprestimos.FirstOrDefault(
                            e => e.IdEmprestimo == m.IdEmprestimo.Value
                        )
                        : null,

                    m.IdSalaOrigem,
                    SalaOrigem = m.IdSalaOrigem.HasValue
                        ? salas.FirstOrDefault(
                            s => s.IdSala == m.IdSalaOrigem.Value
                        )
                        : null,

                    m.IdSalaDestino,
                    SalaDestino = m.IdSalaDestino.HasValue
                        ? salas.FirstOrDefault(
                            s => s.IdSala == m.IdSalaDestino.Value
                        )
                        : null,

                    m.TipoMovimentacao,
                    m.Quantidade,
                    m.DataMovimentacao,
                    m.Descricao
                });
            }

            return resultado;
        }

        [HttpGet]
        public async Task<IActionResult> GetMovimentacoes()
        {
            var movimentacoes =
                await _movimentacaoRepository.ObterTodasAsync();

            var resultado = await MontarResultadoAsync(movimentacoes);

            return Ok(resultado);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetMovimentacao(int id)
        {
            var movimentacao =
                await _movimentacaoRepository.ObterPorIdAsync(id);

            if (movimentacao == null)
                return NotFound(new
                {
                    mensagem = "Movimentação não encontrada."
                });

            var resultado = await MontarResultadoAsync(
                new List<Movimentacao> { movimentacao }
            );

            return Ok(resultado.First());
        }

        [HttpGet("produto/{id:int}")]
        public async Task<IActionResult> GetPorProduto(int id)
        {
            var produto = await _produtoRepository.ObterPorIdAsync(id);

            if (produto == null)
                return NotFound(new
                {
                    mensagem = "Produto não encontrado."
                });

            var movimentacoes =
                await _movimentacaoRepository.ObterPorProdutoAsync(id);

            var resultado = await MontarResultadoAsync(movimentacoes);

            return Ok(resultado);
        }

        [HttpGet("sala/{id:int}")]
        public async Task<IActionResult> GetPorSala(int id)
        {
            var sala = await _salaRepository.ObterPorIdAsync(id);

            if (sala == null)
                return NotFound(new
                {
                    mensagem = "Sala não encontrada."
                });

            var movimentacoes =
                await _movimentacaoRepository.ObterPorSalaAsync(id);

            var resultado = await MontarResultadoAsync(movimentacoes);

            return Ok(resultado);
        }

        [HttpGet("usuario/{id:int}")]
        public async Task<IActionResult> GetPorUsuario(int id)
        {
            var usuario = await _usuarioRepository.ObterPorIdAsync(id);

            if (usuario == null)
                return NotFound(new
                {
                    mensagem = "Usuário não encontrado."
                });

            var movimentacoes =
                await _movimentacaoRepository.ObterPorUsuarioAsync(id);

            var resultado = await MontarResultadoAsync(movimentacoes);

            return Ok(resultado);
        }
    }
}
