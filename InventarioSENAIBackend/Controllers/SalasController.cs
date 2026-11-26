
using Microsoft.AspNetCore.Mvc;
using InventarioSENAIBackend.Models;
using InventarioSENAIBackend.Repository;

namespace InventarioSENAIBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SalasController : ControllerBase
    {
        private readonly SalaStorageRepository _repository;

        public SalasController(SalaStorageRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetSalas()
        {
            var salas = await _repository.ObterTodasAsync();

            return Ok(salas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSala(int id)
        {
            var sala = await _repository.ObterPorIdAsync(id);

            if (sala == null)
                return NotFound("Sala não encontrada.");

            return Ok(sala);
        }

        [HttpPost]
        public async Task<IActionResult> PostSala(Sala sala)
        {
            sala.IdSala = 0;

            var novaSala = await _repository.AdicionarAsync(sala);

            return CreatedAtAction(
                nameof(GetSala),
                new { id = novaSala.IdSala },
                novaSala
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutSala(int id, Sala dados)
        {
            var sala = await _repository.ObterPorIdAsync(id);

            if (sala == null)
                return NotFound("Sala não encontrada.");

            sala.NomeSala = dados.NomeSala;
            sala.CodigoSala = dados.CodigoSala;
            sala.Descricao = dados.Descricao;
            sala.Localizacao = dados.Localizacao;
            sala.Status = dados.Status;

            await _repository.AtualizarAsync(sala);

            return NoContent();
        }
    }
}
