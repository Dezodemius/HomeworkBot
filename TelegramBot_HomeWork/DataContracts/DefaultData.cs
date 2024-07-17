using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DataContracts
{
  static public class DefaultData
  {
    static public BotCommon.BotConfigManager configManager;
    static public BotCommon.BotConfig botConfig;

    static DefaultData()
    {
      configManager = new BotCommon.BotConfigManager();
      botConfig = DataContracts.DefaultData.configManager.Config;
    }
  }
}
