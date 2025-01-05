using HomeWorkTelegramBot.DataBase;
using HomeWorkTelegramBot.Models;
using System.Collections.Generic;
using static HomeWorkTelegramBot.Config.Logger;

namespace HomeWorkTelegramBot.Core
{
  internal static class CourseService
  {
    private static readonly CourseRepository courseRepository = new CourseRepository();

    /// <summary>
    /// Получает список всех курсов и логирует это действие.
    /// </summary>
    /// <returns>Список курсов.</returns>
    public static List<Courses> GetAllCourses()
    {
      var courses = courseRepository.GetAllCourses();
      LogInformation($"Получено {courses.Count} курсов.");
      return courses;
    }

    /// <summary>
    /// Получает курс по идентификатору и логирует это действие.
    /// </summary>
    /// <param name="courseId">Идентификатор курса.</param>
    /// <returns>Курс или null, если курс не найден.</returns>
    public static Courses GetCourseById(int courseId)
    {
      var course = courseRepository.GetCourseById(courseId);
      if (course != null)
      {
        LogInformation($"Курс найден: {course.Name}");
      }
      else
      {
        LogWarning($"Курс с Id {courseId} не найден.");
      }
      return course;
    }

    /// <summary>
    /// Получает все курсы, принадлежащие преподавателю по идентификатору его чата и логирует это действие.
    /// </summary>
    /// <param name="teacherId">Идентификатор курса.</param>
    /// <returns>Список курсов или null, если курсы не найдены.</returns>
    public static List<Courses> GetAllCoursesByTeacherId(long teacherId)
    {
      var courses = courseRepository.GetCoursesByTeacherId(teacherId);
      if (courses != null)
      {
        LogInformation($"Найдено {courses.Count} у преподавателя с ChatId {teacherId}");
      }
      else
      {
        LogWarning($"Для преподавателя с ChatId {teacherId} курсы не найдены.");
      }

      return courses;
    }
  }
}