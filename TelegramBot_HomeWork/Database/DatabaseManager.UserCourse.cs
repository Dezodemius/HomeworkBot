using System;
using System.Collections.Generic;
using System.Data.SQLite;
using DataContracts.Models;
using DataContracts;

namespace Database
{
  public partial class DatabaseManager
  {
    /// <summary>
    /// Добавляет связь между пользователем и курсом.
    /// </summary>
    /// <param name="userCourse">Модель связи между пользователем и курсом</param>
    public void CreateUserCourse(UserCourse userCourse)
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      string query = "INSERT INTO UserCourses (UserId, CourseId) VALUES (@UserId, @CourseId)";
      using var command = new SQLiteCommand(query, connection);
      command.Parameters.AddWithValue("@UserId", userCourse.UserId);
      command.Parameters.AddWithValue("@CourseId", userCourse.CourseId);

      command.ExecuteNonQuery();
      Logger.LogInfo("Новая связь между пользователем и курсом добавлена в базу данных.");
    }

    /// <summary>
    /// Удаляет связь между пользователем и курсом по идентификаторам.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="courseId">Идентификатор курса</param>
    public void DeleteUserCourse(int userId, int courseId)
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      string query = "DELETE FROM UserCourses WHERE UserId = @UserId AND CourseId = @CourseId";
      using var command = new SQLiteCommand(query, connection);
      command.Parameters.AddWithValue("@UserId", userId);
      command.Parameters.AddWithValue("@CourseId", courseId);

      int rowsAffected = command.ExecuteNonQuery();
      if (rowsAffected > 0)
      {
        Logger.LogInfo($"Связь между пользователем {userId} и курсом {courseId} успешно удалена.");
      }
      else
      {
        Logger.LogError($"Связь между пользователем {userId} и курсом {courseId} не найдена.");
      }
    }

    /// <summary>
    /// Обновляет связь между пользователем и курсом.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="oldCourseId">Старый идентификатор курса</param>
    /// <param name="newCourseId">Новый идентификатор курса</param>
    public void UpdateUserCourse(int userId, int oldCourseId, int newCourseId)
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      string query = @"UPDATE UserCourses 
                      SET CourseId = @NewCourseId 
                      WHERE UserId = @UserId AND CourseId = @OldCourseId";
      using var command = new SQLiteCommand(query, connection);
      command.Parameters.AddWithValue("@NewCourseId", newCourseId);
      command.Parameters.AddWithValue("@UserId", userId);
      command.Parameters.AddWithValue("@OldCourseId", oldCourseId);

      int rowsAffected = command.ExecuteNonQuery();
      if (rowsAffected > 0)
      {
        Logger.LogInfo($"Связь между пользователем {userId} и курсом обновлена с курса {oldCourseId} на курс {newCourseId}.");
      }
      else
      {
        Logger.LogError($"Связь между пользователем {userId} и курсом {oldCourseId} не найдена для обновления.");
      }
    }

    /// <summary>
    /// Возвращает все связи между пользователями и курсами.
    /// </summary>
    /// <returns>Список всех связей UserCourse</returns>
    public List<UserCourse> GetAllUserCourses()
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      string query = "SELECT UserId, CourseId FROM UserCourses";
      using var command = new SQLiteCommand(query, connection);

      using var reader = command.ExecuteReader();
      var userCourses = new List<UserCourse>();

      while (reader.Read())
      {
        var userCourse = new UserCourse
        {
          UserId = Convert.ToInt32(reader["UserId"]),
          CourseId = Convert.ToInt32(reader["CourseId"])
        };

        userCourses.Add(userCourse);
      }

      Logger.LogInfo($"Получено {userCourses.Count} связей между пользователями и курсами из базы данных.");
      return userCourses;
    }
  }
}
