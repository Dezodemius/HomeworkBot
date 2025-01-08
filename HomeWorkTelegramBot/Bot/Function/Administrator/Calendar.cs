using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;

namespace HomeWorkTelegramBot.Bot.Function.Administrator
{
  class Calendar
  {

    private static readonly Dictionary<long, DateSelectionState> DateSelectionStates = new Dictionary<long, DateSelectionState>();

    public async Task StartDateSelectionAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      var yearKeyboard = CreateYearSelection();
      await TelegramBotHandler.SendMessageAsync(botClient, callbackQuery.From.Id, "Выберите год:", yearKeyboard, callbackQuery.Message.MessageId);
    }

    public async Task StartDateSelectionAsync(ITelegramBotClient botClient, Message message)
    {
      var yearKeyboard = CreateYearSelection();
      await TelegramBotHandler.SendMessageAsync(botClient, message.From.Id, "Выберите год:", yearKeyboard);
    }

    public async Task HandleDateSelectionAsync(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      var data = callbackQuery.Data.Split(':');
      string action = data[0];
      long chatId = callbackQuery.Message.Chat.Id;

      if (!DateSelectionStates.ContainsKey(chatId))
      {
        DateSelectionStates[chatId] = new DateSelectionState();
      }

      switch (action)
      {
        case "/selectYear":
          DateSelectionStates[chatId].Year = int.Parse(data[1]);
          await ShowMonthSelection(botClient, callbackQuery);
          break;
        case "/selectMonth":
          DateSelectionStates[chatId].Month = int.Parse(data[2]);
          await ShowDaySelection(botClient, callbackQuery);
          break;
        case "/selectDay":
          DateSelectionStates[chatId].Day = int.Parse(data[3]);
          await ConfirmDateSelection(botClient, callbackQuery);
          break;
      }
    }

    private async Task ShowMonthSelection(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      var year = DateSelectionStates[callbackQuery.Message.Chat.Id].Year;
      var monthKeyboard = CreateMonthSelection(year);
      await TelegramBotHandler.SendMessageAsync(botClient, callbackQuery.Message.Chat.Id, "Выберите месяц:", monthKeyboard, callbackQuery.Message.Id);
    }

    private async Task ShowDaySelection(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      var state = DateSelectionStates[callbackQuery.Message.Chat.Id];
      var dayKeyboard = CreateDaySelection(state.Year, state.Month);
      await TelegramBotHandler.SendMessageAsync(botClient, callbackQuery.Message.Chat.Id, "Выберите день:",  dayKeyboard, callbackQuery.Message.Id);
    }

    private async Task ConfirmDateSelection(ITelegramBotClient botClient, CallbackQuery callbackQuery)
    {
      var state = DateSelectionStates[callbackQuery.Message.Chat.Id];
      await TelegramBotHandler.SendMessageAsync(botClient, callbackQuery.Message.Chat.Id, $"Вы выбрали дату: {state.Day}/{state.Month}/{state.Year}", null, callbackQuery.Message.Id);
    }

    public DateSelectionState ReturnBirthDate(ITelegramBotClient botClient, long chatId)
    {
      DateSelectionStates.TryGetValue(chatId, out var state);
      return state;
    }

    public static InlineKeyboardMarkup CreateYearSelection()
    {
      int currentYear = DateTime.Now.Year;
      int startYear = currentYear - 100;

      var years = new List<List<InlineKeyboardButton>>();
      var yearRow = new List<InlineKeyboardButton>();

      for (int year = startYear; year <= currentYear; year++)
      {
        yearRow.Add(InlineKeyboardButton.WithCallbackData(year.ToString(), $"/selectYear:{year}"));

        if (yearRow.Count == 8)
        {
          years.Add(yearRow);
          yearRow = new List<InlineKeyboardButton>();
        }
      }

      if (yearRow.Count > 0)
      {
        years.Add(yearRow);
      }

      return new InlineKeyboardMarkup(years);
    }

    public static InlineKeyboardMarkup CreateMonthSelection(int year)
    {
      var months = new List<List<InlineKeyboardButton>>();
      var monthRow = new List<InlineKeyboardButton>();

      for (int month = 1; month <= 12; month++)
      {
        var monthName = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
        monthRow.Add(InlineKeyboardButton.WithCallbackData(monthName, $"/selectMonth:{year}:{month}"));

        if (monthRow.Count == 3)
        {
          months.Add(monthRow);
          monthRow = new List<InlineKeyboardButton>();
        }
      }

      if (monthRow.Count > 0)
      {
        months.Add(monthRow);
      }

      return new InlineKeyboardMarkup(months);
    }

    public static InlineKeyboardMarkup CreateDaySelection(int year, int month)
    {
      var days = new List<List<InlineKeyboardButton>>();
      var daysInMonth = DateTime.DaysInMonth(year, month);
      var daysRow = new List<InlineKeyboardButton>();

      for (int day = 1; day <= daysInMonth; day++)
      {
        if (daysRow.Count == 7)
        {
          days.Add(daysRow);
          daysRow = new List<InlineKeyboardButton>();
        }
        daysRow.Add(InlineKeyboardButton.WithCallbackData(day.ToString(), $"/selectDay:{year}:{month}:{day}"));
      }

      if (daysRow.Count > 0)
      {
        days.Add(daysRow);
      }

      return new InlineKeyboardMarkup(days);
    }

    public class DateSelectionState
    {
      public int Year { get; set; }
      public int Month { get; set; }
      public int Day { get; set; }

      public DateOnly ToDateOnly()
      {
        return new DateOnly(Year, Month, Day);
      }
    }
  }
}
