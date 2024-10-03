namespace TelegramBot.Interfaces
{
    public interface IMessageHandler
    {
        Task<string> ProcessMessageAsync(string message);
    }
}