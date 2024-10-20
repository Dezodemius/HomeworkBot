using Database;
using DataContracts.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
  public static class CheckData
  {
    static DatabaseManager dbManager = new DatabaseManager(ApplicationData.ConfigApp.DatabaseConnectionString);

    static public void CheckTables()
    {
      dbManager.EnsureDatabaseCreated();
    }

  }
}
