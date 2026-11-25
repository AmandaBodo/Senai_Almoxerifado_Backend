
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventarioSENAIBackend.Dtos;
using InventarioSENAIBackend.Repository;

namespace InventarioSENAIBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LoginController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LoginController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u =>
                    u.Matricula == request.Matricula);

            if (usuario == null)
                return Unauthorized(new
                {
                    mensagem = "Matrícula ou senha incorreta."
                });

            if (usuario.Status != "Ativo")
                return Unauthorized(new
                {
                    mensagem = "Usuário inativo."
                });

            bool senhaValida = BCrypt.Net.BCrypt.Verify(
                request.Senha,
                usuario.SenhaHash
            );

            if (!senhaValida)
                return Unauthorized(new
                {
                    mensagem = "Matrícula ou senha incorreta."
                });

            return Ok(new
            {
                mensagem = "Login realizado com sucesso.",
                usuario = new
                {
                    usuario.IdUsuario,
                    usuario.Nome,
                    usuario.Matricula,
                    usuario.Email,
                    usuario.Perfil,
                    usuario.Status
                }
            });
        }
    }
}
