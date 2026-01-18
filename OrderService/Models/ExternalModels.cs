namespace OrderService.Models;

// DTOs pour recevoir les données des autres services

public class UserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
}

public class StockCheckResponse
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
    public int AvailableStock { get; set; }
    public bool IsAvailable { get; set; }
    public string Message { get; set; } = string.Empty;
}
