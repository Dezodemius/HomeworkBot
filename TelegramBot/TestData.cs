using System.Collections.Generic;
using TelegramBot.Data;

namespace TelegramBot
{
    /// <summary>
    /// Класс, содержащий тестовые данные для курсов и домашних заданий.
    /// </summary>
    public static class TestData
    {
        /// <summary>
        /// Список тестовых курсов.
        /// </summary>
        public static List<Course> Courses = new List<Course>
        {
            new Course { Id = 1, Name = "Введение в программирование", Description = "Базовый курс для начинающих" },
            new Course { Id = 2, Name = "Алгоритмы и структуры данных", Description = "Продвинутый курс по алгоритмам" },
            new Course { Id = 3, Name = "Веб-разработка", Description = "Создание современных веб-приложений" }
        };

        /// <summary>
        /// Список тестовых домашних заданий.
        /// </summary>
        public static List<Homework> Homeworks = new List<Homework>
        {
            new Homework { Id = 1, CourseId = 1, Title = "Переменные и типы данных", Description = "Изучите основные типы данных в C#" },
            new Homework { Id = 2, CourseId = 1, Title = "Условные операторы", Description = "Практика использования if-else конструкций" },
            new Homework { Id = 3, CourseId = 2, Title = "Сортировка пузырьком", Description = "Реализуйте алгоритм сортировки пузырьком" },
            new Homework { Id = 4, CourseId = 3, Title = "HTML основы", Description = "Создайте простую HTML страницу" }
        };

        /// <summary>
        /// Метод для добавления тестовых данных в базу данных.
        /// </summary>
        /// <param name="dbManager">Менеджер базы данных.</param>
        public static void SeedTestData(DatabaseManager dbManager)
        {
            // Удаление существующих данных
            dbManager.ClearAllTables();

            // Добавление новых тестовых данных
            foreach (var course in Courses)
            {
                dbManager.AddCourse(course);
            }

            foreach (var homework in Homeworks)
            {
                dbManager.AddHomework(homework);
            }
        }
    }

    /// <summary>
    /// Класс, представляющий курс.
    /// </summary>
    public class Course
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }

    /// <summary>
    /// Класс, представляющий домашнее задание.
    /// </summary>
    public class Homework
    {
        public int Id { get; set; }
        public int CourseId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}