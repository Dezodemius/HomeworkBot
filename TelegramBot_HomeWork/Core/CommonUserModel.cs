using Database;
using DataContracts;
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
    /// <param name="chatId">Уникальный идентификатор.</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    static public UserModel? GetUserById(long chatId)
    {
      try
      {
        Logger.LogInfo("Поиск пользователя");
        return dbManager.GetUserByTelegramChatId(chatId);
      }
      catch
      { 
        Logger.LogError("Пользователь не найден!");
        return null;
      }
    }

    /// <summary>
    /// Возвращает роль пользователя по уникальному идентификатору.
    /// </summary>
    /// <param name="chatId">Уникальный идентификатор.</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public static UserRole? GetUserRoleById(long chatId)
    {
      var user = dbManager.GetAllUsers().FirstOrDefault(u => u.TelegramChatId == chatId);
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
    static public List<UserModel> GetAllAdministrators()
    { 
      return dbManager.GetAllUsers().Where(x=>x.Role == UserRole.Administrator).ToList();
    }

    static public List<UserModel> GetAllStudents()
    {
      return dbManager.GetAllUsers().Where(x => x.Role == UserRole.Student).ToList();
    }

    public static List<UserModel> GetAllStudentsByCourse(int courseId)
    {
      var allStudents = dbManager.GetAllUsers().Where(x => x.Role == UserRole.Student).ToList();
      var studentCourseLinks = dbManager.GetAllUserCourses().Where(uc => uc.CourseId == courseId).ToList();
      var studentIds = studentCourseLinks.Select(sc => sc.UserId).ToList();
      var studentsInCourse = allStudents.Where(student => studentIds.Contains(student.TelegramChatId)).ToList();

      return studentsInCourse;
    }


    static public List<UserModel> GetAllTeachers()
    {
      return dbManager.GetAllUsers().Where(x => x.Role == UserRole.Teacher).ToList();
    }

    static public List<UserModel> GetAllUsers()
    {
      return dbManager.GetAllUsers();
    }

    static public void UpdateUserModel(UserModel userModel)
    { 
      dbManager.UpdateUser(userModel);
    }
  }
}
