using Database;
using ModelInterfaceHub;
using ModelInterfaceHub.Data;
using ModelInterfaceHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
  static public class CommonDataModel
  {

    static DatabaseManager dbManager = new DatabaseManager(ApplicationData.ConfigApp.DatabaseConnectionString);

    /// <summary>
    /// Возвращает модель пользователя по уникальному идентификатору.
    /// </summary>
    /// <param name="userId">Уникальный идентификатор.</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    static public UserModel GetUserById(long userId)
    {
      return dbManager.GetUserById(userId);
    }

    /// <summary>
    /// Возвращает модель пользователя по уникальному идентификатору.
    /// </summary>
    /// <param name="userId">Уникальный идентификатор.</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    static public HomeWorkModel GetHomrWorkById(int homeId)
    {
      return dbManager.GetAssignmentById(homeId);
    }

    /// <summary>
    /// Возвращает роль пользователя по уникальному идентификатору.
    /// </summary>
    /// <param name="userId">Уникальный идентификатор.</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    static public UserRole GetUserRoleById(long userId)
    {
      throw new NotImplementedException();
    }


    static public void SeedTestData()
    { 
      dbManager.SeedTestData();
    }
  }
}
