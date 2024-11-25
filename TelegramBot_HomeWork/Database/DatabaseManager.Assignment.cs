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
    /// Добавляет новое задание в систему.
    /// </summary>
    /// <param name="assignment">Модель задания</param>
    public void CreateAssignment(Assignment assignment)
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      string query = "INSERT INTO Assignments (CourseId, Title, Description, DueDate) " +
                     "VALUES (@CourseId, @Title, @Description, @DueDate)";
      using var command = new SQLiteCommand(query, connection);
      command.Parameters.AddWithValue("@CourseId", assignment.CourseId);
      command.Parameters.AddWithValue("@Title", assignment.Title);
      command.Parameters.AddWithValue("@Description", assignment.Description);
      command.Parameters.AddWithValue("@DueDate", assignment.DueDate?.ToString("yyyy-MM-dd HH:mm:ss"));

      command.ExecuteNonQuery();
      Logger.LogInfo($"Задание {assignment.Title} добавлено.");
    }

    /// <summary>
    /// Удаляет задание из системы по AssignmentId и CourseId.
    /// </summary>
    /// <param name="assignmentId">Идентификатор задания</param>
    /// <param name="courseId">Идентификатор курса</param>
    public void DeleteAssignment(int assignmentId, int courseId)
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      string query = "DELETE FROM Assignments WHERE AssignmentId = @AssignmentId AND CourseId = @CourseId";
      using var command = new SQLiteCommand(query, connection);
      command.Parameters.AddWithValue("@AssignmentId", assignmentId);
      command.Parameters.AddWithValue("@CourseId", courseId);

      int rowsAffected = command.ExecuteNonQuery();
      if (rowsAffected > 0)
      {
        Logger.LogInfo($"Задание с AssignmentId {assignmentId} и CourseId {courseId} удалено.");
      }
      else
      {
        Logger.LogError($"Задание с AssignmentId {assignmentId} и CourseId {courseId} не найдено.");
      }
    }

    /// <summary>
    /// Обновляет информацию о задании в системе.
    /// </summary>
    /// <param name="assignment">Модель задания</param>
    public void UpdateAssignment(Assignment assignment)
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      string query = "UPDATE Assignments " +
                     "SET CourseId = @CourseId, Title = @Title, Description = @Description, DueDate = @DueDate " +
                     "WHERE AssignmentId = @AssignmentId";
      using var command = new SQLiteCommand(query, connection);
      command.Parameters.AddWithValue("@CourseId", assignment.CourseId);
      command.Parameters.AddWithValue("@Title", assignment.Title);
      command.Parameters.AddWithValue("@Description", assignment.Description);
      command.Parameters.AddWithValue("@DueDate", assignment.DueDate?.ToString("yyyy-MM-dd HH:mm:ss"));
      command.Parameters.AddWithValue("@AssignmentId", assignment.AssignmentId);

      int rowsAffected = command.ExecuteNonQuery();
      if (rowsAffected > 0)
      {
        Logger.LogInfo($"Информация о задании {assignment.Title} обновлена.");
      }
      else
      {
        Logger.LogError($"Задание с AssignmentId {assignment.AssignmentId} не найдено для обновления.");
      }
    }

    /// <summary>
    /// Возвращает все задания для указанного курса.
    /// </summary>
    /// <param name="courseId">Идентификатор курса</param>
    /// <returns>Список заданий</returns>
    public List<Assignment> GetAssignmentsByCourse(int courseId)
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      string query = "SELECT AssignmentId, CourseId, Title, Description, DueDate FROM Assignments WHERE CourseId = @CourseId";
      using var command = new SQLiteCommand(query, connection);
      command.Parameters.AddWithValue("@CourseId", courseId);

      using var reader = command.ExecuteReader();
      var assignments = new List<Assignment>();

      while (reader.Read())
      {
        var assignment = new Assignment
        {
          AssignmentId = Convert.ToInt32(reader["AssignmentId"]),
          CourseId = Convert.ToInt32(reader["CourseId"]),
          Title = reader["Title"].ToString(),
          Description = reader["Description"].ToString(),
          DueDate = reader["DueDate"] != DBNull.Value
                    ? DateTime.Parse(reader["DueDate"].ToString())
                    : (DateTime?)null
        };

        assignments.Add(assignment);
      }

      Logger.LogInfo($"Получено {assignments.Count} заданий для курса с CourseId {courseId}.");
      return assignments;
    }

    /// <summary>
    /// Возвращает домашнюю работу по уникальному идентификатору.
    /// </summary>
    /// <param name="courseId">Идентификатор курса.</param>
    /// <param name="homeId">Идентификатор задания.</param>
    /// <returns>Модель задания.</returns>
    public Assignment? GetAssignmentById(int courseId, int homeId)
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      string query = "SELECT AssignmentId, CourseId, Title, Description, DueDate FROM Assignments WHERE CourseId = @CourseId AND AssignmentId = @HomeId";
      using var command = new SQLiteCommand(query, connection);
      command.Parameters.AddWithValue("@CourseId", courseId);
      command.Parameters.AddWithValue("@HomeId", homeId);

      using var reader = command.ExecuteReader();

      if (reader.Read())
      {
        return new Assignment
        {
          AssignmentId = Convert.ToInt32(reader["AssignmentId"]),
          CourseId = Convert.ToInt32(reader["CourseId"]),
          Title = reader["Title"].ToString(),
          Description = reader["Description"].ToString(),
          DueDate = reader["DueDate"] != DBNull.Value ? DateTime.Parse(reader["DueDate"].ToString()) : (DateTime?)null
        };
      }

      return null;
    }
  }
}
