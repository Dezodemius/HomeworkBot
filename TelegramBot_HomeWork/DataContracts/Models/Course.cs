using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace DataContracts.Models
{
  public enum CreateStep
  { 
    Start,
    Teacher,
    Name,
    Completed
  }

  public class Course
  {
    public int CourseId { get; set; }
    public string CourseName { get; set; }
    public long? TeacherId { get; set; }
    private CreateStep Step { get; set; } = CreateStep.Start;

    public void SetStep(CreateStep createStep)
    { 
      Step = createStep;
    }

    public CreateStep GetStep()
    {
       return Step;
    }

  }

}
