using System.Net;
using System.Net.Http.Json;
using Model;

namespace view.Services;

public sealed class LawApiClient
{
    private readonly HttpClient http;

    public LawApiClient(HttpClient http)
    {
        this.http = http;
    }

    // Retrieve laws, optionally filtering by text or category.
    public async Task<List<Law>> GetLawsAsync(
        string? search = null,
        int? categoryId = null)
    {
        var query = new List<string>();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query.Add(
                $"search={Uri.EscapeDataString(search.Trim())}");
        }

        if (categoryId.HasValue)
        {
            query.Add($"categoryId={categoryId.Value}");
        }

        string url = "api/laws";

        if (query.Count > 0)
        {
            url += "?" + string.Join("&", query);
        }

        return await http.GetFromJsonAsync<List<Law>>(url)
            ?? new List<Law>();
    }

    // Retrieve a single law.
    public async Task<Law?> GetLawAsync(int id)
    {
        using var response =
            await http.GetAsync($"api/laws/{id}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<Law>();
    }

    // Retrieve categories for the search filter.
    public async Task<List<LawCategory>> GetCategoriesAsync()
    {
        return await http.GetFromJsonAsync<List<LawCategory>>(
            "api/laws/categories")
            ?? new List<LawCategory>();
    }
}