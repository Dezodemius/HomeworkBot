using System;
using HomeWorkTelegramBot.Bot;
using HomeWorkTelegramBot.Bot.Function.GitUtilities;
using HomeWorkTelegramBot.Config;
using HomeWorkTelegramBot.Core;
using HomeWorkTelegramBot.DataBase;
using static HomeWorkTelegramBot.Config.Logger;

namespace HomeWorkTelegramBot
{
  internal class Program
  {
    private static async Task Main(string[] args)
    {
      try
      {
        var config = ApplicationData.ConfigApp;
        LogInformation($"Строка подключения к базе данных: {config.DataPath}");
        LogInformation($"Токен бота: {config.BotToken}");
        LogInformation($"ID администратора: {config.AdminId}");

        using var dbContext = new ApplicationDbContext();
        ApplicationData.DbContext = dbContext;

        // dbContext.DeleteDatabase();
        dbContext.CheckDatabaseAndTables();

        // var seeder = new DataSeeder(dbContext);
        // seeder.SeedData();
        // LogInformation("База данных заполнена тестовыми данными.");

        //TestGit();

        var botHandler = new TelegramBotHandler(config.BotToken);
        await botHandler.StartBotAsync();
        await Task.Delay(-1);
      }
      catch (Exception ex)
      {
        LogException(ex);
        throw;
      }
    }

    static void TestGit()
    {
      var CourseEnrollment = CourseEnrollmentService.GetCourseEnrollmentById(1);
      var answer = AnswerService.GetAnswerById(40);
      string repositoryUrl = "https://github.com/Dezodemius/HomeworkBot/tree/debug_AI";
      var result = GitLinkValidator.GetErrorMessage(new GitRepositoryManager().CloneAndPrepareRepository(repositoryUrl, CourseEnrollment, answer));
      Console.WriteLine(result);
    }
  }
}
