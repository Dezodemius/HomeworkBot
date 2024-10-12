using ModelInterfaceHub.Models;
using System.Data.SQLite;

namespace Database
{
  /// <summary>
  /// Класс для управления базой данных SQLite.
  /// </summary>
  public partial class DatabaseManager
  {
    /// <summary>
    /// Строка подключения к базе данных.
    /// </summary>
    protected internal readonly string _connectionString;

    /// <summary>
    /// Инициализирует новый экземпляр класса DatabaseManager.
    /// </summary>
    /// <param name="connectionString">Строка подключения к базе данных.</param>
    public DatabaseManager(string connectionString)
    {
      _connectionString = connectionString;
    }

    /// <summary>
    /// Возвращает модель пользователя по уникальному идентификатору.
    /// </summary>
    /// <param name="userId">Уникальный идентификатор.</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public UserModel GetUserById(long userId)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Возвращает модель пользователя по уникальному идентификатору.
    /// </summary>
    /// <param name="userId">Уникальный идентификатор.</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public List<HomeWorkModel> GetAllHomeWorks(long userId)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Возвращает список студентов, выполнивших конкретное домашнее задание.
    /// </summary>
    /// <param name="title">Название домашнего задания.</param>
    /// <returns>Список студентов.</returns>
    public List<string> GetStudentName(string title)
    {
      var connection = new SQLiteConnection(_connectionString);
      connection.Open();
      var command = connection.CreateCommand();
      command.CommandText = @"
     SELECT 
         U.FirstName, 
         U.LastName
     FROM 
         Users U
     JOIN 
         Submissions S ON U.UserId = S.StudentId
     JOIN 
         Assignments A ON S.AssignmentId = A.AssignmentId
     WHERE 
         S.Status = 'Approved' 
         AND A.Title = @title;";

      command.Parameters.AddWithValue("@title", title);

      var studentNames = new List<string>();

      using (var reader = command.ExecuteReader())
      {
        while (reader.Read())
        {
          var fullName = $"{reader.GetString(0)} {reader.GetString(1)}";
          studentNames.Add(fullName);
        }
      }

      if (studentNames.Capacity != 0)
      {
        foreach (var studentname in studentNames)
        Console.WriteLine(studentname);
        
        return studentNames;
      }
      else throw new SystemException();
    }

  }
}