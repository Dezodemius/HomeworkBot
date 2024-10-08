using Telegram.Bot;

namespace ModelInterfaceHub.Interfaces
{
  public interface IMessageHandler
  {
    /// <summary>
    /// ������������ �������� ��������� �� ������������.
    /// </summary>
    /// <param name="message">����� ��������� �� ������������.</param>
    /// <returns>����� �� ��������� ������������.</returns>
    Task ProcessMessageAsync(ITelegramBotClient botClient, long chatId, string message);

    /// <summary>
    /// ������������ callback ������ �� ������������.
    /// </summary>
    /// <param name="message">����� ��������� �� ������������.</param>
    /// <returns>����� �� ��������� ������������.</returns>
    Task ProcessCallbackAsync(ITelegramBotClient botClient, long chatId, string callbackData);
  }
}