using System.Net;
using System.Net.Http.Json;
using Model;

namespace view.Services;

public sealed class ClassApiClient
{
    private readonly HttpClient http;

    public ClassApiClient(HttpClient http)
    {
        this.http = http;
    }

    // מחזיר את הכיתות של המורה המחובר.
    public async Task<List<Classroom>> GetTeacherClassesAsync(
        int teacherId)
    {
        return await http.GetFromJsonAsync<List<Classroom>>(
            $"api/classes/teacher/{teacherId}")
            ?? new List<Classroom>();
    }

    // יוצר כיתה חדשה.
    public async Task<bool> CreateClassAsync(
        string name,
        int teacherId)
    {
        Classroom classroom = new Classroom
        {
            Name = name,
            TeacherId = teacherId
        };

        using HttpResponseMessage response =
            await http.PostAsJsonAsync(
                "api/classes",
                classroom);

        return response.IsSuccessStatusCode;
    }

    // מוחק כיתה.
    public async Task<bool> DeleteClassAsync(
        int classId,
        int teacherId)
    {
        using HttpResponseMessage response =
            await http.DeleteAsync(
                $"api/classes/{classId}?teacherId={teacherId}");

        return response.StatusCode == HttpStatusCode.NoContent;
    }
}