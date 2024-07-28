using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RoleModels
{
  internal class CurrentTeachingAssignment
  {
    public Teacher Teacher { get; set; }
    public Student Student { get; set; }

    internal CurrentTeachingAssignment(Teacher teacher, Student student)
    {
      this.Teacher = teacher;
      this.Student = student;
    }

  }
}
