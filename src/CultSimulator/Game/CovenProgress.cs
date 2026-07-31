using System.Collections.Immutable;

namespace CultSimulator.Game;

public static class CovenProgress
{
    public static IReadOnlyList<WorldLocationDef> RivalsInOrder(ImmutableArray<WorldLocationDef> locations) =>
        locations.Where(l => l.Id != "skanor").OrderBy(l => l.FollowersRequired).ToList();

    public static WorldLocationDef? NextTarget(GameState state, ImmutableArray<WorldLocationDef> locations)
    {
        foreach (var loc in RivalsInOrder(locations))
        {
            var coven = state.FindCoven(loc.Id);
            if (coven == null || !coven.Converted) return loc;
        }
        return null;
    }

    public static double ConversionProgress(GameState state, WorldLocationDef loc)
    {
        var total = TotalFollowers(state);
        if (total >= loc.FollowersRequired) return 1.0;
        return (double)total / loc.FollowersRequired;
    }

    public static double TakeoverProgress(GameState state, WorldLocationDef loc) => ConversionProgress(state, loc);

    public static bool CanConvert(GameState state, WorldLocationDef loc)
    {
        if (loc.Id == "skanor") return false;
        var coven = state.FindCoven(loc.Id);
        if (coven != null && coven.Converted) return false;
        return TotalFollowers(state) >= loc.FollowersRequired;
    }

    public static bool CanTakeover(GameState state, WorldLocationDef loc) => CanConvert(state, loc);

    public static void Takeover(GameState state, WorldLocationDef loc)
    {
        if (!CanConvert(state, loc)) return;
        var home = state.HomeCoven;
        home.Faith *= (1.0 - GameBalance.CovenTakeoverFaithPercent);
        home.Gold *= (1.0 - GameBalance.CovenTakeoverGoldPercent);
        home.Followers = (int)Math.Ceiling(home.Followers * (1.0 - GameBalance.CovenTakeoverFollowerPercent));
        var existing = state.FindCoven(loc.Id);
        if (existing != null)
        {
            existing.Converted = true; existing.Followers = 0; existing.Faith = 0; existing.Gold = 0; existing.PreachCount = 0;
            existing.Buildings = new Dictionary<BuildingType, int>(); existing.Upgrades = new List<UpgradeId>();
        }
        else { state.Covens.Add(new CovenState { Id = loc.Id, Converted = true, Buildings = new Dictionary<BuildingType, int>(), Upgrades = new List<UpgradeId>() }); }
    }

    public static void SwitchActive(GameState state, string covenId)
    {
        var coven = state.FindCoven(covenId);
        if (coven != null && coven.Converted) state.ActiveCovenId = covenId;
    }

    public static bool IsHomeCoven(GameState state) => state.ActiveCovenId == "skanor" || string.IsNullOrEmpty(state.ActiveCovenId);
    public static int TotalFollowers(GameState state) => state.Covens.Where(c => c.Converted).Sum(c => c.Followers);

    public static bool IsContinentComplete(GameState state, ImmutableArray<WorldLocationDef> locations, string continent)
    {
        var covens = locations.Where(l => string.Equals(l.Continent, continent, StringComparison.OrdinalIgnoreCase) && l.Id != "skanor").ToList();
        if (covens.Count == 0) return false;
        return covens.All(l => state.FindCoven(l.Id)?.Converted == true);
    }

    public static string? CurrentContinent(GameState state, ImmutableArray<WorldLocationDef> locations)
    {
        foreach (var continent in ContinentThemes.ProgressionOrder)
        {
            if (!IsContinentComplete(state, locations, continent))
                return continent;
        }
        return null;
    }

    public static bool IsContinentUnlocked(GameState state, ImmutableArray<WorldLocationDef> locations, string continent)
    {
        var idx = Array.IndexOf(ContinentThemes.ProgressionOrder, continent);
        if (idx <= 0) return true;
        for (int i = 0; i < idx; i++)
        {
            if (!IsContinentComplete(state, locations, ContinentThemes.ProgressionOrder[i]))
                return false;
        }
        return true;
    }

    public static bool HasCovenInContinent(GameState state, ImmutableArray<WorldLocationDef> locations, string continent)
    {
        if (continent == "europe") return true;
        return state.Covens.Any(c => c.Converted &&
            string.Equals(locations.FirstOrDefault(l => l.Id == c.Id)?.Continent, continent, StringComparison.OrdinalIgnoreCase));
    }

    public static bool CanConvertInContinent(GameState state, ImmutableArray<WorldLocationDef> locations, string continent)
    {
        return HasCovenInContinent(state, locations, continent);
    }

    /// <summary>
    /// Returns the continent that was just completed (all covens converted)
    /// and whose foothold reward has not yet been granted. Null if no
    /// newly-completed continent is pending.
    /// </summary>
    public static string? NewlyCompletedContinent(GameState state, ImmutableArray<WorldLocationDef> locations)
    {
        foreach (var continent in ContinentThemes.ProgressionOrder)
        {
            if (!IsContinentComplete(state, locations, continent)) continue;
            if (continent == "oceania") continue; // last continent — no next foothold
            var foothold = ContinentFootholds.ForCompleted(continent);
            if (foothold == null) continue;
            if (state.RevealedFootholds.Contains(foothold.CovenId)) continue;
            if (state.PendingContinentStory == continent) continue;
            return continent;
        }
        return null;
    }

    /// <summary>
    /// Marks a continent's completion story as pending display. The actual
    /// foothold coven is granted when <see cref="GrantFoothold"/> is called
    /// after the player dismisses the story.
    /// </summary>
    public static void MarkContinentStoryPending(GameState state, string continent)
    {
        state.PendingContinentStory = continent;
    }

    /// <summary>
    /// Grants the foothold coven for the pending continent: adds it to the
    /// revealed list, creates a converted coven entry, and clears the
    /// pending story flag. Returns the foothold def, or null if none pending.
    /// </summary>
    public static FootholdDef? GrantFoothold(GameState state, ImmutableArray<WorldLocationDef> locations)
    {
        var continent = state.PendingContinentStory;
        if (string.IsNullOrEmpty(continent)) return null;
        var foothold = ContinentFootholds.ForCompleted(continent);
        if (foothold == null) { state.PendingContinentStory = null; return null; }

        if (!state.RevealedFootholds.Contains(foothold.CovenId))
            state.RevealedFootholds.Add(foothold.CovenId);

        if (state.FindCoven(foothold.CovenId) == null)
        {
            state.Covens.Add(new CovenState
            {
                Id = foothold.CovenId,
                Converted = true,
                Buildings = new Dictionary<BuildingType, int>(),
                Upgrades = new List<UpgradeId>()
            });
        }
        else
        {
            var existing = state.FindCoven(foothold.CovenId)!;
            existing.Converted = true;
        }

        state.PendingContinentStory = null;
        return foothold;
    }
}
