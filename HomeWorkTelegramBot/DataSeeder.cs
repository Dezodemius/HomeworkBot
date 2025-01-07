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
      if (!_context.Users.Any())
      {
        var users = GenerateUsers(10);
        _context.Users.AddRange(users);
        _context.SaveChanges();
      }

      if (!_context.Courses.Any())
      {
        var existingTeacherIds = _context.Users
          .Where(u => u.UserRole == User.Role.Teacher)
          .Select(u => u.ChatId)
          .ToList();
        var courses = GenerateCourses(5, existingTeacherIds);
        _context.Courses.AddRange(courses);
        _context.SaveChanges();
      }

      if (!_context.TaskWorks.Any())
      {
        // Получаем существующие CourseId
        var existingCourseIds = _context.Courses.Select(c => c.Id).ToList();
        var tasks = GenerateTaskWorks(20, existingCourseIds);
        _context.TaskWorks.AddRange(tasks);
        _context.SaveChanges(); // Сохраняем задания, чтобы получить их реальные ID
      }

      if (!_context.Answers.Any())
      {
        // Получаем существующие TaskId и UserId
        var existingTaskIds = _context.TaskWorks.Select(t => t.Id).ToList();
        var existingUserIds = _context.Users.Select(u => u.ChatId).ToList();
        var answers = GenerateAnswers(50, existingTaskIds, existingUserIds);
        _context.Answers.AddRange(answers);
        _context.SaveChanges(); // Сохраняем ответы
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
        .RuleFor(u => u.UserRole, f => f.PickRandom<User.Role>());

      return faker.Generate(count);
    }


    /// <summary>
    /// Генерирует список курсов.
    /// </summary>
    /// <param name="count">Количество курсов для генерации.</param>
    /// <returns>Список курсов.</returns>
    private List<Courses> GenerateCourses(int count, List<long> teacherIds)
    {
      var faker = new Faker<Courses>()
        .RuleFor(c => c.TeacherId, f => f.PickRandom(teacherIds)) // Используем существующие TeacherId
        .RuleFor(c => c.Name, f => f.Commerce.Department())
        .RuleFor(c => c.Description, f => f.Lorem.Sentence());

      return faker.Generate(count);
    }

    /// <summary>
    /// Генерирует список заданий.
    /// </summary>
    /// <param name="count">Количество заданий для генерации.</param>
    /// <returns>Список заданий.</returns>
    private List<TaskWork> GenerateTaskWorks(int count, List<int> courseIds)
    {
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
    private List<Answer> GenerateAnswers(int count, List<int> taskIds, List<long> userIds)
    {
      var faker = new Faker<Answer>()
        .RuleFor(a => a.AnswerText, f => f.Lorem.Sentence())
        .RuleFor(a => a.TaskId, f => f.PickRandom(taskIds)) // Используем существующие TaskId
        .RuleFor(a => a.UserId, f => f.PickRandom(userIds)) // Используем существующие UserId
        .RuleFor(a => a.Date, f => f.Date.Recent())
        .RuleFor(a => a.Status, f => f.PickRandom<Answer.TaskStatus>());

      return faker.Generate(count);
    }
  }
}