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

    static public List<Assignment> GetHomeWork(int courseId)
    {
      return dbManager.GetAssignmentsByCourse(courseId);
    }

    /// <summary>
    /// Возвращает модель домашней работы по уникальному идентификатору.
    /// </summary>
    /// <param name="userId">Уникальный идентификатор.</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    static public Assignment GetHomeWorkById(int courseId, int homeId)
    {
      return (Assignment)(dbManager.GetAssignmentsByCourse(courseId).Where(x => x.AssignmentId == homeId));
    }

    /// <summary>
    /// Получает список домашних работ для указанного студента.
    /// </summary>
    /// <param name="userId">Идентификатор студента.</param>
    /// <returns>Список моделей задач для домашнего задания.</returns>
    static public List<Submission> GetHomeworkForStudent(long userId)
    {
      // List<StudentHomeWorkModel>? tasks = new List<StudentHomeWorkModel>();
      // var allTasks = dbManager.GetAllHomeWorksForStudents();
      // if (allTasks.Any())
      // {
      //   tasks = allTasks.FindAll(x => x.IdStudent == userId);
      // }
      // return tasks;

      throw new NotImplementedException();
    }

    /// <summary>
    /// Получает список задач для указанного домашнего задания или для всех дз.
    /// </summary>
    /// <param name="statusWork">Статус задания.</param>
    /// <param name="homeworkId">Идентификатор домашнего задания.</param>
    /// <returns>Список моделей непроверенных задач для домашнего задания.</returns>
    static public List<Submission> GetTasksForHomework(StatusWork statusWork, int homeworkId = -1)
    {
      // List<StudentHomeWorkModel>? tasks = new List<StudentHomeWorkModel>();
      // var allTasks = dbManager.GetAllHomeWorksForStudents();
      // if (homeworkId != -1)
      // {
      //   if (allTasks.Any())
      //   {
      //     tasks = allTasks.FindAll(x => x.IdHomeWork == homeworkId && x.Status == statusWork);
      //   }
      // }
      // else
      // {
      //   if (allTasks.Any())
      //   {
      //     tasks = allTasks.FindAll(x => x.Status == statusWork);
      //   }
      // }
      // return tasks;
      throw new NotImplementedException();
    }

    /// <summary>
    /// Добавляет домашнюю работу в БД.
    /// </summary>
    /// <param name="homeWorkModel"></param>
    static public void AddHomeWork(Assignment assignment)
    {
      dbManager.CreateAssignment(assignment);
    }

  }
}
