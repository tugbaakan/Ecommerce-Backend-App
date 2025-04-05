using EcommerceApi.Models;

namespace EcommerceApi.Data;

public static class SeedData
{
    public static void Initialize(ApplicationDbContext context)
    {
        if (!context.Customers.Any())
        {
            var customers = new List<Customer>
            {
                new Customer { Name = "John Doe", Address = "123 Main St, New York, NY" },
                new Customer { Name = "Jane Smith", Address = "456 Oak Ave, Los Angeles, CA" },
                new Customer { Name = "Bob Johnson", Address = "789 Pine Rd, Chicago, IL" }
            };
            context.Customers.AddRange(customers);
            context.SaveChanges();
        }

        if (!context.Products.Any())
        {
            // Generate timestamp-based barcodes with 1-second intervals
            var baseTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var products = new List<Product>
            {
                new Product { Barcode = baseTimestamp.ToString("X"), Description = "Laptop", Quantity = 10, Price = 999.99m },
                new Product { Barcode = (baseTimestamp + 1000).ToString("X"), Description = "Smartphone", Quantity = 20, Price = 699.99m },
                new Product { Barcode = (baseTimestamp + 2000).ToString("X"), Description = "Headphones", Quantity = 30, Price = 99.99m },
                new Product { Barcode = (baseTimestamp + 3000).ToString("X"), Description = "Tablet", Quantity = 15, Price = 499.99m },
                new Product { Barcode = (baseTimestamp + 4000).ToString("X"), Description = "Smartwatch", Quantity = 25, Price = 199.99m }
            };
            context.Products.AddRange(products);
            context.SaveChanges();
        }

        if (!context.CustomerOrders.Any())
        {
            var customer1 = context.Customers.First();
            var customer2 = context.Customers.Skip(1).First();
            var product1 = context.Products.First();
            var product2 = context.Products.Skip(1).First();

            var orders = new List<CustomerOrder>
            {
                new CustomerOrder
                {
                    CustomerId = customer1.Id,
                    OrderAddress = customer1.Address,
                    OrderItems = new List<OrderItem>
                    {
                        new OrderItem { ProductId = product1.Id, ProductQuantity = 2 },
                        new OrderItem { ProductId = product2.Id, ProductQuantity = 1 }
                    }
                },
                new CustomerOrder
                {
                    CustomerId = customer2.Id,
                    OrderAddress = customer2.Address,
                    OrderItems = new List<OrderItem>
                    {
                        new OrderItem { ProductId = product2.Id, ProductQuantity = 3 }
                    }
                }
            };
            context.CustomerOrders.AddRange(orders);
            context.SaveChanges();
        }
    }
} 