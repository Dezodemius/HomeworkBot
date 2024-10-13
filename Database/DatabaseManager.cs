using ModelInterfaceHub.Models;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;

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
    /// Возвращает список моделей выполненных пользователями домашних заданий.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public List<StudentHomeWorkModel> GetAllHomeWorks()
    {
      throw new NotImplementedException();
    }

    /// <summary>
    /// Добавляет запись о домашней работе в базу данных.
    /// </summary>
    public void CreateHomeWork(HomeWorkModel homeWorkModel)
    {
      string sqlExpression = $"INSERT INTO Assignments (Title, Description) VALUE (@Title, @Description)";
      SqlConnection connection = new SqlConnection(@_connectionString);
      SqlCommand command = new SqlCommand(sqlExpression, connection);
      command.Parameters.AddWithValue("@Title", homeWorkModel.Title);
      command.Parameters.AddWithValue("@Description", homeWorkModel.Description);

      using (connection)
      {
        connection.Open();
        command.ExecuteNonQuery();
      }
    }

    /// <summary>
    /// Добавляет запись выданного домашнего задания в таблицу Submissions.
    /// </summary>
    public void CreateStudentHomeWork(StudentHomeWorkModel studentHomeWorkModel)
    {
      string sqlExpression = $"INSERT INTO Submissions (AssignmentId, StudentId, GithubLink, Status, TeacherComment) VALUE (@AssignmentId, @StudentId, @GithubLink, @Status, @TeacherComment)";
      SqlConnection connection = new SqlConnection(@_connectionString);
      SqlCommand command = new SqlCommand(sqlExpression, connection);
      command.Parameters.AddWithValue("@AssignmentId", studentHomeWorkModel.IdHomeWork);
      command.Parameters.AddWithValue("@StudentId", studentHomeWorkModel.IdStudent);
      command.Parameters.AddWithValue("@GithubLink", studentHomeWorkModel.GithubLink);
      command.Parameters.AddWithValue("@Status", studentHomeWorkModel.Status);
      command.Parameters.AddWithValue("@TeacherComment", studentHomeWorkModel.TeacherComment);

      using (connection)
      {
        connection.Open();
        command.ExecuteNonQuery();
      }
    }
  }
}