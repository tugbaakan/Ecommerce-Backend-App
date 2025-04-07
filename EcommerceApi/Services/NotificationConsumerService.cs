using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using EcommerceApi.Models.Dto;

namespace EcommerceApi.Services;

public class NotificationConsumerService : BackgroundService
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<NotificationConsumerService> _logger;
    private const string ExchangeName = "notifications";
    private const string QueueName = "notification_queue";

    public NotificationConsumerService(ILogger<NotificationConsumerService> logger)
    {
        _logger = logger;
        
        var factory = new ConnectionFactory
        {
            HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
            Port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672"),
            UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER") ?? "guest",
            Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest"
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        
        // Ensure the queue exists
        _channel.QueueDeclare(QueueName, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(QueueName, ExchangeName, routingKey: "");
        
        _logger.LogInformation("Notification consumer service initialized");
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        
        consumer.Received += (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var notification = JsonSerializer.Deserialize<NotificationDto>(message);

                if (notification != null)
                {
                    WriteNotificationToFile(notification);
                    _channel.BasicAck(ea.DeliveryTag, false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notification");
                _channel.BasicNack(ea.DeliveryTag, false, true);
            }
        };

        _channel.BasicConsume(queue: QueueName,
                            autoAck: false,
                            consumer: consumer);

        return Task.CompletedTask;
    }

    private void WriteNotificationToFile(NotificationDto notification)
    {
        var notificationsDir = Path.Combine(Directory.GetCurrentDirectory(), "notifications");
        Directory.CreateDirectory(notificationsDir);

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var filename = $"{notification.Type}_{notification.OrderId}_{timestamp}.txt";
        var filePath = Path.Combine(notificationsDir, filename);

        var content = $"""
            Type: {notification.Type}
            Recipient: {notification.Recipient}
            Subject: {notification.Subject}
            Order ID: {notification.OrderId}
            Timestamp: {notification.Timestamp}
            
            Content:
            {notification.Content}
            """;

        File.WriteAllText(filePath, content);
        _logger.LogInformation("Notification written to file: {FilePath}", filePath);
    }

    public override void Dispose()
    {
        _channel?.Dispose();
        _connection?.Dispose();
        base.Dispose();
    }
} 