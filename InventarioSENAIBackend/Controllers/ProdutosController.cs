
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventarioSENAIBackend.Models;
using InventarioSENAIBackend.Dtos;
using InventarioSENAIBackend.Repository;

namespace InventarioSENAIBackend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProdutosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProdutosController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetProdutos()
        {
            var produtos = await _context.Produtos
                .Include(p => p.Sala)
                .ToListAsync();

            return Ok(produtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduto(int id)
        {
            var produto = await _context.Produtos
                .Include(p => p.Sala)
                .FirstOrDefaultAsync(p => p.IdProduto == id);

            if (produto == null)
                return NotFound(new { mensagem = "Produto não encontrado." });

            return Ok(produto);
        }

        [HttpPost]
        public async Task<IActionResult> CriarProduto(ProdutoRequest request)
        {
            var salaExiste = await _context.Salas
                .AnyAsync(s => s.IdSala == request.IdSala);

            if (!salaExiste)
                return BadRequest(new { mensagem = "A sala informada não existe." });

            if (request.QuantidadeTotal < 0 ||
                request.QuantidadeDisponivel < 0 ||
                request.QuantidadeDisponivel > request.QuantidadeTotal)
            {
                return BadRequest(new { mensagem = "As quantidades informadas são inválidas." });
            }

            var codigoExiste = await _context.Produtos
                .AnyAsync(p => p.CodigoQR == request.CodigoQR);

            if (codigoExiste)
                return BadRequest(new { mensagem = "Esse código QR já está cadastrado." });

            var produto = new Produto
            {
                Nome = request.Nome,
                Descricao = request.Descricao,
                Categoria = request.Categoria,
                QuantidadeTotal = request.QuantidadeTotal,
                QuantidadeDisponivel = request.QuantidadeDisponivel,
                EstadoConservacao = request.EstadoConservacao,
                CodigoQR = request.CodigoQR,
                IdSala = request.IdSala,
                DataCadastro = DateTime.Now
            };

            _context.Produtos.Add(produto);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetProduto),
                new { id = produto.IdProduto },
                produto
            );
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> AtualizarProduto(
            int id, ProdutoRequest request)
        {
            var produto = await _context.Produtos.FindAsync(id);

            if (produto == null)
                return NotFound(new { mensagem = "Produto não encontrado." });

            var salaExiste = await _context.Salas
                .AnyAsync(s => s.IdSala == request.IdSala);

            if (!salaExiste)
                return BadRequest(new { mensagem = "A sala informada não existe." });

            if (request.QuantidadeTotal < 0 ||
                request.QuantidadeDisponivel < 0 ||
                request.QuantidadeDisponivel > request.QuantidadeTotal)
            {
                return BadRequest(new { mensagem = "As quantidades informadas são inválidas." });
            }

            var codigoExiste = await _context.Produtos
                .AnyAsync(p => p.CodigoQR == request.CodigoQR &&
                               p.IdProduto != id);

            if (codigoExiste)
                return BadRequest(new { mensagem = "Esse código QR já pertence a outro produto." });

            produto.Nome = request.Nome;
            produto.Descricao = request.Descricao;
            produto.Categoria = request.Categoria;
            produto.QuantidadeTotal = request.QuantidadeTotal;
            produto.QuantidadeDisponivel = request.QuantidadeDisponivel;
            produto.EstadoConservacao = request.EstadoConservacao;
            produto.CodigoQR = request.CodigoQR;
            produto.IdSala = request.IdSala;

            await _context.SaveChangesAsync();

            return Ok(produto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> ExcluirProduto(int id)
        {
            var produto = await _context.Produtos.FindAsync(id);

            if (produto == null)
                return NotFound(new { mensagem = "Produto não encontrado." });

            _context.Produtos.Remove(produto);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
