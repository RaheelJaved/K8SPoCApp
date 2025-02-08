namespace PassengerService.Messaging
{
    public interface IRabbitMQService
    {
        Task SendMessageAsync(string message, object queueName);
    }
}