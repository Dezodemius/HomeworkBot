using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using SqlHelper;
using DataContracts;

namespace TelegramBot_HomeWork
{
  static internal class TelegramBot
  {
    
    static TelegramBotClient botClient;
    static SqliteDatabase sqliteDatabase;
    static DataContracts.IParticipant participant;

    internal static async Task StartBot()
    {
      if (!await sqliteDatabase.ParticipantsTableExistsAsync())
      {
        await sqliteDatabase.CreateParticipantsTableAsync();
      }

      if (!await sqliteDatabase.HomeworkTableExistsAsync())
      {
        await sqliteDatabase.CreateHomeworkTableAsync();
      }

      botClient.StartReceiving(Update, Error);
      Console.ReadLine();
    }

    private static async Task Error(ITelegramBotClient client, Exception exception, CancellationToken token)
    {
      throw new NotImplementedException();
    }

    private static async Task Update(ITelegramBotClient client, Update update, CancellationToken token)
    {
      var message = update.Message;

      if (message != null)
      {
        IParticipant.UserRole userRole =  await sqliteDatabase.GetUserRoleAsync(message.Chat.Id.ToString());
        switch (userRole)
        {
          case IParticipant.UserRole.Student:
            participant = new RoleModels.Student(message.Chat.Id.ToString(), $"{message.Chat.LastName} {message.Chat.FirstName}");
            break;
          case IParticipant.UserRole.Teacher:
            participant = new RoleModels.Teacher(message.Chat.Id.ToString(), $"{message.Chat.LastName} {message.Chat.FirstName}");
            break;
          case IParticipant.UserRole.Admin:
            participant = new RoleModels.Administrator();
            break;
          default:
            await client.SendTextMessageAsync(message.Chat.Id, "Вы не зарегистрированы в системе! Пожалуйста, обратитесь за помощью к своему преподавателю.");
            return;
            ;
        }

        await participant.StartAsync(client, update, token);
      }
    }

    static TelegramBot()
    {
      botClient = new TelegramBotClient(DefaultData.botConfig.BotToken);
      sqliteDatabase = new SqliteDatabase(DefaultData.botConfig.DbConnectionString, null, null);
    }
  }
}
