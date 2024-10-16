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

   
    static public List<StudentHomeWorkModel> GetTasksForHomework(int homeworkId, DatabaseManager dbManager)
    {
      List<StudentHomeWorkModel>? tasks = new List<StudentHomeWorkModel>();
      var allTasks = dbManager.GetAllHomeWorks();
      if (allTasks.Any())
      {
        tasks = allTasks.FindAll(x => x.IdHomeWork == homeworkId);
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
    /// Получает список студентов, выполнивших конкретное домашнее задание.
    /// </summary>
    /// <param name="homewokrId">Идентификатор домашнего задания.</param>
    /// <returns></returns>
    /// <exception cref="SystemException">Исключение, которое возникает, если таких студентов нет.</exception>
    /// <exception cref="Exception">Другие исклбчения, которые могут возникнуть.</exception>
    static public List<string> GetStudentsCompletedHomework(int homewokrId)
    {
      DatabaseManager databaseManager = new DatabaseManager(connectionString);

      try
      {
        return databaseManager.GetStudentName(homewokrId);
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
