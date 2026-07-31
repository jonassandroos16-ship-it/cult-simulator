using System.Collections.Immutable;

namespace CultSimulator.Game;

public class WorldLocationService
{
    private readonly HttpClient _http;
    private ImmutableArray<WorldLocationDef> _locations = ImmutableArray<WorldLocationDef>.Empty;
    private bool _loaded = false;

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

    /// <summary>
    /// Merges revealed foothold covens into the loaded locations so the map
    /// and progression logic see them. Called after load and after each
    /// foothold reveal. Idempotent — footholds already present are skipped.
    /// </summary>
    public void SyncFootholds(GameState state)
    {
        if (!_loaded) return;
        var existing = new HashSet<string>(_locations.Select(l => l.Id));
        var toAdd = new List<WorldLocationDef>();
        foreach (var id in state.RevealedFootholds)
        {
            if (existing.Contains(id)) continue;
            var foothold = ContinentFootholds.ForCompletedContinent.Values
                .FirstOrDefault(f => f.CovenId == id);
            if (foothold == null) continue;
            toAdd.Add(ContinentFootholds.ToLocation(foothold));
            existing.Add(id);
        }
        if (toAdd.Count > 0)
            _locations = _locations.AddRange(toAdd);
    }
}
