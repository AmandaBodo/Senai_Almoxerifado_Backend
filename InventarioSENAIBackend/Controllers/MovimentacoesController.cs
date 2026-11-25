
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventarioSENAIBackend.Models;
using InventarioSENAIBackend.Dtos;
using InventarioSENAIBackend.Repository;

namespace InventarioSENAIBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovimentacoesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MovimentacoesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetMovimentacoes()
        {
            var movimentacoes = await _context.Movimentacoes
                .Include(m => m.Produto)
                .Include(m => m.Usuario)
                .Include(m => m.Emprestimo)
                .Include(m => m.SalaOrigem)
                .Include(m => m.SalaDestino)
                .OrderByDescending(m => m.DataMovimentacao)
                .ToListAsync();

            return Ok(movimentacoes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMovimentacao(int id)
        {
            var movimentacao = await _context.Movimentacoes
                .Include(m => m.Produto)
                .Include(m => m.Usuario)
                .Include(m => m.Emprestimo)
                .Include(m => m.SalaOrigem)
                .Include(m => m.SalaDestino)
                .FirstOrDefaultAsync(m => m.IdMovimentacao == id);

            if (movimentacao == null)
                return NotFound(new { mensagem = "Movimentação não encontrada." });

            return Ok(movimentacao);
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
                return BadRequest(new { mensagem = "Tipo de movimentação inválido." });

            if (request.Quantidade <= 0)
                return BadRequest(new { mensagem = "A quantidade deve ser maior que zero." });

            var produtoExiste = await _context.Produtos
                .AnyAsync(p => p.IdProduto == request.IdProduto);

            if (!produtoExiste)
                return BadRequest(new { mensagem = "Produto não encontrado." });

            var usuarioExiste = await _context.Usuarios
                .AnyAsync(u => u.IdUsuario == request.IdUsuario &&
                               u.Status == "Ativo");

            if (!usuarioExiste)
                return BadRequest(new { mensagem = "Usuário não encontrado ou inativo." });

            if (request.IdEmprestimo.HasValue)
            {
                var emprestimoExiste = await _context.Emprestimos
                    .AnyAsync(e => e.IdEmprestimo == request.IdEmprestimo);

                if (!emprestimoExiste)
                    return BadRequest(new { mensagem = "Empréstimo não encontrado." });
            }

            if (request.IdSalaOrigem.HasValue)
            {
                var salaOrigemExiste = await _context.Salas
                    .AnyAsync(s => s.IdSala == request.IdSalaOrigem);

                if (!salaOrigemExiste)
                    return BadRequest(new { mensagem = "Sala de origem não encontrada." });
            }

            if (request.IdSalaDestino.HasValue)
            {
                var salaDestinoExiste = await _context.Salas
                    .AnyAsync(s => s.IdSala == request.IdSalaDestino);

                if (!salaDestinoExiste)
                    return BadRequest(new { mensagem = "Sala de destino não encontrada." });
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

            _context.Movimentacoes.Add(movimentacao);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetMovimentacao),
                new { id = movimentacao.IdMovimentacao },
                movimentacao
            );
        }
    }
}
