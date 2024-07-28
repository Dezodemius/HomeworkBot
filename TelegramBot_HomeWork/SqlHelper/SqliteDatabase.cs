using DataContracts;
using System.Data.SQLite;
using Telegram.Bot.Types;
using static DataContracts.IParticipant;

namespace SqlHelper
{
  public class SqliteDatabase
  {

    #region Свойства.

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

    #endregion

    #region Методы.

    #region Работа с таблицами.

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
                HomeworkStatus TEXT NOT NULL
            );";

        using (var command = new SQLiteCommand(createTableQuery, connection))
        {
          await command.ExecuteNonQueryAsync();
        }
      }
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
                    INSERT INTO {HomeworkTableName} (UserId, FullName, HomeworkNumber, Git, HomeworkStatus)
                    VALUES (@UserId, @FullName, @HomeworkNumber, @Git, @HomeworkStatus);";

          using (var command = new SQLiteCommand(insertQuery, connection))
          {
            command.Parameters.AddWithValue("@UserId", student.Id);
            command.Parameters.AddWithValue("@FullName", student.FullName);
            command.Parameters.AddWithValue("@HomeworkNumber", homeworkNumber);
            command.Parameters.AddWithValue("@Git", git);
            command.Parameters.AddWithValue("@HomeworkStatus", IParticipant.HomeworkStatus.NotReviewed);

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

    #endregion

    #region Общее.

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

    #endregion

    #region Студенты.

    /// <summary>
    /// Получает список всех студентов.
    /// </summary>
    /// <returns>Список студентов, где каждый студент представлен кортежем, содержащим идентификатор студента (UserId) и его полное имя (FullName).</returns>
    public async Task<List<Tuple<string, string>>> GetAllStudentsAsync()
    {
      try
      {
        using (var connection = new SQLiteConnection(ConnectionString))
        {
          await connection.OpenAsync();

          // Выбираем только тех участников, у которых роль "Student"
          string query = $"SELECT Id, FullName FROM {ParticipantsTableName} WHERE Role = @Role;";

          using (var command = new SQLiteCommand(query, connection))
          {
            // Добавляем параметр для роли "Student"
            command.Parameters.AddWithValue("@Role", DataContracts.IParticipant.UserRole.Student.ToString());

            var students = new List<Tuple<string, string>>();
            using (var reader = await command.ExecuteReaderAsync())
            {
              while (await reader.ReadAsync())
              {
                string userId = reader["Id"].ToString();
                string fullName = reader["FullName"].ToString();
                students.Add(new Tuple<string, string>(userId, fullName));
              }
            }

            return students;
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
        return null;
      }
    }

    /// <summary>
    /// Получает идентификатор студента, чье полное имя точно соответствует указанному.
    /// </summary>
    /// <param name="fullName">Полное имя студента для поиска.</param>
    /// <returns>Идентификатор студента или null, если студент не найден.</returns>
    public async Task<string> SearchStudentIdByFullNameExactAsync(string fullName)
    {
      try
      {
        using (var connection = new SQLiteConnection(ConnectionString))
        {
          await connection.OpenAsync();

          // Используем параметризованный запрос для предотвращения SQL-инъекций
          string query = $"SELECT UserId FROM {HomeworkTableName} WHERE FullName = @FullName LIMIT 1;";

          using (var command = new SQLiteCommand(query, connection))
          {
            // Устанавливаем параметр для точного совпадения
            command.Parameters.AddWithValue("@FullName", fullName);

            using (var reader = await command.ExecuteReaderAsync())
            {
              if (await reader.ReadAsync())
              {
                return reader["UserId"].ToString();
              }
            }

            // Возвращаем null, если студент не найден
            return null;
          }
        }
      }
      catch
      {
        return null;
      }
    }

    /// <summary>
    /// Получает идентификатор студента, чье полное имя точно соответствует указанному.
    /// </summary>
    /// <param name="id">id студента для поиска.</param>
    /// <returns>Идентификатор студента или null, если студент не найден.</returns>
    public async Task<string> SearchStudentFullNameByIdExactAsync(string id)
    {
      try
      {
        using (var connection = new SQLiteConnection(ConnectionString))
        {
          await connection.OpenAsync();

          // Используем параметризованный запрос для предотвращения SQL-инъекций
          string query = $"SELECT FullName FROM {HomeworkTableName} WHERE UserId = @UserId LIMIT 1;";

          using (var command = new SQLiteCommand(query, connection))
          {
            // Устанавливаем параметр для точного совпадения
            command.Parameters.AddWithValue("@UserId", id);

            using (var reader = await command.ExecuteReaderAsync())
            {
              if (await reader.ReadAsync())
              {
                return reader["FullName"].ToString();
              }
            }

            // Возвращаем null, если студент не найден
            return null;
          }
        }
      }
      catch
      {
        return null;
      }
    }

    /// <summary>
    /// Получает список всех домашних заданий и их статусов для указанного пользователя.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя (UserId).</param>
    /// <returns>Список кортежей, содержащих номер домашнего задания (HomeworkNumber) и статус домашнего задания на русском языке (HomeworkStatus).</returns>
    public async Task<List<Tuple<string, string>>> GetHomeworkStatusesAsync(string userId)
    {
      var statusTranslations = new Dictionary<HomeworkStatus, string>
    {
        { HomeworkStatus.NotSubmitted, "Не сдано" },
        { HomeworkStatus.NotReviewed, "Не проверено" },
        { HomeworkStatus.RequiresRevision, "Требует доработки" },
        { HomeworkStatus.Accepted, "Принято" }
    };

      using (var connection = new SQLiteConnection(ConnectionString))
      {
        await connection.OpenAsync();

        string query = $"SELECT HomeworkNumber, HomeworkStatus FROM {HomeworkTableName} WHERE UserId = @UserId;";

        using (var command = new SQLiteCommand(query, connection))
        {
          command.Parameters.AddWithValue("@UserId", userId);

          var homeworkStatuses = new List<Tuple<string, string>>();
          using (var reader = await command.ExecuteReaderAsync())
          {
            while (await reader.ReadAsync())
            {
              string homeworkNumber = reader["HomeworkNumber"].ToString();
              if (Enum.TryParse(reader["HomeworkStatus"].ToString(), out HomeworkStatus homeworkStatus))
              {
                string statusInRussian = statusTranslations[homeworkStatus];
                homeworkStatuses.Add(new Tuple<string, string>(homeworkNumber, statusInRussian));
              }
            }
          }

          return homeworkStatuses;
        }
      }
    }

    /// <summary>
    /// Получает ссылку на домашнее задание по идентификатору студента и номеру домашнего задания.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя (UserId).</param>
    /// <param name="homeworkNumber">Номер домашнего задания (HomeworkNumber).</param>
    /// <returns>Ссылка на домашнее задание (Git) или null, если запись не найдена.</returns>
    public async Task<string> GetHomeworkLinkAsync(string userId, string homeworkNumber)
    {
      try
      {
        using (var connection = new SQLiteConnection(ConnectionString))
        {
          await connection.OpenAsync();

          string query = $"SELECT Git FROM {HomeworkTableName} WHERE UserId = @UserId AND HomeworkNumber = @HomeworkNumber;";

          using (var command = new SQLiteCommand(query, connection))
          {
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@HomeworkNumber", homeworkNumber);

            var result = await command.ExecuteScalarAsync();

            return result?.ToString(); // Преобразуем результат в строку и возвращаем
          }
        }
      }
      catch (Exception ex)
      {
        // Логируем исключение и возвращаем null в случае ошибки
        Console.WriteLine($"Ошибка при получении ссылки на ДЗ: {ex.Message}");
        return null;
      }
    }

    /// <summary>
    /// Получает значение статуса дз по идентификатору студента и номеру домашнего задания HomeworkNumber.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя (UserId).</param>
    /// <param name="homeworkNumber">Номер домашнего задания (HomeworkNumber).</param>
    /// <returns>Статус домашнего задания (HomeworkStatus) или null, если запись не найдена.</returns>
    public async Task<HomeworkStatus?> GetHomeworkStatusAsync(string userId, string homeworkNumber)
    {
      using (var connection = new SQLiteConnection(ConnectionString))
      {
        await connection.OpenAsync();

        string query = $"SELECT HomeworkStatus FROM {HomeworkTableName} WHERE UserId = @UserId AND HomeworkNumber = @HomeworkNumber;";

        using (var command = new SQLiteCommand(query, connection))
        {
          command.Parameters.AddWithValue("@UserId", userId);
          command.Parameters.AddWithValue("@HomeworkNumber", homeworkNumber);

          var result = await command.ExecuteScalarAsync();

          if (result != null && Enum.TryParse(result.ToString(), out HomeworkStatus status))
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
    /// Получает список всех домашних заданий и их статусов с ссылками для указанного пользователя.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя (UserId).</param>
    /// <returns>Список кортежей, содержащих номер домашнего задания (HomeworkNumber), статус домашнего задания на русском языке (HomeworkStatus) и ссылку на домашнее задание (Git).</returns>
    public async Task<List<Tuple<string, string, string>>> GetHomeworkStatusesWithLinksAsync(string userId)
    {
      var statusTranslations = new Dictionary<HomeworkStatus, string>
            {
                { HomeworkStatus.NotSubmitted, "Не сдано" },
                { HomeworkStatus.NotReviewed, "Не проверено" },
                { HomeworkStatus.RequiresRevision, "Требует доработки" },
                { HomeworkStatus.Accepted, "Принято" }
            };

      using (var connection = new SQLiteConnection(ConnectionString))
      {
        await connection.OpenAsync();

        string query = $"SELECT HomeworkNumber, HomeworkStatus, Git FROM {HomeworkTableName} WHERE UserId = @UserId;";

        using (var command = new SQLiteCommand(query, connection))
        {
          command.Parameters.AddWithValue("@UserId", userId);

          var homeworkStatuses = new List<Tuple<string, string, string>>();
          using (var reader = await command.ExecuteReaderAsync())
          {
            while (await reader.ReadAsync())
            {
              string homeworkNumber = reader["HomeworkNumber"].ToString();
              string gitLink = reader["Git"].ToString();

              if (Enum.TryParse(reader["HomeworkStatus"].ToString(), out HomeworkStatus homeworkStatus))
              {
                string statusInRussian = statusTranslations[homeworkStatus];
                homeworkStatuses.Add(new Tuple<string, string, string>(homeworkNumber, statusInRussian, gitLink));
              }
            }
          }

          return homeworkStatuses;
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

    /// <summary>
    /// Обновляет статус домашнего задания для указанного пользователя и номера домашнего задания.
    /// </summary>
    /// <param name="userId">Идентификатор пользователя (UserId).</param>
    /// <param name="homeworkNumber">Номер домашнего задания (HomeworkNumber).</param>
    /// <param name="newStatus">Новый статус домашнего задания (HomeworkStatus).</param>
    /// <returns>Возвращает true, если обновление прошло успешно; иначе false.</returns>
    public async Task<bool> UpdateHomeworkStatusAsync(string userId, string homeworkNumber, HomeworkStatus newStatus)
    {
      try
      {
        using (var connection = new SQLiteConnection(ConnectionString))
        {
          await connection.OpenAsync();

          // Используем параметризованный запрос для предотвращения SQL-инъекций
          string query = $"UPDATE {HomeworkTableName} SET HomeworkStatus = @HomeworkStatus WHERE UserId = @UserId AND HomeworkNumber = @HomeworkNumber;";

          using (var command = new SQLiteCommand(query, connection))
          {
            // Устанавливаем параметры для запроса
            command.Parameters.AddWithValue("@UserId", userId);
            command.Parameters.AddWithValue("@HomeworkNumber", homeworkNumber);
            command.Parameters.AddWithValue("@HomeworkStatus", newStatus.ToString());

            int rowsAffected = await command.ExecuteNonQueryAsync();

            // Если обновление затронуло хотя бы одну строку, то возврат true
            return rowsAffected > 0;
          }
        }
      }
      catch (Exception ex)
      {
        // Логируем исключение и возвращаем false в случае ошибки
        Console.WriteLine($"Ошибка при обновлении статуса домашнего задания: {ex.Message}");
        return false;
      }
    }

    #endregion

    #endregion

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
