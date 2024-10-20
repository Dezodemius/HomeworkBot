using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataContracts.Models
{

  /// <summary>
  /// Модель, представляющая 
  /// </summary>
  public class StudentHomeWorkModel
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
      /// Невыполненное домашнее задание
      /// </summary>
      Unfulfilled
    }

    /// <summary>
    /// Уникальный идентификатор выданного домашнего задания.
    /// </summary>
    public int IdSubmissions { get; set; }

    /// <summary>
    /// Уникальный идентификатор домашнего задания.
    /// </summary>
    public int IdHomeWork { get; set; }

    /// <summary>
    /// Уникальный идентификатор студента.
    /// </summary>
    public int IdStudent { get; set; }

    /// <summary>
    /// Ссылка на GitHub с выполненным заданием.
    /// </summary>
    public string GithubLink { get; set; }

    /// <summary>
    /// Статус домашнего задания.
    /// </summary>
    public StatusWork Status { get; set; }

    /// <summary>
    /// Комментарий преподавателя к домашнему заданию.
    /// </summary>
    public string TeacherComment { get; set; }

    /// <summary>
    /// Конструктор класса StudentHomeWorkModel.
    /// </summary>
    /// <param name="idSubmissions">Уникальный идентификатор выданного домашнего задания.</param>
    /// <param name="idHomeWork">Уникальный идентификатор домашнего задания.</param>
    /// <param name="idStudent">Уникальный идентификатор студента.</param>
    /// <param name="githubLink">Ссылка на GitHub с выполненным заданием.</param>
    /// <param name="status">Статус домашнего задания.</param>
    /// <param name="teacherComment">Комментарий преподавателя к домашнему заданию.</param>
    public StudentHomeWorkModel(int idHomeWork, int idSubmissions, int idStudent, string githubLink, StatusWork status, string teacherComment)
    {
      this.IdSubmissions = idSubmissions;
      this.IdHomeWork = idHomeWork;
      this.IdStudent = idStudent;
      this.GithubLink = githubLink;
      this.Status = status;
      this.TeacherComment = teacherComment;
    }
  }
}
