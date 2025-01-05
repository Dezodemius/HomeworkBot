using HomeWorkTelegramBot.DataBase;
using HomeWorkTelegramBot.Models;
using System.Collections.Generic;
using static HomeWorkTelegramBot.Config.Logger;

namespace HomeWorkTelegramBot.Core
{
  internal static class TaskWorkService
  {
    private static readonly TaskWorkRepository taskWorkRepository = new TaskWorkRepository();

    /// <summary>
    /// Добавляет новое задание и логирует это действие.
    /// </summary>
    /// <param name="taskWork">Объект задания для добавления.</param>
    public static void AddTaskWork(TaskWork taskWork)
    {
      taskWorkRepository.AddTaskWork(taskWork);
      LogInformation($"Добавлено новое задание: {taskWork.Name}");
    }

    /// <summary>
    /// Удаляет задание и логирует это действие.
    /// </summary>
    /// <param name="taskId">Идентификатор задания.</param>
    public static void DeleteTaskWork(int taskId)
    {
      taskWorkRepository.DeleteTaskWork(taskId);
      LogInformation($"Удалено задание с Id {taskId}");
    }

    /// <summary>
    /// Получает задание по идентификатору и логирует это действие.
    /// </summary>
    /// <param name="taskId">Идентификатор задания.</param>
    /// <returns>Объект TaskWork или null, если задание не найдено.</returns>
    public static TaskWork GetTaskWorkById(int taskId)
    {
      var taskWork = taskWorkRepository.GetTaskWorkById(taskId);
      if (taskWork != null)
      {
        LogInformation($"Найдено задание с Id {taskId}");
      }
      else
      {
        LogWarning($"Задание с Id {taskId} не найдено.");
      }
      return taskWork;
    }

    /// <summary>
    /// Получает все задания для указанного курса и логирует это действие.
    /// </summary>
    /// <param name="courseId">Идентификатор курса.</param>
    /// <returns>Список заданий для курса.</returns>
    public static List<TaskWork> GetTaskWorksByCourseId(int courseId)
    {
      var taskWorks = taskWorkRepository.GetTaskWorksByCourseId(courseId);
      LogInformation($"Получено {taskWorks.Count} заданий для курса с Id {courseId}");
      return taskWorks;
    }

    /// <summary>
    /// Получает все задания и логирует это действие.
    /// </summary>
    /// <returns>Список всех заданий.</returns>
    public static List<TaskWork> GetAllTaskWorks()
    {
      var taskWorks = taskWorkRepository.GetAllTaskWorks();
      LogInformation($"Получено {taskWorks.Count} заданий.");
      return taskWorks;
    }
  }
}