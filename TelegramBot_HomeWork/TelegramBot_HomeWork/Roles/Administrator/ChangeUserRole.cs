using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core;
using DataContracts.Models;
using Telegram.Bot;
using TelegramBot.Model;

namespace TelegramBot.Roles.Administrator
{
  internal class ChangeUserRole
  {
    UserModel _user;
    internal ChangeUserRole(UserModel user)
    {
      _user = user;
    }

    internal async Task ChangeRole(ITelegramBotClient botClient, long chatId, string message)
    {
      switch (_user.GetChangeStep())
      {
        case ChangeRoleStep.Role:
          {
            var roleNames = new Dictionary<UserRole, string>
            {
                { UserRole.Administrator, "Администратор" },
                { UserRole.Student, "Студент" },
                { UserRole.Teacher, "Преподаватель" }
            };

            var nameRole = Enum.GetValues(typeof(UserRole));
            List<CallbackModel> data = new List<CallbackModel>();
            foreach (var item in nameRole)
            {
              string roleName = roleNames[(UserRole)item];
              data.Add(new CallbackModel(roleName, $"/changeUserRole_role_{item}"));
            }
            await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите новую роль:", TelegramBotHandler.GetInlineKeyboardMarkupAsync(data));
            _user.SetChangeStep(ChangeRoleStep.Completed);
            return;
          }
        case ChangeRoleStep.Completed:
          {
            var data = message.Split('_');
            if (Enum.TryParse(typeof(UserRole), data.Last(), out var parsedRole))
            {
              _user.Role = (UserRole)parsedRole;

              CommonUserModel.UpdateUserModel(_user);
              await TelegramBotHandler.SendMessageAsync(botClient, chatId, $"Роль пользователя {_user.LastName} {_user.FirstName} изменена на {_user.Role}");
              Administrator.changeUserRole.Remove(chatId);
            }
            else
            {
              await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Ошибка: некорректная роль. Попробуйте ещё раз.");
            }
            return;
          }

        default:
          {
            await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Неизвестный шаг изменения роли.");
            return;
          }
      }
    }

  }
}
