namespace EcommerceApi.Models.Dto;

public class CustomerOrderDto
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public string OrderAddress { get; set; } = string.Empty;
    public List<OrderItemDto> OrderItems { get; set; } = new();
} 