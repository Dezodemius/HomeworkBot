﻿using Database;
using DataContracts.Data;
using DataContracts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
  public static class CommonUserModel
  {
    static readonly DatabaseManager dbManager = new DatabaseManager(ApplicationData.ConfigApp.DatabaseConnectionString);

    /// <summary>
    /// Возвращает модель пользователя по уникальному идентификатору.
    /// </summary>
    /// <param name="userId">Уникальный идентификатор.</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    static public UserModel GetUserById(long userId)
    {
      // return dbManager.GetUserById(userId);
      throw new NotImplementedException();
    }

    /// <summary>
    /// Возвращает роль пользователя по уникальному идентификатору.
    /// </summary>
    /// <param name="userId">Уникальный идентификатор.</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    static public UserRole GetUserRoleById(long userId)
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
      //try
      //{
      //  return dbManager.GetStudentName(homewokrId);
      //}
      //catch (SystemException)
      //{
      //  Console.WriteLine($"Нет студентов с таким выполненным домашним заданием ");
      //  throw new SystemException();
      //}
      //catch (Exception ex)
      //{
      //  Console.WriteLine($"Ошибка: {ex.Message}");
      //  throw new Exception();
      //}

      throw new NotImplementedException();
    }
  }
}