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
    /// Возвращает все курсы.
    /// </summary>
    /// <returns>Список всех курсов.</returns>
    public static List<Course> GetAllCourses()
    {
      return dbManager.GetAllCourses();
    }

    /// <summary>
    /// Возвращает все курсы пользователя.
    /// </summary>
    /// <param name="idUser">Идентификатор пользователя.</param>
    /// <returns>Список курсов пользователя.</returns>
    public static List<Course> GetAllUserCourses(long idUser)
    {
      return dbManager.GetAllUserCoursesByUserId(idUser);
    }

    /// <summary>
    /// Возвращает название курса по его идентификатору.
    /// </summary>
    /// <param name="courseId">Идентификатор курса.</param>
    /// <returns>Название курса.</returns>
    public static string GetNameCourse(int courseId)
    {
      return dbManager.GetCourseNameById(courseId);
    }

    /// <summary>
    /// Добавляет новый курс.
    /// </summary>
    /// <param name="course">Модель курса.</param>
    public static void AddCourse(Course course)
    {
      dbManager.CreateCourse(course);
    }

    public static void AddUserInCourse(long userId, int courseId)
    {
      UserCourse userCourse = new UserCourse();
      userCourse.UserId = userId;
      userCourse.CourseId = courseId;

      dbManager.CreateUserCourse(userCourse);
    }
  }
}