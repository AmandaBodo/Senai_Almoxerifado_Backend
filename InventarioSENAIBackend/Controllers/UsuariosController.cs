
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventarioSENAIBackend.Models;
using InventarioSENAIBackend.Repository;

namespace InventarioSENAIBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UsuariosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsuarios()
        {
            var usuarios = await _context.Usuarios
                .Select(u => new
                {
                    u.IdUsuario,
                    u.Nome,
                    u.Matricula,
                    u.Email,
                    u.Perfil,
                    u.Status
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUsuario(int id)
        {
            var usuario = await _context.Usuarios
                .Where(u => u.IdUsuario == id)
                .Select(u => new
                {
                    u.IdUsuario,
                    u.Nome,
                    u.Matricula,
                    u.Email,
                    u.Perfil,
                    u.Status
                })
                .FirstOrDefaultAsync();

            if (usuario == null)
                return NotFound("Usuário não encontrado.");

            return Ok(usuario);
        }

        [HttpPost]
        public async Task<IActionResult> PostUsuario(Usuario usuario)
        {
            usuario.IdUsuario = 0;
            usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(usuario.SenhaHash);

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUsuario), new { id = usuario.IdUsuario }, new
            {
                usuario.IdUsuario,
                usuario.Nome,
                usuario.Matricula,
                usuario.Email,
                usuario.Perfil,
                usuario.Status
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsuario(int id, Usuario dados)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return NotFound("Usuário não encontrado.");

            usuario.Nome = dados.Nome;
            usuario.Matricula = dados.Matricula;
            usuario.Email = dados.Email;
            usuario.Perfil = dados.Perfil;
            usuario.Status = dados.Status;

            if (!string.IsNullOrWhiteSpace(dados.SenhaHash))
                usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(dados.SenhaHash);

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUsuario(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return NotFound("Usuário não encontrado.");

            usuario.Status = "Inativo";

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
