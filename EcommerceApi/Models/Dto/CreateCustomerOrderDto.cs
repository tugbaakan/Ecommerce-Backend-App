namespace EcommerceApi.Models.Dto;

public class CreateCustomerOrderDto
{
    public int CustomerId { get; set; }
    public List<CreateOrderItemDto> OrderItems { get; set; } = new();
} 