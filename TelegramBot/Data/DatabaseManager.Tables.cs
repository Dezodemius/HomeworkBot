using System.Data.SQLite;
using System.IO;

namespace TelegramBot.Data
{
    public partial class DatabaseManager
    {
        /// <summary>
        /// Проверяет существование базы данных и создает ее, если она не существует.
        /// </summary>
        public void EnsureDatabaseCreated()
        {
            var dbPath = _connectionString.Replace("Data Source=", "");
            if (!File.Exists(dbPath))
            {
                SQLiteConnection.CreateFile(dbPath);
            }
        }

        /// <summary>
        /// Проверяет существование всех необходимых таблиц и создает их, если они не существуют.
        /// </summary>
        public void EnsureTablesCreated()
        {
            using var connection = new SQLiteConnection(_connectionString);
            connection.Open();

            CreateUsersTable(connection);
            CreateCoursesTable(connection);
            CreateAssignmentsTable(connection);
            CreateSubmissionsTable(connection);
            CreateRegistrationRequestsTable(connection);
            CreateUserCoursesTable(connection);
            CreateHomeworkCommandsTable(connection);
        }

        /// <summary>
        /// Создает таблицу Users, если она не существует.
        /// </summary>
        /// <param name="connection">Открытое соединение с базой данных.</param>
        private void CreateUsersTable(SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Users (
                    UserId INTEGER PRIMARY KEY AUTOINCREMENT,
                    TelegramChatId INTEGER UNIQUE NOT NULL,
                    FirstName TEXT NOT NULL,
                    LastName TEXT NOT NULL,
                    Email TEXT UNIQUE NOT NULL,
                    Role TEXT NOT NULL CHECK(Role IN ('Student', 'Teacher', 'Administrator')),
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                );";
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Создает таблицу Courses, если она не существует.
        /// </summary>
        /// <param name="connection">Открытое соединение с базой данных.</param>
        private void CreateCoursesTable(SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Courses (
                    CourseId INTEGER PRIMARY KEY AUTOINCREMENT,
                    CourseName TEXT NOT NULL,
                    Description TEXT,
                    TeacherId INTEGER,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (TeacherId) REFERENCES Users(UserId)
                );";
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Создает таблицу Assignments, если она не существует.
        /// </summary>
        /// <param name="connection">Открытое соединение с базой данных.</param>
        private void CreateAssignmentsTable(SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Assignments (
                    AssignmentId INTEGER PRIMARY KEY AUTOINCREMENT,
                    CourseId INTEGER NOT NULL,
                    Title TEXT NOT NULL,
                    Description TEXT,
                    DueDate DATETIME,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (CourseId) REFERENCES Courses(CourseId)
                );";
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Создает таблицу Submissions, если она не существует.
        /// </summary>
        /// <param name="connection">Открытое соединение с базой данных.</param>
        private void CreateSubmissionsTable(SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Submissions (
                    SubmissionId INTEGER PRIMARY KEY AUTOINCREMENT,
                    AssignmentId INTEGER NOT NULL,
                    StudentId INTEGER NOT NULL,
                    GithubLink TEXT NOT NULL,
                    SubmissionDate DATETIME DEFAULT CURRENT_TIMESTAMP,
                    Status TEXT DEFAULT 'Submitted' CHECK(Status IN ('Submitted', 'Reviewed', 'Approved', 'Rejected')),
                    TeacherComment TEXT,
                    FOREIGN KEY (AssignmentId) REFERENCES Assignments(AssignmentId),
                    FOREIGN KEY (StudentId) REFERENCES Users(UserId)
                );";
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Создает таблицу RegistrationRequests, если она не существует.
        /// </summary>
        /// <param name="connection">Открытое соединение с базой данных.</param>
        private void CreateRegistrationRequestsTable(SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS RegistrationRequests (
                    RequestId INTEGER PRIMARY KEY AUTOINCREMENT,
                    TelegramChatId INTEGER UNIQUE NOT NULL,
                    FirstName TEXT NOT NULL,
                    LastName TEXT NOT NULL,
                    Email TEXT UNIQUE NOT NULL,
                    RequestedRole TEXT NOT NULL CHECK(RequestedRole IN ('Student', 'Teacher')),
                    CourseId INTEGER NOT NULL,
                    Status TEXT DEFAULT 'Pending' CHECK(Status IN ('Pending', 'Approved', 'Rejected')),
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (CourseId) REFERENCES Courses(CourseId)
                );";
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Создает таблицу UserCourses, если она не существует.
        /// </summary>
        /// <param name="connection">Открытое соединение с базой данных.</param>
        private void CreateUserCoursesTable(SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS UserCourses (
                    UserId INTEGER,
                    CourseId INTEGER,
                    PRIMARY KEY (UserId, CourseId),
                    FOREIGN KEY (UserId) REFERENCES Users(UserId),
                    FOREIGN KEY (CourseId) REFERENCES Courses(CourseId)
                );";
            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Создает таблицу HomeworkCommands, если она не существует.
        /// </summary>
        /// <param name="connection">Открытое соединение с базой данных.</param>
        private void CreateHomeworkCommandsTable(SQLiteConnection connection)
        {
            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS HomeworkCommands (
                    CommandId INTEGER PRIMARY KEY AUTOINCREMENT,
                    AssignmentId INTEGER NOT NULL,
                    CommandText TEXT NOT NULL,
                    Description TEXT,
                    FOREIGN KEY (AssignmentId) REFERENCES Assignments(AssignmentId)
                );";
            command.ExecuteNonQuery();
        }
    }
}