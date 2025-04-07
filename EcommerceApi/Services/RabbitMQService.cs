using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using EcommerceApi.Models.Dto;

namespace EcommerceApi.Services;

public interface IRabbitMQService
{
    void SendNotification(NotificationDto notification);
}

public class RabbitMQService : IRabbitMQService, IDisposable
{
    private IConnection? _connection;
    private IModel? _channel;
    private readonly ILogger<RabbitMQService> _logger;
    private const string ExchangeName = "notifications";
    private const string QueueName = "notification_queue";
    private readonly object _lock = new object();
    private bool _isDisposed;

    public RabbitMQService(ILogger<RabbitMQService> logger)
    {
        _logger = logger;
        InitializeConnection();
    }

    private void InitializeConnection()
    {
        try
        {
            var factory = new ConnectionFactory
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
                Port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672"),
                UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest",
                Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest",
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            
            // Declare exchange with durable setting
            _channel.ExchangeDeclare(
                exchange: ExchangeName,
                type: ExchangeType.Direct,
                durable: true,
                autoDelete: false,
                arguments: null
            );
            
            // Declare queue with durable setting
            _channel.QueueDeclare(
                queue: QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            );
            
            // Bind queue to exchange
            _channel.QueueBind(
                queue: QueueName,
                exchange: ExchangeName,
                routingKey: ""
            );
            
            _logger.LogInformation("RabbitMQ service initialized successfully with exchange '{ExchangeName}'", ExchangeName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ connection");
            throw; // Re-throw the exception to prevent the service from starting with a failed connection
        }
    }

    public void SendNotification(NotificationDto notification)
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(RabbitMQService));
        }

        try
        {
            EnsureConnection();

            if (_channel == null || !_channel.IsOpen)
            {
                _logger.LogError("RabbitMQ channel is not available");
                return;
            }

            try
            {
                // Try to declare the exchange (idempotent operation)
                _channel.ExchangeDeclare(
                    exchange: ExchangeName,
                    type: ExchangeType.Direct,
                    durable: true,
                    autoDelete: false,
                    arguments: null
                );

                // Ensure queue exists
                _channel.QueueDeclare(
                    queue: QueueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                // Ensure queue is bound to exchange
                _channel.QueueBind(
                    queue: QueueName,
                    exchange: ExchangeName,
                    routingKey: ""
                );

                var message = JsonSerializer.Serialize(notification);
                var body = Encoding.UTF8.GetBytes(message);
                
                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;
                
                _channel.BasicPublish(
                    exchange: ExchangeName,
                    routingKey: "",
                    basicProperties: properties,
                    body: body
                );
                
                _logger.LogInformation("Notification sent for order {OrderId}", notification.OrderId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RabbitMQ operations for order {OrderId}", notification.OrderId);
                // Try to reinitialize the connection
                InitializeConnection();
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification for order {OrderId}", notification.OrderId);
            // Don't throw the exception, just log it
            // This way the order creation won't fail even if RabbitMQ is down
        }
    }

    private void EnsureConnection()
    {
        if (_connection?.IsOpen == true && _channel?.IsOpen == true)
        {
            return;
        }

        lock (_lock)
        {
            if (_connection?.IsOpen == true && _channel?.IsOpen == true)
            {
                return;
            }

            try
            {
                Dispose();
                InitializeConnection();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to re-establish RabbitMQ connection");
                // Don't throw, just log the error
            }
        }
    }

    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        
        try
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disposing RabbitMQ resources");
        }
    }
} 