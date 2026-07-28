using System.Net.Http.Json;
using System.Collections.Immutable;

namespace CultSimulator.Game;

/// <summary>
/// Loads coven data from JSON files in wwwroot/data/covens/.
/// Each coven is a separate JSON file listed in manifest.json.
/// </summary>
public class WorldLocationService
{
    private readonly HttpClient _http;
    private ImmutableArray<WorldLocationDef> _locations = ImmutableArray<WorldLocationDef>.Empty;
    private bool _loaded;

    public WorldLocationService(HttpClient http) => _http = http;

    public ImmutableArray<WorldLocationDef> Locations => _locations;
    public bool IsLoaded => _loaded;

    public WorldLocationDef? Find(string id) =>
        _locations.FirstOrDefault(l => l.Id == id);

    public async Task LoadAsync()
    {
        if (_loaded) return;

        try
        {
            var manifest = await _http.GetFromJsonAsync<string[]>("data/covens/manifest.json");
            if (manifest == null) return;

            var builder = ImmutableArray.CreateBuilder<WorldLocationDef>();
            foreach (var id in manifest)
            {
                var loc = await _http.GetFromJsonAsync<WorldLocationDef>($"data/covens/{id}.json");
                if (loc != null) builder.Add(loc);
            }
            _locations = builder.ToImmutable();
            _loaded = true;
        }
        catch
        {
            _locations = ImmutableArray<WorldLocationDef>.Empty;
            _loaded = true;
        }
    }
}
