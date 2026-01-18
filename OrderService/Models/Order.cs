namespace OrderService.Models;

public class Order
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal TotalPrice { get; set; }
    public string Status { get; set; } = "En attente";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Informations dénormalisées pour éviter les jointures
    public string? UserName { get; set; }
    public string? ProductName { get; set; }
}

public class CreateOrderDto
{
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}
