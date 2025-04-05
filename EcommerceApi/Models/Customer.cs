using System.ComponentModel.DataAnnotations;

namespace EcommerceApi.Models;

public class Customer
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Address { get; set; } = string.Empty;

    public virtual ICollection<CustomerOrder> Orders { get; set; } = new List<CustomerOrder>();
} 