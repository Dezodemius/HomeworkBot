using System;
using System.ComponentModel;

namespace HomeWorkTelegramBot.Models
{
  /// <summary>
  /// Представляет ответ на задание в системе.
  /// </summary>
  public class Answer
  {
    /// <summary>
    /// Статус ответа.
    /// </summary>
    public enum TaskStatus
    {
      /// <summary>
      /// Ответ не был дан.
      /// </summary>
      [Description("Ответ не был дан")]
      NotAnswered = 0,

      /// <summary>
      /// Ответ был дан.
      /// </summary>
      [Description("Отправлено на проверку")]
      Answered = 1,

      /// <summary>
      /// Ответ правильный.
      /// </summary>
      [Description("Ответ на задание принят")]
      CorrectAnswer = 2,

      /// <summary>
      /// Ответ неправильный.
      /// </summary>
      [Description("Ответ на задание необходимо доработать")]
      IncorrectAnswer = 3,
    }

    /// <summary>
    /// Уникальный идентификатор ответа.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Текст ответа.
    /// </summary>
    public string AnswerText { get; set; }

    /// <summary>
    /// Идентификатор курса, к которому относится ответ.
    /// </summary>
    public int CourseId { get; set; }

    /// <summary>
    /// Идентификатор задания, к которому относится ответ.
    /// </summary>
    public int TaskId { get; set; }

    /// <summary>
    /// Идентификатор чата пользователя, который дал ответ на задание.
    /// </summary>
    public long UserId { get; set; }

    /// <summary>
    /// Дата и время, когда был дан ответ.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Статус работы.
    /// </summary>
    public TaskStatus Status { get; set; }
  }
}