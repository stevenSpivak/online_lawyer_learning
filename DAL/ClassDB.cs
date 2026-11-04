using System;
using System.Collections.Generic;
using System.Text;
using Model;
using MySql.Data.MySqlClient;

namespace DAL
{
    public class ClassDB
    {
        private readonly string connectionString;

        public ClassDB(string connectionString)
        {
            this.connectionString = connectionString;
        }

        // מחזיר את כל הכיתות של מורה מסוים.
        public async Task<List<Classroom>> GetClassesForTeacherAsync(
            int teacherId)
        {
            string sql = @"
                SELECT
                    c.ClassID,
                    c.ClassName,
                    c.ClassCode,
                    c.TeacherID,
                    c.CreatedAt,
                    COUNT(ca.UserID) AS StudentCount
                FROM Classes c
                LEFT JOIN ClassAttendees ca
                    ON ca.ClassID = c.ClassID
                WHERE c.TeacherID = @TeacherID
                GROUP BY
                    c.ClassID,
                    c.ClassName,
                    c.ClassCode,
                    c.TeacherID,
                    c.CreatedAt
                ORDER BY c.CreatedAt DESC;";

            using var connection = new MySqlConnection(connectionString);

            await connection.OpenAsync();

            using var command =
                new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@TeacherID",
                teacherId);

            var classes = new List<Classroom>();

            using var reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                classes.Add(new Classroom
                {
                    Id = reader.GetInt32(
                        reader.GetOrdinal("ClassID")),

                    Name = reader.GetString(
                        reader.GetOrdinal("ClassName")),

                    Code = reader.GetString(
                        reader.GetOrdinal("ClassCode")),

                    TeacherId = reader.GetInt32(
                        reader.GetOrdinal("TeacherID")),

                    Students = Convert.ToInt32(
                        reader["StudentCount"]),

                    CreatedAt = reader.GetDateTime(
                        reader.GetOrdinal("CreatedAt"))
                });
            }

            return classes;
        }

        // יוצר כיתה חדשה.
        public async Task<int> CreateClassAsync(
            string name,
            int teacherId)
        {
            string code = await GenerateUniqueCodeAsync();

            string sql = @"
                INSERT INTO Classes
                    (ClassName, ClassCode, TeacherID)
                VALUES
                    (@ClassName, @ClassCode, @TeacherID);";

            using var connection =
                new MySqlConnection(connectionString);

            await connection.OpenAsync();

            using var command =
                new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@ClassName",
                name);

            command.Parameters.AddWithValue(
                "@ClassCode",
                code);

            command.Parameters.AddWithValue(
                "@TeacherID",
                teacherId);

            await command.ExecuteNonQueryAsync();

            return (int)command.LastInsertedId;
        }

        // מוחק כיתה רק אם היא שייכת למורה שביקש למחוק אותה.
        public async Task<bool> DeleteClassAsync(
            int classId,
            int teacherId)
        {
            string sql = @"
                DELETE FROM Classes
                WHERE ClassID = @ClassID
                AND TeacherID = @TeacherID;";

            using var connection =
                new MySqlConnection(connectionString);

            await connection.OpenAsync();

            using var command =
                new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@ClassID",
                classId);

            command.Parameters.AddWithValue(
                "@TeacherID",
                teacherId);

            return await command.ExecuteNonQueryAsync() > 0;
        }

        // בודק אם למורה כבר קיימת כיתה בשם הזה.
        public async Task<bool> ClassNameExistsAsync(
            string name,
            int teacherId)
        {
            string sql = @"
                SELECT EXISTS(
                    SELECT 1
                    FROM Classes
                    WHERE ClassName = @ClassName
                    AND TeacherID = @TeacherID
                );";

            using var connection =
                new MySqlConnection(connectionString);

            await connection.OpenAsync();

            using var command =
                new MySqlCommand(sql, connection);

            command.Parameters.AddWithValue(
                "@ClassName",
                name);

            command.Parameters.AddWithValue(
                "@TeacherID",
                teacherId);

            return Convert.ToInt64(
                await command.ExecuteScalarAsync()) == 1;
        }

        // יוצר קוד קצר לכיתה ובודק שהוא ייחודי.
        private async Task<string> GenerateUniqueCodeAsync()
        {
            while (true)
            {
                string code =
                    Guid.NewGuid()
                        .ToString("N")[..4]
                        .ToUpperInvariant();

                string sql = @"
                    SELECT EXISTS(
                        SELECT 1
                        FROM Classes
                        WHERE ClassCode = @ClassCode
                    );";

                using var connection =
                    new MySqlConnection(connectionString);

                await connection.OpenAsync();

                using var command =
                    new MySqlCommand(sql, connection);

                command.Parameters.AddWithValue(
                    "@ClassCode",
                    code);

                bool exists =
                    Convert.ToInt64(
                        await command.ExecuteScalarAsync()) == 1;

                if (!exists)
                {
                    return code;
                }
            }
        }
    }
}