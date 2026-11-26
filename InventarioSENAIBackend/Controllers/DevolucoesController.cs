
using Microsoft.AspNetCore.Mvc;
using InventarioSENAIBackend.Models;
using InventarioSENAIBackend.Dtos;
using InventarioSENAIBackend.Repository;

namespace InventarioSENAIBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DevolucoesController : ControllerBase
    {
        private readonly DevolucaoStorageRepository _devolucaoRepository;
        private readonly EmprestimoStorageRepository _emprestimoRepository;
        private readonly UsuarioStorageRepository _usuarioRepository;
        private readonly ProdutoStorageRepository _produtoRepository;

        public DevolucoesController(
            DevolucaoStorageRepository devolucaoRepository,
            EmprestimoStorageRepository emprestimoRepository,
            UsuarioStorageRepository usuarioRepository,
            ProdutoStorageRepository produtoRepository)
        {
            _devolucaoRepository = devolucaoRepository;
            _emprestimoRepository = emprestimoRepository;
            _usuarioRepository = usuarioRepository;
            _produtoRepository = produtoRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetDevolucoes()
        {
            var devolucoes = await _devolucaoRepository.ObterTodasAsync();

            return Ok(devolucoes.Select(d => new
            {
                d.IdDevolucao,
                d.IdEmprestimo,
                d.IdUsuarioConferente,
                d.DataDevolucao,
                d.EstadoDevolucao,
                d.ComponentesConferidos,
                d.Observacoes
            }));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDevolucao(int id)
        {
            var devolucao = await _devolucaoRepository.ObterPorIdAsync(id);

            if (devolucao == null)
                return NotFound(new
                {
                    mensagem = "Devolução não encontrada."
                });

            return Ok(new
            {
                devolucao.IdDevolucao,
                devolucao.IdEmprestimo,
                devolucao.IdUsuarioConferente,
                devolucao.DataDevolucao,
                devolucao.EstadoDevolucao,
                devolucao.ComponentesConferidos,
                devolucao.Observacoes
            });
        }

        [HttpPost]
        public async Task<IActionResult> CriarDevolucao(
            DevolucaoRequest request)
        {
            var estadosValidos = new[]
            {
                "Completa",
                "Incompleta",
                "Danificada"
            };

            if (!estadosValidos.Contains(request.EstadoDevolucao))
                return BadRequest(new
                {
                    mensagem = "Estado de devolução inválido."
                });

            var emprestimo = await _emprestimoRepository.ObterPorIdAsync(
                request.IdEmprestimo
            );

            if (emprestimo == null)
                return NotFound(new
                {
                    mensagem = "Empréstimo não encontrado."
                });

            if (emprestimo.Status == "Devolvido")
                return BadRequest(new
                {
                    mensagem = "Esse empréstimo já foi devolvido."
                });

            var usuario = await _usuarioRepository.ObterPorIdAsync(
                request.IdUsuarioConferente
            );

            if (usuario == null || usuario.Status != "Ativo")
                return BadRequest(new
                {
                    mensagem = "Usuário conferente não encontrado ou inativo."
                });

            var produto = await _produtoRepository.ObterPorIdAsync(
                emprestimo.IdProduto
            );

            if (produto == null)
                return BadRequest(new
                {
                    mensagem = "Produto associado ao empréstimo não encontrado."
                });

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
                produto.QuantidadeDisponivel += emprestimo.Quantidade;

                await _produtoRepository.AtualizarAsync(produto);
            }

            await _emprestimoRepository.AtualizarAsync(emprestimo);

            var novaDevolucao = await _devolucaoRepository.AdicionarAsync(
                devolucao
            );

            return CreatedAtAction(
                nameof(GetDevolucao),
                new { id = novaDevolucao.IdDevolucao },
                new
                {
                    novaDevolucao.IdDevolucao,
                    novaDevolucao.IdEmprestimo,
                    novaDevolucao.IdUsuarioConferente,
                    novaDevolucao.DataDevolucao,
                    novaDevolucao.EstadoDevolucao,
                    novaDevolucao.ComponentesConferidos,
                    novaDevolucao.Observacoes
                }
            );
        }
    }
}
