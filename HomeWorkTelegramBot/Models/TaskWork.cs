namespace HomeWorkTelegramBot.Models
{
  public class TaskWork
  {
    /// <summary>
    /// Уникальный идентификатор задания.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Уникальный идентификатор курса.
    /// </summary>
    public int CourseId { get; set; }

    /// <summary>
    /// Название задания.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Описание задания.
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с курсом.
    /// </summary>
    public Courses Course { get; set; }

    /// <summary>
    /// Навигационное свойство для связи с ответами.
    /// </summary>
    public ICollection<Answer> Answers { get; set; }
  }
}
