using KolmeoAPI.DTOs;
using KolmeoAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KolmeoAPI.Controllers
{
    /// <summary>
    /// API endpoint for Products
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ProductContext _context;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(ProductContext context, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/Products
        /// <summary>
        /// Gets list of products
        /// </summary>
        /// <param name="minPrice">Optional parameter to set minimum price of all products returned</param>
        /// <param name="maxPrice">Optional parameter to set maximum price of all products returned</param>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts(decimal minPrice = 0, decimal maxPrice = decimal.MaxValue)
        {
            _logger.LogDebug("Getting list of products");

            if (_context.Products == null)
            {
                return NotFound();
            }
            var a = await _context.Products.ToListAsync();

            return await _context.Products.Where(product => product.Price >= minPrice && product.Price <= maxPrice).Select(product => ProductToDTO(product)).ToListAsync();
        }

        // GET: api/Products/5
        /// <summary>
        /// Get product with specified id
        /// </summary>
        /// <param name="id">Id of product to retrieve</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDTO>> GetProduct(long id)
        {
            _logger.LogDebug($"Getting a product with id {id}");

            if (_context.Products == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return ProductToDTO(product);
        }

        // PUT: api/Products/5
        /// <summary>
        /// Updates product with a given id
        /// </summary>
        /// <param name="id">Id of product to update</param>
        /// <param name="productDTO">Data to update product with</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(long id, ProductDTO productDTO)
        {
            _logger.LogDebug($"Updating a product with name \"{productDTO.Name}\"");

            if (id != productDTO.Id)
            {
                return BadRequest();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            product.Name = productDTO.Name;
            product.Description = productDTO.Description;
            product.Price = productDTO.Price;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException) when (!ProductExists(id))
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/Products
        /// <summary>
        /// Creates a new product with given data
        /// </summary>
        /// <param name="productDTO">Data to create product with</param>
        [HttpPost]
        public async Task<ActionResult<ProductDTO>> PostProduct(ProductDTO productDTO)
        {
            _logger.LogDebug($"Creating a new product with name \"{productDTO.Name}\"");

            if (_context.Products == null)
            {
                return Problem("Entity set 'ProductContext.Products'  is null.");
            }

            var product = new Product
            {
                Name = productDTO.Name,
                Description = productDTO.Description,
                Price = productDTO.Price
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetProduct),
                new { id = product.Id },
                ProductToDTO(product));
        }

        // DELETE: api/Products/5
        /// <summary>
        /// Delete product with given id
        /// </summary>
        /// <param name="id">Id of product to delete</param>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(long id)
        {
            _logger.LogDebug($"Deleting product with id {id}");

            if (_context.Products == null)
            {
                return NotFound();
            }
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductExists(long id)
        {
            return (_context.Products?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        private static ProductDTO ProductToDTO(Product product) =>
        new ProductDTO
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price
        };
    }
}
