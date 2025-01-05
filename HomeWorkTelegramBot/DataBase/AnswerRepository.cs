using HomeWorkTelegramBot.Config;
using HomeWorkTelegramBot.Models;
using System.Collections.Generic;
using System.Linq;

namespace HomeWorkTelegramBot.DataBase
{
  internal class AnswerRepository
  {
    /// <summary>
    /// Добавляет новый ответ в базу данных.
    /// </summary>
    /// <param name="answer">Объект ответа для добавления.</param>
    public void AddAnswer(Answer answer)
    {
      ApplicationData.DbContext.Answers.Add(answer);
      ApplicationData.DbContext.SaveChanges();
    }

    /// <summary>
    /// Удаляет ответ из базы данных.
    /// </summary>
    /// <param name="answerId">Идентификатор ответа.</param>
    public void DeleteAnswer(int answerId)
    {
      var answer = ApplicationData.DbContext.Answers
                      .FirstOrDefault(a => a.Id == answerId);
      if (answer != null)
      {
        ApplicationData.DbContext.Answers.Remove(answer);
        ApplicationData.DbContext.SaveChanges();
      }
    }

    /// <summary>
    /// Получает ответ по идентификатору.
    /// </summary>
    /// <param name="answerId">Идентификатор ответа.</param>
    /// <returns>Объект Answer или null, если ответ не найден.</returns>
    public Answer GetAnswerById(int answerId)
    {
      return ApplicationData.DbContext.Answers.FirstOrDefault(a => a.Id == answerId);
    }

    /// <summary>
    /// Получает все ответы пользователя по идентификатору.
    /// </summary>
    /// <param name="userId">Идентификатор ответа.</param>
    /// <returns>Объект Answer или null, если ответ не найден.</returns>
    public List<Answer> GetAnswersByUserId(long userId)
    {
      return ApplicationData.DbContext.Answers
        .Where(a => a.UserId == userId)
        .ToList();
    }

    /// <summary>
    /// Получает все ответы для указанного задания.
    /// </summary>
    /// <param name="taskId">Идентификатор задания.</param>
    /// <returns>Список ответов для задания.</returns>
    public List<Answer> GetAnswersByTaskId(int taskId)
    {
      return ApplicationData.DbContext.Answers.Where(a => a.TaskId == taskId).ToList();
    }

    /// <summary>
    /// Получает все ответы.
    /// </summary>
    /// <returns>Список всех ответов.</returns>
    public List<Answer> GetAllAnswers()
    {
      return ApplicationData.DbContext.Answers.ToList();
    }

    /// <summary>
    /// Обновляет ответ в таблице.
    /// </summary>
    /// <param name="answer">Экземпляр класса Answer.</param>
    public void UpdateAnswer(Answer answer)
    {
      ApplicationData.DbContext.Answers.Update(answer);
      ApplicationData.DbContext.SaveChanges();
    }
  }
}