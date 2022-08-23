using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SimpleRabbitTest.Infra.DTOs;
using SimpleRabbitTest.Notification;
using SimpleRabbitTest.Options;
using System.Text;

namespace SimpleRabbitTest.Consumers
{
  public class ProcessMessageConsumer : BackgroundService
  {
    private readonly RabbitMQConfig _config;
    private readonly IConnection _conn;
    private readonly IModel _channel;
    private readonly IServiceProvider _serviceProvider;

    public ProcessMessageConsumer(IOptions<RabbitMQConfig> option, IServiceProvider serviceProvider) {
        _config = option.Value;
        _serviceProvider = serviceProvider;

        var factory = new ConnectionFactory {
            HostName = _config.Host,
            UserName = _config.User,
            Password = _config.Password
        };

        _conn = factory.CreateConnection();
        _channel = _conn.CreateModel();

        _channel.QueueDeclare(
            queue: _config.Queue,
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null           
        );

    }
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (sender, eventArgs) => {
            var contentArray = eventArgs.Body.ToArray();
            var contentString = Encoding.UTF8.GetString(contentArray);
            var msg = JsonConvert.DeserializeObject<MessageInputModel>(contentString);
            if (msg != null) {
                NotifyUser(msg);
            }
            _channel.BasicAck(eventArgs.DeliveryTag, false);
        };

        _channel.BasicConsume(_config.Queue, false, consumer);

        return Task.CompletedTask;
    }

    public void NotifyUser(MessageInputModel msg) {
           
        using (var scope = _serviceProvider.CreateScope()) {
            var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
            notificationService.NotifyUser(msg.FromId, msg.ToId, msg.Content ?? "No content");
        }
    }
  }
}