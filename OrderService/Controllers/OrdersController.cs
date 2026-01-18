using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Data;
using OrderService.Models;
using OrderService.Services;

namespace OrderService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderContext _context;
    private readonly OrderOrchestrationService _orchestrationService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        OrderContext context,
        OrderOrchestrationService orchestrationService,
        ILogger<OrdersController> logger)
    {
        _context = context;
        _orchestrationService = orchestrationService;
        _logger = logger;
    }

    /// <summary>
    /// Obtenir toutes les commandes
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
    {
        _logger.LogInformation("Récupération de toutes les commandes");
        return await _context.Orders.ToListAsync();
    }

    /// <summary>
    /// Obtenir une commande par son ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Order>> GetOrder(int id)
    {
        _logger.LogInformation("Récupération de la commande avec ID: {OrderId}", id);

        var order = await _context.Orders.FindAsync(id);

        if (order == null)
        {
            return NotFound(new { message = $"Commande avec ID {id} non trouvée" });
        }

        return order;
    }

    /// <summary>
    /// Créer une nouvelle commande
    /// DÉMONSTRATION: Communication inter-services
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Order>> CreateOrder(CreateOrderDto createOrderDto)
    {
        _logger.LogInformation("🎯 Création d'une nouvelle commande: UserId={UserId}, ProductId={ProductId}, Quantity={Quantity}",
            createOrderDto.UserId, createOrderDto.ProductId, createOrderDto.Quantity);

        try
        {
            // Étape 1: Valider l'utilisateur via UserService
            _logger.LogInformation("1️⃣ Validation de l'utilisateur...");
            var user = await _orchestrationService.ValidateUser(createOrderDto.UserId);
            if (user == null)
            {
                return BadRequest(new { message = $"Utilisateur avec ID {createOrderDto.UserId} non trouvé" });
            }

            // Étape 2: Valider le produit via ProductService
            _logger.LogInformation("2️⃣ Validation du produit...");
            var product = await _orchestrationService.ValidateProduct(createOrderDto.ProductId);
            if (product == null)
            {
                return BadRequest(new { message = $"Produit avec ID {createOrderDto.ProductId} non trouvé" });
            }

            // Étape 3: Vérifier le stock
            _logger.LogInformation("3️⃣ Vérification du stock...");
            var stockCheck = await _orchestrationService.CheckStock(createOrderDto.ProductId, createOrderDto.Quantity);
            if (stockCheck == null || !stockCheck.IsAvailable)
            {
                return BadRequest(new
                {
                    message = "Stock insuffisant",
                    requested = createOrderDto.Quantity,
                    available = stockCheck?.AvailableStock ?? 0
                });
            }

            // Étape 4: Réduire le stock
            _logger.LogInformation("4️⃣ Réduction du stock...");
            var stockReduced = await _orchestrationService.ReduceStock(createOrderDto.ProductId, createOrderDto.Quantity);
            if (!stockReduced)
            {
                return StatusCode(500, new { message = "Erreur lors de la réduction du stock" });
            }

            // Étape 5: Créer la commande
            _logger.LogInformation("5️⃣ Création de la commande...");
            var order = new Order
            {
                UserId = createOrderDto.UserId,
                ProductId = createOrderDto.ProductId,
                Quantity = createOrderDto.Quantity,
                TotalPrice = product.Price * createOrderDto.Quantity,
                Status = "Confirmée",
                CreatedAt = DateTime.UtcNow,
                UserName = user.Name,
                ProductName = product.Name
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            _logger.LogInformation("✅ Commande créée avec succès: OrderId={OrderId}, Total={Total}",
                order.Id, order.TotalPrice);

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erreur lors de la création de la commande");
            return StatusCode(500, new { message = "Erreur lors de la création de la commande", error = ex.Message });
        }
    }

    /// <summary>
    /// Obtenir les commandes d'un utilisateur spécifique
    /// </summary>
    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByUser(int userId)
    {
        _logger.LogInformation("Récupération des commandes pour l'utilisateur {UserId}", userId);
        return await _context.Orders.Where(o => o.UserId == userId).ToListAsync();
    }

    /// <summary>
    /// Mettre à jour le statut d'une commande
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string status)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
        {
            return NotFound(new { message = $"Commande avec ID {id} non trouvée" });
        }

        order.Status = status;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Statut mis à jour", order });
    }

    /// <summary>
    /// Health check endpoint
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", service = "OrderService", timestamp = DateTime.UtcNow });
    }
}
