using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database;
using DataContracts;
using DataContracts.Data;
using DataContracts.Models;

namespace Core
{
  public static class CommonSubmission
  {
    static DatabaseManager dbManager = new DatabaseManager(ApplicationData.ConfigApp.DatabaseConnectionString);

    /// <summary>
    /// Добавляет новую запись выполнения задания.
    /// </summary>
    /// <param name="submission">Объект <see cref="Submission"/> для добавления.</param>
    public static void CreateSubmission(Submission submission)
    {
      dbManager.CreateSubmission(submission);
    }

    /// <summary>
    /// Получает список всех выполненных заданий для пользователя по его Telegram Chat ID.
    /// </summary>
    /// <param name="telegramChatId">Telegram Chat ID пользователя.</param>
    /// <returns>Список объектов <see cref="Submission"/>.</returns>
    public static List<Submission> GetSubmissionsByTelegramChatId(long telegramChatId)
    {
      var user = CommonUserModel.GetUserByChatId(telegramChatId);
      if (user == null)
      {
        Logger.LogError($"Пользователь с TelegramChatId {telegramChatId} не найден.");
        return new List<Submission>();
      }

      return dbManager.GetSubmissionsByStudentId(user.TelegramChatId);
    }

    /// <summary>
    /// Получает список выполненных заданий для пользователя по курсу.
    /// </summary>
    /// <param name="telegramChatId">Telegram Chat ID пользователя.</param>
    /// <param name="courseId">Идентификатор курса.</param>
    /// <returns>Список объектов <see cref="Submission"/>.</returns>
    public static List<Submission> GetSubmissionsByCourse(long telegramChatId, int courseId)
    {
      var user = CommonUserModel.GetUserByChatId(telegramChatId);
      if (user == null)
      {
        Logger.LogError($"Пользователь с TelegramChatId {telegramChatId} не найден.");
        return new List<Submission>();
      }

      var data = GetSubmissionsByTelegramChatId(telegramChatId);
      return data.Where(x => x.CourseId == courseId).ToList();
    }

    /// <summary>
    /// Обновляет данные выполнения задания.
    /// </summary>
    /// <param name="submission">Объект <see cref="Submission"/> с обновленными данными.</param>
    public static void UpdateSubmission(Submission submission)
    {
      dbManager.UpdateSubmission(submission.SubmissionId, submission);
    }

    /// <summary>
    /// Получает запись о выполнении задания для определенного задания и студента.
    /// </summary>
    /// <param name="telegramChatId">Telegram Chat ID пользователя.</param>
    /// <param name="homeworkId">Идентификатор домашнего задания.</param>
    /// <returns>Объект <see cref="Submission"/>, если найден; иначе null.</returns>
    public static Submission? GetSubmissionForAssignment(long telegramChatId, int homeWorkId)
    {
      var user = CommonUserModel.GetUserByChatId(telegramChatId);

      if (user != null)
      {
        var data = dbManager.GetAllSubmissions().Where(x => x.StudentId == user.TelegramChatId && x.AssignmentId == homeWorkId)
            .FirstOrDefault();


        if (data == null)
        {
          Logger.LogError($"Не найдена запись о подаче для telegramChatId: {telegramChatId}, homeWorkId: {homeWorkId}.");
        }

        return data;
      }
      else
      {
        return null;
      }
    }

    public static List<Submission?> GetSubmissionsByCourseAndAssigment(int assigmentId, int courseId)
    {
      var data = dbManager.GetAllSubmissions().Where(x => x.AssignmentId == assigmentId && x.CourseId == courseId);
      return data.Count() > 0 ? data.ToList() : null;
    }
  }
}
