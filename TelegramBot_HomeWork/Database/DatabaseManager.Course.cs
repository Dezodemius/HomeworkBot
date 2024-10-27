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
    /// Создает новый курс.
    /// </summary>
    /// <param name="course">Модель курса</param>
    public void CreateCourse(Course course)
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      string query = "INSERT INTO Courses (CourseName, TeacherId) VALUES (@CourseName, @TeacherId)";
      using var command = new SQLiteCommand(query, connection);
      command.Parameters.AddWithValue("@CourseName", course.CourseName);
      command.Parameters.AddWithValue("@TeacherId", course.TeacherId.HasValue ? (object)course.TeacherId.Value : DBNull.Value);

      command.ExecuteNonQuery();
      Logger.LogInfo("Новый курс добавлен в базу данных.");
    }

    /// <summary>
    /// Удаляет курс по его уникальному идентификатору.
    /// </summary>
    /// <param name="id">Уникальный идентификатор курса</param>
    public void DeleteCourse(int id)
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      string query = "DELETE FROM Courses WHERE CourseId = @Id";
      using var command = new SQLiteCommand(query, connection);
      command.Parameters.AddWithValue("@Id", id);

      int rowsAffected = command.ExecuteNonQuery();
      if (rowsAffected > 0)
      {
        Logger.LogInfo($"Курс с Id {id} успешно удален.");
      }
      else
      {
        Logger.LogError($"Курс с Id {id} не найден.");
      }
    }

    /// <summary>
    /// Обновляет курс по его уникальному идентификатору.
    /// </summary>
    /// <param name="id">Уникальный идентификатор курса</param>
    /// <param name="updatedCourse">Обновленная модель курса</param>
    public void UpdateCourse(int id, Course updatedCourse)
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      string query = @"UPDATE Courses SET 
                        CourseName = @CourseName, 
                        TeacherId = @TeacherId 
                      WHERE CourseId = @Id";
      using var command = new SQLiteCommand(query, connection);
      command.Parameters.AddWithValue("@CourseName", updatedCourse.CourseName);
      command.Parameters.AddWithValue("@TeacherId", updatedCourse.TeacherId.HasValue ? (object)updatedCourse.TeacherId.Value : DBNull.Value);
      command.Parameters.AddWithValue("@Id", id);

      int rowsAffected = command.ExecuteNonQuery();
      if (rowsAffected > 0)
      {
        Logger.LogInfo($"Курс с Id {id} успешно обновлен.");
      }
      else
      {
        Logger.LogError($"Курс с Id {id} не найден для обновления.");
      }
    }

    /// <summary>
    /// Возвращает все курсы из базы данных.
    /// </summary>
    /// <returns>Список всех курсов</returns>
    public List<Course> GetAllCourses()
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      string query = "SELECT CourseId, CourseName, TeacherId FROM Courses";
      using var command = new SQLiteCommand(query, connection);

      using var reader = command.ExecuteReader();
      var courses = new List<Course>();

      while (reader.Read())
      {
        var course = new Course
        {
          CourseId = Convert.ToInt32(reader["CourseId"]),
          CourseName = reader["CourseName"].ToString(),
          TeacherId = reader["TeacherId"] != DBNull.Value ? Convert.ToInt64(reader["TeacherId"]) : (long?)null
        };

        courses.Add(course);
      }

      Logger.LogInfo($"Получено {courses.Count} курсов из базы данных.");
      return courses;
    }

    /// <summary>
    /// Возвращает все курсы пользователя по его идентификатору.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя.</param>
    /// <returns>Список курсов пользователя.</returns>
    public List<Course> GetAllUserCoursesByUserId(int userId)
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      string query = @"
                SELECT c.CourseId, c.CourseName, c.TeacherId
                FROM Courses c
                JOIN UserCourses uc ON c.CourseId = uc.CourseId
                WHERE uc.UserId = @UserId";
      using var command = new SQLiteCommand(query, connection);
      command.Parameters.AddWithValue("@UserId", userId);

      using var reader = command.ExecuteReader();
      var courses = new List<Course>();

      while (reader.Read())
      {
        var course = new Course
        {
          CourseId = Convert.ToInt32(reader["CourseId"]),
          CourseName = reader["CourseName"].ToString(),
          TeacherId = reader["TeacherId"] != DBNull.Value ? Convert.ToInt64(reader["TeacherId"]) : (long?)null
        };

        courses.Add(course);
      }

      Logger.LogInfo($"Получено {courses.Count} курсов для пользователя с Id {userId}.");
      return courses;
    }

    /// <summary>
    /// Возвращает название курса по его уникальному идентификатору.
    /// </summary>
    /// <param name="courseId">Идентификатор курса.</param>
    /// <returns>Название курса.</returns>
    public string GetCourseNameById(int courseId)
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      string query = "SELECT CourseName FROM Courses WHERE CourseId = @CourseId";
      using var command = new SQLiteCommand(query, connection);
      command.Parameters.AddWithValue("@CourseId", courseId);

      return command.ExecuteScalar() as string;
    }
  }
}
