
using Microsoft.AspNetCore.Mvc;
using InventarioSENAIBackend.Models;
using InventarioSENAIBackend.Repository;

namespace InventarioSENAIBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly UsuarioStorageRepository _repository;

        public UsuariosController(UsuarioStorageRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsuarios()
        {
            var usuarios = await _repository.ObterTodosAsync();

            var resultado = usuarios.Select(u => new
            {
                u.IdUsuario,
                u.Nome,
                u.Matricula,
                u.Email,
                u.Perfil,
                u.Status
            });

            return Ok(resultado);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUsuario(int id)
        {
            var usuario = await _repository.ObterPorIdAsync(id);

            if (usuario == null)
                return NotFound("Usuário não encontrado.");

            return Ok(new
            {
                usuario.IdUsuario,
                usuario.Nome,
                usuario.Matricula,
                usuario.Email,
                usuario.Perfil,
                usuario.Status
            });
        }

        [HttpPost]
        public async Task<IActionResult> PostUsuario(Usuario usuario)
        {
            usuario.IdUsuario = 0;

            usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(
                usuario.SenhaHash
            );

            var novoUsuario = await _repository.AdicionarAsync(usuario);

            return CreatedAtAction(
                nameof(GetUsuario),
                new { id = novoUsuario.IdUsuario },
                new
                {
                    novoUsuario.IdUsuario,
                    novoUsuario.Nome,
                    novoUsuario.Matricula,
                    novoUsuario.Email,
                    novoUsuario.Perfil,
                    novoUsuario.Status
                }
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, Usuario dados)
        {
            var usuario = await _repository.ObterPorIdAsync(id);

            if (usuario == null)
                return NotFound("Usuário não encontrado.");

            usuario.Nome = dados.Nome;
            usuario.Matricula = dados.Matricula;
            usuario.Email = dados.Email;
            usuario.Perfil = dados.Perfil;
            usuario.Status = dados.Status;

            if (!string.IsNullOrWhiteSpace(dados.SenhaHash))
            {
                usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(
                    dados.SenhaHash
                );
            }

            await _repository.AtualizarAsync(usuario);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _repository.ObterPorIdAsync(id);

            if (usuario == null)
                return NotFound("Usuário não encontrado.");

            usuario.Status = "Inativo";

            await _repository.AtualizarAsync(usuario);

            return NoContent();
        }
    }
}
