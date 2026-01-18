using System.Text;
using System.Text.Json;
using OrderService.Models;

namespace OrderService.Services;

/// <summary>
/// Service d'orchestration des commandes qui communique avec UserService et ProductService
/// CONCEPT CLÉ: Communication inter-services via HTTP
/// </summary>
public class OrderOrchestrationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OrderOrchestrationService> _logger;
    private readonly IConfiguration _configuration;

    public OrderOrchestrationService(
        HttpClient httpClient,
        ILogger<OrderOrchestrationService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    /// <summary>
    /// Valider qu'un utilisateur existe en appelant le UserService
    /// </summary>
    public async Task<UserDto?> ValidateUser(int userId)
    {
        try
        {
            var userServiceUrl = _configuration["Services:UserService"] ?? "http://localhost:5001";
            var url = $"{userServiceUrl}/api/users/{userId}";

            _logger.LogInformation("🔄 Appel au UserService: {Url}", url);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("❌ Utilisateur {UserId} non trouvé", userId);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var user = JsonSerializer.Deserialize<UserDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _logger.LogInformation("✅ Utilisateur validé: {UserName}", user?.Name);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erreur lors de la validation de l'utilisateur {UserId}", userId);
            throw new Exception("Impossible de contacter le service utilisateur", ex);
        }
    }

    /// <summary>
    /// Valider qu'un produit existe et vérifier le stock
    /// </summary>
    public async Task<ProductDto?> ValidateProduct(int productId)
    {
        try
        {
            var productServiceUrl = _configuration["Services:ProductService"] ?? "http://localhost:5002";
            var url = $"{productServiceUrl}/api/products/{productId}";

            _logger.LogInformation("🔄 Appel au ProductService: {Url}", url);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("❌ Produit {ProductId} non trouvé", productId);
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var product = JsonSerializer.Deserialize<ProductDto>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _logger.LogInformation("✅ Produit validé: {ProductName}, Prix: {Price}", product?.Name, product?.Price);
            return product;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erreur lors de la validation du produit {ProductId}", productId);
            throw new Exception("Impossible de contacter le service produit", ex);
        }
    }

    /// <summary>
    /// Vérifier la disponibilité du stock
    /// </summary>
    public async Task<StockCheckResponse?> CheckStock(int productId, int quantity)
    {
        try
        {
            var productServiceUrl = _configuration["Services:ProductService"] ?? "http://localhost:5002";
            var url = $"{productServiceUrl}/api/products/{productId}/check-stock?quantity={quantity}";

            _logger.LogInformation("🔄 Vérification du stock: {Url}", url);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            var stockCheck = JsonSerializer.Deserialize<StockCheckResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            _logger.LogInformation("✅ Stock disponible: {IsAvailable}", stockCheck?.IsAvailable);
            return stockCheck;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erreur lors de la vérification du stock");
            throw;
        }
    }

    /// <summary>
    /// Réduire le stock après création de commande
    /// </summary>
    public async Task<bool> ReduceStock(int productId, int quantity)
    {
        try
        {
            var productServiceUrl = _configuration["Services:ProductService"] ?? "http://localhost:5002";
            var url = $"{productServiceUrl}/api/products/{productId}/reduce-stock";

            _logger.LogInformation("🔄 Réduction du stock: {Url}", url);

            var jsonContent = new StringContent(
                JsonSerializer.Serialize(quantity),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(url, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("✅ Stock réduit avec succès");
                return true;
            }

            _logger.LogWarning("❌ Échec de la réduction du stock");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erreur lors de la réduction du stock");
            return false;
        }
    }
}
