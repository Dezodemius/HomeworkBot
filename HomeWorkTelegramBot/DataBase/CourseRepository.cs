using HomeWorkTelegramBot.Config;
using HomeWorkTelegramBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeWorkTelegramBot.DataBase
{
  internal class CourseRepository
  {
    /// <summary>
    /// Получает список всех курсов.
    /// </summary>
    /// <returns>Список курсов.</returns>
    public List<Courses> GetAllCourses()
    {
      return ApplicationData.DbContext.Courses.ToList();
    }

    /// <summary>
    /// Получает курс по идентификатору.
    /// </summary>
    /// <param name="courseId">Идентификатор курса.</param>
    /// <returns>Курс или null, если курс не найден.</returns>
    public Courses GetCourseById(int courseId)
    {
      return ApplicationData.DbContext.Courses.FirstOrDefault(c => c.Id == courseId);
    }
  }
}
