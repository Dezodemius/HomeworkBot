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
    public int SubmissionId { get; set; }
    public int AssignmentId { get; set; }
    public int StudentId { get; set; }
    public string GithubLink { get; set; }
    public DateTime SubmissionDate { get; set; }
    public string Status { get; set; }
    public string TeacherComment { get; set; }

    public Assignment Assignment { get; set; }
    public User Student { get; set; }
  }
}
