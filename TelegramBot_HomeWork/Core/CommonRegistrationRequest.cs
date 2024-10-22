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

    static public List<RegistrationRequest> GetAllRegistrationRequestsAsync()
    {
      return dbManager.GetAllRegistrationRequests();
    }
  }
}
