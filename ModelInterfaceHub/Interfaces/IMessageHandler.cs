namespace ModelInterfaceHub.Interfaces
{
  public interface IMessageHandler
  {
    /// <summary>
    /// ������������ �������� ��������� �� ������������.
    /// </summary>
    /// <param name="message">����� ��������� �� ������������.</param>
    /// <returns>����� �� ��������� ������������.</returns>
    Task<string> ProcessMessageAsync(string message);

    /// <summary>
    /// ������������ callback ������ �� ������������.
    /// </summary>
    /// <param name="message">����� ��������� �� ������������.</param>
    /// <returns>����� �� ��������� ������������.</returns>
    Task<string> ProcessCallbackAsync(string callbackData);
  }
}