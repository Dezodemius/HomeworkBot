using TelegramBot.Data;
using TelegramBot.Models;

namespace TelegramBot.Models
{
    /// <summary>
    /// Класс, представляющий запрос на регистрацию пользователя.
    /// </summary>
    public class RegistrationRequest
    {
        /// <summary>
        /// Идентификатор запроса.
        /// </summary>
        public long RequestId { get; set; }

        /// <summary>
        /// Идентификатор чата Telegram.
        /// </summary>
        public long TelegramChatId { get; set; }

        /// <summary>
        /// Имя пользователя.
        /// </summary>
        public string FirstName { get; set; }

        /// <summary>
        /// Фамилия пользователя.
        /// </summary>
        public string LastName { get; set; }

        /// <summary>
        /// Электронная почта пользователя.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Запрашиваемая роль пользователя.
        /// </summary>
        public UserRole RequestedRole { get; set; }

        /// <summary>
        /// Идентификатор курса.
        /// </summary>
        public int CourseId { get; set; }

        /// <summary>
        /// Статус запроса.
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Текущий шаг регистрации.
        /// </summary>
        public RegistrationStep Step { get; set; } = RegistrationStep.FirstName;

        /// <summary>
        /// Инициализирует новый экземпляр класса RegistrationRequest.
        /// </summary>
        /// <param name="telegramChatId">Идентификатор чата Telegram.</param>
        public RegistrationRequest(long telegramChatId)
        {
            TelegramChatId = telegramChatId;
            Status = "Pending";
            RequestedRole = UserRole.Student; // По умолчанию регистрируем как студента
        }

        /// <summary>
        /// Обрабатывает текущий шаг регистрации.
        /// </summary>
        /// <param name="message">Сообщение от пользователя.</param>
        /// <param name="dbManager">Менеджер базы данных.</param>
        /// <returns>Ответ на текущий шаг регистрации.</returns>
        public async Task<string> ProcessRegistrationStepAsync(string message, DatabaseManager dbManager)
        {
            switch (Step)
            {
                case RegistrationStep.FirstName:
                    FirstName = message;
                    Step = RegistrationStep.LastName;
                    return "Отлично! Теперь введите вашу фамилию:";

                case RegistrationStep.LastName:
                    LastName = message;
                    Step = RegistrationStep.Email;
                    return "Хорошо. Теперь введите ваш email:";

                case RegistrationStep.Email:
                    Email = message;
                    Step = RegistrationStep.Course;
                    var courses = await dbManager.GetCoursesAsync();
                    return $"Выберите курс:\n{string.Join("\n", courses.Select(c => $"{c.Id}. {c.Name}"))}";

                case RegistrationStep.Course:
                    if (int.TryParse(message, out int courseId))
                    {
                        CourseId = courseId;
                        Step = RegistrationStep.Completed;
                        await dbManager.AddRegistrationRequestAsync(this);
                        return "Ваша заявка на регистрацию принята. Пожалуйста, ожидайте подтверждения от администратора.";
                    }
                    else
                    {
                        return "Неверный формат ID курса. Пожалуйста, введите число.";
                    }

                case RegistrationStep.Completed:
                    return "Ваша регистрация уже завершена. Ожидайте подтверждения от администратора.";

                default:
                    return "Неизвестный шаг регистрации.";
            }
        }
    }

    /// <summary>
    /// Перечисление шагов регистрации.
    /// </summary>
    public enum RegistrationStep
    {
        FirstName,
        LastName,
        Email,
        Course,
        Completed
    }
}