using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContracts.Models
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

  public enum ChangeRoleStep
  { 
    Start,
    Role,
    Completed
  }

  /// <summary>
  /// Представляет пользователя в системе с общими атрибутами для всех ролей.
  /// </summary>
  public class UserModel
  {

    /// <summary>
    /// Получает или устанавливает идентификатор чата Telegram пользователя.
    /// </summary>
    public int UserId { get; set; }

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

    private ChangeRoleStep Step { get; set; } = ChangeRoleStep.Start;

    /// <summary>
    /// Инициализирует новый экземпляр класса User с указанными параметрами.
    /// </summary>
    /// <param name="telegramChatId">Идентификатор чата Telegram.</param>
    /// <param name="firstName">Имя пользователя.</param>
    /// <param name="lastName">Фамилия пользователя.</param>
    /// <param name="email">Адрес электронной почты пользователя.</param>
    /// <param name="role">Роль пользователя в системе.</param>
    public UserModel(long telegramChatId, string firstName, string lastName, string email, UserRole role)
    {
      TelegramChatId = telegramChatId;
      FirstName = firstName;
      LastName = lastName;
      Email = email;
      Role = role;
    }

    /// <summary>
    /// Инициализирует новый экземпляр класса User с указанными параметрами.
    /// </summary>
    /// <param name="telegramChatId">Идентификатор чата Telegram.</param>
    /// <param name="firstName">Имя пользователя.</param>
    /// <param name="lastName">Фамилия пользователя.</param>
    /// <param name="email">Адрес электронной почты пользователя.</param>
    /// <param name="role">Роль пользователя в системе.</param>
    public UserModel(int userId, long telegramChatId, string firstName, string lastName, string email, UserRole role)
    {
      UserId = userId;
      TelegramChatId = telegramChatId;
      FirstName = firstName;
      LastName = lastName;
      Email = email;
      Role = role;
    }

    public void SetChangeStep(ChangeRoleStep changeRoleStep)
    {
      Step = changeRoleStep;
    }
    public ChangeRoleStep GetChangeStep()
    {
      return Step;
    }


    /// <summary>
    /// Возвращает строковое представление пользователя.
    /// </summary>
    /// <returns>Строка, содержащая информацию о пользователе.</returns>
    public override string ToString()
    {
      return $"{FirstName} {LastName} ({Role}) - {Email}";
    }

  }
}
