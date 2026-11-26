
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
        private readonly MovimentacaoStorageRepository _movimentacaoRepository;

        public DevolucoesController(
            DevolucaoStorageRepository devolucaoRepository,
            EmprestimoStorageRepository emprestimoRepository,
            UsuarioStorageRepository usuarioRepository,
            ProdutoStorageRepository produtoRepository,
            MovimentacaoStorageRepository movimentacaoRepository)
        {
            _devolucaoRepository = devolucaoRepository;
            _emprestimoRepository = emprestimoRepository;
            _usuarioRepository = usuarioRepository;
            _produtoRepository = produtoRepository;
            _movimentacaoRepository = movimentacaoRepository;
        }

        private string VerificarPrazo(
            DateTime dataDevolucao,
            DateTime dataPrevista)
        {
            return dataDevolucao <= dataPrevista
                ? "No prazo"
                : "Em atraso";
        }

        [HttpGet]
        public async Task<IActionResult> GetDevolucoes()
        {
            var devolucoes = await _devolucaoRepository.ObterTodasAsync();
            var emprestimos = await _emprestimoRepository.ObterTodosAsync();

            var resultado = devolucoes.Select(d =>
            {
                var emprestimo = emprestimos.FirstOrDefault(
                    e => e.IdEmprestimo == d.IdEmprestimo
                );

                return new
                {
                    d.IdDevolucao,
                    d.IdEmprestimo,
                    d.IdUsuarioConferente,
                    d.DataDevolucao,
                    d.QuantidadeBoa,
                    d.QuantidadeDanificada,
                    d.ComponentesConferidos,
                    d.Observacoes,
                    DataPrevistaDevolucao =
                        emprestimo?.DataPrevistaDevolucao,
                    SituacaoPrazo = emprestimo == null
                        ? "Empréstimo não encontrado"
                        : VerificarPrazo(
                            d.DataDevolucao,
                            emprestimo.DataPrevistaDevolucao
                        )
                };
            });

            return Ok(resultado);
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

            var emprestimo = await _emprestimoRepository.ObterPorIdAsync(
                devolucao.IdEmprestimo
            );

            return Ok(new
            {
                devolucao.IdDevolucao,
                devolucao.IdEmprestimo,
                devolucao.IdUsuarioConferente,
                devolucao.DataDevolucao,
                devolucao.QuantidadeBoa,
                devolucao.QuantidadeDanificada,
                devolucao.ComponentesConferidos,
                devolucao.Observacoes,
                DataPrevistaDevolucao =
                    emprestimo?.DataPrevistaDevolucao,
                SituacaoPrazo = emprestimo == null
                    ? "Empréstimo não encontrado"
                    : VerificarPrazo(
                        devolucao.DataDevolucao,
                        emprestimo.DataPrevistaDevolucao
                    )
            });
        }

        [HttpPost]
        public async Task<IActionResult> CriarDevolucao(
            DevolucaoRequest request)
        {
            if (request.QuantidadeBoa < 0 ||
                request.QuantidadeDanificada < 0)
            {
                return BadRequest(new
                {
                    mensagem = "As quantidades não podem ser negativas."
                });
            }

            int quantidadeDevolvida =
                request.QuantidadeBoa +
                request.QuantidadeDanificada;

            if (quantidadeDevolvida <= 0)
                return BadRequest(new
                {
                    mensagem = "Informe pelo menos um item devolvido."
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
                    mensagem = "Esse empréstimo já foi finalizado."
                });

            int quantidadePendente =
                emprestimo.Quantidade - emprestimo.QuantidadeDevolvida;

            if (quantidadeDevolvida > quantidadePendente)
                return BadRequest(new
                {
                    mensagem = "A quantidade devolvida ultrapassa os itens pendentes.",
                    quantidadePendente,
                    quantidadeInformada = quantidadeDevolvida
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
                QuantidadeBoa = request.QuantidadeBoa,
                QuantidadeDanificada = request.QuantidadeDanificada,
                ComponentesConferidos = request.ComponentesConferidos,
                Observacoes = request.Observacoes
            };

            // Atualização do estoque
            produto.QuantidadeDisponivel += request.QuantidadeBoa;
            produto.QuantidadeDanificada += request.QuantidadeDanificada;

            await _produtoRepository.AtualizarAsync(produto);

            // Atualização do empréstimo
            emprestimo.QuantidadeDevolvida += quantidadeDevolvida;

            int quantidadeRestante =
                emprestimo.Quantidade - emprestimo.QuantidadeDevolvida;

            emprestimo.Status = quantidadeRestante == 0
                ? "Devolvido"
                : "Ativo";

            await _emprestimoRepository.AtualizarAsync(emprestimo);

            var novaDevolucao = await _devolucaoRepository.AdicionarAsync(
                devolucao
            );

            // Registro automático dos itens devolvidos em boas condições
            if (request.QuantidadeBoa > 0)
            {
                var movimentacaoBoa = new Movimentacao
                {
                    IdProduto = emprestimo.IdProduto,
                    IdUsuario = request.IdUsuarioConferente,
                    IdEmprestimo = emprestimo.IdEmprestimo,
                    IdSalaOrigem = emprestimo.IdSalaDestino,
                    IdSalaDestino = emprestimo.IdSalaOrigem,
                    TipoMovimentacao = "Devolucao",
                    Quantidade = request.QuantidadeBoa,
                    DataMovimentacao = DateTime.Now,
                    Descricao = "Devolução de itens em boas condições."
                };

                await _movimentacaoRepository.AdicionarAsync(
                    movimentacaoBoa
                );
            }

            // Registro automático dos itens devolvidos danificados
            if (request.QuantidadeDanificada > 0)
            {
                var movimentacaoDanificada = new Movimentacao
                {
                    IdProduto = emprestimo.IdProduto,
                    IdUsuario = request.IdUsuarioConferente,
                    IdEmprestimo = emprestimo.IdEmprestimo,
                    IdSalaOrigem = emprestimo.IdSalaDestino,
                    IdSalaDestino = emprestimo.IdSalaOrigem,
                    TipoMovimentacao = "Manutencao",
                    Quantidade = request.QuantidadeDanificada,
                    DataMovimentacao = DateTime.Now,
                    Descricao = "Devolução de itens danificados."
                };

                await _movimentacaoRepository.AdicionarAsync(
                    movimentacaoDanificada
                );
            }

            return CreatedAtAction(
                nameof(GetDevolucao),
                new { id = novaDevolucao.IdDevolucao },
                new
                {
                    novaDevolucao.IdDevolucao,
                    novaDevolucao.IdEmprestimo,
                    novaDevolucao.IdUsuarioConferente,
                    novaDevolucao.DataDevolucao,
                    novaDevolucao.QuantidadeBoa,
                    novaDevolucao.QuantidadeDanificada,
                    novaDevolucao.ComponentesConferidos,
                    novaDevolucao.Observacoes,
                    QuantidadeTotalEmprestada = emprestimo.Quantidade,
                    QuantidadeDevolvida = emprestimo.QuantidadeDevolvida,
                    QuantidadePendente = quantidadeRestante,
                    StatusEmprestimo = emprestimo.Status,
                    SituacaoPrazo = VerificarPrazo(
                        novaDevolucao.DataDevolucao,
                        emprestimo.DataPrevistaDevolucao
                    )
                }
            );
        }
    }
}
