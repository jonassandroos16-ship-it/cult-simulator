using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.JSInterop;

namespace CultSimulator.Game;

public class CloudSaveService
{
    private readonly HttpClient _http;
    private readonly IJSRuntime _js;
    private string? _gistId;
    private string? _token;

    private const string GistIdKey = "cult_simulator_gist_id";
    private const string TokenKey = "cult_simulator_github_token";
    private const string GistFilename = "cult-simulator-save.json";
    private const string GistDescription = "Cult Simulator Cloud Save";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public CloudSaveService(HttpClient http, IJSRuntime js)
    {
        _http = http;
        _js = js;
    }

    public bool HasToken => !string.IsNullOrWhiteSpace(_token);

    public async Task InitAsync()
    {
        try
        {
            _token = await _js.InvokeAsync<string>("localStorage.getItem", TokenKey);
            _gistId = await _js.InvokeAsync<string>("localStorage.getItem", GistIdKey);
        }
        catch { }
    }

    public async Task SetTokenAsync(string token)
    {
        _token = string.IsNullOrWhiteSpace(token) ? null : token.Trim();
        try
        {
            if (_token != null)
                await _js.InvokeVoidAsync("localStorage.setItem", TokenKey, _token);
            else
                await _js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
        }
        catch { }
    }

    public async Task<bool> ValidateTokenAsync()
    {
        if (string.IsNullOrWhiteSpace(_token)) return false;
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "https://api.github.com/user");
            req.Headers.Add("Authorization", $"token {_token}");
            req.Headers.Add("Accept", "application/vnd.github+json");
            req.Headers.Add("User-Agent", "CultSimulator");
            var resp = await _http.SendAsync(req);
            return resp.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task SaveToCloudAsync(string json)
    {
        if (string.IsNullOrWhiteSpace(_token)) return;
        try
        {
            if (string.IsNullOrWhiteSpace(_gistId))
            {
                await CreateGistAndSaveAsync(json);
            }
            else
            {
                await UpdateGistAsync(json);
            }
        }
        catch { }
    }

    private async Task CreateGistAndSaveAsync(string json)
    {
        var payload = new
        {
            description = GistDescription,
            @public = false,
            files = new Dictionary<string, object>
            {
                [GistFilename] = new { content = json }
            }
        };
        var req = new HttpRequestMessage(HttpMethod.Post, "https://api.github.com/gists")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, JsonOpts), Encoding.UTF8, "application/json")
        };
        req.Headers.Add("Authorization", $"token {_token}");
        req.Headers.Add("Accept", "application/vnd.github+json");
        req.Headers.Add("User-Agent", "CultSimulator");

        var resp = await _http.SendAsync(req);
        if (!resp.IsSuccessStatusCode) return;

        var body = await resp.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(body);
        _gistId = doc.RootElement.GetProperty("id").GetString();
        if (_gistId != null)
        {
            try { await _js.InvokeVoidAsync("localStorage.setItem", GistIdKey, _gistId); } catch { }
        }
    }

    private async Task UpdateGistAsync(string json)
    {
        var payload = new
        {
            files = new Dictionary<string, object>
            {
                [GistFilename] = new { content = json }
            }
        };
        var req = new HttpRequestMessage(HttpMethod.Patch, $"https://api.github.com/gists/{_gistId}")
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, JsonOpts), Encoding.UTF8, "application/json")
        };
        req.Headers.Add("Authorization", $"token {_token}");
        req.Headers.Add("Accept", "application/vnd.github+json");
        req.Headers.Add("User-Agent", "CultSimulator");

        await _http.SendAsync(req);
    }

    public async Task<string?> LoadFromCloudAsync()
    {
        if (string.IsNullOrWhiteSpace(_token) || string.IsNullOrWhiteSpace(_gistId)) return null;
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, $"https://api.github.com/gists/{_gistId}");
            req.Headers.Add("Authorization", $"token {_token}");
            req.Headers.Add("Accept", "application/vnd.github+json");
            req.Headers.Add("User-Agent", "CultSimulator");

            var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode) return null;

            var body = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(body);
            if (!doc.RootElement.TryGetProperty("files", out var files) || !files.TryGetProperty(GistFilename, out var file))
                return null;
            if (file.TryGetProperty("content", out var content))
                return content.GetString();
            return null;
        }
        catch { return null; }
    }
}
