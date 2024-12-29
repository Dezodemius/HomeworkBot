using HomeWorkTelegramBot.Config;
using HomeWorkTelegramBot.Models;
using Microsoft.EntityFrameworkCore;

namespace HomeWorkTelegramBot.DataBase
{
  internal class TeacherRepository
  {
    /// <summary>
    /// Добавляет новое домашнее задание для курса.
    /// </summary>
    /// <param name="task">Объект домашнего задания для добавления.</param>
    public void CreateTask(TaskWork task)
    {
      ApplicationData.DbContext.TaskWorks.Add(task);
      ApplicationData.DbContext.SaveChanges();
    }

    /// <summary>
    /// Обновляет статус выполнения домашнего задания.
    /// </summary>
    /// <param name="status">Новый статус ответа на домашнее задание.</param>
    /// <param name="answerId">Уникальный идентификатор ответа на домашнее задание.</param>
    public void UpdateAnswerStatus(Answer.TaskStatus status, int answerId)
    {
      var answer = ApplicationData.DbContext.Answers
        .FirstOrDefault(a => a.Id == answerId);
      if (answer == null)
      {
        answer.Status = status;
        ApplicationData.DbContext.Answers.Update(answer);
        ApplicationData.DbContext.SaveChanges();
      }
    }

    /// <summary>
    /// Асинхронно получает ответы на домашние задания конкретного пользователя.
    /// </summary>
    /// <param name="chatId">Уникальный идентификатор чата пользователя.</param>
    /// <returns>Список ответов на домашние задания.</returns>
    public async Task<List<Answer>> GetUserAnswersAsync(long chatId)
    {
      return await ApplicationData.DbContext.Answers
        .Where(a => a.UserId == chatId)
        .ToListAsync();
    }

    /// <summary>
    /// Асинхронно получает ответы всех студентов на домашнее задание с заданным идентификатором.
    /// </summary>
    /// <param name="taskId">Уникальный идентификатор домашнего задания.</param>
    /// <returns>Список ответов на заданное домашнее задание.</returns>
    public async Task<List<Answer>> GetTaskWorksAsync(int taskId)
    {
      return await ApplicationData.DbContext.Answers
        .Where(a => a.TaskId == taskId)
        .ToListAsync();
    }
  }
}
