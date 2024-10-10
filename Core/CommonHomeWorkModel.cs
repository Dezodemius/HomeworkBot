﻿using ModelInterfaceHub.Models;
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
    /// <param name="homeworkId">Идентификатор домашнего задания.</param>
    /// <returns>Список моделей задач для домашнего задания.</returns>
    static public List<HomeWorkModel> GetTasksForHomework(int homeworkId)
    { 
      throw new NotImplementedException();
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