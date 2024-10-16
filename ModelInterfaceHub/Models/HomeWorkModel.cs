using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModelInterfaceHub.Models
{
  /// <summary>
  /// Модель, представляющая домашнее задание
  /// </summary>
  public class HomeWorkModel
  {
    /// <summary>
    /// Уникальный идентификатор домашнего задания
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Название домашнего задания
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Описание домашнего задания
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Конструктор класса HomeWorkModel
    /// </summary>
    /// <param name="title">Название домашнего задания</param>
    /// <param name="description">Описание домашнего задания</param>
    public HomeWorkModel(string title, string description)
    {
      Title = title;
      Description = description;
    }

    /// <summary>
    /// Конструктор класса HomeWorkModel
    /// </summary>
    /// <param name="id">Уникальный идентификатор домашнего задания</param>
    /// <param name="title">Название домашнего задания</param>
    /// <param name="description">Описание домашнего задания</param>
    public HomeWorkModel(int id, string title, string description, StatusWork statusWork)
    {
      Id = id;
      Title = title;
      Description = description;
    }
  }
}
