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
            if (string.IsNullOrWhiteSpace(usuario.Matricula))
                return BadRequest("A matrícula é obrigatória.");

            if (string.IsNullOrWhiteSpace(usuario.Email))
                return BadRequest("O e-mail é obrigatório.");

            if (string.IsNullOrWhiteSpace(usuario.SenhaHash))
                return BadRequest("A senha é obrigatória.");

            var usuarios = await _repository.ObterTodosAsync();

            if (usuarios.Any(u =>
                string.Equals(
                    u.Matricula.Trim(),
                    usuario.Matricula.Trim(),
                    StringComparison.OrdinalIgnoreCase
                )))
            {
                return BadRequest("Esta matrícula já está cadastrada.");
            }

            if (usuarios.Any(u =>
                string.Equals(
                    u.Email.Trim(),
                    usuario.Email.Trim(),
                    StringComparison.OrdinalIgnoreCase
                )))
            {
                return BadRequest("Este e-mail já está cadastrado.");
            }

            usuario.IdUsuario = 0;
            usuario.Matricula = usuario.Matricula.Trim();
            usuario.Email = usuario.Email.Trim();
            usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(
                usuario.SenhaHash
            );
            usuario.Status = "Ativo";

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

            if (string.IsNullOrWhiteSpace(dados.Matricula))
                return BadRequest("A matrícula é obrigatória.");

            if (string.IsNullOrWhiteSpace(dados.Email))
                return BadRequest("O e-mail é obrigatório.");

            var usuarios = await _repository.ObterTodosAsync();

            if (usuarios.Any(u =>
                u.IdUsuario != id &&
                string.Equals(
                    u.Matricula.Trim(),
                    dados.Matricula.Trim(),
                    StringComparison.OrdinalIgnoreCase
                )))
            {
                return BadRequest("Esta matrícula já está cadastrada por outro usuário.");
            }

            if (usuarios.Any(u =>
                u.IdUsuario != id &&
                string.Equals(
                    u.Email.Trim(),
                    dados.Email.Trim(),
                    StringComparison.OrdinalIgnoreCase
                )))
            {
                return BadRequest("Este e-mail já está cadastrado por outro usuário.");
            }

            usuario.Nome = dados.Nome;
            usuario.Matricula = dados.Matricula.Trim();
            usuario.Email = dados.Email.Trim();
            usuario.Perfil = dados.Perfil;

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

        [HttpPatch("{id}/desativar")]
        public async Task<IActionResult> DesativarUsuario(int id)
        {
            var usuario = await _repository.ObterPorIdAsync(id);

            if (usuario == null)
                return NotFound("Usuário não encontrado.");

            usuario.Status = "Inativo";

            await _repository.AtualizarAsync(usuario);

            return Ok("Usuário desativado com sucesso.");
        }

        [HttpPatch("{id}/ativar")]
        public async Task<IActionResult> AtivarUsuario(int id)
        {
            var usuario = await _repository.ObterPorIdAsync(id);

            if (usuario == null)
                return NotFound("Usuário não encontrado.");

            usuario.Status = "Ativo";

            await _repository.AtualizarAsync(usuario);

            return Ok("Usuário ativado com sucesso.");
        }
    }
}
