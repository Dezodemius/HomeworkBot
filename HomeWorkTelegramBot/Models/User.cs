using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeWorkTelegramBot.Models
{
  internal class User
  {
    /// <summary>
    /// Перечисление, представляющее роли пользователей в системе.
    /// </summary>
    public enum Role
    {
      /// <summary>
      /// Роль студента.
      /// </summary>
      Student = 1,

      /// <summary>
      /// Роль учителя.
      /// </summary>
      Teacher = 2,

      /// <summary>
      /// Роль администратора.
      /// </summary>
      Admin = 3,

      /// <summary>
      /// Роль незарегистрированного пользователя.
      /// </summary>
      UnregisteredUser = 4,
    }

    /// <summary>
    /// Идентификатор записи о пользователе.
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

    /// <summary>
    /// Роль пользователя.
    /// </summary>
    public Role UserRole { get; set; }
  }
}
