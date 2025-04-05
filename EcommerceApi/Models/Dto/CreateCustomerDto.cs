using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.Models.Dto;

public class CreateCustomerDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Address { get; set; } = string.Empty;
} 