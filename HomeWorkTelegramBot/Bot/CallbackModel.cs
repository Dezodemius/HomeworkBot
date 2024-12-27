using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeWorkTelegramBot.Bot
{
  /// <summary>
  /// Модель данных для представления callback-запросов в Telegram.
  /// </summary>
  internal class CallbackModel
  {
    /// <summary>
    /// Название кнопки, отображаемое пользователю.
    /// </summary>
    internal string Name { get; set; }

    /// <summary>
    /// Команда, отправляемая при нажатии на кнопку.
    /// </summary>
    internal string Command { get; set; }

    /// <summary>
    /// Инициализирует новый экземпляр класса <see cref="CallbackModel"/>.
    /// </summary>
    /// <param name="name">Название кнопки.</param>
    /// <param name="command">Команда для выполнения.</param>
    internal CallbackModel(string name, string command)
    {
      Name = name;
      Command = command;
    }
  }
}
