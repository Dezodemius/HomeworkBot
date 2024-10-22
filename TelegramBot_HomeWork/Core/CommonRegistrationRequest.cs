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
  static public class CommonRegistrationRequest
  {
    static DatabaseManager dbManager = new DatabaseManager(ApplicationData.ConfigApp.DatabaseConnectionString);

    static public List<RegistrationRequest> GetAllRegistrationRequests()
    {
      return dbManager.GetAllRegistrationRequests();
    }

    static public RegistrationRequest GetRegistrationRequests(long telegramId)
    {
      var data = dbManager.GetAllRegistrationRequests().Where(x => x.TelegramChatId == telegramId);

      if (data.Count() != 0)
        return data.First();
      else
        return null;
    }

    static public void AddRegistrationRequests(RegistrationRequest registrationRequest)
    {
      dbManager.CreateRegistrationRequest(registrationRequest);
    }

    static public void UpdateStatusRegistrationRequests(RegistrationRequest registrationRequest, string status)
    {
      registrationRequest.Status = status;
      dbManager.UpdateRegistrationRequest(registrationRequest.RequestId, registrationRequest);
    }

    static public void DeleteRegistrationRequests(RegistrationRequest registrationRequest) 
    {
      dbManager.DeleteRegistrationRequest(registrationRequest.RequestId);
    }
  }
}
