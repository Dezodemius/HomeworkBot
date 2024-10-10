using Database;
using ModelInterfaceHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
  /// <summary>
  /// Статический класс, предоставляющий общие методы для работы с моделью домашнего задания.
  /// </summary>
  static public class CommonHomeWorkModel
  {
    /// <summary>
    /// Получает список задач для указанного домашнего задания.
    /// </summary>
    /// <param name="dbManager">Менеджер базы данных.</param>
    /// <param name="homeworkId">Идентификатор домашнего задания.</param>
    /// <returns>Список моделей задач для домашнего задания.</returns>
    static public List<HomeWorkModel> GetTasksForHomework(int homeworkId, DatabaseManager dbManager)
    {
      List<HomeWorkModel>? tasks = new List<HomeWorkModel>();
      var allTasks = dbManager.GetAllHomeWorks().FindAll(x=>x.Id==homeworkId);
      if (allTasks.Any())
      {
        tasks = allTasks.FindAll(x => x.Id == homeworkId);
      }
      return tasks;
    }

    /// <summary>
    /// Получает список непроверенных задач для указанного домашнего задания.
    /// </summary>
    /// <param name="homeworkId">Идентификатор домашнего задания.</param>
    /// <returns>Список моделей непроверенных задач для домашнего задания.</returns>
    static public List<HomeWorkModel> GetUncheckedTasksForHomework(int homeworkId)
    { 
      throw new NotImplementedException();
    }

    /// <summary>
    /// Получает список выполненных домашних заданий студентов для указанного идентификатора задания.
    /// </summary>
    /// <param name="homeworkId">Идентификатор домашнего задания.</param>
    /// <returns>Список моделей выполненных домашних заданий.</returns>
    static public List<HomeWorkModel> GetStudentsCompletedHomework(int homeworkId)
    {
      throw new NotImplementedException();
    }
  }
}
