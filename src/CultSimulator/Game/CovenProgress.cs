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
        var covens = locations.Where(l => string.Equals(l.Continent, continent, StringComparison.OrdinalIgnoreCase) && l.Id != "skanor");
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
}