using Core;
using Database;
using ModelInterfaceHub.Interfaces;
using ModelInterfaceHub.Models;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace TelegramBot.Roles
{
  /// <summary>
  /// Представляет пользователя с ролью учителя в системе.
  /// </summary>
  public class Teacher : UserModel, IMessageHandler
  {

    /// <summary>
    /// Инициализирует новый экземпляр класса Teacher.
    /// </summary>
    /// <param name="telegramChatId">Идентификатор чата Telegram.</param>
    /// <param name="firstName">Имя учителя.</param>
    /// <param name="lastName">Фамилия учителя.</param>
    /// <param name="email">Адрес электронной почты учителя.</param>
    public Teacher(long telegramChatId, string firstName, string lastName, string email)
        : base(telegramChatId, firstName, lastName, email, UserRole.Teacher) { }

    /// <summary>
    /// Обрабатывает входящее сообщение от преподавателя.
    /// </summary>
    /// <param name="message">Текст сообщения от пропеодавателя.</param>
    /// <returns>Ответ на сообщение учителя.</returns>
    public async Task ProcessMessageAsync(ITelegramBotClient botClient, long chatId, string message)
    {
      if (string.IsNullOrEmpty(message)) return;

      if (message.ToLower().Contains("/start"))
      {
        var keyboard = new InlineKeyboardMarkup(new[]
        {
          new[]
          {
            InlineKeyboardButton.WithCallbackData("задания на проверку", "/get_jobhomework")
          },
          new[]
          {
            InlineKeyboardButton.WithCallbackData("домашние задание", "/get_homework")
          },
          new[]
          {
            InlineKeyboardButton.WithCallbackData("выполненые ДЗ", "/get_completedhomework")
          },
          new[]
          {
            InlineKeyboardButton.WithCallbackData("невыполненые ДЗ", "/get_uncompletedhomework")
          },
          new[]
          {
            InlineKeyboardButton.WithCallbackData("студенты с провер ДЗ", "/get_listcheckedstudent")
          }
        });
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "выберите действие: ", keyboard);  
      }
    }

    /// <summary>
    /// Обработка Callback запросов от преподавателя.
    /// </summary>
    /// <param name="callbackData">Данные обратного вызова.</param>
    /// <returns>Результат обработки запроса.</returns>
    /// <exception cref="NotImplementedException"></exception>
    public async Task ProcessCallbackAsync(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {
      //throw new NotImplementedException(); 
      if (string.IsNullOrEmpty(callbackData)) return;
      else 
      {        
        if (callbackData.Contains("/get_jobhomework"))
        {
          await ViewUncheckedHomeworks(botClient, chatId);
        }
        else if (callbackData.Contains("/get_homework"))
        {
          await DisplayHomeworkButtons(botClient, chatId, callbackData, messageId);
        }
        else if (callbackData.Contains("/get_completedhomework"))
        {
          await ShowCompletedHomeworks(botClient, chatId);
        }
        else if (callbackData.Contains("/get_uncompletedhomework"))
        {
          await ShowUncompletedHomeworks(botClient, chatId);
        }
        else if (callbackData.Contains("/start"))
        {
          await botClient.DeleteMessageAsync(chatId, messageId);
          await Task.Delay(10);
          await ProcessMessageAsync(botClient, chatId, callbackData);
        }
      }
    }

    /// <summary>
    /// Просмотр непроверенных домашних заданий.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task ViewUncheckedHomeworks(ITelegramBotClient botClient,long chatId)
    {
      throw new NotImplementedException();  
    }

    /// <summary>
    /// Отображение непроверенных домашних заданий.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task ShowUncheckedHomeworks(ITelegramBotClient botClient, long chatId)
    { 
      throw new NotImplementedException();  
    }

    /// <summary>
    /// Отображение кнопок для работы с домашними заданиями.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task DisplayHomeworkButtons(ITelegramBotClient botClient, long chatId, string callbackData, int messageId)
    {

      var inlineKeyboardMarkup = await ViewListHomework();
      
          //var keyboard = new InlineKeyboardMarkup(new[]
          //{
          //  new[]
          //  {
          //    InlineKeyboardButton.WithCallbackData("Bottom1", "/bottom1"),
          //  },
          //  new[]
          //  {
          //    InlineKeyboardButton.WithCallbackData("Bottom2", "/bottom2"),
          //  }

          //});
          await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите ДЗ: ", inlineKeyboardMarkup, messageId);
        
    }
    /// <summary>
    /// Отображает список домашних заданий
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private async Task<InlineKeyboardMarkup> ViewListHomework()
    {
     throw new NotImplementedException();
    }

    /// <summary>
    /// Отображение невыполненных домашних заданий.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task ShowUncompletedHomeworks(ITelegramBotClient botClient, long chatId)
    { 
      throw new NotImplementedException();  
    }

    /// <summary>
    /// Отображение выполненных домашних заданий.
    /// </summary>
    /// <returns>Задача, представляющая асинхронную операцию.</returns>
    public async Task ShowCompletedHomeworks(ITelegramBotClient botClient, long chatId)
    {
      throw new NotImplementedException();
    }

  }
}