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
    /// Получает модель пользователя по идентификатору чата Telegram.
    /// </summary>
    /// <param name="chatId">Уникальный идентификатор чата Telegram.</param>
    /// <returns>Объект <see cref="UserModel"/>, если пользователь найден; иначе null.</returns>
    static public UserModel? GetUserByChatId(long chatId)
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
    /// Определяет роль пользователя по идентификатору чата Telegram.
    /// </summary>
    /// <param name="chatId">Уникальный идентификатор чата Telegram.</param>
    /// <returns>Роль пользователя, если он найден; иначе null.</returns>
    public static UserRole? GetUserRoleByChatId(long chatId)
    {
      var user = dbManager.GetAllUsers().FirstOrDefault(u => u.TelegramChatId == chatId);
      return user?.Role;
    }

    /// <summary>
    /// Добавляет нового студента в базу данных.
    /// </summary>
    /// <param name="registrationRequest">Запрос на регистрацию с данными студента.</param>
    static public void RegisterStudent(RegistrationRequest registrationRequest)
    {
      var userModel = new UserModel(registrationRequest.TelegramChatId, registrationRequest.FirstName, registrationRequest.LastName, registrationRequest.Email, UserRole.Student);
      dbManager.CreateUser(userModel);
    }

    /// <summary>
    /// Возвращает список всех администраторов.
    /// </summary>
    /// <returns>Список объектов <see cref="UserModel"/> с ролью администратора.</returns>
    static public List<UserModel> GetAdministrators()
    { 
      return dbManager.GetAllUsers().Where(x=>x.Role == UserRole.Administrator).ToList();
    }

    /// <summary>
    /// Возвращает список всех студентов.
    /// </summary>
    /// <returns>Список объектов <see cref="UserModel"/> с ролью студента.</returns>
    static public List<UserModel> GetStudents()
    {
      return dbManager.GetAllUsers().Where(x => x.Role == UserRole.Student).ToList();
    }

    /// <summary>
    /// Возвращает список студентов, зарегистрированных на указанный курс.
    /// </summary>
    /// <param name="courseId">Идентификатор курса.</param>
    /// <returns>Список объектов <see cref="UserModel"/> студентов, относящихся к курсу.</returns>
    public static List<UserModel> GetStudentsByCourseId(int courseId)
    {
      var studentIds = dbManager.GetAllUserCourses()
                                .Where(uc => uc.CourseId == courseId)
                                .Select(uc => uc.UserId)
                                .ToHashSet(); 

      return dbManager.GetAllUsers()
                      .Where(user => user.Role == UserRole.Student && studentIds.Contains(user.TelegramChatId))
                      .ToList();
    }

    /// <summary>
    /// Возвращает список всех преподавателей.
    /// </summary>
    /// <returns>Список объектов <see cref="UserModel"/> с ролью преподавателя.</returns>
    static public List<UserModel> GetTeachers()
    {
      return dbManager.GetAllUsers().Where(x => x.Role == UserRole.Teacher).ToList();
    }

    /// <summary>
    /// Возвращает список всех пользователей.
    /// </summary>
    /// <returns>Список всех объектов <see cref="UserModel"/>.</returns>
    static public List<UserModel> GetUsers()
    {
      return dbManager.GetAllUsers();
    }

    /// <summary>
    /// Обновляет данные пользователя в базе данных.
    /// </summary>
    /// <param name="userModel">Модель пользователя с обновленными данными.</param>
    static public void UpdateUser(UserModel userModel)
    { 
      dbManager.UpdateUser(userModel);
    }
  }
}
