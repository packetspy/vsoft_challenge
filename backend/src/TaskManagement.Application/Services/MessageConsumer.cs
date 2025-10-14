using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using TaskManagement.Application.Interfaces;
using TaskManagement.Core.Entities;

namespace TaskManagement.Application.Services;

public class MessageConsumer : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptions<RabbitMQSettings> _settings;
    private readonly ILogger<MessageConsumer> _logger;
    private IConnection _connection;
    private IModel _channel;
    private string _queueName;

    public MessageConsumer(IServiceProvider serviceProvider, IOptions<RabbitMQSettings> settings, ILogger<MessageConsumer> logger)
    {
        _serviceProvider = serviceProvider;
        _settings = settings;
        _logger = logger;
        InitializeRabbitMQ();
    }

    private void InitializeRabbitMQ()
    {
        try
        {
            var factory = new ConnectionFactory()
            {
                HostName = _settings.Value.HostName,
                UserName = _settings.Value.UserName,
                Password = _settings.Value.Password,
                Port = _settings.Value.Port,
                VirtualHost = _settings.Value.VirtualHost
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare("task.notifications", ExchangeType.Topic, durable: true);
            _channel.ExchangeDeclare("user.notifications", ExchangeType.Fanout, durable: true);

            _queueName = "task.notifications.queue";
            _channel.QueueDeclare(
                queue: _queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            _channel.QueueBind(
                queue: _queueName,
                exchange: "task.notifications",
                routingKey: "user.#"); 

            _channel.QueueBind(
                queue: _queueName,
                exchange: "user.notifications",
                routingKey: ""); 

            _logger.LogInformation("RabbitMQ Consumer started with queue: {QueueName}", _queueName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize RabbitMQ Consumer");
            throw;
        }
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            using var scope = _serviceProvider.CreateScope();
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

            try
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var messageData = JsonSerializer.Deserialize<NotificationMessage>(message);

                if (messageData != null)
                {
                    await notificationService.HandleNotificationAsync(messageData);
                    _channel.BasicAck(ea.DeliveryTag, false);

                    _logger.LogInformation("Notification processed: {MessageType}", messageData.MessageType);
                }
                else
                {
                    _channel.BasicAck(ea.DeliveryTag, false); 
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notification");
                _channel.BasicNack(ea.DeliveryTag, false, true); 
            }
        };

        _channel.BasicConsume(
            queue: _queueName,
            autoAck: false, 
            consumer: consumer);

        _logger.LogInformation("Started consuming messages from queue: {QueueName}", _queueName);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel?.Close();
        _channel?.Dispose();
        _connection?.Close();
        _connection?.Dispose();
        base.Dispose();
    }
}