
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventarioSENAIBackend.Models;
using InventarioSENAIBackend.Dtos;
using InventarioSENAIBackend.Repository;

namespace InventarioSENAIBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DevolucoesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public DevolucoesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetDevolucoes()
        {
            var devolucoes = await _context.Devolucoes
                .Select(d => new
                {
                    d.IdDevolucao,
                    d.IdEmprestimo,
                    d.IdUsuarioConferente,
                    d.DataDevolucao,
                    d.EstadoDevolucao,
                    d.ComponentesConferidos,
                    d.Observacoes
                })
                .ToListAsync();

            return Ok(devolucoes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDevolucao(int id)
        {
            var devolucao = await _context.Devolucoes
                .Where(d => d.IdDevolucao == id)
                .Select(d => new
                {
                    d.IdDevolucao,
                    d.IdEmprestimo,
                    d.IdUsuarioConferente,
                    d.DataDevolucao,
                    d.EstadoDevolucao,
                    d.ComponentesConferidos,
                    d.Observacoes
                })
                .FirstOrDefaultAsync();

            if (devolucao == null)
                return NotFound(new { mensagem = "Devolução não encontrada." });

            return Ok(devolucao);
        }

        [HttpPost]
        public async Task<IActionResult> CriarDevolucao(DevolucaoRequest request)
        {
            var estadosValidos = new[]
            {
                "Completa",
                "Incompleta",
                "Danificada"
            };

            if (!estadosValidos.Contains(request.EstadoDevolucao))
                return BadRequest(new { mensagem = "Estado de devolução inválido." });

            var emprestimo = await _context.Emprestimos
                .Include(e => e.Produto)
                .FirstOrDefaultAsync(e => e.IdEmprestimo == request.IdEmprestimo);

            if (emprestimo == null)
                return NotFound(new { mensagem = "Empréstimo não encontrado." });

            if (emprestimo.Status == "Devolvido")
                return BadRequest(new { mensagem = "Esse empréstimo já foi devolvido." });

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.IdUsuario == request.IdUsuarioConferente);

            if (usuario == null || usuario.Status != "Ativo")
                return BadRequest(new { mensagem = "Usuário conferente não encontrado ou inativo." });

            var devolucao = new Devolucao
            {
                IdEmprestimo = request.IdEmprestimo,
                IdUsuarioConferente = request.IdUsuarioConferente,
                DataDevolucao = DateTime.Now,
                EstadoDevolucao = request.EstadoDevolucao,
                ComponentesConferidos = request.ComponentesConferidos,
                Observacoes = request.Observacoes
            };

            emprestimo.Status = "Devolvido";

            if (request.EstadoDevolucao == "Completa")
            {
                emprestimo.Produto.QuantidadeDisponivel += emprestimo.Quantidade;
            }

            _context.Devolucoes.Add(devolucao);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetDevolucao),
                new { id = devolucao.IdDevolucao },
                new
                {
                    devolucao.IdDevolucao,
                    devolucao.IdEmprestimo,
                    devolucao.DataDevolucao,
                    devolucao.EstadoDevolucao,
                    devolucao.ComponentesConferidos,
                    devolucao.Observacoes
                }
            );
        }
    }
}
