
using Microsoft.AspNetCore.Mvc;
using InventarioSENAIBackend.Models;
using InventarioSENAIBackend.Repository;
using InventarioSENAIBackend.Dtos;

namespace InventarioSENAIBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutosController : ControllerBase
    {
        private readonly ProdutoStorageRepository _repository;
        private readonly SalaStorageRepository _salaRepository;
        private readonly EmprestimoStorageRepository _emprestimoRepository;
        private readonly UsuarioStorageRepository _usuarioRepository;
        private readonly MovimentacaoStorageRepository _movimentacaoRepository;

        public ProdutosController(
            ProdutoStorageRepository repository,
            SalaStorageRepository salaRepository,
            EmprestimoStorageRepository emprestimoRepository,
            UsuarioStorageRepository usuarioRepository,
            MovimentacaoStorageRepository movimentacaoRepository)
        {
            _repository = repository;
            _salaRepository = salaRepository;
            _emprestimoRepository = emprestimoRepository;
            _usuarioRepository = usuarioRepository;
            _movimentacaoRepository = movimentacaoRepository;
        }

        // LISTAR TODOS OS PRODUTOS

        [HttpGet]
        public async Task<IActionResult> GetProdutos()
        {
            var produtos = await _repository.ObterTodosAsync();
            var salas = await _salaRepository.ObterTodasAsync();

            foreach (var produto in produtos)
            {
                produto.Sala = salas.FirstOrDefault(
                    s => s.IdSala == produto.IdSala
                );
            }

            return Ok(produtos);
        }

        // BUSCAR PRODUTO POR ID

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduto(int id)
        {
            var produto = await _repository.ObterPorIdAsync(id);

            if (produto == null)
                return NotFound("Produto não encontrado.");

            produto.Sala = await _salaRepository.ObterPorIdAsync(
                produto.IdSala
            );

            return Ok(produto);
        }

        // LISTAR PRODUTOS COM ESTOQUE BAIXO

        [HttpGet("estoque-baixo")]
        public async Task<IActionResult> GetEstoqueBaixo()
        {
            var produtos = await _repository.ObterTodosAsync();
            var salas = await _salaRepository.ObterTodasAsync();

            var resultado = produtos
                .Where(p => p.QuantidadeDisponivel < 5)
                .Select(p => new
                {
                    p.IdProduto,
                    p.Nome,
                    p.Categoria,
                    p.QuantidadeTotal,
                    p.QuantidadeDisponivel,
                    p.QuantidadeDanificada,
                    p.CodigoQR,
                    Sala = salas.FirstOrDefault(
                        s => s.IdSala == p.IdSala
                    )?.NomeSala
                });

            return Ok(resultado);
        }

        // CADASTRAR PRODUTO

        [HttpPost]
        public async Task<IActionResult> PostProduto(ProdutoRequest dados)
        {
            if (dados.IdUsuario <= 0)
                return BadRequest("O ID do usuário é obrigatório.");

            var usuario = await _usuarioRepository.ObterPorIdAsync(
                dados.IdUsuario
            );

            if (usuario == null)
                return BadRequest("Usuário não encontrado.");

            if (!string.Equals(
                usuario.Status,
                "Ativo",
                StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Usuário inativo não pode cadastrar produtos.");
            }

            // Alunos não podem cadastrar produtos.

            if (string.Equals(
                usuario.Perfil,
                "Aluno",
                StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(403,
                    "Usuários com perfil Aluno não podem cadastrar produtos.");
            }

            // Somente professores e administradores podem cadastrar.

            bool perfilPermitido =
                string.Equals(usuario.Perfil, "Professor",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(usuario.Perfil, "Administrador",
                    StringComparison.OrdinalIgnoreCase);

            if (!perfilPermitido)
                return StatusCode(403,
                    "Perfil sem permissão para cadastrar produtos.");

            if (string.IsNullOrWhiteSpace(dados.CodigoSala))
                return BadRequest("O código da sala é obrigatório.");

            var salas = await _salaRepository.ObterTodasAsync();

            var sala = salas.FirstOrDefault(s =>
                string.Equals(
                    s.CodigoSala,
                    dados.CodigoSala.Trim(),
                    StringComparison.OrdinalIgnoreCase
                )
            );

            if (sala == null)
                return BadRequest("A sala informada não existe.");

            if (dados.QuantidadeTotal < 0 ||
                dados.QuantidadeDisponivel < 0 ||
                dados.QuantidadeDisponivel > dados.QuantidadeTotal)
            {
                return BadRequest("As quantidades informadas são inválidas.");
            }

            if (string.IsNullOrWhiteSpace(dados.CodigoQR))
                return BadRequest("O código QR é obrigatório.");

            var produtos = await _repository.ObterTodosAsync();

            if (produtos.Any(p => p.CodigoQR == dados.CodigoQR))
                return BadRequest("Já existe um produto com esse código QR.");

            var produto = new Produto
            {
                Nome = dados.Nome,
                Descricao = dados.Descricao,
                Categoria = dados.Categoria,
                QuantidadeTotal = dados.QuantidadeTotal,
                QuantidadeDisponivel = dados.QuantidadeDisponivel,
                QuantidadeDanificada = 0,
                EstadoConservacao = dados.EstadoConservacao,
                CodigoQR = dados.CodigoQR,
                IdSala = sala.IdSala,
                DataCadastro = DateTime.Now,
                Sala = null
            };

            var novoProduto = await _repository.AdicionarAsync(produto);

            // Registrar entrada inicial automaticamente.

            var movimentacao = new Movimentacao
            {
                IdProduto = novoProduto.IdProduto,
                IdUsuario = usuario.IdUsuario,
                IdEmprestimo = null,
                IdSalaOrigem = null,
                IdSalaDestino = sala.IdSala,
                TipoMovimentacao = "Entrada",
                Quantidade = novoProduto.QuantidadeTotal,
                DataMovimentacao = DateTime.Now,
                Descricao = "Entrada inicial registrada automaticamente no cadastro do produto."
            };

            await _movimentacaoRepository.AdicionarAsync(movimentacao);

            novoProduto.Sala = sala;

            return CreatedAtAction(
                nameof(GetProduto),
                new { id = novoProduto.IdProduto },
                novoProduto
            );
        }

        // EDITAR PRODUTO

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduto(
            int id,
            ProdutoRequest dados)
        {
            var produto = await _repository.ObterPorIdAsync(id);

            if (produto == null)
                return NotFound("Produto não encontrado.");

            if (string.IsNullOrWhiteSpace(dados.CodigoSala))
                return BadRequest("O código da sala é obrigatório.");

            var salas = await _salaRepository.ObterTodasAsync();

            var sala = salas.FirstOrDefault(s =>
                string.Equals(
                    s.CodigoSala,
                    dados.CodigoSala.Trim(),
                    StringComparison.OrdinalIgnoreCase
                )
            );

            if (sala == null)
                return BadRequest("A sala informada não existe.");

            if (dados.QuantidadeTotal < 0 ||
                dados.QuantidadeDisponivel < 0)
            {
                return BadRequest("As quantidades informadas são inválidas.");
            }

            if (string.IsNullOrWhiteSpace(dados.CodigoQR))
                return BadRequest("O código QR é obrigatório.");

            var produtos = await _repository.ObterTodosAsync();

            if (produtos.Any(p =>
                p.CodigoQR == dados.CodigoQR &&
                p.IdProduto != id))
            {
                return BadRequest(
                    "Já existe outro produto com esse código QR."
                );
            }

            var emprestimos = await _emprestimoRepository.ObterTodosAsync();

            int quantidadeEmprestada = emprestimos
                .Where(e =>
                    e.IdProduto == id &&
                    e.Status == "Ativo")
                .Sum(e => e.Quantidade - e.QuantidadeDevolvida);

            int quantidadeOcupada =
                dados.QuantidadeDisponivel +
                produto.QuantidadeDanificada +
                quantidadeEmprestada;

            if (dados.QuantidadeDisponivel > dados.QuantidadeTotal ||
                dados.QuantidadeTotal < quantidadeOcupada)
            {
                return BadRequest(new
                {
                    mensagem = "O estoque total não pode ser menor que a soma das unidades disponíveis, emprestadas e danificadas.",
                    quantidadeDisponivel = dados.QuantidadeDisponivel,
                    quantidadeEmprestada,
                    quantidadeDanificada = produto.QuantidadeDanificada,
                    quantidadeMinima = quantidadeOcupada
                });
            }

            produto.Nome = dados.Nome;
            produto.Descricao = dados.Descricao;
            produto.Categoria = dados.Categoria;
            produto.QuantidadeTotal = dados.QuantidadeTotal;
            produto.QuantidadeDisponivel = dados.QuantidadeDisponivel;
            produto.EstadoConservacao = dados.EstadoConservacao;
            produto.CodigoQR = dados.CodigoQR;
            produto.IdSala = sala.IdSala;

            await _repository.AtualizarAsync(produto);

            return NoContent();
        }

        // REGISTRAR ENTRADA ADICIONAL

        [HttpPost("{id}/entrada")]
        public async Task<IActionResult> RegistrarEntrada(
            int id,
            EntradaProdutoRequest dados)
        {
            if (dados.IdUsuario <= 0)
                return BadRequest("O ID do usuário é obrigatório.");

            if (dados.Quantidade <= 0)
                return BadRequest("A quantidade deve ser maior que zero.");

            var usuario = await _usuarioRepository.ObterPorIdAsync(
                dados.IdUsuario
            );

            if (usuario == null)
                return BadRequest("Usuário não encontrado.");

            if (!string.Equals(
                usuario.Status,
                "Ativo",
                StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Usuário inativo não pode registrar entradas.");
            }

            bool perfilPermitido =
                string.Equals(usuario.Perfil, "Professor",
                    StringComparison.OrdinalIgnoreCase) ||
                string.Equals(usuario.Perfil, "Administrador",
                    StringComparison.OrdinalIgnoreCase);

            if (!perfilPermitido)
                return StatusCode(403,
                    "Somente professores e administradores podem registrar entradas.");

            var produto = await _repository.ObterPorIdAsync(id);

            if (produto == null)
                return NotFound("Produto não encontrado.");

            produto.QuantidadeTotal += dados.Quantidade;
            produto.QuantidadeDisponivel += dados.Quantidade;

            await _repository.AtualizarAsync(produto);

            var movimentacao = new Movimentacao
            {
                IdProduto = produto.IdProduto,
                IdUsuario = usuario.IdUsuario,
                IdEmprestimo = null,
                IdSalaOrigem = null,
                IdSalaDestino = produto.IdSala,
                TipoMovimentacao = "Entrada",
                Quantidade = dados.Quantidade,
                DataMovimentacao = DateTime.Now,
                Descricao = string.IsNullOrWhiteSpace(dados.Descricao)
                    ? "Entrada adicional de estoque."
                    : dados.Descricao
            };

            await _movimentacaoRepository.AdicionarAsync(movimentacao);

            return Ok(new
            {
                mensagem = "Entrada registrada com sucesso.",
                produto.IdProduto,
                produto.Nome,
                produto.QuantidadeTotal,
                produto.QuantidadeDisponivel,
                quantidadeAdicionada = dados.Quantidade
            });
        }
    }
}
