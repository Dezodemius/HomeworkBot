using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeWorkTelegramBot.Models
{
  /// <summary>
  /// Модель данных курса.
  /// </summary>
  internal class Courses
  {
    /// <summary>
    /// Уникальный идентификатор курса.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Название курса.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Описание курса.
    /// </summary>
    public string Description { get; set; }
  }
}
