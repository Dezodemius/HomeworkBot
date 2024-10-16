using ModelInterfaceHub.Models;
using System.Data.SqlClient;
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
    /// <param name="userId">Уникальный идентификатор чата ТГ.</param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public UserModel GetUserById(long userId)
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      using var command = connection.CreateCommand();
      command.CommandText = 
        $"SELECT UserId, TelegramChatId, FirstName, LastName, Email, Role " +
        $"FROM Users " +
        $"WHERE TelegramChatId = {userId}";

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
    public List<StudentHomeWorkModel> GetAllUserHomeWorks(long userId)
    {
      var homeWorks = new List<StudentHomeWorkModel>();

      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      using var command = connection.CreateCommand();
      command.CommandText = @"
     SELECT s.SubmissionId, a.AssignmentId, a.Title, a.Description, s.GithubLink, s.Status, s.TeacherComment
     FROM Assignments a
     JOIN Submissions s ON a.AssignmentId = s.AssignmentId
     WHERE s.StudentId = @userId";

      command.Parameters.AddWithValue("@userId", userId);

      using var reader = command.ExecuteReader();
      while (reader.Read())
      {
        int submissionId = reader.GetInt32(0);
        int assignmentId = reader.GetInt32(1);
        string title = reader.GetString(2);
        string description = reader.IsDBNull(3) ? null : reader.GetString(3);
        string githubLink = reader.GetString(4);
        StatusWork status = (StatusWork)Enum.Parse(typeof(StatusWork), reader.GetString(5));
        string teacherComment = reader.IsDBNull(6) ? null : reader.GetString(6);

        homeWorks.Add(new StudentHomeWorkModel(assignmentId, submissionId, (int)userId, githubLink, status, teacherComment));
      }

      return homeWorks;
    }

    /// <summary>
    /// Возвращает все домашние работы.
    /// </summary>
    /// <returns></returns>
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
        FROM Submissions";

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

    /// <summary>
    /// Получает список студентов, выполнивших определенное домашнее задание.
    /// </summary>
<<<<<<< Updated upstream
    /// <param name="title">Название домашнего задания.</param>
    /// <returns>Список студентов.</returns>
    public List<string> GetStudentName(string title)
=======
    /// <param name="homeworkId">Идентификатор домашнего задания.</param>
    /// <returns>Список студентов, выполнивших конкретное домашнее задание.</returns>
    /// <exception cref="SystemException">Исключение, которое возникает, если таких студентов нет.</exception>
    public List<string> GetStudentName(int homeworkId)
>>>>>>> Stashed changes
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
     WHERE 
         S.Status = 'Approved' 
<<<<<<< Updated upstream
         AND A.Title = @title;";

      command.Parameters.AddWithValue("@title", title);
=======
         AND S.AssignmentId = @homewokrId;";

      command.Parameters.AddWithValue("@homewokrId", homeworkId);
>>>>>>> Stashed changes

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

    /// <summary>
    /// Возвращает домашнее задание по его уникальному идентификатору.
    /// </summary>
    /// <param name="id">Уникальный идентификатор домашнего задания.</param>
    /// <returns>Объект HomeWorkModel с данными о домашнем задании.</returns>
    public HomeWorkModel GetAssignmentById(int id)
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      var command = connection.CreateCommand();
      command.CommandText = @"
            SELECT AssignmentId, Title, Description
            FROM Assignments
            WHERE AssignmentId = @id";

      command.Parameters.AddWithValue("@id", id);

      using var reader = command.ExecuteReader();

      if (reader.Read())
      {
        var assignmentId = reader.GetInt32(0);
        var title = reader.GetString(1);
        var description = reader.IsDBNull(2) ? null : reader.GetString(2); 

        return new HomeWorkModel(title, description) { Id = assignmentId };
      }
      else
      {
        throw new Exception($"Домашнее задание с ID {id} не найдено.");
      }
    }
    public void SeedTestData()
    {
      var connection = new SQLiteConnection(_connectionString);
      connection.Open();
      var clearTablesCommand = connection.CreateCommand();
      clearTablesCommand.CommandText = @"
        DELETE FROM Submissions;
        DELETE FROM Assignments;
        DELETE FROM Users;";
      clearTablesCommand.ExecuteNonQuery();

      var insertUserCommand = connection.CreateCommand();
      insertUserCommand.CommandText = @"
        INSERT OR IGNORE INTO Users (TelegramChatId, FirstName, LastName, Email, Role)
        VALUES (467266623, 'Daniil', 'Ivanov', 'daniil@example.com', 'Student');";
      insertUserCommand.ExecuteNonQuery();

      var insertAssignmentCommand = connection.CreateCommand();
      for (int i = 1; i <= 8; i++)
      {
        insertAssignmentCommand.CommandText = $@"
            INSERT INTO Assignments (Title, Description)
            VALUES ('Домашнее задание №{i}', 'Описание задания {i}');";
        insertAssignmentCommand.ExecuteNonQuery();
      }

      var insertSubmissionCommand = connection.CreateCommand();
      var statuses = new[] { "Checked", "Unchecked", "NeedsRevision", "Unfulfilled" };

      for (int i = 1; i <= 8; i++)
      {
        var status = statuses[(i - 1) / 2];
        insertSubmissionCommand.CommandText = $@"
            INSERT INTO Submissions (AssignmentId, StudentId, GithubLink, Status, TeacherComment)
            VALUES ({i}, 467266623, 'https://github.com/daniil/hw{i}', '{status}', 'Комментарий к заданию {i}');";
        insertSubmissionCommand.ExecuteNonQuery();
      }
    }
  }
}