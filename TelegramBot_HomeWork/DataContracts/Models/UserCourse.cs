using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DataContracts.Models
{
  public class UserCourse
  {
    public int UserCourseId { get; set; }
    public int UserId { get; set; }

    public int CourseId { get; set; }
  }
}
