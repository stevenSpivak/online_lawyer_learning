using System.Data.Common;
using Model;
using MySql.Data.MySqlClient;

namespace DAL;

public class LawCatalog
{
    private readonly string connectionString;

    public LawCatalog(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public async Task<List<Law>> GetAllAsync(
        string? search = null,
        int? categoryId = null)
    {
        const string sql = @"
            SELECT
                l.LawID,
                l.Title,
                l.Content,
                l.CategoryID,
                c.CategoryName,
                l.CreatedAt
            FROM Laws l
            INNER JOIN Categories c
                ON c.CategoryID = l.CategoryID
            WHERE
                (@Search IS NULL
                 OR l.Title LIKE @Search
                 OR l.Content LIKE @Search
                 OR c.CategoryName LIKE @Search)
                AND
                (@CategoryID IS NULL
                 OR l.CategoryID = @CategoryID)
            ORDER BY l.Title;";

        var laws = new List<Law>();

        using var connection =
            new MySqlConnection(connectionString);

        await connection.OpenAsync();

        using var command =
            new MySqlCommand(sql, connection);

        object searchValue =
            string.IsNullOrWhiteSpace(search)
                ? DBNull.Value
                : $"%{search.Trim()}%";

        command.Parameters.AddWithValue(
            "@Search",
            searchValue);

        object categoryValue =
            categoryId.HasValue
                ? categoryId.Value
                : DBNull.Value;

        command.Parameters.AddWithValue(
            "@CategoryID",
            categoryValue);

        using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            laws.Add(ReadLaw(reader));
        }

        return laws;
    }


    public async Task<Law?> GetByIdAsync(int lawId)
    {
        const string sql = @"
            SELECT
                l.LawID,
                l.Title,
                l.Content,
                l.CategoryID,
                c.CategoryName,
                l.CreatedAt
            FROM Laws l
            INNER JOIN Categories c
                ON c.CategoryID = l.CategoryID
            WHERE l.LawID = @LawID
            LIMIT 1;";

        using var connection =
            new MySqlConnection(connectionString);

        await connection.OpenAsync();

        using var command =
            new MySqlCommand(sql, connection);

        command.Parameters.AddWithValue(
            "@LawID",
            lawId);

        using var reader =
            await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return ReadLaw(reader);
        }

        return null;
    }


    public async Task<List<LawCategory>> GetCategoriesAsync()
    {
        const string sql = @"
            SELECT
                CategoryID,
                CategoryName
            FROM Categories
            ORDER BY CategoryName;";

        var categories = new List<LawCategory>();

        using var connection =
            new MySqlConnection(connectionString);

        await connection.OpenAsync();

        using var command =
            new MySqlCommand(sql, connection);

        using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            categories.Add(new LawCategory
            {
                Id = reader.GetInt32(
                    reader.GetOrdinal("CategoryID")),

                Name = reader.GetString(
                    reader.GetOrdinal("CategoryName"))
            });
        }

        return categories;
    }


    private static Law ReadLaw(DbDataReader reader)
    {
        int createdAtIndex =
            reader.GetOrdinal("CreatedAt");

        return new Law
        {
            Id = reader.GetInt32(
                reader.GetOrdinal("LawID")),

            Title = reader.GetString(
                reader.GetOrdinal("Title")),

            Content = reader.GetString(
                reader.GetOrdinal("Content")),

            CategoryId = reader.GetInt32(
                reader.GetOrdinal("CategoryID")),

            CategoryName = reader.GetString(
                reader.GetOrdinal("CategoryName")),

            CreatedAt =
                reader.IsDBNull(createdAtIndex)
                    ? null
                    : reader.GetDateTime(createdAtIndex)
        };
    }
}