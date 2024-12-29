using HomeWorkTelegramBot.Bot.Function.Administrator;
using HomeWorkTelegramBot.Bot.Function.Student;
using HomeWorkTelegramBot.Bot.Function.Teacher;
using HomeWorkTelegramBot.Bot.Function;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HomeWorkTelegramBot.Models;

namespace HomeWorkTelegramBot.Bot
{
  internal class RoleHandlerFactory
  {
    public static IRoleHandler CreateHandler(User.Role? role)
    {
      return role switch
      {
        User.Role.Admin => new AdministratorHandler(),
        User.Role.Teacher => new TeacherHandler(),
        User.Role.Student => new StudentHandler(),
        _ => null,
      };
    }
  }
}
