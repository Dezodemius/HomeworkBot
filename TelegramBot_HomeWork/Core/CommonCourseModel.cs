using Database;
using DataContracts.Data;
using DataContracts.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
  public static class CommonCourseModel
  {
    static DatabaseManager dbManager = new DatabaseManager(ApplicationData.ConfigApp.DatabaseConnectionString);

    /// <summary>
    /// Возвращает все курсы.
    /// </summary>
    /// <param name="idUser"></param>
    /// <returns></returns>
    public static List<Course> GetAllCourses()
    {
      return dbManager.GetAllCourses();
    }

    /// <summary>
    /// Возвращает все курсы пользователя.
    /// </summary>
    /// <param name="idUser"></param>
    /// <returns></returns>
    public static List<Course> GetAllUserCourses(int idUser)
    {
      var userCourses = dbManager.GetAllUserCourses().Where(x => x.UserId == idUser).ToList();
      var courseIds = userCourses.Select(uc => uc.CourseId).ToList();
      var courses = dbManager.GetAllCourses().Where(c => courseIds.Contains(c.CourseId)).ToList();

      return courses;
    }

    public static string GetNameCourse(int courseId)
    {
      var data = GetAllCourses().Where(x=> x.CourseId == courseId).First() as Course;
      return data.CourseName;
    }

    public static void AddCourse(Course course)
    { 
      dbManager.CreateCourse(course); 
    }

  }
}
