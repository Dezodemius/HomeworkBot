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
  public class Administrator : IParticipant
  {
    public string Id { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public string FullName { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    public UserRole Role { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

    public Task StartAsync(ITelegramBotClient client, Update update, CancellationToken token)
    {
      throw new NotImplementedException();
    }
  }
}
