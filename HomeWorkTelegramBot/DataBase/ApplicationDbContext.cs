using HomeWorkTelegramBot.Config;
using HomeWorkTelegramBot.DataBase.Configurations;
using HomeWorkTelegramBot.Models;
using Microsoft.EntityFrameworkCore;
using static HomeWorkTelegramBot.Config.Logger;

namespace HomeWorkTelegramBot.DataBase
{
  /// <summary>
  /// Контекст базы данных для приложения HomeWorkTelegramBot.
  /// </summary>
  internal class ApplicationDbContext : DbContext
  {
    /// <summary>
    /// Таблица пользователей.
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// Таблица ответов.
    /// </summary>
    public DbSet<Answer> Answers { get; set; }

    /// <summary>
    /// Таблица заданий.
    /// </summary>
    public DbSet<TaskWork> TaskWorks { get; set; }

    /// <summary>
    /// Таблица курсов.
    /// </summary>
    public DbSet<Courses> Courses { get; set; }

    /// <summary>
    /// Таблица курсов.
    /// </summary>
    public DbSet<CourseEnrollment> CourseEnrollments { get; set; }

    /// <summary>
    /// Таблица регистрации пользователей.
    /// </summary>
    public DbSet<UserRegistration> UserRegistrations { get; set; }

    /// <summary>
    /// Настройки модели базы данных.
    /// </summary>
    /// <param name="modelBuilder">Объект для настройки модели.</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      modelBuilder.ApplyConfiguration(new UserConfiguration());
      modelBuilder.ApplyConfiguration(new CoursesConfiguration());
      modelBuilder.ApplyConfiguration(new TaskWorConfiguration());

      modelBuilder.ApplyConfiguration(new AnswerConfiguration());
      modelBuilder.ApplyConfiguration(new UserRegistrationConfiguration());
      modelBuilder.ApplyConfiguration(new CourseEnrollmentConfiguration());

      base.OnModelCreating(modelBuilder);
    }

    /// <summary>
    /// Настраивает подключение к базе данных.
    /// </summary>
    /// <param name="optionsBuilder">Параметры конфигурации.</param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
      if (string.IsNullOrEmpty(ApplicationData.ConfigApp.DataPath))
      {
        LogError("Путь к базе данных не задан в конфигурации.");
        throw new ArgumentNullException(nameof(ApplicationData.ConfigApp.DataPath), "Путь к базе данных не может быть null.");
      }

      //var dbPath = Path.Combine(Directory.GetCurrentDirectory(), ApplicationData.ConfigApp.DataPath);
      var dbPath = Path.Combine(AppContext.BaseDirectory, ApplicationData.ConfigApp.DataPath);
      var connectionString = $"Data Source={dbPath}";
      LogInformation($"Используется строка подключения: {connectionString}");
      optionsBuilder
        .UseSqlite(connectionString)
        .EnableSensitiveDataLogging()
        .EnableDetailedErrors();
    }

    /// <summary>
    /// Проверяет существование базы данных и всех таблиц.
    /// </summary>
    public void CheckDatabaseAndTables()
    {
      LogInformation("Проверка существования базы данных и таблиц начата.");

      try
      {
        if (Database.EnsureCreated())
        {
          LogInformation("База данных была создана.");
        }
        else
        {
          LogInformation("База данных уже существует.");
        }

        if (Database.CanConnect())
        {
          LogInformation("Подключение к базе данных успешно.");

          var tablesExist = Answers.Any() || Users.Any() || TaskWorks.Any() || Courses.Any() || CourseEnrollments.Any();

          if (tablesExist)
          {
            LogInformation("Все таблицы существуют и доступны.");
          }
          else
          {
            LogError("Не удалось найти данные в одной или нескольких таблицах.");
          }
        }
        else
        {
          LogError("Не удалось подключиться к базе данных.");
        }
      }
      catch (Exception ex)
      {
        LogException(ex);
        throw;
      }
    }

    /// <summary>
    /// Удаляет базу данных, если она существует.
    /// </summary>
    public void DeleteDatabase()
    {
      LogInformation("Попытка удаления базы данных.");
      try
      {
        if (Database.EnsureDeleted())
        {
          LogInformation("База данных успешно удалена.");
        }
        else
        {
          LogInformation("База данных не была удалена, так как она не существует.");
        }
      }
      catch (Exception ex)
      {
        LogException(ex);
        throw;
      }
    }

    /// <summary>
    /// Проверяет, есть ли данные в таблице для указанного типа.
    /// </summary>
    /// <typeparam name="T">Тип сущности для проверки.</typeparam>
    /// <returns>True, если в таблице есть данные, иначе false.</returns>
    static public bool HasData<T>() where T : class
    {
      return new ApplicationDbContext().Set<T>().Any();
    }
  }
}