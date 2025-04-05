# Ecommerce Backend API

A RESTful API for managing an e-commerce system with products, customers, and orders.

## Features

- Product management (CRUD operations)
- Customer management (CRUD operations)
- Order management (CRUD operations)
- Redis caching for product data
- Entity Framework Core for data persistence
- PostgreSQL database

## API Endpoints

### Products

- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get a specific product
- `POST /api/products` - Create a new product
- `PUT /api/products/{id}` - Update a product
- `DELETE /api/products/{id}` - Delete a product

### Customers

- `GET /api/customers` - Get all customers
- `GET /api/customers/{id}` - Get a specific customer
- `POST /api/customers` - Create a new customer
- `PUT /api/customers/{id}` - Update a customer
- `DELETE /api/customers/{id}` - Delete a customer

### Customer Orders

- `GET /api/customerOrders` - Get all customer orders
- `GET /api/customerOrders/{id}` - Get a specific customer order
- `POST /api/customerOrders` - Create a new customer order
- `PUT /api/customerOrders/{id}` - Update a customer order
- `DELETE /api/customerOrders/{id}` - Delete a customer order

## Order Update Functionality

The `PUT /api/customerOrders/{id}` endpoint allows you to modify an existing order in the following ways:

1. Update the delivery address
2. Modify product quantities
3. Add new products to the order
4. Remove products from the order

### Example Request

```json
{
    "orderAddress": "New Address, City, State",
    "orderItems": [
        {
            "productId": 1,
            "productQuantity": 3
        },
        {
            "productId": 2,
            "productQuantity": 1
        }
    ]
}
```

- `orderAddress` (optional): New delivery address for the order
- `orderItems` (optional): List of products to update in the order
  - `productId`: ID of the product
  - `productQuantity`: New quantity for the product

### Notes

- Both `orderAddress` and `orderItems` are optional fields
- If `orderItems` is provided, it will replace all existing items in the order
- Products not included in `orderItems` will be removed from the order
- New products can be added by including them in `orderItems`
- Product quantities can be modified by updating the `productQuantity` value

## Prerequisites

- .NET 8.0 SDK
- PostgreSQL
- Redis Server

## Setup

1. Clone the repository
2. Update the connection strings in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Host=localhost;Database=ecommercedb;Username=your_username;Password=your_password;Port=5432",
       "Redis": "localhost:6379"
     }
   }
   ```
3. Run the database migrations:
   ```bash
   dotnet ef database update
   ```
4. Run the application:
   ```bash
   dotnet run
   ```

## Technologies Used

- ASP.NET Core 8.0
- Entity Framework Core
- PostgreSQL
- Redis
- AutoMapper
- Swagger/OpenAPI