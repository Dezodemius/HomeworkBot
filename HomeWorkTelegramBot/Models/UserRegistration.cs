using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeWorkTelegramBot.Models
{
  public class UserRegistration
  {
    /// <summary>
    /// Идентификатор чата пользователя.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Идентификатор чата пользователя.
    /// </summary>
    public long ChatId { get; set; }

    /// <summary>
    /// Имя пользователя.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Фамилия пользователя.
    /// </summary>
    public string Surname { get; set; }

    /// <summary>
    /// Отчество пользователя.
    /// </summary>
    public string Lastname { get; set; }

    /// <summary>
    /// Почта пользователя.
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Дата рождения.
    /// </summary>
    public DateOnly BirthDate { get; set; }

    public int CourseId { get; set; }
  }
}
