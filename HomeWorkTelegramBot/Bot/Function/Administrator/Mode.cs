using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeWorkTelegramBot.Bot.Function.Administrator
{
  internal class Mode
  {
    static public bool CreateCourse { get; set; }

    static public void AllReset()
    { 
      CreateCourse = false;
    }
  }
}
