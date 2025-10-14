using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using TaskManagement.Application.Interfaces;
using TaskManagement.Core.Entities;
using RabbitChannel = RabbitMQ.Client.IModel;
using RabbitConnection = RabbitMQ.Client.IConnection;

namespace TaskManagement.Application.Services;

public class MessageProducer : IMessageProducer, IDisposable
{
    private readonly RabbitConnection _connection;
    private readonly RabbitChannel _channel;
    private readonly ILogger<MessageProducer> _logger;
    private bool _disposed = false;

    private const string TaskExchange = "task.notifications";
    private const string UserExchange = "user.notifications";

    private const string TaskQueue = "user.notification.queue";
    private const string TaskUpdateQueue = "task.update.queue";

    public MessageProducer(IOptions<RabbitMQSettings> settings, ILogger<MessageProducer> logger)
    {
        _logger = logger;

        try
        {
            var factory = new ConnectionFactory()
            {
                HostName = settings.Value.HostName,
                UserName = settings.Value.UserName,
                Password = settings.Value.Password,
                Port = settings.Value.Port,
                VirtualHost = settings.Value.VirtualHost
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.BasicReturn += (sender, args) =>
            {
                _logger.LogWarning("MESSAGE NOT ROUTED: {ReplyText}", args.ReplyText);
                _logger.LogWarning("Exchange: {Exchange}, Routing Key: {RoutingKey}", args.Exchange, args.RoutingKey);
            };

            _channel.ExchangeDeclare(TaskExchange, ExchangeType.Topic, durable: true);
            _channel.ExchangeDeclare(UserExchange, ExchangeType.Fanout, durable: true);

            _channel.QueueDeclare(TaskQueue, durable: true, exclusive: false, autoDelete: false);
            _channel.QueueDeclare(TaskUpdateQueue, durable: true, exclusive: false, autoDelete: false);

            _channel.QueueBind(TaskQueue, TaskExchange, "user.#");
            _channel.QueueBind(TaskUpdateQueue, UserExchange, "");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to establish RabbitMQ connection");
            throw;
        }
    }

    public async Task PublishTaskAssignedMessageAsync(Guid taskItemId, Guid assignedToUserId)
    {
        try
        {
            var message = new
            {
                MessageId = Guid.NewGuid(),
                TaskItemId = taskItemId,
                AssignedToUserId = assignedToUserId,
                AssignedAt = DateTime.UtcNow,
                MessageType = "TaskAssigned",
                Description = "You have been assigned a new task"
            };

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));
            var routingKey = $"user.{assignedToUserId}";

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.MessageId = Guid.NewGuid().ToString();

            _channel.BasicPublish(
                exchange: TaskExchange,
                routingKey: routingKey,
                mandatory: true,
                basicProperties: properties,
                body: body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish task assignment message");
            throw;
        }

        await Task.CompletedTask;
    }

    public async Task PublishTaskUpdatedMessageAsync(Guid taskItemId, Guid updatedByUserId, string updateType)
    {
        try
        {
            var message = new
            {
                MessageId = Guid.NewGuid(),
                TaskItemId = taskItemId,
                UpdatedByUserId = updatedByUserId,
                UpdatedAt = DateTime.UtcNow,
                MessageType = updateType,
                Description = $"Task has been {updateType.ToLower()}"
            };

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.MessageId = Guid.NewGuid().ToString();

            _channel.BasicPublish(
                exchange: UserExchange,
                routingKey: string.Empty, // Fanout ignora routing key
                mandatory: true,
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Task update message published for task {TaskItemId}", taskItemId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish task update message");
            throw;
        }

        await Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed) return;

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
        finally
        {
            _disposed = true;
        }
    }
}
