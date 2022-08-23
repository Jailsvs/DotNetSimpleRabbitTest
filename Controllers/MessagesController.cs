using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RabbitMQ.Client;
using SimpleRabbitTest.Infra.DTOs;
using SimpleRabbitTest.Options;
using System.Text;

namespace SimpleRabbitTest.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly ILogger<MessagesController> _logger;
    private readonly ConnectionFactory _factory;
    private readonly RabbitMQConfig _config;

    public MessagesController(ILogger<MessagesController> logger, IOptions<RabbitMQConfig> option)
    {
        _logger = logger;
        _config = option.Value;

        _factory = new ConnectionFactory {
            HostName = _config.Host,
            UserName = _config.User,
            Password = _config.Password
        };
    }

    [HttpPost]
    public IActionResult PostMessage([FromBody] MessageInputModel msg){
        using (var connection = _factory.CreateConnection()){
            using (var channel = connection.CreateModel()){
                channel.QueueDeclare(
                    queue: _config.Queue,
                    durable: false,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                var stringfiedMsg = JsonConvert.SerializeObject(msg);
                var bytesMessage = Encoding.UTF8.GetBytes(stringfiedMsg);

                channel.BasicPublish(
                    exchange: "",
                    routingKey: _config.Queue,
                    basicProperties: null,
                    body: bytesMessage
                );

            }
        }
        return Accepted();
    }


}
