using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.Models;

public class OrderItem
{
    public int Id { get; set; }

    [Required]
    public int CustomerOrderId { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required]
    public int ProductQuantity { get; set; }

    public virtual CustomerOrder CustomerOrder { get; set; } = null!;
    public virtual Product Product { get; set; } = null!;
} 