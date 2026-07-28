using System.Collections.Immutable;

namespace CultSimulator.Game;

/// <summary>
/// Coven takeover, switching, and targeting logic. Pure functions over
/// <see cref="GameState"/> + <see cref="WorldLocationDef"/> data so it stays
/// modular and testable without the UI.
/// </summary>
public static class CovenProgress
{
    public static IReadOnlyList<WorldLocationDef> RivalsInOrder(
        ImmutableArray<WorldLocationDef> locations) =>
        locations
            .Where(l => l.Id != "skanor")
            .OrderBy(l => l.FollowersRequired)
            .ToList();

    public static WorldLocationDef? NextTarget(
        GameState state,
        ImmutableArray<WorldLocationDef> locations)
    {
        foreach (var loc in RivalsInOrder(locations))
        {
            var coven = state.FindCoven(loc.Id);
            if (coven == null || !coven.TakenOver) return loc;
        }
        return null;
    }

    public static double TakeoverProgress(
        GameState state,
        WorldLocationDef loc)
    {
        var home = state.HomeCoven;
        if (home.Followers >= loc.FollowersRequired) return 1.0;
        return (double)home.Followers / loc.FollowersRequired;
    }

    public static bool CanTakeover(
        GameState state,
        WorldLocationDef loc)
    {
        if (loc.Id == "skanor") return false;
        var coven = state.FindCoven(loc.Id);
        if (coven != null && coven.TakenOver) return false;
        return state.HomeCoven.Followers >= loc.FollowersRequired;
    }

    public static void Takeover(GameState state, WorldLocationDef loc)
    {
        if (!CanTakeover(state, loc)) return;

        var home = state.HomeCoven;
        home.Faith *= (1.0 - GameBalance.CovenTakeoverFaithPercent);
        home.Gold *= (1.0 - GameBalance.CovenTakeoverGoldPercent);
        home.Followers = (int)Math.Ceiling(home.Followers * (1.0 - GameBalance.CovenTakeoverFollowerPercent));

        var existing = state.FindCoven(loc.Id);
        if (existing != null)
        {
            existing.TakenOver = true;
            existing.Followers = 0;
            existing.Faith = 0;
            existing.Gold = 0;
            existing.PreachCount = 0;
            existing.Buildings = new Dictionary<BuildingType, int>();
            existing.Upgrades = new List<UpgradeId>();
        }
        else
        {
            state.Covens.Add(new CovenState
            {
                Id = loc.Id,
                TakenOver = true,
                Buildings = new Dictionary<BuildingType, int>(),
                Upgrades = new List<UpgradeId>()
            });
        }
    }

    public static void SwitchActive(GameState state, string covenId)
    {
        var coven = state.FindCoven(covenId);
        if (coven != null && coven.TakenOver)
            state.ActiveCovenId = covenId;
    }

    public static bool IsHomeCoven(GameState state) =>
        state.ActiveCovenId == "skanor" || string.IsNullOrEmpty(state.ActiveCovenId);

    public static int TotalFollowers(GameState state) =>
        state.Covens.Where(c => c.TakenOver).Sum(c => c.Followers);
}
