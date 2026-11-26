using Microsoft.AspNetCore.Mvc;
using InventarioSENAIBackend.Models;
using InventarioSENAIBackend.Repository;

namespace InventarioSENAIBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutosController : ControllerBase
    {
        private readonly ProdutoStorageRepository _repository;
        private readonly SalaStorageRepository _salaRepository;

        public ProdutosController(
            ProdutoStorageRepository repository,
            SalaStorageRepository salaRepository)
        {
            _repository = repository;
            _salaRepository = salaRepository;
        }

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

        [HttpPost]
        public async Task<IActionResult> PostProduto(Produto produto)
        {
            produto.IdProduto = 0;
            produto.Sala = null;

            var sala = await _salaRepository.ObterPorIdAsync(
                produto.IdSala
            );

            if (sala == null)
                return BadRequest("A sala informada não existe.");

            if (produto.QuantidadeTotal < 0 ||
                produto.QuantidadeDisponivel < 0 ||
                produto.QuantidadeDisponivel > produto.QuantidadeTotal)
            {
                return BadRequest("As quantidades informadas são inválidas.");
            }

            if (string.IsNullOrWhiteSpace(produto.CodigoQR))
                return BadRequest("O código QR é obrigatório.");

            var produtos = await _repository.ObterTodosAsync();

            if (produtos.Any(p => p.CodigoQR == produto.CodigoQR))
                return BadRequest("Já existe um produto com esse código QR.");

            if (produto.DataCadastro == default)
                produto.DataCadastro = DateTime.Now;

            var novoProduto = await _repository.AdicionarAsync(produto);

            return CreatedAtAction(
                nameof(GetProduto),
                new { id = novoProduto.IdProduto },
                novoProduto
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduto(int id, Produto dados)
        {
            var produto = await _repository.ObterPorIdAsync(id);

            if (produto == null)
                return NotFound("Produto não encontrado.");

            var sala = await _salaRepository.ObterPorIdAsync(
                dados.IdSala
            );

            if (sala == null)
                return BadRequest("A sala informada não existe.");

            if (dados.QuantidadeTotal < 0 ||
                dados.QuantidadeDisponivel < 0 ||
                dados.QuantidadeDisponivel > dados.QuantidadeTotal)
            {
                return BadRequest("As quantidades informadas são inválidas.");
            }

            var produtos = await _repository.ObterTodosAsync();

            if (produtos.Any(p =>
                p.CodigoQR == dados.CodigoQR &&
                p.IdProduto != id))
            {
                return BadRequest("Já existe outro produto com esse código QR.");
            }

            produto.Nome = dados.Nome;
            produto.Descricao = dados.Descricao;
            produto.Categoria = dados.Categoria;
            produto.QuantidadeTotal = dados.QuantidadeTotal;
            produto.QuantidadeDisponivel = dados.QuantidadeDisponivel;
            produto.EstadoConservacao = dados.EstadoConservacao;
            produto.CodigoQR = dados.CodigoQR;
            produto.IdSala = dados.IdSala;
            produto.DataCadastro = dados.DataCadastro;

            await _repository.AtualizarAsync(produto);

            return NoContent();
        }
    }
}
