using HomeWorkTelegramBot.DataBase;
using HomeWorkTelegramBot.Models;
using System.Collections.Generic;
using static HomeWorkTelegramBot.Config.Logger;

namespace HomeWorkTelegramBot.Core
{
  internal static class CourseEnrollmentService
  {
    private static readonly CourseEnrollmentRepository enrollmentRepository = new CourseEnrollmentRepository();

    /// <summary>
    /// Добавляет новую запись о зачислении пользователя на курс и логирует это действие.
    /// </summary>
    /// <param name="enrollment">Объект зачисления для добавления.</param>
    public static void AddCourseEnrollment(CourseEnrollment enrollment)
    {
      try
      {
        enrollmentRepository.AddCourseEnrollment(enrollment);
        LogInformation($"Добавлена новая запись о зачислении: UserId - {enrollment.UserId}, CourseId - {enrollment.CourseId}");
      }
      catch (Exception ex)
      {
        LogError($"Ошибка при добавлении записи о зачислении: {ex.Message}. Inner Exception: {ex.InnerException?.Message}");
        throw;
      }
    }

    /// <summary>
    /// Удаляет запись о зачислении пользователя на курс и логирует это действие.
    /// </summary>
    /// <param name="enrollmentId">Идентификатор записи о зачислении.</param>
    public static void DeleteCourseEnrollment(int enrollmentId)
    {
      try
      {
        enrollmentRepository.DeleteCourseEnrollment(enrollmentId);
        LogInformation($"Удалена запись о зачислении с Id {enrollmentId}");
      }
      catch (Exception ex)
      {
        LogError($"Ошибка при удалении записи о зачислении: {ex.Message}");
        throw;
      }
    }

    /// <summary>
    /// Получает запись о зачислении пользователя на курс по идентификатору и логирует это действие.
    /// </summary>
    /// <param name="enrollmentId">Идентификатор записи о зачислении.</param>
    /// <returns>Объект CourseEnrollment или null, если запись не найдена.</returns>
    public static CourseEnrollment GetCourseEnrollmentById(int enrollmentId)
    {
      try
      {
        var enrollment = enrollmentRepository.GetCourseEnrollmentById(enrollmentId);
        if (enrollment != null)
        {
          LogInformation($"Найдена запись о зачислении с Id {enrollmentId}");
        }
        else
        {
          LogWarning($"Запись о зачислении с Id {enrollmentId} не найдена.");
        }
        return enrollment;
      }
      catch (Exception ex)
      {
        LogError($"Ошибка при получении записи о зачислении: {ex.Message}");
        throw;
      }
    }

    /// <summary>
    /// Получает все записи о зачислении пользователя на курсы и логирует это действие.
    /// </summary>
    /// <returns>Список записей о зачислении.</returns>
    public static List<CourseEnrollment> GetAllCourseEnrollments()
    {
      try
      {
        var enrollments = enrollmentRepository.GetAllCourseEnrollments();
        LogInformation($"Получено {enrollments.Count} записей о зачислении.");
        return enrollments;
      }
      catch (Exception ex)
      {
        LogError($"Ошибка при получении всех записей о зачислении: {ex.Message}");
        throw;
      }
    }
  }
}