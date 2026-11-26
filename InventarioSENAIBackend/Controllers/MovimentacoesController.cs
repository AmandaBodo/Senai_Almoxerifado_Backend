
using Microsoft.AspNetCore.Mvc;
using InventarioSENAIBackend.Models;
using InventarioSENAIBackend.Dtos;
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

        [HttpGet]
        public async Task<IActionResult> GetMovimentacoes()
        {
            var movimentacoes =
                await _movimentacaoRepository.ObterTodasAsync();

            var produtos = await _produtoRepository.ObterTodosAsync();
            var usuarios = await _usuarioRepository.ObterTodosAsync();
            var emprestimos = await _emprestimoRepository.ObterTodosAsync();
            var salas = await _salaRepository.ObterTodasAsync();

            var resultado = movimentacoes
                .OrderByDescending(m => m.DataMovimentacao)
                .Select(m => new
                {
                    m.IdMovimentacao,
                    m.IdProduto,
                    Produto = produtos.FirstOrDefault(
                        p => p.IdProduto == m.IdProduto
                    ),
                    m.IdUsuario,
                    Usuario = usuarios
                        .Where(u => u.IdUsuario == m.IdUsuario)
                        .Select(u => new
                        {
                            u.IdUsuario,
                            u.Nome,
                            u.Matricula,
                            u.Email,
                            u.Perfil,
                            u.Status
                        })
                        .FirstOrDefault(),
                    m.IdEmprestimo,
                    Emprestimo = emprestimos.FirstOrDefault(
                        e => e.IdEmprestimo == m.IdEmprestimo
                    ),
                    m.IdSalaOrigem,
                    SalaOrigem = salas.FirstOrDefault(
                        s => s.IdSala == m.IdSalaOrigem
                    ),
                    m.IdSalaDestino,
                    SalaDestino = salas.FirstOrDefault(
                        s => s.IdSala == m.IdSalaDestino
                    ),
                    m.TipoMovimentacao,
                    m.Quantidade,
                    m.DataMovimentacao,
                    m.Descricao
                });

            return Ok(resultado);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMovimentacao(int id)
        {
            var movimentacao =
                await _movimentacaoRepository.ObterPorIdAsync(id);

            if (movimentacao == null)
                return NotFound(new
                {
                    mensagem = "Movimentação não encontrada."
                });

            var produto = await _produtoRepository.ObterPorIdAsync(
                movimentacao.IdProduto
            );

            var usuario = await _usuarioRepository.ObterPorIdAsync(
                movimentacao.IdUsuario
            );

            var emprestimo = movimentacao.IdEmprestimo.HasValue
                ? await _emprestimoRepository.ObterPorIdAsync(
                    movimentacao.IdEmprestimo.Value)
                : null;

            var salaOrigem = movimentacao.IdSalaOrigem.HasValue
                ? await _salaRepository.ObterPorIdAsync(
                    movimentacao.IdSalaOrigem.Value)
                : null;

            var salaDestino = movimentacao.IdSalaDestino.HasValue
                ? await _salaRepository.ObterPorIdAsync(
                    movimentacao.IdSalaDestino.Value)
                : null;

            return Ok(new
            {
                movimentacao.IdMovimentacao,
                movimentacao.IdProduto,
                Produto = produto,
                movimentacao.IdUsuario,
                Usuario = usuario == null ? null : new
                {
                    usuario.IdUsuario,
                    usuario.Nome,
                    usuario.Matricula,
                    usuario.Perfil,
                    usuario.Status
                },
                movimentacao.IdEmprestimo,
                Emprestimo = emprestimo,
                movimentacao.IdSalaOrigem,
                SalaOrigem = salaOrigem,
                movimentacao.IdSalaDestino,
                SalaDestino = salaDestino,
                movimentacao.TipoMovimentacao,
                movimentacao.Quantidade,
                movimentacao.DataMovimentacao,
                movimentacao.Descricao
            });
        }

        [HttpPost]
        public async Task<IActionResult> CriarMovimentacao(
            MovimentacaoRequest request)
        {
            var tiposValidos = new[]
            {
                "Entrada",
                "Saida",
                "Emprestimo",
                "Devolucao",
                "Transferencia",
                "Ajuste",
                "Manutencao"
            };

            if (!tiposValidos.Contains(request.TipoMovimentacao))
                return BadRequest(new
                {
                    mensagem = "Tipo de movimentação inválido."
                });

            if (request.Quantidade <= 0)
                return BadRequest(new
                {
                    mensagem = "A quantidade deve ser maior que zero."
                });

            var produto = await _produtoRepository.ObterPorIdAsync(
                request.IdProduto
            );

            if (produto == null)
                return BadRequest(new
                {
                    mensagem = "Produto não encontrado."
                });

            var usuario = await _usuarioRepository.ObterPorIdAsync(
                request.IdUsuario
            );

            if (usuario == null || usuario.Status != "Ativo")
                return BadRequest(new
                {
                    mensagem = "Usuário não encontrado ou inativo."
                });

            if (request.IdEmprestimo.HasValue)
            {
                var emprestimo =
                    await _emprestimoRepository.ObterPorIdAsync(
                        request.IdEmprestimo.Value
                    );

                if (emprestimo == null)
                    return BadRequest(new
                    {
                        mensagem = "Empréstimo não encontrado."
                    });
            }

            if (request.IdSalaOrigem.HasValue)
            {
                var salaOrigem = await _salaRepository.ObterPorIdAsync(
                    request.IdSalaOrigem.Value
                );

                if (salaOrigem == null)
                    return BadRequest(new
                    {
                        mensagem = "Sala de origem não encontrada."
                    });
            }

            if (request.IdSalaDestino.HasValue)
            {
                var salaDestino = await _salaRepository.ObterPorIdAsync(
                    request.IdSalaDestino.Value
                );

                if (salaDestino == null)
                    return BadRequest(new
                    {
                        mensagem = "Sala de destino não encontrada."
                    });
            }

            var movimentacao = new Movimentacao
            {
                IdProduto = request.IdProduto,
                IdUsuario = request.IdUsuario,
                IdEmprestimo = request.IdEmprestimo,
                IdSalaOrigem = request.IdSalaOrigem,
                IdSalaDestino = request.IdSalaDestino,
                TipoMovimentacao = request.TipoMovimentacao,
                Quantidade = request.Quantidade,
                DataMovimentacao = DateTime.Now,
                Descricao = request.Descricao
            };

            var novaMovimentacao =
                await _movimentacaoRepository.AdicionarAsync(movimentacao);

            return CreatedAtAction(
                nameof(GetMovimentacao),
                new { id = novaMovimentacao.IdMovimentacao },
                new
                {
                    novaMovimentacao.IdMovimentacao,
                    novaMovimentacao.IdProduto,
                    novaMovimentacao.IdUsuario,
                    novaMovimentacao.IdEmprestimo,
                    novaMovimentacao.IdSalaOrigem,
                    novaMovimentacao.IdSalaDestino,
                    novaMovimentacao.TipoMovimentacao,
                    novaMovimentacao.Quantidade,
                    novaMovimentacao.DataMovimentacao,
                    novaMovimentacao.Descricao
                }
            );
        }
    }
}
