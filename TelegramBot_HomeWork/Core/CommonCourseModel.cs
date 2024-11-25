using Database;
using DataContracts.Data;
using DataContracts.Models;
using System;
using System.Collections.Generic;

namespace Core
{
  public static class CommonCourseModel
  {
    static DatabaseManager dbManager = new DatabaseManager(ApplicationData.ConfigApp.DatabaseConnectionString);

    /// <summary>
    /// Получает список всех курсов из базы данных.
    /// </summary>
    /// <returns>Список всех курсов.</returns>
    public static List<Course> GetCourses()
    {
      return dbManager.GetAllCourses();
    }

    /// <summary>
    /// Возвращает все курсы пользователя.
    /// </summary>
    /// <param name="idUser">Идентификатор пользователя.</param>
    /// <returns>Список курсов пользователя.</returns>
    public static List<Course> GetUserCourses(long idUser)
    {
      return dbManager.GetAllUserCoursesByUserId(idUser);
    }

    /// <summary>
    /// Возвращает название курса по заданному идентификатору.
    /// </summary>
    /// <param name="courseId">Идентификатор курса.</param>
    /// <returns>Название курса.</returns>
    public static string GetCourseNameById(int courseId)
    {
      return dbManager.GetCourseNameById(courseId);
    }

    /// <summary>
    /// Добавляет новый курс в базу данных.
    /// </summary>
    /// <param name="course">Объект курса для добавления.</param>
    public static void CreateNewCourse(Course course)
    {
      dbManager.CreateCourse(course);
    }

    /// <summary>
    /// Записывает пользователя на указанный курс.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <param name="courseId">Идентификатор курса.</param>
    public static void EnrollUserToCourse(long userId, int courseId)
    {
      UserCourse userCourse = new UserCourse();
      userCourse.UserId = userId;
      userCourse.CourseId = courseId;

      dbManager.CreateUserCourse(userCourse);
    }
  }
}