using Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;

namespace ModelInterfaceHub.Models
{
  public class CreatingRequest
  {
    public CreatingStep Step { get; set; } = CreatingStep.Title;
    public string homeworkTitle { get; set; }
    public string homeworkDescription { get; set; }

    public async Task<string> ProcessCreatingNewHomeworkStepAsync(ITelegramBotClient botClient)
    {
      var message = await UserResponce(botClient);
      switch (Step)
      {
        case (CreatingStep.Title):
          if (!string.IsNullOrEmpty(message))
          {
            this.homeworkTitle = message;
            Step = CreatingStep.Description;
            return "Отлично! Теперь введите описание курса:";
          }
          else
          {
            return "Название домашнего задания не может быть пустой строкой. " +
              "\nПожалуйста, введите название домашнего задания.";
          }

        case (CreatingStep.Description):
          if (!string.IsNullOrEmpty(message))
          {
            this.homeworkDescription = message;
            Step = CreatingStep.Title;
            CommonDataModel.CreateHomework(new HomeWorkModel(this.homeworkTitle, this.homeworkDescription));
            return "Домашнее задание успешно добавлено.";
          }
          else
          {
            return "Описание домашнего задания не может быть пустой строкой. " +
              "\nПожалуйста, введите описание домашнего задания.";
          }

        default:
          return "Неизвестный шаг при добавлении домашнего задания.";
      }
    }

    async public static Task<string> UserResponce(ITelegramBotClient botClient)
    {
      var offset = 0;
      var returnNewMessage = string.Empty;

      while (true)
      {
        var newMessage = await botClient.GetUpdatesAsync(offset, limit: 0);
        var stopCheck = false;

        foreach (var messages in newMessage)
        {
          offset = messages.Id + 1;

          if (messages.Message != null)
          {
            var m = messages.Message.Text;

            if (m[0] != '/')
            {
              returnNewMessage = m;
              stopCheck = true;

              break;
            }
          }
        }
        if (stopCheck)
          break;
      }
      return returnNewMessage;
    }
  }
  public enum CreatingStep
  {
    Title,
    Description
  }
}
