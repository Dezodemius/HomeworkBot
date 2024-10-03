using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Models
{
  /// <summary>
  /// Перечисление, представляющее роли пользователей в системе.
  /// </summary>
  public enum UserRole
  {
    /// <summary>
    /// Роль ученика.
    /// </summary>
    Student,

    /// <summary>
    /// Роль учителя.
    /// </summary>
    Teacher,

    /// <summary>
    /// Роль администратора.
    /// </summary>
    Administrator
  }

  /// <summary>
  /// Представляет пользователя в системе с общими атрибутами для всех ролей.
  /// </summary>
  public class User
  {
    /// <summary>
    /// Получает или устанавливает идентификатор чата Telegram пользователя.
    /// </summary>
    public long TelegramChatId { get; set; }

    /// <summary>
    /// Получает или устанавливает имя пользователя.
    /// </summary>
    public string FirstName { get; set; }

    /// <summary>
    /// Получает или устанавливает фамилию пользователя.
    /// </summary>
    public string LastName { get; set; }

    /// <summary>
    /// Получает или устанавливает адрес электронной почты пользователя.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Получает или устанавливает роль пользователя в системе.
    /// </summary>
    public UserRole Role { get; set; }

    /// <summary>
    /// Инициализирует новый экземпляр класса User с указанными параметрами.
    /// </summary>
    /// <param name="telegramChatId">Идентификатор чата Telegram.</param>
    /// <param name="firstName">Имя пользователя.</param>
    /// <param name="lastName">Фамилия пользователя.</param>
    /// <param name="email">Адрес электронной почты пользователя.</param>
    /// <param name="role">Роль пользователя в системе.</param>
    public User(long telegramChatId, string firstName, string lastName, string email, UserRole role)
    {
      TelegramChatId = telegramChatId;
      FirstName = firstName;
      LastName = lastName;
      Email = email;
      Role = role;
    }

    /// <summary>
    /// Возвращает строковое представление пользователя.
    /// </summary>
    /// <returns>Строка, содержащая информацию о пользователе.</returns>
    public override string ToString()
    {
      return $"{FirstName} {LastName} ({Role}) - {Email}";
    }

    /// <summary>
    /// Обрабатывает входящее сообщение от пользователя.
    /// </summary>
    /// <param name="message">Текст сообщения от пользователя.</param>
    /// <returns>Ответ на сообщение пользователя.</returns>
    public virtual async Task<string> ProcessMessageAsync(string message)
    {
      // Базовая реализация
      return "Это базовый ответ от класса User.";
    }


    /// <summary>
    /// Обрабатывает входящее сообщение от пользователя.
    /// </summary>
    /// <param name="message">Текст сообщения от пользователя.</param>
    /// <returns>Ответ на сообщение пользователя.</returns>
    public virtual async Task<string> ProcessCallbackAsync(string callbackData)
    {
      // Базовая реализация
      return "Это базовый ответ от класса User.";
    }

    /// <summary>
    /// Обрабатывает входящее сообщение от пользователя.
    /// </summary>
    /// <param name="message">Текст сообщения от пользователя.</param>
    /// <returns>Ответ на сообщение пользователя.</returns>
    public virtual InlineKeyboardMarkup Start()
    {
      // Базовая реализация
      return "Это базовый ответ от класса User.";
    }

  }
}