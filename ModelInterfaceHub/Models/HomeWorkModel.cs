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
    /// Статусы домашнего задания
    /// </summary>
    public enum StatusWork
    { 
      /// <summary>
      /// Проверенное домашнее задание
      /// </summary>
      Checked,

      /// <summary>
      /// Непроверенное домашнее задание
      /// </summary>
      Unchecked,

      /// <summary>
      /// Домашнее задание, требующее доработки
      /// </summary>
      NeedsRevision,

      /// <summary>
      /// Непрочитанное домашнее задание
      /// </summary>
      Unread
    }

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

    public StatusWork Status { get; set; }

    /// <summary>
    /// Конструктор класса HomeWorkModel
    /// </summary>
    /// <param name="title">Название домашнего задания</param>
    /// <param name="description">Описание домашнего задания</param>
    public HomeWorkModel(string title, string description, StatusWork statusWork)
    {
      Title = title;
      Description = description;
      Status = statusWork;
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
      Status = statusWork;
    }
  }
}
