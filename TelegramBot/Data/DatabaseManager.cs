using System.Data.SQLite;
using System.IO;
using TelegramBot.Models;
using TelegramBot.Roles;

namespace TelegramBot.Data
{
    /// <summary>
    /// Класс для управления базой данных SQLite.
    /// </summary>
    public partial class DatabaseManager
    {
        /// <summary>
        /// Строка подключения к базе данных.
        /// </summary>
        protected internal readonly string _connectionString;

        /// <summary>
        /// Инициализирует новый экземпляр класса DatabaseManager.
        /// </summary>
        /// <param name="connectionString">Строка подключения к базе данных.</param>
        public DatabaseManager(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Здесь будут методы для взаимодействия с базой данных
        // Например:

        /// <summary>
        /// Добавляет нового пользователя в базу данных.
        /// </summary>
        /// <param name="user">Объект пользователя для добавления.</param>
        public void AddUser(User user)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Users (TelegramChatId, FirstName, LastName, Email, Role)
                VALUES (@TelegramChatId, @FirstName, @LastName, @Email, @Role);";
            command.Parameters.AddWithValue("@TelegramChatId", user.TelegramChatId);
            command.Parameters.AddWithValue("@FirstName", user.FirstName);
            command.Parameters.AddWithValue("@LastName", user.LastName);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@Role", user.Role.ToString()); // Преобразуем enum в строку
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Проверяет наличие администраторов в базе данных.
        /// </summary>
        /// <returns>true, если есть хотя бы один администратор; иначе false.</returns>
        public bool HasAdministrators()
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM Users WHERE Role = 'Administrator'";
            int count = Convert.ToInt32(command.ExecuteScalar());
            return count > 0;
        }

        // Добавьте здесь другие методы для работы с базой данных

        /// <summary>
        /// Асинхронно получает роль пользователя по идентификатору чата.
        /// </summary>
        /// <param name="chatId">Идентификатор чата пользователя.</param>
        /// <returns>Роль пользователя или null, если пользователь не найден.</returns>
        public async Task<UserRole?> GetUserRoleAsync(long chatId)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT Role FROM Users WHERE TelegramChatId = @ChatId";
            command.Parameters.AddWithValue("@ChatId", chatId);
            var result = await command.ExecuteScalarAsync();
            if (result != null && Enum.TryParse<UserRole>(result.ToString(), out var role))
            {
                return role;
            }
            return null;
        }

        /// <summary>
        /// Асинхронно получает данные пользователя по идентификатору чата.
        /// </summary>
        /// <param name="chatId">Идентификатор чата пользователя.</param>
        /// <returns>Объект UserData с данными пользователя или null, если пользователь не найден.</returns>
        public async Task<User> GetUserDataAsync(long chatId)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT FirstName, LastName, Email, Role FROM Users WHERE TelegramChatId = @ChatId";
            command.Parameters.AddWithValue("@ChatId", chatId);
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User(
                    chatId,
                    reader.GetString(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    Enum.Parse<UserRole>(reader.GetString(3))
                );
            }
            return null;
        }

        /// <summary>
        /// Добавляет новый курс в базу данных.
        /// </summary>
        /// <param name="course">Объект курса для добавления.</param>
        public void AddCourse(Course course)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Courses (CourseName, Description)
                VALUES (@CourseName, @Description);";
            command.Parameters.AddWithValue("@CourseName", course.Name);
            command.Parameters.AddWithValue("@Description", course.Description);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Добавляет новое домашнее задание в базу данных.
        /// </summary>
        /// <param name="homework">Объект домашнего задания для добавления.</param>
        public void AddHomework(Homework homework)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO Assignments (CourseId, Title, Description)
                VALUES (@CourseId, @Title, @Description);";
            command.Parameters.AddWithValue("@CourseId", homework.CourseId);
            command.Parameters.AddWithValue("@Title", homework.Title);
            command.Parameters.AddWithValue("@Description", homework.Description);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Асинхронно получает список всех курсов из базы данных.
        /// </summary>
        /// <returns>Список объектов Course.</returns>
        public async Task<List<Course>> GetCoursesAsync()
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT CourseId, CourseName, Description FROM Courses";
            using var reader = await command.ExecuteReaderAsync();
            var courses = new List<Course>();
            while (await reader.ReadAsync())
            {
                courses.Add(new Course
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2)
                });
            }
            return courses;
        }

        /// <summary>
        /// Добавляет студента на курс.
        /// </summary>
        /// <param name="chatId">Идентификатор чата студента.</param>
        /// <param name="courseId">Идентификатор курса.</param>
        public void AddStudentToCourse(long chatId, int courseId)
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO UserCourses (UserId, CourseId)
                VALUES ((SELECT UserId FROM Users WHERE TelegramChatId = @ChatId), @CourseId);";
            command.Parameters.AddWithValue("@ChatId", chatId);
            command.Parameters.AddWithValue("@CourseId", courseId);
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Проверяет, зарегистрирован ли пользователь на указанный курс.
        /// </summary>
        /// <param name="chatId">Идентификатор чата пользователя.</param>
        /// <param name="courseId">Идентификатор курса.</param>
        /// <returns>True, если пользователь уже зарегистрирован на курс, иначе False.</returns>
        public async Task<bool> IsUserRegisteredForCourseAsync(long chatId, int courseId)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT COUNT(*) FROM UserCourses
                WHERE UserId = (SELECT UserId FROM Users WHERE TelegramChatId = @ChatId)
                AND CourseId = @CourseId";
            command.Parameters.AddWithValue("@ChatId", chatId);
            command.Parameters.AddWithValue("@CourseId", courseId);
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) > 0;
        }

        /// <summary>
        /// Асинхронно добавляет запрос на регистрацию в базу данных.
        /// </summary>
        /// <param name="request">Объект запроса на регистрацию.</param>
        public async Task AddRegistrationRequestAsync(RegistrationRequest request)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO RegistrationRequests (TelegramChatId, FirstName, LastName, Email, RequestedRole, CourseId, Status)
                VALUES (@ChatId, @FirstName, @LastName, @Email, @Role, @CourseId, @Status);";
            command.Parameters.AddWithValue("@ChatId", request.TelegramChatId);
            command.Parameters.AddWithValue("@FirstName", request.FirstName);
            command.Parameters.AddWithValue("@LastName", request.LastName);
            command.Parameters.AddWithValue("@Email", request.Email);
            command.Parameters.AddWithValue("@Role", request.RequestedRole.ToString());
            command.Parameters.AddWithValue("@CourseId", request.CourseId);
            command.Parameters.AddWithValue("@Status", request.Status);
            await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Асинхронно получает словарь всех заявок на регистрацию.
        /// </summary>
        /// <returns>Словарь объектов RegistrationRequest, где ключ - RequestId.</returns>
        public async Task<Dictionary<long, RegistrationRequest>> GetAllRegistrationRequestsAsync()
        {
            var requests = new Dictionary<long, RegistrationRequest>();
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM RegistrationRequests";
            
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var requestId = reader.GetInt64(reader.GetOrdinal("RequestId"));
                requests[requestId] = new RegistrationRequest(reader.GetInt64(reader.GetOrdinal("TelegramChatId")))
                {
                    RequestId = requestId,
                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    RequestedRole = Enum.Parse<UserRole>(reader.GetString(reader.GetOrdinal("RequestedRole"))),
                    CourseId = reader.GetInt32(reader.GetOrdinal("CourseId")),
                    Status = reader.GetString(reader.GetOrdinal("Status"))
                };
            }
            return requests;
        }

        /// <summary>
        /// Асинхронно получает идентификатор чата администратора.
        /// </summary>
        /// <returns>Идентификатор чата администратора или null, если администратор не найден.</returns>
        public async Task<long?> GetAdminIdAsync()
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT TelegramChatId FROM Users WHERE Role = 'Administrator' LIMIT 1";
            var result = await command.ExecuteScalarAsync();
            return result != null ? Convert.ToInt64(result) : null;
        }

        /// <summary>
        /// Асинхронно получает название курса по его идентификатору.
        /// </summary>
        /// <param name="courseId">Идентификатор курса.</param>
        /// <returns>Название курса или "Неизвестный курс", если курс не найден.</returns>
        public async Task<string> GetCourseNameAsync(int courseId)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT CourseName FROM Courses WHERE CourseId = @CourseId";
            command.Parameters.AddWithValue("@CourseId", courseId);
            var result = await command.ExecuteScalarAsync();
            return result?.ToString() ?? "Неизвестный курс";
        }

        /// <summary>
        /// Асинхронно одобряет регистрацию пользователя.
        /// </summary>
        /// <param name="chatId">Идентификатор чата пользователя.</param>
        public async Task ApproveRegistrationAsync(long chatId)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();
            using var transaction = connection.BeginTransaction();

            try
            {
                var command = connection.CreateCommand();
                command.Transaction = transaction;

                // Получаем данные из RegistrationRequests
                command.CommandText = "SELECT * FROM RegistrationRequests WHERE TelegramChatId = @ChatId";
                command.Parameters.AddWithValue("@ChatId", chatId);
                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    var firstName = reader.GetString(reader.GetOrdinal("FirstName"));
                    var lastName = reader.GetString(reader.GetOrdinal("LastName"));
                    var email = reader.GetString(reader.GetOrdinal("Email"));
                    var courseId = reader.GetInt32(reader.GetOrdinal("CourseId"));

                    reader.Close();

                    // Добавляем пользователя в таблицу Users
                    command.CommandText = @"
                        INSERT INTO Users (TelegramChatId, FirstName, LastName, Email, Role)
                        VALUES (@ChatId, @FirstName, @LastName, @Email, 'Student');";
                    command.Parameters.AddWithValue("@FirstName", firstName);
                    command.Parameters.AddWithValue("@LastName", lastName);
                    command.Parameters.AddWithValue("@Email", email);
                    await command.ExecuteNonQueryAsync();
                    Console.WriteLine($"User {chatId} added to Users table");

                    // Добавляем пользователя на курс
                    command.CommandText = @"
                        INSERT INTO UserCourses (UserId, CourseId)
                        VALUES ((SELECT UserId FROM Users WHERE TelegramChatId = @ChatId), @CourseId);";
                    command.Parameters.AddWithValue("@CourseId", courseId);
                    await command.ExecuteNonQueryAsync();
                    Console.WriteLine($"User {chatId} added to course {courseId}");

                    // Удаляем запрос на регистрацию
                    command.CommandText = "DELETE FROM RegistrationRequests WHERE TelegramChatId = @ChatId";
                    await command.ExecuteNonQueryAsync();
                    Console.WriteLine($"Registration request for user {chatId} deleted");
                }
                else
                {
                    Console.WriteLine($"No registration request found for user {chatId}");
                }

                transaction.Commit();
                Console.WriteLine($"Registration approved for user {chatId}");
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"Error approving registration: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Асинхронно отклоняет регистрацию пользователя.
        /// </summary>
        /// <param name="chatId">Идентификатор чата пользователя.</param>
        public async Task RejectRegistrationAsync(long chatId)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "UPDATE RegistrationRequests SET Status = 'Rejected' WHERE TelegramChatId = @ChatId";
            command.Parameters.AddWithValue("@ChatId", chatId);
            await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Очищает все таблицы в базе данных.
        /// </summary>
        public void ClearAllTables()
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var command = connection.CreateCommand();

                // Отключаем проверку внешних ключей на время удаления
                command.CommandText = "PRAGMA foreign_keys = OFF;";
                command.ExecuteNonQuery();

                // Список таблиц для очистки
                var tables = new[] { "Courses", "Assignments", "Users", "UserCourses", "RegistrationRequests", "HomeworkCommands" };

                foreach (var table in tables)
                {
                    command.CommandText = $"DELETE FROM {table};";
                    command.ExecuteNonQuery();
                }

                // Сбрасываем автоинкрементные счетчики
                foreach (var table in tables)
                {
                    command.CommandText = $"DELETE FROM sqlite_sequence WHERE name = '{table}';";
                    command.ExecuteNonQuery();
                }

                // Включаем обратно проверку внешних ключей
                command.CommandText = "PRAGMA foreign_keys = ON;";
                command.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Добавляет столбец CourseId в таблицу RegistrationRequests, если он еще не существует.
        /// </summary>
        public void AddCourseIdColumnToRegistrationRequests()
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();
            var command = connection.CreateCommand();

            // Проверяем, существует ли уже столбец CourseId
            command.CommandText = "PRAGMA table_info(RegistrationRequests);";
            using var reader = command.ExecuteReader();
            bool courseIdExists = false;
            while (reader.Read())
            {
                if (reader["name"].ToString() == "CourseId")
                {
                    courseIdExists = true;
                    break;
                }
            }

            // Если столбец не существует, добавляем его
            if (!courseIdExists)
            {
                command.CommandText = @"
                    ALTER TABLE RegistrationRequests
                    ADD COLUMN CourseId INTEGER NOT NULL DEFAULT 0;";
                command.ExecuteNonQuery();
                Console.WriteLine("CourseId column added to RegistrationRequests table.");
            }
            else
            {
                Console.WriteLine("CourseId column already exists in RegistrationRequests table.");
            }
        }

        /// <summary>
        /// Асинхронно получает запрос на регистрацию по идентификатору чата.
        /// </summary>
        /// <param name="chatId">Идентификатор чата пользователя.</param>
        /// <returns>Объект RegistrationRequest или null, если запрос не найден.</returns>
        public async Task<RegistrationRequest> GetRegistrationRequestAsync(long chatId)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT * FROM RegistrationRequests 
                WHERE TelegramChatId = @ChatId AND Status = 'Pending'";
            command.Parameters.AddWithValue("@ChatId", chatId);
            
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new RegistrationRequest(chatId)
                {
                    RequestId = reader.GetInt32(reader.GetOrdinal("RequestId")),
                    FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                    LastName = reader.GetString(reader.GetOrdinal("LastName")),
                    Email = reader.GetString(reader.GetOrdinal("Email")),
                    RequestedRole = Enum.Parse<UserRole>(reader.GetString(reader.GetOrdinal("RequestedRole"))),
                    CourseId = reader.GetInt32(reader.GetOrdinal("CourseId")),
                    Status = reader.GetString(reader.GetOrdinal("Status"))
                };
            }
            return null;
        }

        /// <summary>
        /// Асинхронно отправляет домашнее задание.
        /// </summary>
        /// <param name="chatId">Идентификатор чата пользователя.</param>
        /// <param name="homeworkId">Идентификатор домашнего задания.</param>
        /// <param name="githubLink">Ссылка на GitHub с выполненным заданием.</param>
        public async Task SubmitHomeworkAsync(long chatId, int homeworkId, string githubLink)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = @"
                INSERT OR REPLACE INTO Submissions (AssignmentId, StudentId, GithubLink, Status)
                VALUES (@HomeworkId, (SELECT UserId FROM Users WHERE TelegramChatId = @ChatId), @GithubLink, 'Submitted')";
            command.Parameters.AddWithValue("@HomeworkId", homeworkId);
            command.Parameters.AddWithValue("@ChatId", chatId);
            command.Parameters.AddWithValue("@GithubLink", githubLink);
            await command.ExecuteNonQueryAsync();
        }

        /// <summary>
        /// Асинхронно получает статус домашнего задания.
        /// </summary>
        /// <param name="chatId">Идентификатор чата пользователя.</param>
        /// <param name="homeworkId">Идентификатор домашнего задания.</param>
        /// <returns>Статус домашнего задания или "Not submitted", если задание не найдено.</returns>
        public async Task<string> GetHomeworkStatusAsync(long chatId, int homeworkId)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT Status FROM Submissions
                WHERE AssignmentId = @HomeworkId AND StudentId = (SELECT UserId FROM Users WHERE TelegramChatId = @ChatId)";
            command.Parameters.AddWithValue("@HomeworkId", homeworkId);
            command.Parameters.AddWithValue("@ChatId", chatId);
            var result = await command.ExecuteScalarAsync();
            return result?.ToString() ?? "Not submitted";
        }

        /// <summary>
        /// Асинхронно получает список курсов студента.
        /// </summary>
        /// <param name="chatId">Идентификатор чата студента.</param>
        /// <returns>Список курсов, на которые записан студент.</returns>
        public async Task<List<Course>> GetStudentCoursesAsync(long chatId)
        {
            using var connection = new SQLiteConnection(_connectionString);
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = @"
                SELECT c.CourseId, c.CourseName, c.Description
                FROM Courses c
                JOIN UserCourses uc ON c.CourseId = uc.CourseId
                WHERE uc.UserId = (SELECT UserId FROM Users WHERE TelegramChatId = @ChatId)";
            command.Parameters.AddWithValue("@ChatId", chatId);
            using var reader = await command.ExecuteReaderAsync();
            var courses = new List<Course>();
            while (await reader.ReadAsync())
            {
                courses.Add(new Course
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Description = reader.GetString(2)
                });
            }
            return courses;
        }
    }
}