using HomeWorkTelegramBot.Bot.Function.Processing;
using HomeWorkTelegramBot.Config;
using HomeWorkTelegramBot.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using static HomeWorkTelegramBot.Config.Logger;

namespace HomeWorkTelegramBot.Bot
{
  internal class TelegramBotHandler
  {
    /// <summary>
    /// Клиент Telegram бота.
    /// </summary>
    private readonly ITelegramBotClient _botClient;

    // Кэш для хранения данных о страницах для каждого чата и сообщения
    private static readonly ConcurrentDictionary<long, ConcurrentDictionary<int, List<CallbackModel>>> PaginationCache = new ConcurrentDictionary<long, ConcurrentDictionary<int, List<CallbackModel>>>();

    /// <summary>
    /// Инициализирует новый экземпляр класса TelegramBotHandler.
    /// </summary>
    /// <param name="dbManager">Менеджер базы данных.</param>
    /// <param name="botToken">Токен Telegram бота.</param>
    public TelegramBotHandler(string botToken)
    {
      _botClient = new TelegramBotClient(botToken);
    }

    /// <summary>
    /// Запускает бота и начинает обработку сообщений.
    /// </summary>
    public async Task StartBotAsync()
    {
      var cts = new CancellationTokenSource();
      var receiverOptions = new ReceiverOptions
      {
        AllowedUpdates = Array.Empty<UpdateType>()
      };

      _botClient.StartReceiving(
          updateHandler: HandleUpdateAsync,
          errorHandler: HandlePollingErrorAsync,
          receiverOptions: receiverOptions,
          cancellationToken: cts.Token
      );

      var me = await _botClient.GetMe(cts.Token);
      LogInformation($"Начала работы с @{me.Username}");
    }


    /// <summary>
    /// Обрабатывает входящие обновления от Telegram.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="update">Объект обновления.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
      if (update.Type == UpdateType.Message && update.Message?.Text != null)
      {
        await MessageProcessing.HandleMessageAsync(botClient, update.Message, cancellationToken);
      }
      else if (update.Type == UpdateType.CallbackQuery)
      {
        await CallbackProcessing.HandleCallbackQueryAsync(botClient, update.CallbackQuery, cancellationToken);
      }
    }

    /// <summary>
    /// Обрабатывает ошибки, возникающие при получении обновлений.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="exception">Возникшее исключение.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
      var ErrorMessage = exception switch
      {
        ApiRequestException apiRequestException
            => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
        _ => exception.ToString()
      };

      LogError(ErrorMessage);
      return Task.CompletedTask;
    }

    /// <summary>
    /// Асинхронно отправляет или редактирует сообщение пользователю через Telegram бота.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="message">Текст сообщения для отправки или редактирования.</param>
    /// <param name="inlineKeyboardMarkup">Опциональная встроенная клавиатура.</param>
    /// <param name="messageId">Идентификатор сообщения для редактирования (если есть).</param>
    /// <returns>Задача, представляющая асинхронную операцию отправки или редактирования сообщения.</returns>
    internal static async Task<Message> SendMessageAsync(ITelegramBotClient botClient, long chatId, string message, InlineKeyboardMarkup inlineKeyboardMarkup = null, int? messageId = null)
    {
      try
      {
        if (inlineKeyboardMarkup == null && messageId == null)
        {
          return await botClient.SendMessage(chatId, message);
        }
        else if (inlineKeyboardMarkup == null && messageId.HasValue)
        {
          return await botClient.EditMessageText(chatId, messageId.Value, message);
        }
        else if (messageId.HasValue)
        {
          return await botClient.EditMessageText(chatId, messageId.Value, message, replyMarkup: inlineKeyboardMarkup);
        }
        else
        {
          return await botClient.SendMessage(chatId, message, replyMarkup: inlineKeyboardMarkup);
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine(ex.ToString());
        return await SendMessageAsync(botClient, chatId, "Произошла системная ошибка! Повторите попытку позже...");
      }
    }

    /// <summary>
    /// Возвращает кнопки сообщения по моделям данных.
    /// </summary>
    /// <param name="data">Модели Callback/</param>
    /// <returns>Кнопки сообщения по моделям данных.</returns>
    internal static InlineKeyboardMarkup GetInlineKeyboardMarkupAsync(List<CallbackModel> data)
    {

      List<List<InlineKeyboardButton>> buttons = new List<List<InlineKeyboardButton>>();

      foreach (var callbackModel in data)
      {
        buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(callbackModel.Name, callbackModel.Command) });
      }

      return new InlineKeyboardMarkup(buttons);
    }

    /// <summary>
    /// Возвращает кнопки сообщения по модели данных.
    /// </summary>
    /// <param name="callbackModel">Модель Callback.</param>
    /// <returns>Кнопка сообщения.</returns>
    internal static InlineKeyboardMarkup GetInlineKeyboardMarkupAsync(CallbackModel callbackModel)
    {
      List<List<InlineKeyboardButton>> buttons = new List<List<InlineKeyboardButton>>();
      buttons.Add(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(callbackModel.Name, callbackModel.Command) });

      return new InlineKeyboardMarkup(buttons);
    }

    /// <summary>
    /// Создает разметку встроенной клавиатуры с пагинацией для списка элементов.
    /// </summary>
    /// <param name="items">Список объектов <see cref="CallbackModel"/>, которые нужно отобразить.</param>
    /// <param name="currentPage">Текущая страница, которую нужно отобразить.</param>
    /// <param name="itemsPerPage">Количество элементов на странице (по умолчанию 9).</param>
    /// <returns>Объект <see cref="InlineKeyboardMarkup"/>, содержащий кнопки для текущей страницы и кнопки навигации.</returns>
    internal static InlineKeyboardMarkup GetPaginatedInlineKeyboardMarkup(
        List<CallbackModel> items,
        int currentPage = 0)
    {
      int itemsPerPage = ApplicationData.ConfigApp.ItemsPerPage;
      // Вычисляем общее количество страниц
      var totalPages = (int)Math.Ceiling(items.Count / (double)itemsPerPage);
      currentPage = Math.Max(0, Math.Min(currentPage, totalPages - 1));

      // Извлекаем элементы для текущей страницы
      var paginatedItems = items
          .Skip(currentPage * itemsPerPage)
          .Take(itemsPerPage)
          .ToList();

      // Создаем кнопки для каждого элемента на текущей странице
      var buttons = GetInlineKeyboardMarkupAsync(paginatedItems).InlineKeyboard.ToList();

      // Добавляем кнопки навигации
      var navigationButtons = new List<InlineKeyboardButton>();
      if (currentPage > 0)
      {
        navigationButtons.Add(InlineKeyboardButton.WithCallbackData("⬅️ Назад", $"/page:{currentPage - 1}:{itemsPerPage}"));
      }
      if (currentPage < totalPages - 1)
      {
        navigationButtons.Add(InlineKeyboardButton.WithCallbackData("Вперед ➡️", $"/page:{currentPage + 1}:{itemsPerPage}"));
      }

      if (navigationButtons.Any())
      {
        buttons.Add(navigationButtons);
      }

      return new InlineKeyboardMarkup(buttons);
    }

    /// <summary>
    /// Инициализирует данные для пагинации и сохраняет их в кэше.
    /// </summary>
    /// <param name="chatId">Идентификатор чата пользователя.</param>
    /// <param name="messageId">Идентификатор сообщения.</param>
    /// <param name="items">Список объектов <see cref="CallbackModel"/>, которые нужно отобразить.</param>
    internal static void InitializePagination(long chatId, int messageId, List<CallbackModel> items)
    {
      var messageCache = PaginationCache.GetOrAdd(chatId, new ConcurrentDictionary<int, List<CallbackModel>>());
      messageCache[messageId] = items;
      LogInformation($"Инициализация пагинации для chatId: {chatId}, messageId: {messageId}");
    }

    /// <summary>
    /// Обрабатывает нажатие на кнопки пагинации и обновляет отображаемую страницу.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="callbackQuery">Объект callback-запроса.</param>
    /// <param name="itemsPerPage">Количество элементов на странице (по умолчанию 9).</param>
    /// <returns>Задача, представляющая асинхронную операцию обработки нажатия.</returns>
    internal static async Task HandlePaginationCallbackAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      if (callbackQuery.Data.StartsWith("/page:"))
      {
        // Извлекаем номер страницы и itemsPerPage из команды
        var pageData = callbackQuery.Data.Split(':');
        if (pageData.Length == 3 && int.TryParse(pageData[1], out int newPage) && int.TryParse(pageData[2], out int itemsPerPage))
        {
          LogInformation($"Обработка пагинации для chatId: {callbackQuery.Message.Chat.Id}, messageId: {callbackQuery.Message.MessageId}, новая страница: {newPage}, itemsPerPage: {itemsPerPage}");

          // Получаем данные из кэша
          if (PaginationCache.TryGetValue(callbackQuery.Message.Chat.Id, out var messageCache) && messageCache.TryGetValue(callbackQuery.Message.MessageId, out var items))
          {
            LogInformation("Данные для пагинации найдены в кэше.");

            // Создаем новую разметку клавиатуры для новой страницы
            var inlineKeyboard = GetPaginatedInlineKeyboardMarkup(items, newPage);

            // Обновляем сообщение с новой клавиатурой
            await botClient.EditMessageReplyMarkupAsync(
                chatId: callbackQuery.Message.Chat.Id,
                messageId: callbackQuery.Message.MessageId,
                replyMarkup: inlineKeyboard
            );
          }
          else
          {
            LogError("Данные для пагинации не найдены в кэше.");
          }
        }
      }
    }
  }
}
