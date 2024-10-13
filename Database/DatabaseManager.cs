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
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      using var command = connection.CreateCommand();
      command.CommandText = "SELECT UserId, TelegramChatId, FirstName, LastName, Email, Role FROM Users WHERE UserId = @userId";
      command.Parameters.AddWithValue("@userId", userId);

      using var reader = command.ExecuteReader();
      if (reader.Read())
      {
        return new UserModel(
            reader.GetInt64(1),
            reader.GetString(2),
            reader.GetString(3),
            reader.GetString(4),
            Enum.Parse<UserRole>(reader.GetString(5))
        );
      }

      return null; // Пользователь не найден
    }

    /// <summary>
    /// Возвращает все домашки пользователя по уникальному идентификатору.
    /// </summary>
    /// <param name="userId">Уникальный идентификатор.</param>
    /// <returns></returns>
    public List<HomeWorkModel> GetAllUserHomeWorks(long userId)
    {
      var homeWorks = new List<HomeWorkModel>();

      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      using var command = connection.CreateCommand();
      command.CommandText = @"
        SELECT a.AssignmentId, a.Title, a.Description
        FROM Assignments a
        JOIN Submissions s ON a.AssignmentId = s.AssignmentId
        WHERE s.StudentId = @userId";

      command.Parameters.AddWithValue("@userId", userId);

      using var reader = command.ExecuteReader();
      while (reader.Read())
      {
        int assignmentId = reader.GetInt32(0);
        string title = reader.GetString(1);
        string description = reader.IsDBNull(2) ? null : reader.GetString(2);

        homeWorks.Add(new HomeWorkModel(assignmentId, title, description));
      }

      return homeWorks;
    }

    /// <summary>
    /// Возвращает список моделей выполненных пользователями домашних заданий.
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public List<StudentHomeWorkModel> GetAllHomeWorks()
    {
      var homeWorks = new List<StudentHomeWorkModel>();

      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      var command = connection.CreateCommand();
      command.CommandText = @"
        SELECT 
            SubmissionId, 
            AssignmentId, 
            StudentId, 
            GithubLink, 
            Status, 
            TeacherComment 
        FROM Submissions
        WHERE Status NOT IN ('Unfulfilled', 'NeedsRevision');";

      using var reader = command.ExecuteReader();
      while (reader.Read())
      {
        var submissionId = reader.GetInt32(0);
        var assignmentId = reader.GetInt32(1);
        var studentId = reader.GetInt32(2);
        var githubLink = reader.GetString(3);
        var status = (StatusWork)Enum.Parse(typeof(StatusWork), reader.GetString(4));
        var teacherComment = reader.IsDBNull(5) ? null : reader.GetString(5);

        var homeWorkModel = new StudentHomeWorkModel(assignmentId, submissionId, studentId, githubLink, status, teacherComment);
        homeWorks.Add(homeWorkModel);
      }

      return homeWorks;
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