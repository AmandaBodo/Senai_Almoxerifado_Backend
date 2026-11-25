
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventarioSENAIBackend.Models;
using InventarioSENAIBackend.Dtos;
using InventarioSENAIBackend.Repository;

namespace InventarioSENAIBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmprestimosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public EmprestimosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetEmprestimos()
        {
            var emprestimos = await _context.Emprestimos
                .Include(e => e.Usuario)
                .Include(e => e.Produto)
                .Include(e => e.SalaOrigem)
                .Include(e => e.SalaDestino)
                .ToListAsync();

            return Ok(emprestimos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmprestimo(int id)
        {
            var emprestimo = await _context.Emprestimos
                .Include(e => e.Usuario)
                .Include(e => e.Produto)
                .Include(e => e.SalaOrigem)
                .Include(e => e.SalaDestino)
                .FirstOrDefaultAsync(e => e.IdEmprestimo == id);

            if (emprestimo == null)
                return NotFound(new { mensagem = "Empréstimo não encontrado." });

            return Ok(emprestimo);
        }

        [HttpPost]
        public async Task<IActionResult> CriarEmprestimo(EmprestimoRequest request)
        {
            if (request.Quantidade <= 0)
                return BadRequest(new { mensagem = "A quantidade deve ser maior que zero." });

            if (request.DataPrevistaDevolucao.Date < DateTime.Today)
                return BadRequest(new { mensagem = "A data de devolução não pode estar no passado." });

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == request.IdUsuario);

            if (usuario == null || usuario.Status != "Ativo")
                return BadRequest(new { mensagem = "Usuário não encontrado ou inativo." });

            var produto = await _context.Produtos
                .FirstOrDefaultAsync(p => p.IdProduto == request.IdProduto);

            if (produto == null)
                return BadRequest(new { mensagem = "Produto não encontrado." });

            if (produto.QuantidadeDisponivel < request.Quantidade)
                return BadRequest(new { mensagem = "Quantidade disponível insuficiente." });

            var salaOrigem = await _context.Salas
                .AnyAsync(s => s.IdSala == request.IdSalaOrigem);

            var salaDestino = await _context.Salas
                .AnyAsync(s => s.IdSala == request.IdSalaDestino);

            if (!salaOrigem || !salaDestino)
                return BadRequest(new { mensagem = "Uma das salas informadas não existe." });

            var emprestimo = new Emprestimo
            {
                IdUsuario = request.IdUsuario,
                IdProduto = request.IdProduto,
                IdSalaOrigem = request.IdSalaOrigem,
                IdSalaDestino = request.IdSalaDestino,
                Quantidade = request.Quantidade,
                DataEmprestimo = DateTime.Now,
                DataPrevistaDevolucao = request.DataPrevistaDevolucao,
                Status = "Ativo"
            };

            produto.QuantidadeDisponivel -= request.Quantidade;

            _context.Emprestimos.Add(emprestimo);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetEmprestimo),
                new { id = emprestimo.IdEmprestimo },
                emprestimo
            );
        }
    }
}
