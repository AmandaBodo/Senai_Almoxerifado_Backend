
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventarioSENAIBackend.Models;
using InventarioSENAIBackend.Dtos;
using InventarioSENAIBackend.Repository;

namespace InventarioSENAIBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SalasController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetSalas()
        {
            var salas = await _context.Salas.ToListAsync();

            return Ok(salas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSala(int id)
        {
            var sala = await _context.Salas.FindAsync(id);

            if (sala == null)
                return NotFound(new { mensagem = "Sala não encontrada." });

            return Ok(sala);
        }

        [HttpPost]
        public async Task<IActionResult> CriarSala(SalaRequest request)
        {
            var codigoExiste = await _context.Salas
                .AnyAsync(s => s.CodigoSala == request.CodigoSala);

            if (codigoExiste)
                return BadRequest(new { mensagem = "Esse código de sala já está cadastrado." });

            var sala = new Sala
            {
                NomeSala = request.NomeSala,
                CodigoSala = request.CodigoSala,
                Descricao = request.Descricao,
                Localizacao = request.Localizacao,
                Status = request.Status
            };

            _context.Salas.Add(sala);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetSala),
                new { id = sala.IdSala },
                sala
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarSala(int id, SalaRequest request)
        {
            var sala = await _context.Salas.FindAsync(id);

            if (sala == null)
                return NotFound(new { mensagem = "Sala não encontrada." });

            var codigoExiste = await _context.Salas
                .AnyAsync(s => s.CodigoSala == request.CodigoSala && s.IdSala != id);

            if (codigoExiste)
                return BadRequest(new { mensagem = "Esse código já pertence a outra sala." });

            sala.NomeSala = request.NomeSala;
            sala.CodigoSala = request.CodigoSala;
            sala.Descricao = request.Descricao;
            sala.Localizacao = request.Localizacao;
            sala.Status = request.Status;

            await _context.SaveChangesAsync();

            return Ok(sala);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> ExcluirSala(int id)
        {
            var sala = await _context.Salas.FindAsync(id);

            if (sala == null)
                return NotFound(new { mensagem = "Sala não encontrada." });

            sala.Status = "Inativo";

            await _context.SaveChangesAsync();

            return Ok(new { mensagem = "Sala desativada com sucesso." });
        }
    }
}
