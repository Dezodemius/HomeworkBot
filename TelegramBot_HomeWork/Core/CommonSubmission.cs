using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database;
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
      var data = dbManager.GetAllSubmissions().Where(x=>x.SubmissionId == user.UserId).ToList();

      return data;
    }
  }
}
