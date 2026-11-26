using Microsoft.AspNetCore.Mvc;
using InventarioSENAIBackend.Models;
using InventarioSENAIBackend.Repository;
using InventarioSENAIBackend.Dtos;

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

        // GET: api/Salas
        [HttpGet]
        public async Task<IActionResult> GetSalas()
        {
            var salas = await _repository.ObterTodasAsync();

            return Ok(salas);
        }

        // GET: api/Salas/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSala(int id)
        {
            var sala = await _repository.ObterPorIdAsync(id);

            if (sala == null)
                return NotFound("Sala não encontrada.");

            return Ok(sala);
        }

        // POST: api/Salas
        [HttpPost]
        public async Task<IActionResult> PostSala(SalaRequest dados)
        {
            var sala = new Sala
            {
                NomeSala = dados.NomeSala,
                CodigoSala = dados.CodigoSala,
                Descricao = dados.Descricao,
                Localizacao = dados.Localizacao,
                Status = dados.Status
            };

            var novaSala = await _repository.AdicionarAsync(sala);

            return CreatedAtAction(
                nameof(GetSala),
                new { id = novaSala.IdSala },
                novaSala
            );
        }

        // PUT: api/Salas/1
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSala(
            int id,
            SalaRequest dados)
        {
            var sala = await _repository.ObterPorIdAsync(id);

            if (sala == null)
                return NotFound("Sala não encontrada.");

            sala.NomeSala = dados.NomeSala;
            sala.CodigoSala = dados.CodigoSala;
            sala.Descricao = dados.Descricao;
            sala.Localizacao = dados.Localizacao;
            sala.Status = dados.Status;

            bool atualizado = await _repository.AtualizarAsync(sala);

            if (!atualizado)
                return NotFound("Não foi possível atualizar a sala.");

            return NoContent();
        }
    }
}
