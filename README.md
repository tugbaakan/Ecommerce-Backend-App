# Ecommerce Backend API

A RESTful API for managing an e-commerce system with products, customers, and orders.

## Features

- Product management (CRUD operations)
- Customer management (CRUD operations)
- Order management (CRUD operations)
- Redis caching for product data
- Entity Framework Core for data persistence
- PostgreSQL database
- RabbitMQ for order notifications- PostgreSQL database

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
- DotNetEnv package (for .env file support)
- RabbitMQ

## Setup

1. Clone the repository
2. Install DotNetEnv package:
   ```bash
   dotnet add package DotNetEnv
   ```
3. Create a `.env` file in the project root:
   ```bash
   cp .env.example .env
   ```
4. Edit the `.env` file with your configuration:
   ```
   DB_HOST=localhost
   DB_PORT=5432
   DB_NAME=ecommercedb
   DB_USERNAME=your_database_username
   DB_PASSWORD=your_database_password
   REDIS_HOST=localhost
   REDIS_PORT=6379
   REDIS_CACHE_TIMEOUT=30
   RABBITMQ_HOST=localhost
   RABBITMQ_PORT=5672
   RABBITMQ_USER=guest
   RABBITMQ_PASSWORD=guest
   ```
5. Run the database migrations:
   ```bash
   dotnet ef database update
   ```
6. Run the application:
   ```bash
   dotnet run
   ```

## Security Notes

- Never commit sensitive information like database credentials to version control
- The `.env` file is included in `.gitignore` to prevent accidental commits
- Use `.env.example` as a template for required environment variables
- In production, consider using a secrets management service
- Regularly rotate database credentials
- Use strong, unique passwords for database access
- Consider implementing IP whitelisting for database access

## Technologies Used

- ASP.NET Core 8.0
- Entity Framework Core
- PostgreSQL
- Redis
- AutoMapper
- Swagger/OpenAPI

## Notification System

The application uses RabbitMQ for sending order notifications:

1. **Email Notifications**:
   - Sent when an order is created
   - Includes order details and confirmation

2. **SMS Notifications**:
   - Sent when an order is created
   - Brief order confirmation message

3. **Notification Storage**:
   - Notifications are stored in the `notifications` directory
   - Each notification is saved as a text file
   - File naming format: `{type}_{orderId}_{timestamp}.txt`

### RabbitMQ Setup

1. Install RabbitMQ:
   - Download and install from: https://www.rabbitmq.com/download.html
   - Make sure Erlang is installed (required for RabbitMQ)

2. Enable Management Plugin:
   ```bash
   rabbitmq-plugins enable rabbitmq_management
   ```

3. Access Management UI:
   - URL: http://localhost:15672
   - Default credentials: guest/guest