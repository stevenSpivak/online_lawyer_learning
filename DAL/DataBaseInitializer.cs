using System.Data;
using MySql.Data.MySqlClient;

namespace DAL
{
    public static class DataBaseInitializer
    {
        private const string DatabaseName = "OLLDB";

        public static async Task InitializeAsync(string serverConnectionString)
        {
            await CreateDatabaseAsync(serverConnectionString);

            string dbConnectionString = $"{serverConnectionString}Database={DatabaseName};";

            await CreateTablesAsync(dbConnectionString);
            await SeedRolesAndCategoriesAsync(dbConnectionString);
        }

        private static async Task CreateDatabaseAsync(string serverConnectionString)
        {
            string sql = $"CREATE DATABASE IF NOT EXISTS `{DatabaseName}` CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;";

            using var connection = new MySqlConnection(serverConnectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand(sql, connection);

            await command.ExecuteNonQueryAsync();
        }

        private static async Task CreateTablesAsync(string dbConnectionString)
        {
            string createTablesSql = @"
                CREATE TABLE IF NOT EXISTS Roles (
                    RoleID INT AUTO_INCREMENT PRIMARY KEY,
                    RoleName VARCHAR(30) NOT NULL UNIQUE
                ) ENGINE=InnoDB;

                CREATE TABLE IF NOT EXISTS Users (
                    UserID INT AUTO_INCREMENT PRIMARY KEY,
                    Username VARCHAR(50) NOT NULL UNIQUE,
                    Email VARCHAR(100) NOT NULL UNIQUE,
                    PasswordHash VARCHAR(255) NOT NULL,
                    RoleID INT NOT NULL,
                    IsActive BOOLEAN NOT NULL DEFAULT TRUE,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID) ON DELETE RESTRICT ON UPDATE CASCADE
                ) ENGINE=InnoDB;

                CREATE TABLE IF NOT EXISTS Categories (
                    CategoryID INT AUTO_INCREMENT PRIMARY KEY,
                    CategoryName VARCHAR(50) NOT NULL UNIQUE
                ) ENGINE=InnoDB;

                CREATE TABLE IF NOT EXISTS Laws (
                    LawID INT AUTO_INCREMENT PRIMARY KEY,
                    Title VARCHAR(150) NOT NULL,
                    Content TEXT NOT NULL,
                    CategoryID INT NOT NULL,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID) ON DELETE RESTRICT ON UPDATE CASCADE
                ) ENGINE=InnoDB;

                CREATE TABLE IF NOT EXISTS Classes (
                    ClassID INT AUTO_INCREMENT PRIMARY KEY,
                    ClassName VARCHAR(100) NOT NULL,
                    ClassCode VARCHAR(10) NOT NULL UNIQUE,
                    TeacherID INT NOT NULL,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (TeacherID) REFERENCES Users(UserID) ON DELETE RESTRICT ON UPDATE CASCADE
                ) ENGINE=InnoDB;

                CREATE TABLE IF NOT EXISTS ClassAttendees (
                    ClassID INT NOT NULL,
                    UserID INT NOT NULL,
                    JoinedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    PRIMARY KEY (ClassID, UserID),
                    FOREIGN KEY (ClassID) REFERENCES Classes(ClassID) ON DELETE CASCADE ON UPDATE CASCADE,
                    FOREIGN KEY (UserID) REFERENCES Users(UserID) ON DELETE CASCADE ON UPDATE CASCADE
                ) ENGINE=InnoDB;

                CREATE TABLE IF NOT EXISTS Inquiries (
                    InquiryID INT AUTO_INCREMENT PRIMARY KEY,
                    StudentID INT NOT NULL,
                    QuestionText TEXT NOT NULL,
                    AIResponseText TEXT NULL,
                    TeacherAnswerText TEXT NULL,
                    TeacherID INT NULL,
                    CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                    FOREIGN KEY (StudentID) REFERENCES Users(UserID) ON DELETE CASCADE ON UPDATE CASCADE,
                    FOREIGN KEY (TeacherID) REFERENCES Users(UserID) ON DELETE SET NULL ON UPDATE CASCADE
                ) ENGINE=InnoDB;
            ";

            using var connection = new MySqlConnection(dbConnectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand(createTablesSql, connection);
            await command.ExecuteNonQueryAsync();
        }

        private static async Task SeedRolesAndCategoriesAsync(string dbConnectionString)
        {
            using var connection = new MySqlConnection(dbConnectionString);
            await connection.OpenAsync();

            string checkSqlR = "SELECT COUNT(*) FROM Roles;";
            using var checkCmdR = new MySqlCommand(checkSqlR, connection);
            long countR = Convert.ToInt64(await checkCmdR.ExecuteScalarAsync());

            if (countR == 0)
            {
                string insertRolesSql = @"
                    INSERT INTO Roles (RoleName) VALUES 
                    ('Admin'),
                    ('Teacher'),
                    ('Student');
                ";
                using var insertCmdR = new MySqlCommand(insertRolesSql, connection);
                await insertCmdR.ExecuteNonQueryAsync();
            }

            string checkSqlC = "SELECT COUNT(*) FROM Categories;";
            using var checkCmdC = new MySqlCommand(checkSqlC, connection);
            long countC = Convert.ToInt64(await checkCmdC.ExecuteScalarAsync());

            if (countC == 0)
            {
                string insertCategoriesSql = @"
                    INSERT INTO Categories (CategoryName) VALUES 
                    ('Tort Law'),
                    ('Student Rights'),
                    ('Traffic & Roads');
                ";
                using var insertCmdC = new MySqlCommand(insertCategoriesSql, connection);
                await insertCmdC.ExecuteNonQueryAsync();
            }
        }
    }
}