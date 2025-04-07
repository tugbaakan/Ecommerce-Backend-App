using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EcommerceApi.Data;
using EcommerceApi.Models;
using EcommerceApi.Models.Dto;
using Microsoft.Extensions.Logging;
using EcommerceApi.Services;

namespace EcommerceApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerOrdersController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IRabbitMQService _rabbitMQService;

    public CustomerOrdersController(ApplicationDbContext context
        , IRabbitMQService rabbitMQService)
    {
        _context = context;
        _rabbitMQService = rabbitMQService;
    }

    // GET: api/customerorders
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CustomerOrderDto>>> GetCustomerOrders()
    {
        var orders = await _context.CustomerOrders
            .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
            .Select(o => new CustomerOrderDto
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                OrderAddress = o.OrderAddress,
                OrderItems = o.OrderItems.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductQuantity = i.ProductQuantity,
                    ProductDescription = i.Product.Description,
                    ProductPrice = i.Product.Price
                }).ToList()
            })
            .ToListAsync();

        return orders;
    }

    // GET: api/customerorders/5
    [HttpGet("{id}")]
    public async Task<ActionResult<CustomerOrderDto>> GetCustomerOrder(int id)
    {
        var order = await _context.CustomerOrders
            .Include(o => o.OrderItems)
                .ThenInclude(i => i.Product)
            .Where(o => o.Id == id)
            .Select(o => new CustomerOrderDto
            {
                Id = o.Id,
                CustomerId = o.CustomerId,
                OrderAddress = o.OrderAddress,
                OrderItems = o.OrderItems.Select(i => new OrderItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductQuantity = i.ProductQuantity,
                    ProductDescription = i.Product.Description,
                    ProductPrice = i.Product.Price
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (order == null)
        {
            return NotFound();
        }

        return order;
    }

    // POST: api/customerOrders
    [HttpPost]
    public async Task<ActionResult<CustomerOrderDto>> CreateCustomerOrder(CreateCustomerOrderDto createOrderDto)
    {
        // Validate customer exists
        var customer = await _context.Customers.FindAsync(createOrderDto.CustomerId);
        if (customer == null)
        {
            return BadRequest("Customer not found");
        }

        var customerOrder = new CustomerOrder
        {
            CustomerId = createOrderDto.CustomerId,
            OrderAddress = customer.Address,
            OrderItems = new List<OrderItem>()
        };

        // Process each order item
        foreach (var itemDto in createOrderDto.OrderItems)
        {
            // Validate product exists and has sufficient quantity
            var product = await _context.Products.FindAsync(itemDto.ProductId);
            if (product == null)
            {
                return BadRequest($"Product with ID {itemDto.ProductId} not found");
            }

            if (product.Quantity < itemDto.Quantity)
            {
                return BadRequest($"Insufficient quantity for product {product.Description}");
            }

            // Create order item
            var orderItem = new OrderItem
            {
                ProductId = itemDto.ProductId,
                ProductQuantity = itemDto.Quantity
            };

            // Update product quantity
            product.Quantity -= itemDto.Quantity;

            customerOrder.OrderItems.Add(orderItem);
        }

        _context.CustomerOrders.Add(customerOrder);
        await _context.SaveChangesAsync();

        // Send email notification
        var emailNotification = new NotificationDto																				
        {
            Type = "email",
            Recipient = "customer@example.com", // Replace with actual customer email
            Subject = $"Order Confirmation - Order #{customerOrder.Id}",
            Content = $"Thank you for your order! Your order #{customerOrder.Id} has been received and is being processed.",
            OrderId = customerOrder.Id,
            Timestamp = DateTime.UtcNow
        };
        _rabbitMQService.SendNotification(emailNotification);

        // Send SMS notification
        var smsNotification = new NotificationDto
        {
            Type = "sms",
            Recipient = "+1234567890", // Replace with actual customer phone
            Subject = "Order Confirmation",
            Content = $"Your order #{customerOrder.Id} has been received. Thank you for shopping with us!",
            OrderId = customerOrder.Id,
            Timestamp = DateTime.UtcNow																			  
        };
         _rabbitMQService.SendNotification(smsNotification);

        var createdOrder = new CustomerOrderDto
        {
            Id = customerOrder.Id,
            CustomerId = customerOrder.CustomerId,
            OrderAddress = customerOrder.OrderAddress,
            OrderItems = customerOrder.OrderItems.Select(i => new OrderItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductQuantity = i.ProductQuantity,
                ProductDescription = i.Product.Description,
                ProductPrice = i.Product.Price
            }).ToList()
        };

        return CreatedAtAction(nameof(GetCustomerOrder), new { id = customerOrder.Id }, createdOrder);
    }

    // DELETE: api/customerorders/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteCustomerOrder(int id)
    {
        var customerOrder = await _context.CustomerOrders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (customerOrder == null)
        {
            return NotFound();
        }

        // Restore product quantities
        foreach (var item in customerOrder.OrderItems)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product != null)
            {
                product.Quantity += item.ProductQuantity;
            }
        }

        _context.CustomerOrders.Remove(customerOrder);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateCustomerOrder(int id, UpdateCustomerOrderDto updateOrderDto)
    {
        var order = await _context.CustomerOrders
            .Include(o => o.OrderItems)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        // Update address if provided
        if (!string.IsNullOrEmpty(updateOrderDto.OrderAddress))
        {
            order.OrderAddress = updateOrderDto.OrderAddress;
        }

        // Update order items if provided
        if (updateOrderDto.OrderItems != null)
        {
            // Remove existing order items
            _context.OrderItems.RemoveRange(order.OrderItems);
            order.OrderItems.Clear();

            // Add new order items
            foreach (var itemDto in updateOrderDto.OrderItems)
            {
                var product = await _context.Products.FindAsync(itemDto.ProductId);
                if (product == null)
                {
                    return BadRequest($"Product with ID {itemDto.ProductId} not found");
                }

                order.OrderItems.Add(new OrderItem
                {
                    ProductId = itemDto.ProductId,
                    ProductQuantity = itemDto.ProductQuantity
                });
            }
        }

        try
        {
            await _context.SaveChangesAsync();
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.CustomerOrders.AnyAsync(o => o.Id == id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
    }
} 