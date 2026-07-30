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

    public IReadOnlyList<WorldLocationDef> ForContinent(string continent) =>
        _locations.Where(l => string.Equals(l.Continent, continent, StringComparison.OrdinalIgnoreCase)).ToList();

    public IReadOnlyList<string> ContinentsWithCovens() =>
        _locations.Select(l => l.Continent).Distinct().ToList();

    public IReadOnlyList<WorldLocationDef> CovensForContinentInOrder(string continent) =>
        ForContinent(continent).OrderBy(l => l.FollowersRequired).ToList();

    public async Task LoadAsync()
    {
        if (_loaded) return;

        try
        {
            var manifest = await _http.GetFromJsonAsync<string[]>("data/covens/manifest.json");
            if (manifest == null) { _loaded = true; return; }

            var list = new List<WorldLocationDef>();
            foreach (var id in manifest)
            {
                try
                {
                    var loc = await _http.GetFromJsonAsync<WorldLocationDef>($"data/covens/{id}.json");
                    if (loc != null) list.Add(loc);
                }
                catch { /* skip missing coven file */ }
            }
            _locations = list.ToImmutableArray();
        }
        catch { /* manifest missing */ }
        _loaded = true;
    }
}