namespace HomeWorkTelegramBot.Models
{
  /// <summary>
  /// Модель, представляющая запись пользователя на курс.
  /// </summary>
  public class CourseEnrollment
  {
    /// <summary>
    /// Уникальный идентификатор записи.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Идентификатор курса.
    /// </summary>
    public int CourseId { get; set; }

    /// <summary>
    /// Идентификатор пользователя.
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Навигационное свойство для курса.
    /// </summary>
    public Courses Course { get; set; }

    /// <summary>
    /// Навигационное свойство для пользователя.
    /// </summary>
    public User User { get; set; }
  }
}
