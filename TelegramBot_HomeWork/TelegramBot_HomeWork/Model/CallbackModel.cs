using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramBot.Model
{
  internal class CallbackModel
  {
    internal string Name { get; set; }

    internal string Command { get; set; }

    internal CallbackModel(string name, string command)
    {
      Name = name;
      Command = command;
    }
  }
}
