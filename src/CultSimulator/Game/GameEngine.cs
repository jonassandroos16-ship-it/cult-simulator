using System.Collections.Immutable;

namespace CultSimulator.Game;

/// <summary>
/// Pure game logic. All methods are static and side-effecting on the passed state.
/// Works on <see cref="CovenState"/>; the old <see cref="GameState"/> overloads
/// delegate to the active coven so existing call sites keep working.
/// </summary>
public static class GameEngine
{
    public static double PreachMultiplier(CovenState s)
    {
        double mult = 1.0 + s.Followers * 0.01;
        if (s.HasUpgrade(UpgradeId.Hymnal)) mult *= 2.0;
        if (s.HasUpgrade(UpgradeId.Ascendance)) mult *= 1.5;
        return mult;
    }

    public static double FaithMultiplier(CovenState s)
    {
        double mult = 1.0 + s.Buildings.GetValueOrDefault(BuildingType.Monolith) * GameBalance.MonolithFaithBonus;
        if (s.HasUpgrade(UpgradeId.Visions)) mult *= 2.0;
        if (s.HasUpgrade(UpgradeId.Ascendance)) mult *= 1.5;
        return mult;
    }

    public static double GoldMultiplier(CovenState s)
    {
        double mult = 1.0 + s.Buildings.GetValueOrDefault(BuildingType.Treasury) * GameBalance.TreasuryGoldBonus;
        if (s.HasUpgrade(UpgradeId.Relics)) mult *= 2.0;
        if (s.HasUpgrade(UpgradeId.Ascendance)) mult *= 1.5;
        return mult;
    }

    public static int BuildingCost(BuildingDef def, int owned) =>
        (int)Math.Ceiling(def.BaseCost * Math.Pow(def.Growth, owned));

    public static bool CanAfford(CovenState s, int faithCost, int goldCost) =>
        s.Faith >= faithCost && s.Gold >= goldCost;

    public static bool CanRecruit(CovenState s) => s.Faith >= GameBalance.RecruitCost;

    public static bool UpgradeUnlocked(CovenState s, UpgradeDef def) =>
        s.Followers >= def.UnlockFollowers;

    public static bool CanBuyUpgrade(CovenState s, UpgradeDef def) =>
        !s.HasUpgrade(def.Id) && UpgradeUnlocked(s, def) && CanAfford(s, def.FaithCost, def.GoldCost);

    public static (double faith, double gold) TickIncome(CovenState s)
    {
        double faith = s.Followers * GameBalance.FollowerFaithPerSec;
        double gold = s.Followers * GameBalance.FollowerGoldPerSec;
        faith += s.Buildings.GetValueOrDefault(BuildingType.Shrine) * GameBalance.ShrineFaithPerSec;
        gold += s.Buildings.GetValueOrDefault(BuildingType.Cathedral) * GameBalance.CathedralGoldPerSec;
        faith *= FaithMultiplier(s);
        gold *= GoldMultiplier(s);
        return (faith, gold);
    }

    public static void TickAllCovens(GameState state)
    {
        foreach (var coven in state.Covens)
        {
            if (!coven.TakenOver) continue;
            var (faith, gold) = TickIncome(coven);
            coven.Faith += faith;
            coven.Gold += gold;
        }
    }

    public static RankDef RankFor(int followers)
    {
        RankDef? current = null;
        foreach (var r in GameData.Ranks)
            if (followers >= r.MinFollowers) current = r;
        return current!;
    }

    public static RankDef? NextRank(int followers)
    {
        foreach (var r in GameData.Ranks)
            if (r.MinFollowers > followers) return r;
        return null;
    }

    public static double RankProgress(CovenState s)
    {
        var current = RankFor(s.Followers);
        var next = NextRank(s.Followers);
        if (next == null) return 1.0;
        return (double)(s.Followers - current.MinFollowers) / (next.MinFollowers - current.MinFollowers);
    }

    public static double Preach(CovenState s)
    {
        s.PreachCount++;
        var gained = PreachMultiplier(s);
        s.Faith += gained;
        return gained;
    }

    public static void Recruit(CovenState s)
    {
        if (!CanRecruit(s)) return;
        s.Faith -= GameBalance.RecruitCost;
        s.Followers++;
    }

    public static void BuyBuilding(CovenState s, BuildingType type)
    {
        var def = GameData.Buildings.First(b => b.Type == type);
        int owned = s.Buildings.GetValueOrDefault(type);
        int cost = BuildingCost(def, owned);
        if (def.CostResource == ResourceKind.Faith) { if (s.Faith < cost) return; s.Faith -= cost; }
        else { if (s.Gold < cost) return; s.Gold -= cost; }
        s.Buildings[type] = owned + 1;
    }

    public static void BuyUpgrade(CovenState s, UpgradeId id)
    {
        var def = GameData.Upgrades.First(u => u.Id == id);
        if (!CanBuyUpgrade(s, def)) return;
        s.Faith -= def.FaithCost;
        s.Gold -= def.GoldCost;
        s.Upgrades.Add(id);
    }

    public static GameState InitialState() => new()
    {
        CultName = "",
        StoryShown = false,
        ActiveCovenId = "skanor",
        Covens = new List<CovenState>
        {
            new CovenState { Id = "skanor", TakenOver = true }
        },
        StartedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
    };

    // --- GameState overloads (delegate to active coven) ---

    public static double PreachMultiplier(GameState s) => PreachMultiplier(s.ActiveCoven);
    public static double FaithMultiplier(GameState s) => FaithMultiplier(s.ActiveCoven);
    public static double GoldMultiplier(GameState s) => GoldMultiplier(s.ActiveCoven);
    public static bool CanRecruit(GameState s) => CanRecruit(s.ActiveCoven);
    public static bool UpgradeUnlocked(GameState s, UpgradeDef def) => UpgradeUnlocked(s.ActiveCoven, def);
    public static bool CanBuyUpgrade(GameState s, UpgradeDef def) => CanBuyUpgrade(s.ActiveCoven, def);
    public static bool CanAfford(GameState s, int faithCost, int goldCost) => CanAfford(s.ActiveCoven, faithCost, goldCost);
    public static (double faith, double gold) TickIncome(GameState s) => TickIncome(s.ActiveCoven);
    public static double RankProgress(GameState s) => RankProgress(s.ActiveCoven);
    public static double Preach(GameState s) => Preach(s.ActiveCoven);
    public static void Recruit(GameState s) => Recruit(s.ActiveCoven);
    public static void BuyBuilding(GameState s, BuildingType type) => BuyBuilding(s.ActiveCoven, type);
    public static void BuyUpgrade(GameState s, UpgradeId id) => BuyUpgrade(s.ActiveCoven, id);
}
