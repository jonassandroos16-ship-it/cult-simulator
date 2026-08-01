using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.JSInterop;

namespace CultSimulator.Game;

public class CloudSaveService
{
    private readonly HttpClient _http;
    private readonly string _supabaseUrl;
    private readonly string _supabaseKey;
    private string? _saveId;

    private const string SaveIdKey = "cult_simulator_save_id";
    private const string SavesTable = "saves";

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public CloudSaveService(HttpClient http)
    {
        _http = http;
        _supabaseUrl = Environment.GetEnvironmentVariable("VITE_SUPABASE_URL")
            ?? "https://0ec90b57d6e95fcbda19832f.supabase.co";
        _supabaseKey = Environment.GetEnvironmentVariable("VITE_SUPABASE_ANON_KEY")
            ?? "";
    }

    public async Task<string> GetOrCreateSaveIdAsync(IJSRuntime js)
    {
        if (_saveId != null) return _saveId;
        try
        {
            _saveId = await js.InvokeAsync<string>("localStorage.getItem", SaveIdKey);
            if (string.IsNullOrWhiteSpace(_saveId))
            {
                _saveId = Guid.NewGuid().ToString("N");
                await js.InvokeVoidAsync("localStorage.setItem", SaveIdKey, _saveId);
            }
        }
        catch
        {
            _saveId = Guid.NewGuid().ToString("N");
        }
        return _saveId;
    }

    public async Task SaveToCloudAsync(string saveId, string json)
    {
        if (string.IsNullOrWhiteSpace(_supabaseKey)) return;
        try
        {
            var url = $"{_supabaseUrl}/rest/v1/{SavesTable}?id=eq.{Uri.EscapeDataString(saveId)}";
            var payload = JsonSerializer.Serialize(new
            {
                id = saveId,
                data = json,
                updated_at = DateTimeOffset.UtcNow
            }, JsonOpts);
            var content = new StringContent(payload, System.Text.Encoding.UTF8, "application/json");

            var req = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = content
            };
            req.Headers.Add("apikey", _supabaseKey);
            req.Headers.Add("Authorization", $"Bearer {_supabaseKey}");
            req.Headers.Add("Prefer", "resolution=merge-duplicates,return=minimal");

            var resp = await _http.SendAsync(req);
        }
        catch { }
    }

    public async Task<string?> LoadFromCloudAsync(string saveId)
    {
        if (string.IsNullOrWhiteSpace(_supabaseKey)) return null;
        try
        {
            var url = $"{_supabaseUrl}/rest/v1/{SavesTable}?select=data&id=eq.{Uri.EscapeDataString(saveId)}&limit=1&order=updated_at.desc";
            var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Add("apikey", _supabaseKey);
            req.Headers.Add("Authorization", $"Bearer {_supabaseKey}");

            var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode) return null;

            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.GetArrayLength() == 0) return null;
            var data = doc.RootElement[0].GetProperty("data").GetString();
            return data;
        }
        catch { return null; }
    }
}
