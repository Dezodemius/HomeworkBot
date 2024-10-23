using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContracts.Models
{
  /// <summary>
  /// Перечисление шагов регистрации.
  /// </summary>
  public enum RegistrationStep
  {
    Start,
    Course,
    FirstName,
    LastName,
    Email,
    Completed
  }
  public class RegistrationRequest
  {
    /// <summary>
    /// Идентификатор запроса.
    /// </summary>
    public int RequestId { get; set; }

    /// <summary>
    /// Идентификатор чата Telegram.
    /// </summary>
    public long TelegramChatId { get; set; }

    /// <summary>
    /// Имя пользователя.
    /// </summary>
    public string FirstName { get; set; }

    /// <summary>
    /// Фамилия пользователя.
    /// </summary>
    public string LastName { get; set; }

    /// <summary>
    /// Электронная почта пользователя.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Идентификатор курса.
    /// </summary>
    public int CourseId { get; set; }

    private RegistrationStep Step { get; set; }

    /// <summary>
    /// Статус запроса.
    /// </summary>
    public string Status { get; set; }

    public RegistrationRequest() 
    {
      Status = "Pending";
    }
    public RegistrationRequest(long idchat) : this()
    { 
      TelegramChatId = idchat;
    }

    public RegistrationStep GetStep()
    { 
      return Step;
    }

    public void SetStep(RegistrationStep registrationStep)
    {
      Step = registrationStep;
    }
  }
}
