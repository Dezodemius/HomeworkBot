using Database;
using DataContracts.Data;
using DataContracts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DataContracts.Models.Submission;

namespace Core
{
  static public class CommonHomeWork
  {
    static DatabaseManager dbManager = new DatabaseManager(ApplicationData.ConfigApp.DatabaseConnectionString);

    /// <summary>
    /// Получает список домашних заданий для указанного курса.
    /// </summary>
    /// <param name="courseId">Идентификатор курса.</param>
    /// <returns>Список объектов <see cref="Assignment"/> для курса.</returns>
    static public List<Assignment> GetAssignmentsByCourseId(int courseId)
    {
      return dbManager.GetAssignmentsByCourse(courseId);
    }

    /// <summary>
    /// Получает домашнее задание по его идентификатору и идентификатору курса.
    /// </summary>
    /// <param name="courseId">Идентификатор курса.</param>
    /// <param name="assignmentId">Идентификатор домашнего задания.</param>
    /// <returns>Объект <see cref="Assignment"/>, если найден; иначе null.</returns>
    static public Assignment? GetAssignmentById(int courseId, int homeId)
    {
      try
      {
        return dbManager.GetAssignmentsByCourse(courseId).Where(hw => hw.AssignmentId == homeId).First();
      }
      catch
      {
        return null;
      }
    }

    /// <summary>
    /// Добавляет новое домашнее задание в базу данных.
    /// </summary>
    /// <param name="assignment">Объект домашнего задания для добавления.</param>
    static public void CreateAssignment(Assignment assignment)
    {
      dbManager.CreateAssignment(assignment);
    }

  }
}
