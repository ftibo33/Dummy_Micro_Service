using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Models;

namespace ProductService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly ProductContext _context;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(ProductContext context, ILogger<ProductsController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtenir tous les produits
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        _logger.LogInformation("Récupération de tous les produits");
        return await _context.Products.ToListAsync();
    }

    /// <summary>
    /// Obtenir un produit par son ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        _logger.LogInformation("Récupération du produit avec ID: {ProductId}", id);

        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            _logger.LogWarning("Produit avec ID {ProductId} non trouvé", id);
            return NotFound(new { message = $"Produit avec ID {id} non trouvé" });
        }

        return product;
    }

    /// <summary>
    /// Créer un nouveau produit
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        _logger.LogInformation("Création d'un nouveau produit: {ProductName}", product.Name);

        product.CreatedAt = DateTime.UtcNow;
        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    /// <summary>
    /// Mettre à jour un produit existant
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, Product product)
    {
        if (id != product.Id)
        {
            return BadRequest(new { message = "L'ID ne correspond pas" });
        }

        _logger.LogInformation("Mise à jour du produit avec ID: {ProductId}", id);

        _context.Entry(product).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await ProductExists(id))
            {
                return NotFound(new { message = $"Produit avec ID {id} non trouvé" });
            }
            throw;
        }

        return NoContent();
    }

    /// <summary>
    /// Supprimer un produit
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        _logger.LogInformation("Suppression du produit avec ID: {ProductId}", id);

        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound(new { message = $"Produit avec ID {id} non trouvé" });
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Vérifier la disponibilité du stock pour un produit
    /// </summary>
    [HttpGet("{id}/check-stock")]
    public async Task<ActionResult<object>> CheckStock(int id, [FromQuery] int quantity)
    {
        _logger.LogInformation("Vérification du stock pour le produit {ProductId}, quantité: {Quantity}", id, quantity);

        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound(new { message = $"Produit avec ID {id} non trouvé" });
        }

        var isAvailable = product.Stock >= quantity;

        return Ok(new
        {
            productId = id,
            productName = product.Name,
            requestedQuantity = quantity,
            availableStock = product.Stock,
            isAvailable = isAvailable,
            message = isAvailable ? "Stock suffisant" : "Stock insuffisant"
        });
    }

    /// <summary>
    /// Réduire le stock d'un produit (utilisé par OrderService)
    /// </summary>
    [HttpPost("{id}/reduce-stock")]
    public async Task<IActionResult> ReduceStock(int id, [FromBody] int quantity)
    {
        _logger.LogInformation("Réduction du stock pour le produit {ProductId}, quantité: {Quantity}", id, quantity);

        var product = await _context.Products.FindAsync(id);

        if (product == null)
        {
            return NotFound(new { message = $"Produit avec ID {id} non trouvé" });
        }

        if (product.Stock < quantity)
        {
            return BadRequest(new { message = "Stock insuffisant" });
        }

        product.Stock -= quantity;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Stock réduit avec succès", newStock = product.Stock });
    }

    private async Task<bool> ProductExists(int id)
    {
        return await _context.Products.AnyAsync(e => e.Id == id);
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "ProductService", timestamp = DateTime.UtcNow });
    }
}
