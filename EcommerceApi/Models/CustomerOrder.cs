using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.Models;

public class CustomerOrder
{
    public int Id { get; set; }

    [Required]
    public int CustomerId { get; set; }

    [Required]
    [StringLength(200)]
    public string OrderAddress { get; set; } = string.Empty;

    public virtual Customer Customer { get; set; } = null!;
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
} 