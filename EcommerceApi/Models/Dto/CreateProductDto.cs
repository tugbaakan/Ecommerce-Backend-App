using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.Models.Dto;

public class CreateProductDto
{
    [Required]
    [StringLength(500)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public int Quantity { get; set; }

    [Required]
    public decimal Price { get; set; }
} 