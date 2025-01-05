using Bogus;
using HomeWorkTelegramBot.DataBase;
using HomeWorkTelegramBot.Models;

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
      try
      {
        // Сначала создаем пользователей (включая преподавателей)
        if (!_context.Users.Any())
        {
          var users = GenerateUsers(10);

          // Убеждаемся, что среди пользователей есть хотя бы один преподаватель
          if (!users.Any(u => u.UserRole == User.Role.Teacher))
          {
            var teacher = new User
            {
              ChatId = BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0),
              Name = "Default",
              Surname = "Teacher",
              Email = "teacher@example.com",
              UserRole = User.Role.Teacher,
              BirthDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-30))
            };
            users.Add(teacher);
          }

          _context.Users.AddRange(users);
          _context.SaveChanges();
        }

        if (!_context.Courses.Any())
        {
          var courses = GenerateCourses(5);
          _context.Courses.AddRange(courses);
          _context.SaveChanges();
        }

        if (!_context.TaskWorks.Any())
        {
          var tasks = GenerateTaskWorks(20);
          _context.TaskWorks.AddRange(tasks);
          _context.SaveChanges();
        }

        if (!_context.Answers.Any())
        {
          var userIds = _context.Users.Select(u => u.ChatId).ToList();
          var courseIds = _context.Courses.Select(c => c.Id).ToList();
          var taskWorkIds = _context.TaskWorks.Select(t => t.Id).ToList();

          if (userIds.Any() && courseIds.Any() && taskWorkIds.Any())
          {
            var answers = GenerateAnswers(50, userIds, courseIds, taskWorkIds);
            _context.Answers.AddRange(answers);
            _context.SaveChanges();
          }
        }
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error in SeedData: {ex.Message}");
        throw;
      }
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
        .RuleFor(u => u.UserRole, f => f.Random.Int(1, 10) <= 3 ? User.Role.Teacher : User.Role.Student);

      return faker.Generate(count);
    }

    /// <summary>
    /// Генерирует список курсов.
    /// </summary>
    /// <param name="count">Количество курсов для генерации.</param>
    /// <returns>Список курсов.</returns>
    private List<Courses> GenerateCourses(int count)
    {
      var teacherIds = _context.Users
          .Where(u => u.UserRole == User.Role.Teacher)
          .Select(u => u.ChatId)
          .ToList();

      // Если преподавателей нет, создаем хотя бы одного
      if (!teacherIds.Any())
      {
        var teacher = new User
        {
          ChatId = BitConverter.ToInt64(Guid.NewGuid().ToByteArray(), 0),
          Name = "Default",
          Surname = "Teacher",
          Email = "teacher@example.com",
          UserRole = User.Role.Teacher,
          BirthDate = DateOnly.FromDateTime(DateTime.Now.AddYears(-30))
        };
        _context.Users.Add(teacher);
        _context.SaveChanges();
        teacherIds.Add(teacher.ChatId);
      }

      var faker = new Faker<Courses>()
        .RuleFor(c => c.Name, f => f.Commerce.Department())
        .RuleFor(c => c.Description, f => f.Lorem.Sentence())
        .RuleFor(c => c.TeacherId, f => f.PickRandom(teacherIds));

      return faker.Generate(count);
    }

    /// <summary>
    /// Генерирует список заданий.
    /// </summary>
    /// <param name="count">Количество заданий для генерации.</param>
    /// <returns>Список заданий.</returns>
    private List<TaskWork> GenerateTaskWorks(int count)
    {
      var courseIds = _context.Courses.Select(c => c.Id).ToList();

      var faker = new Faker<TaskWork>()
        .RuleFor(t => t.CourseId, f => f.PickRandom(courseIds))
        .RuleFor(t => t.Name, f => f.Commerce.ProductName())
        .RuleFor(t => t.Description, f => f.Lorem.Paragraph());

      return faker.Generate(count);
    }

    /// <summary>
    /// Генерирует список ответов.
    /// </summary>
    /// <param name="count">Количество ответов для генерации.</param>
    /// <returns>Список ответов.</returns>
    private List<Answer> GenerateAnswers(int count, List<long> userIds, List<int> courseIds, List<int> taskWorkIds)
    {
      var faker = new Faker<Answer>()
        .RuleFor(a => a.AnswerText, f => f.Lorem.Sentence()).RuleFor(a => a.CourseId, f => f.PickRandom(courseIds))
        .RuleFor(a => a.CourseId, f => f.PickRandom(courseIds))
        .RuleFor(a => a.TaskId, f => f.PickRandom(taskWorkIds))
        .RuleFor(a => a.UserId, f => f.PickRandom(userIds))
        .RuleFor(a => a.Date, f => f.Date.Recent())
        .RuleFor(a => a.Status, f => f.PickRandom<Answer.TaskStatus>());

      return faker.Generate(count);
    }
  }
}