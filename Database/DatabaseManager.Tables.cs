using System.Data.SQLite;
using System.IO;

namespace Database
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
            CreateAssignmentsTable(connection);
            CreateSubmissionsTable(connection);
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
                    Role TEXT NOT NULL CHECK(Role IN ('Student', 'Teacher', 'Administrator'))
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
                    Title TEXT NOT NULL,
                    Description TEXT
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
                    Status TEXT DEFAULT 'Submitted' CHECK(Status IN ('Submitted', 'Reviewed', 'Approved', 'Rejected')),
                    TeacherComment TEXT,
                    FOREIGN KEY (AssignmentId) REFERENCES Assignments(AssignmentId),
                    FOREIGN KEY (StudentId) REFERENCES Users(UserId)
                );";
            command.ExecuteNonQuery();
        }
    }
}