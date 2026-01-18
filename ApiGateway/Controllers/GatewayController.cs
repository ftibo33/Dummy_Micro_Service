using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace ApiGateway.Controllers;

[ApiController]
[Route("api")]
public class GatewayController : ControllerBase
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<GatewayController> _logger;
    private readonly IConfiguration _configuration;

    public GatewayController(
        HttpClient httpClient,
        ILogger<GatewayController> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    #region User Service Routes

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        return await ForwardRequest("UserService", "/api/users", HttpMethod.Get);
    }

    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        return await ForwardRequest("UserService", $"/api/users/{id}", HttpMethod.Get);
    }

    [HttpPost("users")]
    public async Task<IActionResult> CreateUser([FromBody] JsonElement body)
    {
        return await ForwardRequest("UserService", "/api/users", HttpMethod.Post, body);
    }

    [HttpPut("users/{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] JsonElement body)
    {
        return await ForwardRequest("UserService", $"/api/users/{id}", HttpMethod.Put, body);
    }

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        return await ForwardRequest("UserService", $"/api/users/{id}", HttpMethod.Delete);
    }

    #endregion

    #region Product Service Routes

    [HttpGet("products")]
    public async Task<IActionResult> GetAllProducts()
    {
        return await ForwardRequest("ProductService", "/api/products", HttpMethod.Get);
    }

    [HttpGet("products/{id}")]
    public async Task<IActionResult> GetProduct(int id)
    {
        return await ForwardRequest("ProductService", $"/api/products/{id}", HttpMethod.Get);
    }

    [HttpPost("products")]
    public async Task<IActionResult> CreateProduct([FromBody] JsonElement body)
    {
        return await ForwardRequest("ProductService", "/api/products", HttpMethod.Post, body);
    }

    [HttpPut("products/{id}")]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] JsonElement body)
    {
        return await ForwardRequest("ProductService", $"/api/products/{id}", HttpMethod.Put, body);
    }

    [HttpDelete("products/{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        return await ForwardRequest("ProductService", $"/api/products/{id}", HttpMethod.Delete);
    }

    #endregion

    #region Order Service Routes

    [HttpGet("orders")]
    public async Task<IActionResult> GetAllOrders()
    {
        return await ForwardRequest("OrderService", "/api/orders", HttpMethod.Get);
    }

    [HttpGet("orders/{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        return await ForwardRequest("OrderService", $"/api/orders/{id}", HttpMethod.Get);
    }

    [HttpPost("orders")]
    public async Task<IActionResult> CreateOrder([FromBody] JsonElement body)
    {
        _logger.LogInformation("🎯 API Gateway: Création d'une commande");
        return await ForwardRequest("OrderService", "/api/orders", HttpMethod.Post, body);
    }

    [HttpGet("orders/user/{userId}")]
    public async Task<IActionResult> GetOrdersByUser(int userId)
    {
        return await ForwardRequest("OrderService", $"/api/orders/user/{userId}", HttpMethod.Get);
    }

    #endregion

    #region Health Checks

    [HttpGet("health")]
    public async Task<IActionResult> HealthCheck()
    {
        var health = new Dictionary<string, object>
        {
            { "gateway", "healthy" },
            { "timestamp", DateTime.UtcNow }
        };

        try
        {
            // Vérifier UserService
            var userHealth = await CheckServiceHealth("UserService", "/api/users/health");
            health["userService"] = userHealth;

            // Vérifier ProductService
            var productHealth = await CheckServiceHealth("ProductService", "/api/products/health");
            health["productService"] = productHealth;

            // Vérifier OrderService
            var orderHealth = await CheckServiceHealth("OrderService", "/api/orders/health");
            health["orderService"] = orderHealth;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la vérification de santé");
        }

        return Ok(health);
    }

    private async Task<object> CheckServiceHealth(string serviceName, string path)
    {
        try
        {
            var serviceUrl = _configuration[$"Services:{serviceName}"] ?? $"http://localhost:500{serviceName[0]}";
            var response = await _httpClient.GetAsync($"{serviceUrl}{path}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<object>(content) ?? "healthy";
            }

            return new { status = "unhealthy", code = (int)response.StatusCode };
        }
        catch (Exception ex)
        {
            return new { status = "unhealthy", error = ex.Message };
        }
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Transférer une requête vers un microservice
    /// </summary>
    private async Task<IActionResult> ForwardRequest(
        string serviceName,
        string path,
        HttpMethod method,
        JsonElement? body = null)
    {
        try
        {
            var serviceUrl = _configuration[$"Services:{serviceName}"];
            if (string.IsNullOrEmpty(serviceUrl))
            {
                // Fallback pour développement local
                serviceUrl = serviceName switch
                {
                    "UserService" => "http://localhost:5001",
                    "ProductService" => "http://localhost:5002",
                    "OrderService" => "http://localhost:5003",
                    _ => throw new Exception($"Service {serviceName} inconnu")
                };
            }

            var targetUrl = $"{serviceUrl}{path}";
            _logger.LogInformation("🔄 Gateway: {Method} {TargetUrl}", method, targetUrl);

            var request = new HttpRequestMessage(method, targetUrl);

            if (body.HasValue && (method == HttpMethod.Post || method == HttpMethod.Put))
            {
                var json = JsonSerializer.Serialize(body.Value);
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            }

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("❌ Erreur: {StatusCode} - {Content}", response.StatusCode, content);
            }

            return StatusCode((int)response.StatusCode,
                string.IsNullOrEmpty(content) ? null : JsonSerializer.Deserialize<object>(content));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Erreur lors du transfert vers {ServiceName}", serviceName);
            return StatusCode(503, new
            {
                error = "Service temporairement indisponible",
                service = serviceName,
                message = ex.Message
            });
        }
    }

    #endregion
}
