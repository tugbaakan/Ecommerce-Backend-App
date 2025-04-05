using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.Models.Dto;

public class UpdateCustomerOrderDto
{
    [MaxLength(200)]
    public string? OrderAddress { get; set; }

    public List<UpdateOrderItemDto>? OrderItems { get; set; }
}

public class UpdateOrderItemDto
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int ProductQuantity { get; set; }
} 