using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DataContracts.Models
{
  public class Submission
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

    public int SubmissionId { get; set; }
    public int AssignmentId { get; set; }

    public int CourseId { get; set; }
    public int StudentId { get; set; }
    public string GithubLink { get; set; }
    public DateTime SubmissionDate { get; set; }
    public StatusWork Status { get; set; }
    public string TeacherComment { get; set; }

  }
}
