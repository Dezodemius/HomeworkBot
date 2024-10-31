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

    public static void AddSubmission(Submission submission)
    { 
      dbManager.CreateSubmission(submission);
    }

    public static List<Submission> GetSubmission(long telegramChatId)
    {
      var user = CommonUserModel.GetUserById(telegramChatId);
      if (user == null)
      {
        Logger.LogError($"Пользователь с TelegramChatId {telegramChatId} не найден.");
        return new List<Submission>();
      }

      return dbManager.GetSubmissionsByStudentId(user.TelegramChatId);
    }

    public static void UpdateSubmission(Submission submission)
    {
      dbManager.UpdateSubmission(submission.SubmissionId, submission);
    }

    public static Submission GetSubmissionForHomeWork(long telegramChatId, int homeWorkId)
    {
      var user = CommonUserModel.GetUserById(telegramChatId);

      var data = dbManager.GetAllSubmissions().Where(x => x.StudentId == user.TelegramChatId && x.AssignmentId == homeWorkId)
          .FirstOrDefault();


      if (data == null)
      {
        Logger.LogError($"Не найдена запись о подаче для telegramChatId: {telegramChatId}, homeWorkId: {homeWorkId}.");
      }

      return data;
    }

  }
}
