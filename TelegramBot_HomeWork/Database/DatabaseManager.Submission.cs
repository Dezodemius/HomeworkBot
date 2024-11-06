using DataContracts;
using DataContracts.Models;
using System.Data.SQLite;

namespace Database
{
  public partial class DatabaseManager
  {

    /// <summary>
    /// Создает новую запись Submission.
    /// </summary>
    /// <param name="submission">Модель отправки домашнего задания</param>
    public void CreateSubmission(Submission submission)
    {
      try
      {
        using var connection = new SQLiteConnection(_connectionString);
        connection.Open();

        string query = @"INSERT INTO Submissions 
                        (AssignmentId, StudentId, CourseId, GithubLink, SubmissionDate, Status, TeacherComment) 
                         VALUES 
                        (@AssignmentId, @StudentId, @CourseId, @GithubLink, @SubmissionDate, @Status, @TeacherComment)";
        using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@AssignmentId", submission.AssignmentId);
        command.Parameters.AddWithValue("@StudentId", submission.StudentId);
        command.Parameters.AddWithValue("@CourseId", submission.CourseId);
        command.Parameters.AddWithValue("@GithubLink", submission.GithubLink);
        command.Parameters.AddWithValue("@SubmissionDate", submission.SubmissionDate);
        command.Parameters.AddWithValue("@Status", submission.Status.ToString());
        command.Parameters.AddWithValue("@TeacherComment", submission.TeacherComment);


        command.ExecuteNonQuery();
        Logger.LogInfo("Новая отправка домашнего задания добавлена в базу данных.");
      }
      catch (Exception ex)
      {
        Logger.LogError(ex.ToString());
      }
    }

    /// <summary>
    /// Удаляет запись Submission по её уникальному идентификатору.
    /// </summary>
    /// <param name="id">Уникальный идентификатор Submission</param>
    public void DeleteSubmission(int id)
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      string query = "DELETE FROM Submissions WHERE SubmissionId = @Id";
      using var command = new SQLiteCommand(query, connection);
      command.Parameters.AddWithValue("@Id", id);

      int rowsAffected = command.ExecuteNonQuery();
      if (rowsAffected > 0)
      {
        Logger.LogInfo($"Запись Submission с Id {id} успешно удалена.");
      }
      else
      {
        Logger.LogError($"Запись Submission с Id {id} не найдена.");
      }
    }

    /// <summary>
    /// Обновляет запись Submission по её уникальному идентификатору.
    /// </summary>
    /// <param name="id">Уникальный идентификатор Submission</param>
    /// <param name="updatedSubmission">Обновленная модель Submission</param>
    public void UpdateSubmission(int id, Submission updatedSubmission)
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      using var transaction = connection.BeginTransaction();
      try
      {
        string query = @"UPDATE Submissions SET 
                          AssignmentId = @AssignmentId, 
                          StudentId = @StudentId, 
                          CourseId = @CourseId, 
                          GithubLink = @GithubLink, 
                          SubmissionDate = @SubmissionDate, 
                          Status = @Status, 
                          TeacherComment = @TeacherComment 
                        WHERE SubmissionId = @Id";

        using var command = new SQLiteCommand(query, connection);
        command.Parameters.AddWithValue("@AssignmentId", updatedSubmission.AssignmentId);
        command.Parameters.AddWithValue("@StudentId", updatedSubmission.StudentId);
        command.Parameters.AddWithValue("@CourseId", updatedSubmission.CourseId);
        command.Parameters.AddWithValue("@GithubLink", updatedSubmission.GithubLink);
        command.Parameters.AddWithValue("@SubmissionDate", updatedSubmission.SubmissionDate);
        command.Parameters.AddWithValue("@Status", updatedSubmission.Status.ToString());
        command.Parameters.AddWithValue("@TeacherComment", updatedSubmission.TeacherComment);
        command.Parameters.AddWithValue("@Id", id);

        int rowsAffected = command.ExecuteNonQuery();
        if (rowsAffected > 0)
        {
          Logger.LogInfo($"Запись Submission с Id {id} успешно обновлена.");
        }
        else
        {
          Logger.LogError($"Запись Submission с Id {id} не найдена для обновления.");
        }

        transaction.Commit();
      }
      catch
      {
        transaction.Rollback();
        throw;
      }
    }

    /// <summary>
    /// Возвращает все записи Submission для студента по его Telegram Chat ID.
    /// </summary>
    public List<Submission> GetSubmissionsByStudentId(long studentId)
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      string query = @"SELECT SubmissionId, AssignmentId, StudentId, CourseId, GithubLink, SubmissionDate, Status, TeacherComment 
                       FROM Submissions WHERE StudentId = @StudentId";
      using var command = new SQLiteCommand(query, connection);
      command.Parameters.AddWithValue("@StudentId", studentId);

      return ReadSubmissions(command);
    }

    /// <summary>
    /// Возвращает все записи Submission для конкретного студента и курса.
    /// </summary>
    public List<Submission> GetSubmissionsByCourse(long studentId, int courseId)
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      string query = @"SELECT SubmissionId, AssignmentId, StudentId, CourseId, GithubLink, SubmissionDate, Status, TeacherComment 
                       FROM Submissions WHERE StudentId = @StudentId AND CourseId = @CourseId";
      using var command = new SQLiteCommand(query, connection);
      command.Parameters.AddWithValue("@StudentId", studentId);
      command.Parameters.AddWithValue("@CourseId", courseId);

      return ReadSubmissions(command);
    }

    /// <summary>
    /// Возвращает запись Submission для конкретного задания и студента.
    /// </summary>
    public Submission? GetSubmissionForAssignment(long studentId, int assignmentId)
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      string query = @"SELECT SubmissionId, AssignmentId, StudentId, CourseId, GithubLink, SubmissionDate, Status, TeacherComment 
                       FROM Submissions WHERE StudentId = @StudentId AND AssignmentId = @AssignmentId";
      using var command = new SQLiteCommand(query, connection);
      command.Parameters.AddWithValue("@StudentId", studentId);
      command.Parameters.AddWithValue("@AssignmentId", assignmentId);

      var submissions = ReadSubmissions(command);
      return submissions.FirstOrDefault();
    }

    /// <summary>
    /// Возвращает список записей Submission для указанного задания и курса.
    /// </summary>
    public List<Submission> GetSubmissionsByCourseAndAssignment(int assignmentId, int courseId)
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      string query = @"SELECT SubmissionId, AssignmentId, StudentId, CourseId, GithubLink, SubmissionDate, Status, TeacherComment 
                       FROM Submissions WHERE AssignmentId = @AssignmentId AND CourseId = @CourseId";
      using var command = new SQLiteCommand(query, connection);
      command.Parameters.AddWithValue("@AssignmentId", assignmentId);
      command.Parameters.AddWithValue("@CourseId", courseId);

      return ReadSubmissions(command);
    }

    /// <summary>
    /// Метод для чтения данных из базы и преобразования их в список Submission.
    /// </summary>
    private List<Submission> ReadSubmissions(SQLiteCommand command)
    {
      var submissions = new List<Submission>();
      using var reader = command.ExecuteReader();

      while (reader.Read())
      {
        var submission = new Submission
        {
          SubmissionId = Convert.ToInt32(reader["SubmissionId"]),
          AssignmentId = Convert.ToInt32(reader["AssignmentId"]),
          StudentId = Convert.ToInt32(reader["StudentId"]),
          CourseId = Convert.ToInt32(reader["CourseId"]),
          GithubLink = reader["GithubLink"].ToString(),
          SubmissionDate = Convert.ToDateTime(reader["SubmissionDate"]),
          Status = Enum.Parse<Submission.StatusWork>(reader["Status"].ToString()),
          TeacherComment = reader["TeacherComment"].ToString()
        };

        submissions.Add(submission);
      }

      return submissions;
    }
  }
}
