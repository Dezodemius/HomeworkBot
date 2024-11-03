using Database;
using DataContracts.Data;
using DataContracts.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core
{
  static public class Test
  {
    static DatabaseManager dbManager = new DatabaseManager(ApplicationData.ConfigApp.DatabaseConnectionString);
    public static void CreateTestCoursesAndAssignments()
    {

      var teacher = new UserModel(ApplicationData.ConfigApp.AdminId, "Даниил", "Лукашин", "test@mail.ru", UserRole.Teacher);
      dbManager.CreateUser(teacher);
      var data = dbManager.GetAllUsers();
      foreach (var user in data)
      {
        if (user.TelegramChatId == teacher.TelegramChatId)
        {
          teacher = user;
          break;
        }
      }

      for (int i = 1; i <= 5; i++)
      {
        // Создаем курс
        var course = new Course
        {
          CourseName = $"Test Course {i}",
          TeacherId = teacher.TelegramChatId // Здесь можно указать конкретного учителя, если требуется
        };

        dbManager.CreateCourse(course);
        Console.WriteLine($"Создан курс: {course.CourseName}");

        // Добавляем по 5 домашних заданий для каждого курса
        for (int j = 1; j <= 5; j++)
        {
          var assignment = new Assignment
          {
            CourseId = i,  // Ссылка на курс
            Title = $"Test Assignment {j} for Course {i}",
            Description = $"This is a description for Test Assignment {j} in Test Course {i}",
            DueDate = DateTime.Now.AddDays(7)  // Задаем дедлайн через неделю
          };

          // Добавляем домашнее задание в базу данных
          dbManager.CreateAssignment(assignment);
          Console.WriteLine($"Создано домашнее задание: {assignment.Title} для курса {course.CourseName}");
        }

        UserCourse userCourse = new UserCourse();
        userCourse.CourseId = i;
        userCourse.UserId = teacher.TelegramChatId;
        dbManager.CreateUserCourse(userCourse);
      }

      Console.WriteLine("Тестовые курсы и домашние задания успешно созданы.");
    }
  }
}
