using DataContracts;
using System.Data.SQLite;
using Telegram.Bot.Types;
using static DataContracts.IParticipant;
using static DataContracts.Models.StudentHomeWorkModel;

namespace SqlHelper
{
  public class SqliteDatabase
  {
    /// <summary>
    /// Строка подключение к БД.
    /// </summary>
    public string ConnectionString { get; set; }

    /// <summary>
    /// Имя таблицы.
    /// </summary>
    public string TableName { get; set; }

    /// <summary>
    /// Роль пользователя.
    /// </summary>
    public string UserRole { get; set; }

    /// <summary>
    /// Имя таблицы для участников.
    /// </summary>
    public string ParticipantsTableName { get; set; }

    /// <summary>
    /// Имя таблицы для домашних работ.
    /// </summary>
    public string HomeworkTableName { get; set; }

    /// <summary>
    /// Проверяет существование таблицы в базе данных.
    /// </summary>
    /// <param name="tableName">Имя таблицы для проверки.</param>
    /// <returns>True, если таблица существует, иначе False.</returns>
    public async Task<bool> TableExistsAsync(string tableName)
    {
      using (var connection = new SQLiteConnection(ConnectionString))
      {
        await connection.OpenAsync();

        using (var command = new SQLiteCommand($"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}';", connection))
        {
          var result = await command.ExecuteScalarAsync();
          return result != null;
        }
      }
    }

    /// <summary>
    /// Проверяет существование таблицы участников.
    /// </summary>
    /// <returns>True, если таблица существует, иначе False.</returns>
    public async Task<bool> ParticipantsTableExistsAsync()
    {
      return await TableExistsAsync(ParticipantsTableName);
    }

    /// <summary>
    /// Создает таблицу участников, если она не существует.
    /// </summary>
    public async Task CreateParticipantsTableAsync()
    {
      using (var connection = new SQLiteConnection(ConnectionString))
      {
        await connection.OpenAsync();

        string createTableQuery = $@"
                CREATE TABLE IF NOT EXISTS {ParticipantsTableName} (
                    Id TEXT PRIMARY KEY,
                    FullName TEXT NOT NULL,
                    Role TEXT NOT NULL
                );";

        using (var command = new SQLiteCommand(createTableQuery, connection))
        {
          await command.ExecuteNonQueryAsync();
        }
      }
    }

    /// <summary>
    /// Возвращает роль участника.
    /// </summary>
    /// <param name="chatId">id чата</param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public async Task<UserRole> GetUserRoleAsync(string chatId)
    {
      using (var connection = new SQLiteConnection(ConnectionString))
      {
        await connection.OpenAsync();

        string query = $"SELECT Role FROM {ParticipantsTableName} WHERE Id = @UserId;";

        using (var command = new SQLiteCommand(query, connection))
        {
          command.Parameters.AddWithValue("@UserId", chatId);

          var result = await command.ExecuteScalarAsync();

          if (result != null)
          {
            return Enum.TryParse<UserRole>(result.ToString(), out var role) ? role : IParticipant.UserRole.Student;
          }
          else
          {
            return IParticipant.UserRole.None;
          }
        }
      }
    }

    /// <summary>
    /// Проверяет существование таблицы домашних работ.
    /// </summary>
    /// <returns>True, если таблица существует, иначе False.</returns>
    public async Task<bool> HomeworkTableExistsAsync()
    {
      return await TableExistsAsync(HomeworkTableName);
    }

    /// <summary>
    /// Создает таблицу домашних работ, если она не существует.
    /// </summary>
    public async Task CreateHomeworkTableAsync()
    {
      using (var connection = new SQLiteConnection(ConnectionString))
      {
        await connection.OpenAsync();

        string createTableQuery = $@"
            CREATE TABLE IF NOT EXISTS {HomeworkTableName} (
                UserId TEXT NOT NULL,
                FullName TEXT NOT NULL,
                HomeworkNumber TEXT NOT NULL,
                Git TEXT NOT NULL,
                StatusWork TEXT NOT NULL
            );";

        using (var command = new SQLiteCommand(createTableQuery, connection))
        {
          await command.ExecuteNonQueryAsync();
        }
      }
    }

    /// <summary>
    /// Добавляет новую запись в таблицу домашних работ.
    /// </summary>
    public async Task<bool> AddHomeworkRecordAsync(IParticipant student, string homeworkNumber, string git)
    {
      try
      {
        using (var connection = new SQLiteConnection(ConnectionString))
        {
          await connection.OpenAsync();

          string insertQuery = $@"
                    INSERT INTO {HomeworkTableName} (UserId, FullName, HomeworkNumber, Git, StatusWork)
                    VALUES (@UserId, @FullName, @HomeworkNumber, @Git, @StatusWork);";

          using (var command = new SQLiteCommand(insertQuery, connection))
          {
            command.Parameters.AddWithValue("@UserId", student.Id);
            command.Parameters.AddWithValue("@FullName", student.FullName);
            command.Parameters.AddWithValue("@HomeworkNumber", homeworkNumber);
            command.Parameters.AddWithValue("@Git", git);
            command.Parameters.AddWithValue("@StatusWork", IParticipant.StatusWork.NotReviewed);

            await command.ExecuteNonQueryAsync();
          }
        }
        return true;
      }
      catch (Exception ex) 
      { 
        Console.WriteLine(ex.ToString()); 
        return false;
      }
    }

    /// <summary>
    /// Получает значение столбца StatusWork из таблицы HomeWorkTable по идентификатору UserId и номеру домашнего задания HomeworkNumber.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя (UserId).</param>
    /// <param name="homeworkNumber">Номер домашнего задания (HomeworkNumber).</param>
    /// <returns>Статус домашнего задания (StatusWork) или null, если запись не найдена.</returns>
    public async Task<StatusWork?> GetStatusWorkAsync(string userId, string homeworkNumber)
    {
      using (var connection = new SQLiteConnection(ConnectionString))
      {
        await connection.OpenAsync();

        string query = $"SELECT StatusWork FROM {HomeworkTableName} WHERE UserId = @UserId AND HomeworkNumber = @HomeworkNumber;";

        using (var command = new SQLiteCommand(query, connection))
        {
          command.Parameters.AddWithValue("@UserId", userId);
          command.Parameters.AddWithValue("@HomeworkNumber", homeworkNumber);

          var result = await command.ExecuteScalarAsync();

          if (result != null && Enum.TryParse(result.ToString(), out StatusWork status))
          {
            return status;
          }
          else
          {
            return null;
          }
        }
      }
    }

    /// <summary>
    /// Получает список всех домашних заданий и их статусов для указанного пользователя.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя (UserId).</param>
    /// <returns>Список кортежей, содержащих номер домашнего задания (HomeworkNumber) и статус домашнего задания на русском языке (StatusWork).</returns>
    public async Task<List<Tuple<string, string>>> GetStatusWorkesAsync(string userId)
    {
      var statusTranslations = new Dictionary<StatusWork, string>
    {
        { StatusWork.Unfulfilled, "Не сдано" },
        { StatusWork.Unchecked, "Не проверено" },
        { StatusWork.NeedsRevision, "Требует доработки" },
        { StatusWork.Checked, "Принято" }
    };

      using (var connection = new SQLiteConnection(ConnectionString))
      {
        await connection.OpenAsync();

        string query = $"SELECT HomeworkNumber, StatusWork FROM {HomeworkTableName} WHERE UserId = @UserId;";

        using (var command = new SQLiteCommand(query, connection))
        {
          command.Parameters.AddWithValue("@UserId", userId);

          var StatusWorkes = new List<Tuple<string, string>>();
          using (var reader = await command.ExecuteReaderAsync())
          {
            while (await reader.ReadAsync())
            {
              string homeworkNumber = reader["HomeworkNumber"].ToString();
              if (Enum.TryParse(reader["StatusWork"].ToString(), out StatusWork StatusWork))
              {
                string statusInRussian = statusTranslations[StatusWork];
                StatusWorkes.Add(new Tuple<string, string>(homeworkNumber, statusInRussian));
              }
            }
          }

          return StatusWorkes;
        }
      }
    }


    /// <summary>
    /// Получает все Id из таблицы ParticipantsTable для пользователей со статусом UserRole.Teacher.
    /// </summary>
    /// <returns>Список всех Id пользователей-учителей.</returns>
    public async Task<List<string>> GetAllTeacherIdsAsync()
    {
      List<string> teacherIds = new List<string>();

      using (var connection = new SQLiteConnection(ConnectionString))
      {
        await connection.OpenAsync();

        string query = $"SELECT Id FROM {ParticipantsTableName} WHERE Role = @Role;";

        using (var command = new SQLiteCommand(query, connection))
        {
          command.Parameters.AddWithValue("@Role", IParticipant.UserRole.Teacher.ToString());

          using (var reader = await command.ExecuteReaderAsync())
          {
            while (await reader.ReadAsync())
            {
              string id = reader.GetString(0);
              teacherIds.Add(id);
            }
          }
        }
      }
      return teacherIds;
    }

    public SqliteDatabase(string connectionStr, string tableName, string userRole)
    {
      ConnectionString = connectionStr;
      TableName = tableName;
      UserRole = userRole;
      HomeworkTableName = "HomeWorkTable";
      ParticipantsTableName = "ParticipantsTable";
    }

  }
}
