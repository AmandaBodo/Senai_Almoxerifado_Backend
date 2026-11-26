
using Microsoft.AspNetCore.Mvc;
using InventarioSENAIBackend.Models;
using InventarioSENAIBackend.Dtos;
using InventarioSENAIBackend.Repository;

namespace InventarioSENAIBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmprestimosController : ControllerBase
    {
        private readonly EmprestimoStorageRepository _emprestimoRepository;
        private readonly UsuarioStorageRepository _usuarioRepository;
        private readonly ProdutoStorageRepository _produtoRepository;
        private readonly SalaStorageRepository _salaRepository;

        public EmprestimosController(
            EmprestimoStorageRepository emprestimoRepository,
            UsuarioStorageRepository usuarioRepository,
            ProdutoStorageRepository produtoRepository,
            SalaStorageRepository salaRepository)
        {
            _emprestimoRepository = emprestimoRepository;
            _usuarioRepository = usuarioRepository;
            _produtoRepository = produtoRepository;
            _salaRepository = salaRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetEmprestimos()
        {
            var emprestimos = await _emprestimoRepository.ObterTodosAsync();

            var usuarios = await _usuarioRepository.ObterTodosAsync();
            var produtos = await _produtoRepository.ObterTodosAsync();
            var salas = await _salaRepository.ObterTodasAsync();

            foreach (var emprestimo in emprestimos)
            {
                emprestimo.Usuario = usuarios.FirstOrDefault(
                    u => u.IdUsuario == emprestimo.IdUsuario
                );

                emprestimo.Produto = produtos.FirstOrDefault(
                    p => p.IdProduto == emprestimo.IdProduto
                );

                emprestimo.SalaOrigem = salas.FirstOrDefault(
                    s => s.IdSala == emprestimo.IdSalaOrigem
                );

                emprestimo.SalaDestino = salas.FirstOrDefault(
                    s => s.IdSala == emprestimo.IdSalaDestino
                );
            }

            return Ok(emprestimos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmprestimo(int id)
        {
            var emprestimo = await _emprestimoRepository.ObterPorIdAsync(id);

            if (emprestimo == null)
                return NotFound(new
                {
                    mensagem = "Empréstimo não encontrado."
                });

            emprestimo.Usuario = await _usuarioRepository.ObterPorIdAsync(
                emprestimo.IdUsuario
            );

            emprestimo.Produto = await _produtoRepository.ObterPorIdAsync(
                emprestimo.IdProduto
            );

            emprestimo.SalaOrigem = await _salaRepository.ObterPorIdAsync(
                emprestimo.IdSalaOrigem
            );

            emprestimo.SalaDestino = await _salaRepository.ObterPorIdAsync(
                emprestimo.IdSalaDestino
            );

            return Ok(emprestimo);
        }

        [HttpPost]
        public async Task<IActionResult> CriarEmprestimo(EmprestimoRequest request)
        {
            if (request.Quantidade <= 0)
                return BadRequest(new
                {
                    mensagem = "A quantidade deve ser maior que zero."
                });

            if (request.DataPrevistaDevolucao.Date < DateTime.Today)
                return BadRequest(new
                {
                    mensagem = "A data de devolução não pode estar no passado."
                });

            var usuario = await _usuarioRepository.ObterPorIdAsync(
                request.IdUsuario
            );

            if (usuario == null || usuario.Status != "Ativo")
                return BadRequest(new
                {
                    mensagem = "Usuário não encontrado ou inativo."
                });

            var produto = await _produtoRepository.ObterPorIdAsync(
                request.IdProduto
            );

            if (produto == null)
                return BadRequest(new
                {
                    mensagem = "Produto não encontrado."
                });

            if (produto.QuantidadeDisponivel < request.Quantidade)
                return BadRequest(new
                {
                    mensagem = "Quantidade disponível insuficiente."
                });

            var salaOrigem = await _salaRepository.ObterPorIdAsync(
                request.IdSalaOrigem
            );

            var salaDestino = await _salaRepository.ObterPorIdAsync(
                request.IdSalaDestino
            );

            if (salaOrigem == null || salaDestino == null)
                return BadRequest(new
                {
                    mensagem = "Uma das salas informadas não existe."
                });

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

            await _produtoRepository.AtualizarAsync(produto);

            var novoEmprestimo = await _emprestimoRepository.AdicionarAsync(
                emprestimo
            );

            novoEmprestimo.Usuario = usuario;
            novoEmprestimo.Produto = produto;
            novoEmprestimo.SalaOrigem = salaOrigem;
            novoEmprestimo.SalaDestino = salaDestino;

            return CreatedAtAction(
                nameof(GetEmprestimo),
                new { id = novoEmprestimo.IdEmprestimo },
                novoEmprestimo
            );
        }
    }
}
