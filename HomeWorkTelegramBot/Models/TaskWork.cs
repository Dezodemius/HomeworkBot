using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HomeWorkTelegramBot.Models
{
  internal class TaskWork
  {
    /// <summary>
    /// Уникальный идентификатор задания.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Уникальный идентификатор курса.
    /// </summary>
    public int CourseId { get; set; }

    /// <summary>
    /// Название задания.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Описание задания.
    /// </summary>
    public string Description { get; set; }
  }
}
