using DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using static DataContracts.IParticipant;


namespace RoleModels
{
  public class Teacher : IParticipant
  {
    public string Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string FullName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public UserRole Role { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public async Task StartAsync(ITelegramBotClient client, Update update, CancellationToken token)
    {
      var message = update.Message;

      if (message != null && message.Text != null)
      {
        await client.SendTextMessageAsync(message.Chat.Id, $"Заглушка", cancellationToken: token);
      }
    }
  }
}
