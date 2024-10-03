using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TelegramBot.Models;
using TelegramBot.Data;

namespace TelegramBot
{
  /// <summary>
  /// Главный класс программы, содержащий точку входа.
  /// </summary>
  class Program
  {
    /// <summary>
    /// Точка входа в приложение.
    /// </summary>
    static async Task Main(string[] args)
    {
      Console.WriteLine("Отладка: Программа запущена");

      try
      {
        var config = Config.LoadFromFile("config.yaml");

        Console.WriteLine("Конфигурация успешно загружена");
        Console.WriteLine($"Строка подключения к базе данных: {config.DatabaseConnectionString}");
        Console.WriteLine($"Токен бота: {config.BotToken}");
        Console.WriteLine($"ID администратора: {config.AdminId}");

        var dbManager = new DatabaseManager(config.DatabaseConnectionString);
        dbManager.EnsureDatabaseCreated();
        dbManager.EnsureTablesCreated();

        // Добавляем тестовые данные
        TestData.SeedTestData(dbManager);

        Console.WriteLine("База данных и таблицы успешно созданы, тестовые данные добавлены");

        // Проверка и добавление администратора
        await EnsureAdminExistsAsync(dbManager, config.AdminId);

        var botHandler = new TelegramBotHandler(dbManager, config.BotToken);
        await botHandler.StartBotAsync();

        // Здесь будет код для поддержания бота в рабочем состоянии
        await Task.Delay(-1); // Бесконечное ожидание
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Отладка: Произошла ошибка - {ex.Message}");
        Debug.WriteLine($"Отладка: Подробности исключения - {ex}");
      }

      Console.WriteLine("Отладка: Программа завершена");

      Console.WriteLine("Нажмите любую клавишу для выхода...");
      Console.ReadKey();
    }

    /// <summary>
    /// Проверяет наличие администратора в базе данных и добавляет его, если он отсутствует.
    /// </summary>
    /// <param name="dbManager">Менеджер базы данных.</param>
    /// <param name="adminId">ID администратора из конфигурации.</param>
    private static async Task EnsureAdminExistsAsync(DatabaseManager dbManager, long adminId)
    {
      if (!dbManager.HasAdministrators())
      {
        var admin = new User(adminId, "Admin", "User", "admin@example.com", UserRole.Administrator);
        dbManager.AddUser(admin);
        Console.WriteLine($"Администратор с ID {adminId} добавлен в базу данных.");
      }
      else
      {
        Console.WriteLine("Администратор уже существует в базе данных.");
      }
    }
  }
}
