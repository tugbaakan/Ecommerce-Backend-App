namespace EcommerceApi.Models.Dto;

public class OrderItemDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int ProductQuantity { get; set; }
    public string ProductDescription { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; }
} 