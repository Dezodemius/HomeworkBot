using HomeWorkTelegramBot.Config;
using HomeWorkTelegramBot.Models;
using System.Collections.Generic;
using System.Linq;

namespace HomeWorkTelegramBot.DataBase
{
  internal class CourseEnrollmentRepository
  {
    /// <summary>
    /// Добавляет новую запись о зачислении пользователя на курс в базу данных.
    /// </summary>
    /// <param name="enrollment">Объект зачисления для добавления.</param>
    public void AddCourseEnrollment(CourseEnrollment enrollment)
    {
      ApplicationData.DbContext.CourseEnrollments.Add(enrollment);
      ApplicationData.DbContext.SaveChanges();
    }

    /// <summary>
    /// Удаляет запись о зачислении пользователя на курс из базы данных.
    /// </summary>
    /// <param name="enrollmentId">Идентификатор записи о зачислении.</param>
    public void DeleteCourseEnrollment(int enrollmentId)
    {
      var enrollment = ApplicationData.DbContext.CourseEnrollments
                          .FirstOrDefault(e => e.Id == enrollmentId);
      if (enrollment != null)
      {
        ApplicationData.DbContext.CourseEnrollments.Remove(enrollment);
        ApplicationData.DbContext.SaveChanges();
      }
    }

    /// <summary>
    /// Получает запись о зачислении пользователя на курс по идентификатору.
    /// </summary>
    /// <param name="enrollmentId">Идентификатор записи о зачислении.</param>
    /// <returns>Объект CourseEnrollment или null, если запись не найдена.</returns>
    public CourseEnrollment GetCourseEnrollmentById(int enrollmentId)
    {
      return ApplicationData.DbContext.CourseEnrollments.FirstOrDefault(e => e.Id == enrollmentId);
    }

    /// <summary>
    /// Получает все записи о зачислении пользователя на курсы.
    /// </summary>
    /// <returns>Список записей о зачислении.</returns>
    public List<CourseEnrollment> GetAllCourseEnrollments()
    {
      return ApplicationData.DbContext.CourseEnrollments.ToList();
    }

    /// <summary>
    /// Получает все записи о зачислении пользователя на курсы.
    /// </summary>
    /// <returns>Список записей о зачислении.</returns>
    public List<CourseEnrollment> GetAllUserCourseEnrollments(long chatId)
    {
      return ApplicationData.DbContext.CourseEnrollments
        .Where(ce => ce.UserId == chatId)
        .ToList();
    }
  }
}