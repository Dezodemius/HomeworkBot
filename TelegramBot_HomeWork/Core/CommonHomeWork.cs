using Database;
using DataContracts.Data;
using DataContracts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DataContracts.Models.StudentHomeWorkModel;

namespace Core
{
  static public class CommonHomeWork
  {
    static DatabaseManager dbManager = new DatabaseManager(ApplicationData.ConfigApp.DatabaseConnectionString);

    static public List<HomeWork> GetHomeWork()
    {
      // return dbManager.GetAllHomeWork();
      throw new NotImplementedException();
    }

    /// <summary>
    /// Возвращает модель домашней работы по уникальному идентификатору.
    /// </summary>
    /// <param name="userId">Уникальный идентификатор.</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    static public HomeWork GetHomeWorkById(int homeId)
    {
      // return dbManager.GetAssignmentById(homeId);
      throw new NotImplementedException();
    }

    /// <summary>
    /// Получает список домашних работ для указанного студента.
    /// </summary>
    /// <param name="userId">Идентификатор студента.</param>
    /// <returns>Список моделей задач для домашнего задания.</returns>
    static public List<StudentHomeWorkModel> GetHomeworkForStudent(long userId)
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
    static public List<StudentHomeWorkModel> GetTasksForHomework(StatusWork statusWork, int homeworkId = -1)
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
    /// Создаёт домашнюю работу по названию и описанию
    /// </summary>
    /// <param name="homeWorkModel"></param>
    static public void AddHomeWork(string title, string description)
    {
      // var homeWorkModel = new HomeWork(title, description);
      // dbManager.CreateHomeWork(homeWorkModel);
      throw new NotImplementedException();
    }

  }
}
