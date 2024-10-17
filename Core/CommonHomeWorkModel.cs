using Database;
using ModelInterfaceHub.Data;
using ModelInterfaceHub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace Core
{
  /// <summary>
  /// Статический класс, предоставляющий общие методы для работы с моделью домашнего задания.
  /// </summary>
  static public class CommonHomeWorkModel
  {

    static readonly DatabaseManager dbManager = new DatabaseManager(ApplicationData.ConfigApp.DatabaseConnectionString);

    static public List<HomeWorkModel> GetHomeWork()
    {
      return dbManager.GetAllHomeWork();
    }

    /// <summary>
    /// Получает список домашних работ для указанного студента.
    /// </summary>
    /// <param name="userId">Идентификатор студента.</param>
    /// <returns>Список моделей задач для домашнего задания.</returns>
    static public List<StudentHomeWorkModel> GetHomeworkForStudent(long userId)
    {
      List<StudentHomeWorkModel>? tasks = new List<StudentHomeWorkModel>();
      var allTasks = dbManager.GetAllHomeWorksForStudents();
      if (allTasks.Any())
      {
        tasks = allTasks.FindAll(x => x.IdStudent == userId);
      }
      return tasks;
    }

    /// <summary>
    /// Получает список задач для указанного домашнего задания или для всех дз.
    /// </summary>
    /// <param name="statusWork">Статус задания.</param>
    /// <param name="homeworkId">Идентификатор домашнего задания.</param>
    /// <returns>Список моделей непроверенных задач для домашнего задания.</returns>
    static public List<StudentHomeWorkModel> GetTasksForHomework(StatusWork statusWork, int homeworkId = -1)
    {
      List<StudentHomeWorkModel>? tasks = new List<StudentHomeWorkModel>();
      var allTasks = dbManager.GetAllHomeWorksForStudents();
      if (homeworkId != -1)
      {
        if (allTasks.Any())
        {
          tasks = allTasks.FindAll(x => x.IdHomeWork == homeworkId && x.Status == statusWork);
        }
      }
      else
      {
        if (allTasks.Any())
        {
          tasks = allTasks.FindAll(x => x.Status == statusWork);
        }
      }
      return tasks;
    }

    /// <summary>
    /// Получает список студентов, выполнивших конкретное домашнее задание.
    /// </summary>
    /// <param name="homewokrId">Идентификатор домашнего задания.</param>
    /// <returns></returns>
    /// <exception cref="SystemException">Исключение, которое возникает, если таких студентов нет.</exception>
    /// <exception cref="Exception">Другие исклбчения, которые могут возникнуть.</exception>
    static public List<string> GetStudentsCompletedHomework(int homewokrId)
    {
      try
      {
        return dbManager.GetStudentName(homewokrId);
      }
      catch (SystemException)
      {
        Console.WriteLine($"Нет студентов с таким выполненным домашним заданием ");
        throw new SystemException();
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Ошибка: {ex.Message}");
        throw new Exception();
      }
    }

  }
}
