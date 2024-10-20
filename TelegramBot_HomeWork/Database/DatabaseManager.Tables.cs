using DataContracts.Models;
using System.Data.SQLite;
using System.IO;
using System.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DataContracts; // Для использования Logger

namespace Database
{
  public partial class DatabaseManager
  {
    /// <summary>
    /// Проверяет существование базы данных и создает ее, если она не существует.
    /// </summary>
    public void EnsureDatabaseCreated()
    {
      var dbPath = _connectionString.Replace("Data Source=", "");
      if (!File.Exists(dbPath))
      {
        SQLiteConnection.CreateFile(dbPath);
      }
    }

    /// <summary>
    /// Проверяет существование всех необходимых таблиц и создает их, если они не существуют.
    /// </summary>
    public void EnsureTablesCreated()
    {
      using var connection = new SQLiteConnection(_connectionString);
      connection.Open();

      CreateTableFromModel<UserModel>(connection, "Users");
      CreateTableFromModel<Course>(connection, "Courses");
      CreateTableFromModel<Assignment>(connection, "Assignments");
      CreateTableFromModel<Submission>(connection, "Submissions");
      CreateTableFromModel<RegistrationRequest>(connection, "RegistrationRequests");
      CreateTableFromModel<UserCourse>(connection, "UserCourses");
      CreateTableFromModel<HomeWork>(connection, "Homeworks");
    }

    /// <summary>
    /// Создает таблицу на основе модели, если она не существует, и проверяет её структуру.
    /// </summary>
    private void CreateTableFromModel<T>(SQLiteConnection connection, string tableName)
    {
      try
      {
        var properties = typeof(T).GetProperties();
        var columns = properties.Select(p => $"{p.Name} {GetSQLiteType(p.PropertyType)}").ToList();
        columns.Insert(0, $"{tableName}Id INTEGER PRIMARY KEY AUTOINCREMENT");

        var commandText = $"CREATE TABLE IF NOT EXISTS {tableName} ({string.Join(", ", columns)})";
        using var command = new SQLiteCommand(commandText, connection);
        command.ExecuteNonQuery();

        Logger.LogInfo($"Таблица {tableName} создана или уже существует.");

        // Проверка структуры таблицы после её создания
        VerifyTableStructure<T>(connection, tableName);
        Logger.LogInfo($"Структура таблицы {tableName} успешно проверена.");
      }
      catch (Exception ex)
      {
        Logger.LogError($"Ошибка при создании или проверке таблицы {tableName}: {ex.Message}");
        throw;
      }
    }

    /// <summary>
    /// Возвращает тип данных SQLite для указанного типа C#.
    /// </summary>
    private string GetSQLiteType(Type type)
    {
      if (Nullable.GetUnderlyingType(type) != null)
      {
        type = Nullable.GetUnderlyingType(type);
      }

      if (type == typeof(int) || type == typeof(long))
      {
        return "INTEGER";
      }
      else if (type == typeof(string))
      {
        return "TEXT";
      }
      else if (type == typeof(DateTime))
      {
        return "DATETIME";
      }
      else if (type.IsEnum)
      { 
        return "TEXT"; 
      }

      throw new NotSupportedException($"Тип {type.Name} не поддерживается.");
    }

    /// <summary>
    /// Проверяет структуру таблицы в базе данных на основе модели.
    /// </summary>
    private void VerifyTableStructure<T>(SQLiteConnection connection, string tableName)
    {
      var expectedColumns = typeof(T).GetProperties()
                                     .ToDictionary(p => p.Name, p => GetSQLiteType(p.PropertyType));

      using var command = new SQLiteCommand($"PRAGMA table_info({tableName})", connection);
      using var reader = command.ExecuteReader();

      var actualColumns = new Dictionary<string, string>();
      while (reader.Read())
      {
        var columnName = reader["name"].ToString();
        var columnType = reader["type"].ToString();
        actualColumns[columnName] = columnType;
      }

      foreach (var expectedColumn in expectedColumns)
      {
        if (!actualColumns.ContainsKey(expectedColumn.Key) || actualColumns[expectedColumn.Key] != expectedColumn.Value)
        {
          throw new Exception($"Столбец {expectedColumn.Key} в таблице {tableName} отсутствует или имеет неправильный тип.");
        }
      }
    }
  }
}
