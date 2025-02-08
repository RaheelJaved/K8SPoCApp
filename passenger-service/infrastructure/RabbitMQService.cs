using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using RabbitMQ.Client;

namespace PassengerService.Messaging
{
    public class RabbitMQService : IRabbitMQService, IAsyncDisposable
    {
        private IConnection _connection;
        private IChannel _channel;

        public RabbitMQService(IConfiguration configuration)
        {
            var factory = new ConnectionFactory()
            {
                Uri = new Uri(configuration["RabbitMQ:URI"]),
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            Task.Run(async () =>
            {
                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                // Declare an exchange for passenger events.
                await _channel.ExchangeDeclareAsync(exchange: "passenger.events", type: ExchangeType.Topic, durable: true);
            }).Wait();
        }

        public async Task SendMessageAsync(string eventType, object eventData)
        {
            var message = new
            {
                EventType = eventType,
                Data = eventData,
                Timestamp = DateTime.UtcNow
            };

            var body = JsonSerializer.SerializeToUtf8Bytes(message);

            BasicProperties basicProperties = new BasicProperties() { };

            await _channel.BasicPublishAsync(exchange: "passenger.events",
                                  routingKey: eventType,
                                  basicProperties: basicProperties,
                                  mandatory: false,
                                  body: body);
        }

        public async ValueTask DisposeAsync()
        {
            if (_connection != null)
            {
                await _connection.CloseAsync(); // Close asynchronously
                _connection.Dispose();
            }
        }
    }
}
