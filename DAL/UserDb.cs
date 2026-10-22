using System.Data.Common;
using Model;
using MySql.Data.MySqlClient;

namespace DAL
{
    public class UserDB
    {
        private readonly string connectionString;

        private const string SelectUserSql = @"
            SELECT u.UserID, u.Username, u.Email, u.PasswordHash, u.RoleID, r.RoleName, u.IsActive, u.CreatedAt
            FROM Users u
            INNER JOIN Roles r ON u.RoleID = r.RoleID";

        public UserDB(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public async Task<int> AddUserAsync(User user, string roleName = "Student")
        {
            string sql = @"
                INSERT INTO Users (Username, Email, PasswordHash, IsActive, RoleID)
                SELECT @Username, @Email, @PasswordHash, @IsActive, RoleID
                FROM Roles
                WHERE RoleName = @RoleName;";

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Username", user.Username);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@PasswordHash", user.PasswordHash);
            command.Parameters.AddWithValue("@IsActive", user.IsActive);
            command.Parameters.AddWithValue("@RoleName", roleName);

            int rows = await command.ExecuteNonQueryAsync();

            if (rows == 0)
                throw new InvalidOperationException($"Role '{roleName}' not found.");

            user.Id = (int)command.LastInsertedId;
            return user.Id;
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            string sql = "SELECT EXISTS(SELECT 1 FROM Users WHERE Username = @Username);";

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Username", username);

            return Convert.ToInt64(await command.ExecuteScalarAsync()) == 1;
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            string sql = "SELECT EXISTS(SELECT 1 FROM Users WHERE Email = @Email);";

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", email);

            return Convert.ToInt64(await command.ExecuteScalarAsync()) == 1;
        }
        public async Task<bool> EmailExistsForAnotherUserAsync(int userId, string email)
        {
            string sql = @"
                SELECT EXISTS(
                    SELECT 1
                    FROM Users
                    WHERE Email = @Email
                    AND UserID <> @UserID
                );";

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", email);
            command.Parameters.AddWithValue("@UserID", userId);

            return Convert.ToInt64(await command.ExecuteScalarAsync()) == 1;
        }

        public async Task<User?> CheckLoginAsync(string username, string passwordHash)
        {
            string sql = SelectUserSql + " WHERE u.Username = @Username AND u.IsActive = TRUE;";

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Username", username);

            using var reader = await command.ExecuteReaderAsync();
            if (!await reader.ReadAsync())
                return null;

            User user = ReadUser(reader);

            return user.PasswordHash == passwordHash ? user : null;
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            string sql = SelectUserSql + " WHERE u.UserID = @UserID;";

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@UserID", userId);

            using var reader = await command.ExecuteReaderAsync();
            return await reader.ReadAsync() ? ReadUser(reader) : null;
        }

        public async Task<List<User>> GetAllUsersAsync()
        {
            string sql = SelectUserSql + " ORDER BY u.Username;";

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand(sql, connection);

            var users = new List<User>();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                users.Add(ReadUser(reader));

            return users;
        }

        public async Task<Dictionary<string, int>> CountUsersByRoleAsync()
        {
            string sql = @"
                SELECT r.RoleName, COUNT(u.UserID) AS UserCount
                FROM Roles r
                LEFT JOIN Users u ON u.RoleID = r.RoleID
                GROUP BY r.RoleID, r.RoleName
                ORDER BY r.RoleID;";

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand(sql, connection);

            var counts = new Dictionary<string, int>();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                counts[reader.GetString(reader.GetOrdinal("RoleName"))] = Convert.ToInt32(reader["UserCount"]);

            return counts;
        }

        public async Task<bool> UpdateUserRoleAsync(int userId, string roleName)
        {
            string sql = @"
                UPDATE Users u
                INNER JOIN Roles r ON r.RoleName = @RoleName
                SET u.RoleID = r.RoleID
                WHERE u.UserID = @UserID;";

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@RoleName", roleName);
            command.Parameters.AddWithValue("@UserID", userId);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash)
        {
            string sql = "UPDATE Users SET PasswordHash = @PasswordHash WHERE UserID = @UserID;";

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@PasswordHash", newPasswordHash);
            command.Parameters.AddWithValue("@UserID", userId);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> UpdateEmailAsync(int userId, string newEmail)
        {
            string sql = "UPDATE Users SET Email = @Email WHERE UserID = @UserID;";

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", newEmail);
            command.Parameters.AddWithValue("@UserID", userId);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> UpdateUserStatusAsync(int userId, bool isActive)
        {
            string sql = "UPDATE Users SET IsActive = @IsActive WHERE UserID = @UserID;";

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@IsActive", isActive);
            command.Parameters.AddWithValue("@UserID", userId);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteUserAsync(int userId)
        {
            string sql = "DELETE FROM Users WHERE UserID = @UserID;";

            using var connection = new MySqlConnection(connectionString);
            await connection.OpenAsync();
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@UserID", userId);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        private static User ReadUser(DbDataReader reader)
        {
            int createdAtIndex = reader.GetOrdinal("CreatedAt");

            return new User(
                reader.GetInt32(reader.GetOrdinal("UserID")),
                reader.GetString(reader.GetOrdinal("Username")),
                reader.GetString(reader.GetOrdinal("Email")),
                reader.GetString(reader.GetOrdinal("PasswordHash")),
                reader.GetInt32(reader.GetOrdinal("RoleID")),
                reader.GetString(reader.GetOrdinal("RoleName")),
                reader.GetBoolean(reader.GetOrdinal("IsActive")),
                reader.IsDBNull(createdAtIndex) ? DateTime.MinValue : reader.GetDateTime(createdAtIndex));
        }
    }
}