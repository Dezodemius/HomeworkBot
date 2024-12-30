using Bogus;
using HomeWorkTelegramBot.DataBase;
using HomeWorkTelegramBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HomeWorkTelegramBot
{
  internal class DataSeeder
  {
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Инициализирует новый экземпляр класса DataSeeder с указанным контекстом базы данных.
    /// </summary>
    /// <param name="context">Контекст базы данных.</param>
    public DataSeeder(ApplicationDbContext context)
    {
      _context = context;
    }

    /// <summary>
    /// Заполняет базу данных тестовыми данными, если они отсутствуют.
    /// </summary>
    public void SeedData()
    {
      if (!_context.Users.Any())
      {
        var users = GenerateUsers(10);
        _context.Users.AddRange(users);
      }

      if (!_context.Courses.Any())
      {
        var courses = GenerateCourses(5);
        _context.Courses.AddRange(courses);
      }

      if (!_context.TaskWorks.Any())
      {
        var tasks = GenerateTaskWorks(20);
        _context.TaskWorks.AddRange(tasks);
      }

      if (!_context.Answers.Any())
      {
        var answers = GenerateAnswers(50);
        _context.Answers.AddRange(answers);
      }

      //if (!_context.CourseEnrollments.Any())
      //{
      //  var enrollments = GenerateCourseEnrollments();
      //  _context.CourseEnrollments.AddRange(enrollments);
      //}

      _context.SaveChanges();
    }

    /// <summary>
    /// Генерирует список пользователей.
    /// </summary>
    /// <param name="count">Количество пользователей для генерации.</param>
    /// <returns>Список пользователей.</returns>
    private List<User> GenerateUsers(int count)
    {
      var faker = new Faker<User>()
        .RuleFor(u => u.ChatId, f => BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0))
        .RuleFor(u => u.Name, f => f.Name.FirstName())
        .RuleFor(u => u.Surname, f => f.Name.LastName())
        .RuleFor(u => u.Lastname, f => f.Name.LastName())
        .RuleFor(u => u.Email, f => f.Internet.Email())
        .RuleFor(u => u.BirthDate, f => DateOnly.FromDateTime(f.Date.Past(30, DateTime.Now.AddYears(-18))))
        .RuleFor(u => u.UserRole, f => f.PickRandom<User.Role>());

      return faker.Generate(count);
    }

    /// <summary>
    /// Генерирует список курсов.
    /// </summary>
    /// <param name="count">Количество курсов для генерации.</param>
    /// <returns>Список курсов.</returns>
    private List<Courses> GenerateCourses(int count)
    {
      var faker = new Faker<Courses>()
        .RuleFor(c => c.Name, f => f.Commerce.Department())
        .RuleFor(c => c.Description, f => f.Lorem.Sentence());

      return faker.Generate(count);
    }

    /// <summary>
    /// Генерирует список заданий.
    /// </summary>
    /// <param name="count">Количество заданий для генерации.</param>
    /// <returns>Список заданий.</returns>
    private List<TaskWork> GenerateTaskWorks(int count)
    {
      var faker = new Faker<TaskWork>()
        .RuleFor(t => t.CourseId, f => f.Random.Int(1, 5))
        .RuleFor(t => t.Name, f => f.Commerce.ProductName())
        .RuleFor(t => t.Description, f => f.Lorem.Paragraph());

      return faker.Generate(count);
    }

    /// <summary>
    /// Генерирует список ответов.
    /// </summary>
    /// <param name="count">Количество ответов для генерации.</param>
    /// <returns>Список ответов.</returns>
    private List<Answer> GenerateAnswers(int count)
    {
      var faker = new Faker<Answer>()
        .RuleFor(a => a.AnswerText, f => f.Lorem.Sentence())
        .RuleFor(a => a.CourseId, f => f.Random.Int(1, 5))
        .RuleFor(a => a.TaskId, f => f.Random.Int(1, 20))
        .RuleFor(a => a.UserId, f => BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0))
        .RuleFor(a => a.Date, f => f.Date.Recent())
        .RuleFor(a => a.Status, f => f.PickRandom<Answer.TaskStatus>());

      return faker.Generate(count);
    }

    ///// <summary>
    ///// Генерирует записи о зачислении на курсы, используя существующих пользователей и курсы.
    ///// </summary>
    ///// <returns>Список зачислений на курсы.</returns>
    //private List<CourseEnrollment> GenerateCourseEnrollments()
    //{
    //  var enrollments = new List<CourseEnrollment>();
    //  var faker = new Faker();

    //  var users = _context.Users.ToList();
    //  var courses = _context.Courses.ToList();

    //  foreach (var user in users)
    //  {
    //    var numberOfCourses = faker.Random.Int(2, Math.Min(3, courses.Count));

    //    var selectedCourses = faker.Random.Shuffle(courses)
    //        .Take(numberOfCourses);

    //    foreach (var course in selectedCourses)
    //    {
    //      var enrollment = new CourseEnrollment
    //      {
    //        UserId = user.ChatId,
    //        CourseId = course.Id
    //      };

    //      enrollments.Add(enrollment);
    //    }
    //  }

    //  return enrollments;
    //}
  }
}