using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;

namespace DataContracts
{
  public interface IParticipant
  {
    public enum UserRole
    {
      Student,
      Teacher,
      Admin,
      None,
    }

    public enum HomeworkStatus
    {
      NotSubmitted,
      NotReviewed,
      RequiresRevision,
      Accepted
    }

    public string Id { get; set; }
    public string FullName { get; set; }
    public UserRole Role { get; set; }

    public Task StartAsync(ITelegramBotClient client, Update update, CancellationToken token);
  }
}
