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
    /// Создаёт новую запись выполнения задания.
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
    /// <returns>Список объектов <see cref="Submission"/> для заданного курса.</returns>
    public static List<Submission> GetSubmissionsByCourse(long telegramChatId, int courseId)
    {
      var user = CommonUserModel.GetUserByChatId(telegramChatId);
      if (user == null)
      {
        Logger.LogError($"Пользователь с TelegramChatId {telegramChatId} не найден.");
        return new List<Submission>();
      }

      return dbManager.GetSubmissionsByCourse(user.TelegramChatId, courseId);
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
    /// Получает запись о выполнении задания для конкретного задания и студента.
    /// </summary>
    /// <param name="telegramChatId">Telegram Chat ID пользователя.</param>
    /// <param name="homeworkId">Идентификатор домашнего задания.</param>
    /// <returns>Объект <see cref="Submission"/>, если найден; иначе null.</returns>
    public static Submission? GetSubmissionForAssignment(long telegramChatId, int homeWorkId)
    {
      var user = CommonUserModel.GetUserByChatId(telegramChatId);
      if (user == null)
      {
        Logger.LogError($"Пользователь с TelegramChatId {telegramChatId} не найден.");
        return null;
      }

      var submission = dbManager.GetSubmissionForAssignment(user.TelegramChatId, homeWorkId);
      if (submission == null)
      {
        Logger.LogError($"Не найдена запись о подаче для telegramChatId: {telegramChatId}, homeWorkId: {homeWorkId}.");
      }

      return submission;
    }

    /// <summary>
    /// Получает записи выполнения задания по курсу и заданию.
    /// </summary>
    /// <param name="assignmentId">Идентификатор задания.</param>
    /// <param name="courseId">Идентификатор курса.</param>
    /// <returns>Список объектов <see cref="Submission"/> для заданного курса и задания.</returns>
    public static List<Submission> GetSubmissionsByCourseAndAssignment(int assignmentId, int courseId)
    {
      return dbManager.GetSubmissionsByCourseAndAssignment(assignmentId, courseId);
    }
  }
}
