using System.Net;
using System.Text.Json.Serialization;
using MangaShelf.Controllers;
using Microsoft.AspNetCore.Mvc;

interface IAuthClient
{
    Task LogoutAsync(string? returnUrl = null);
    Task LoginAsync(string username, string password, bool rememberMe, string? returnUrl = null);
    Task<ApiResponse<UserDto>> RegisterAsync(InputModel model, string? returnUrl = null);
}

public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public T? Data { get; set; }
    public IEnumerable<string> Errors { get; set; } = new List<string>();

    public static ApiResponse<T> Success(T data) =>
        new() { IsSuccess = true, Data = data };

    public static ApiResponse<T> Fail(string error) =>
        new() { IsSuccess = false, Errors = new List<string> { error } };
    public static ApiResponse<T> Fail(IEnumerable<string> errors) =>
        new() { IsSuccess = false, Errors = errors };
}


class AuthClient : IAuthClient
{
    private readonly HttpClient httpClient;

    public AuthClient(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task LogoutAsync(string? returnUrl = null)
    {
    }

    public async Task LoginAsync(string username, string password, bool rememberMe, string? returnUrl = null)
    {
    }

    public async Task<ApiResponse<UserDto>> RegisterAsync(InputModel model, string? returnUrl = null)
    {
        var content = new StringContent(System.Text.Json.JsonSerializer.Serialize(model), System.Text.Encoding.UTF8, "application/json");
        var response = await httpClient.PostAsync($"Auth/Register", content);

        return await HandleErrorResponse<UserDto>(response);
    }

    private async Task<ApiResponse<T>> HandleErrorResponse<T>(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadFromJsonAsync<IEnumerable<string>>();
            return ApiResponse<T>.Fail(errorContent);
        }

        var responseData = await response.Content.ReadAsStringAsync();
        var data = System.Text.Json.JsonSerializer.Deserialize<T>(responseData);
        return ApiResponse<T>.Success(data);
    }
}

public class UserDto
{
    [JsonPropertyName("identityUserId")]
    public string IdentityUserId { get; set; } = string.Empty;
    [JsonPropertyName("visibleUsername")]
    public string? VisibleUsername { get; set; }
}