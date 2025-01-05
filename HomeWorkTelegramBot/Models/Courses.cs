using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeWorkTelegramBot.Models
{
  /// <summary>
  /// Модель данных курса.
  /// </summary>
  public class Courses
  {
    /// <summary>
    /// Уникальный идентификатор курса.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Уникальный идентификатор преподавателя.
    /// </summary>
    public long TeacherId { get; set; }

    /// <summary>
    /// Название курса.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Описание курса.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с регистрациями.
    /// </summary>
    public ICollection<UserRegistration> Registrations { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с заданиями.
    /// </summary>
    public ICollection<TaskWork> TaskWorks { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с заданиями.
    /// </summary>
    public ICollection<Answer> Answers { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с заданиями.
    /// </summary>
    public CourseEnrollment CourseEnrollment { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с преподавателем
    /// </summary>
    public User Teacher { get; set; }
  }
}
