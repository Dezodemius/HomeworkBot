using Database;
using DataContracts.Data;
using DataContracts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Core
{
  public static class CommonUserModel
  {
    static readonly DatabaseManager dbManager = new DatabaseManager(ApplicationData.ConfigApp.DatabaseConnectionString);

    /// <summary>
    /// Возвращает модель пользователя по уникальному идентификатору.
    /// </summary>
    /// <param name="userId">Уникальный идентификатор.</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    static public UserModel GetUserById(long userId)
    {
      return dbManager.GetAllUsers().Where(x => x.TelegramChatId == userId).First();
    }

    /// <summary>
    /// Возвращает роль пользователя по уникальному идентификатору.
    /// </summary>
    /// <param name="userId">Уникальный идентификатор.</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static UserRole? GetUserRoleById(long userId)
    {
      var user = dbManager.GetAllUsers().FirstOrDefault(u => u.TelegramChatId == userId);
      return user?.Role;
    }

    /// <summary>
    /// Получает список студентов, выполнивших конкретное домашнее задание.
    /// </summary>
    /// <param name="homewokrId">Идентификатор домашнего задания.</param>
    /// <returns></returns>
    /// <exception cref="SystemException">Исключение, которое возникает, если таких студентов нет.</exception>
    /// <exception cref="Exception">Другие исклбчения, которые могут возникнуть.</exception>
    static public List<string> GetStudentsCompletedHomework(int homewokrId)
    {
      //try
      //{
      //  return dbManager.GetStudentName(homewokrId);
      //}
      //catch (SystemException)
      //{
      //  Console.WriteLine($"Нет студентов с таким выполненным домашним заданием ");
      //  throw new SystemException();
      //}
      //catch (Exception ex)
      //{
      //  Console.WriteLine($"Ошибка: {ex.Message}");
      //  throw new Exception();
      //}

      throw new NotImplementedException();
    }


    static public void AddStudent(RegistrationRequest registrationRequest)
    {
      var userModel = new UserModel(registrationRequest.TelegramChatId, registrationRequest.FirstName, registrationRequest.LastName, registrationRequest.Email, UserRole.Student);
      dbManager.CreateUser(userModel);
    }
    static public List<UserModel> GetAllAdministartor()
    { 
      return dbManager.GetAllUsers().Where(x=>x.Role == UserRole.Administrator).ToList();
    }
  }
}
