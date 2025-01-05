using HomeWorkTelegramBot.Config;
using HomeWorkTelegramBot.Models;
using System.Collections.Generic;
using System.Linq;

namespace HomeWorkTelegramBot.DataBase
{
  internal class TaskWorkRepository
  {
    /// <summary>
    /// Добавляет новое задание в базу данных.
    /// </summary>
    /// <param name="taskWork">Объект задания для добавления.</param>
    public void AddTaskWork(TaskWork taskWork)
    {
      ApplicationData.DbContext.TaskWorks.Add(taskWork);
      ApplicationData.DbContext.SaveChanges();
    }

    /// <summary>
    /// Удаляет задание из базы данных.
    /// </summary>
    /// <param name="taskId">Идентификатор задания.</param>
    public void DeleteTaskWork(int taskId)
    {
      var taskWork = ApplicationData.DbContext.TaskWorks
                          .FirstOrDefault(t => t.Id == taskId);
      if (taskWork != null)
      {
        ApplicationData.DbContext.TaskWorks.Remove(taskWork);
        ApplicationData.DbContext.SaveChanges();
      }
    }

    /// <summary>
    /// Получает задание по идентификатору.
    /// </summary>
    /// <param name="taskId">Идентификатор задания.</param>
    /// <returns>Объект TaskWork или null, если задание не найдено.</returns>
    public TaskWork GetTaskWorkById(int taskId)
    {
      return ApplicationData.DbContext.TaskWorks.FirstOrDefault(t => t.Id == taskId);
    }

    /// <summary>
    /// Получает все задания для указанного курса.
    /// </summary>
    /// <param name="courseId">Идентификатор курса.</param>
    /// <returns>Список заданий для курса.</returns>
    public List<TaskWork> GetTaskWorksByCourseId(int courseId)
    {
      return ApplicationData.DbContext.TaskWorks.Where(t => t.CourseId == courseId).ToList();
    }

    /// <summary>
    /// Получает все задания.
    /// </summary>
    /// <returns>Список всех заданий.</returns>
    public List<TaskWork> GetAllTaskWorks()
    {
      return ApplicationData.DbContext.TaskWorks.ToList();
    }
  }
}