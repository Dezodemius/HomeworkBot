using HomeWorkTelegramBot.DataBase;
using HomeWorkTelegramBot.Models;
using System.Collections.Generic;
using static HomeWorkTelegramBot.Config.Logger;

namespace HomeWorkTelegramBot.Core
{
  internal static class AnswerService
  {
    private static readonly AnswerRepository answerRepository = new AnswerRepository();

    /// <summary>
    /// Добавляет новый ответ и логирует это действие.
    /// </summary>
    /// <param name="answer">Объект ответа для добавления.</param>
    public static void AddAnswer(Answer answer)
    {
      answerRepository.AddAnswer(answer);
      LogInformation($"Добавлен новый ответ: {answer.AnswerText}");
    }

    /// <summary>
    /// Удаляет ответ и логирует это действие.
    /// </summary>
    /// <param name="answerId">Идентификатор ответа.</param>
    public static void DeleteAnswer(int answerId)
    {
      answerRepository.DeleteAnswer(answerId);
      LogInformation($"Удален ответ с Id {answerId}");
    }

    /// <summary>
    /// Получает ответ по идентификатору и логирует это действие.
    /// </summary>
    /// <param name="answerId">Идентификатор ответа.</param>
    /// <returns>Объект Answer или null, если ответ не найден.</returns>
    public static Answer GetAnswerById(int answerId)
    {
      var answer = answerRepository.GetAnswerById(answerId);
      if (answer != null)
      {
        LogInformation($"Найден ответ с Id {answerId}");
      }
      else
      {
        LogWarning($"Ответ с Id {answerId} не найден.");
      }
      return answer;
    }

    /// <summary>
    /// Получает все ответы для указанного задания и логирует это действие.
    /// </summary>
    /// <param name="taskId">Идентификатор задания.</param>
    /// <returns>Список ответов для задания.</returns>
    public static List<Answer> GetAnswersByTaskId(int taskId)
    {
      var answers = answerRepository.GetAnswersByTaskId(taskId);
      LogInformation($"Получено {answers.Count} ответов для задания с Id {taskId}");
      return answers;
    }

    /// <summary>
    /// Получает все ответы для указанного пользователя и логирует это действие.
    /// </summary>
    /// <param name="userId">Идентификатор чата пользователя.</param>
    /// <returns>Список ответов для задания.</returns>
    public static List<Answer> GetAnswersByUserId(long userId)
    {
      var answers = answerRepository.GetAnswersByUserId(userId);
      LogInformation($"Получено {answers.Count} ответов для пользователя с chatId {userId}");
      return answers;
    }

    /// <summary>
    /// Получает все ответы и логирует это действие.
    /// </summary>
    /// <returns>Список всех ответов.</returns>
    public static List<Answer> GetAllAnswers()
    {
      var answers = answerRepository.GetAllAnswers();
      LogInformation($"Получено {answers.Count} ответов.");
      return answers;
    }
  }
}