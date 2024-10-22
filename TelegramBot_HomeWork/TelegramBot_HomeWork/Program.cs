
using DataContracts.Data;
using System.Diagnostics;

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
        var config = ApplicationData.ConfigApp;

        Console.WriteLine("Конфигурация успешно загружена");
        Console.WriteLine($"Строка подключения к базе данных: {config.DatabaseConnectionString}");
        Console.WriteLine($"Токен бота: {config.BotToken}");
        Console.WriteLine($"ID администратора: {config.AdminId}");

        Core.CheckData.CheckTables();
        Console.WriteLine("База данных и таблицы успешно созданы");
        //Core.Test.CreateTestCoursesAndAssignments();
        // Core.CommonDataModel.SeedTestData();
        var botHandler = new TelegramBotHandler(config.BotToken);
        await botHandler.StartBotAsync();
        
        await Task.Delay(-1);
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
  }
}
