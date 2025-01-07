using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using static HomeWorkTelegramBot.Config.Logger;

namespace HomeWorkTelegramBot.Bot.Function.Administrator
{
  internal class LogViewer
  {
    private const string LogDirectory = "logs";

    /// <summary>
    /// Отображает список доступных лог-файлов в чате.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="chatId">Идентификатор чата, куда будет отправлен список логов.</param>
    public static async Task DisplayLogFilesAsync(ITelegramBotClient botClient, long chatId)
    {
      if (!Directory.Exists(LogDirectory))
      {
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Папка с логами не найдена.");
        return;
      }

      var logFiles = Directory.GetFiles(LogDirectory);
      if (logFiles.Length == 0)
      {
        await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Нет доступных логов.");
        return;
      }

      var buttons = new List<CallbackModel>();
      foreach (var file in logFiles)
      {
        var fileName = Path.GetFileName(file);
        buttons.Add(new CallbackModel(fileName, $"/viewLog_{fileName}"));
      }

      var keyboard = TelegramBotHandler.GetInlineKeyboardMarkupAsync(buttons);
      await TelegramBotHandler.SendMessageAsync(botClient, chatId, "Выберите лог для просмотра:", keyboard);
    }

    /// <summary>
    /// Отправляет лог-файл с ошибками и исходный лог-файл в чат.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="callbackQuery">Данные callback-запроса, содержащие информацию о выбранном лог-файле.</param>
    public static async Task SendErrorLogsAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      var fileName = callbackQuery.Data.Replace("/viewLog_", "");
      var filePath = Path.Combine(LogDirectory, fileName);

      if (!System.IO.File.Exists(filePath))
      {
        await TelegramBotHandler.SendMessageAsync(botClient, callbackQuery.Message.Chat.Id, "Файл не найден.");
        return;
      }

      var tempFilePath = Path.Combine(@"C:\Temp", fileName);
      System.IO.File.Copy(filePath, tempFilePath, true);

      var errorFilePath = Path.Combine(@"C:\Temp", $"errors_{fileName}");
      try
      {
        using (var reader = new StreamReader(tempFilePath))
        using (var writer = new StreamWriter(errorFilePath))
        {
          string line;
          while ((line = await reader.ReadLineAsync()) != null)
          {
            if (line.Contains("ERROR"))
            {
              await writer.WriteLineAsync(line);
            }
          }
        }

        using (var errorStream = new FileStream(errorFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
        using (var originalStream = new FileStream(tempFilePath, FileMode.Open, FileAccess.Read, FileShare.Delete))
        {
          var errorInputFile = InputFile.FromStream(errorStream);
          var originalInputFile = InputFile.FromStream(originalStream);

          await botClient.DeleteMessageAsync(callbackQuery.Message.Chat.Id, callbackQuery.Message.MessageId);

          await botClient.SendDocument(
              chatId: callbackQuery.Message.Chat.Id,
              document: errorInputFile,
              caption: "Файл с ошибками"
          );

          await botClient.SendDocument(
              chatId: callbackQuery.Message.Chat.Id,
              document: originalInputFile,
              caption: "Исходный лог-файл"
          );
        }
      }
      finally
      {
        try
        {
          if (System.IO.File.Exists(tempFilePath))
          {
            System.IO.File.Delete(tempFilePath);
            LogInformation($"Временный файл {tempFilePath} успешно удален.");
          }
        }
        catch (Exception ex)
        {
          LogError($"Ошибка при удалении временного файла: {ex.Message}");
        }
      }
    }

    /// <summary>
    /// Обрабатывает выбор лог-файла и отправляет его в чат.
    /// </summary>
    /// <param name="botClient">Клиент Telegram бота.</param>
    /// <param name="callbackQuery">Данные callback-запроса, содержащие информацию о выбранном лог-файле.</param>
    public static async Task HandleLogFileSelectionAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      var fileName = callbackQuery.Data.Replace("/viewLog_", "");
      var filePath = Path.Combine(LogDirectory, fileName);

      if (!System.IO.File.Exists(filePath))
      {
        await TelegramBotHandler.SendMessageAsync(botClient, callbackQuery.Message.Chat.Id, "Файл не найден.");
        return;
      }

      using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
      {
        var inputFile = InputFile.FromStream(stream);
        await botClient.SendDocument(
            chatId: callbackQuery.Message.Chat.Id,
            document: inputFile
        );
      }

    }
  }
}
