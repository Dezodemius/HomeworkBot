using System;

namespace HomeWorkTelegramBot.Models
{
  /// <summary>
  /// Представляет ответ на задание в системе.
  /// </summary>
  internal class Answer
  {
    /// <summary>
    /// Статус ответа.
    /// </summary>
    public enum TaskStatus
    {
      /// <summary>
      /// Ответ не был дан.
      /// </summary>
      NotAnswered = 0,

      /// <summary>
      /// Ответ был дан.
      /// </summary>
      Answered = 1,

      /// <summary>
      /// Ответ правильный.
      /// </summary>
      CorrectAnswer = 2,

      /// <summary>
      /// Ответ неправильный.
      /// </summary>
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
    /// Дата и время, когда был дан ответ.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Статус работы.
    /// </summary>
    public TaskStatus Status { get; set; }
  }
}