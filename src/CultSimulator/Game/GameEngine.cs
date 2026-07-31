using System.Net.Http;

namespace CultSimulator.Game;

public static class GameEngine
{
    public static GameState InitialState() => new()
    {
        Covens = new List<CovenState> { new() { Id = "skanor", Converted = true } },
        ActiveCovenId = "skanor",
        ShadowWar = ShadowWarEngine.CreateInitialState(),
        RivalCults = RivalCultEngine.CreateInitialState(),
        BattleSystem = BattleEngine.CreateInitialState(),
        LastSavedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
    };

    public static void Preach(CovenState c)
    {
        double baseYield = 0.2 + c.Followers * 0.012;
        if (c.HasUpgrade(UpgradeId.Hymnal)) baseYield *= 2;
        if (c.HasUpgrade(UpgradeId.Ascendance)) baseYield *= 1.3;
        c.Faith += baseYield;
        c.PreachCount++;
    }

    public static double Preach(GameState state)
    {
        Preach(state.ActiveCoven);
        return state.ActiveCoven.Faith;
    }

    public static void Recruit(CovenState c)
    {
        var cost = RecruitCostFor(c);
        if (c.Faith < cost) return;
        c.Faith -= cost;
        c.Followers++;
    }

    public static void RecruitMultiple(CovenState c, int max)
    {
        for (int i = 0; i < max; i++)
        {
            var cost = RecruitCostFor(c);
            if (c.Faith < cost) break;
            c.Faith -= cost;
            c.Followers++;
        }
    }

    public static double RecruitCostFor(CovenState c) =>
        Math.Floor(10 * Math.Pow(1.15, c.Followers));

    public static void BuyBuilding(CovenState c, BuildingType type)
    {
        var cost = BuildingCostFor(c, type);
        if (c.Faith < cost) return;
        c.Faith -= cost;
        c.Buildings[type] = c.Buildings.GetValueOrDefault(type) + 1;
    }

    public static double BuildingCostFor(CovenState c, BuildingType type)
    {
        int owned = c.Buildings.GetValueOrDefault(type);
        double baseCost = type switch
        {
            BuildingType.Shrine => 40,
            BuildingType.Altar => 250,
            BuildingType.Ossuary => 1200,
            _ => 5000
        };
        return Math.Floor(baseCost * Math.Pow(1.15, owned));
    }

    public static void BuyBank(CovenState c)
    {
        if (c.Faith < 5000) return;
        c.Faith -= 5000;
        c.HasBank = true;
    }

    public static void BuyUpgrade(CovenState c, UpgradeId id)
    {
        var cost = UpgradeCostFor(id);
        if (c.Faith < cost || c.Upgrades.Contains(id)) return;
        c.Faith -= cost;
        c.Upgrades.Add(id);
    }

    public static double UpgradeCostFor(UpgradeId id) => id switch
    {
        UpgradeId.Hymnal => 500,
        UpgradeId.Ascendance => 5000,
        _ => 0
    };

    public static bool CanAfford(CovenState c, double faith, double gold) => c.Faith >= faith && c.Gold >= gold;

    public static (double faith, double gold) ApplyOfflineIncome(GameState state, long elapsedMs)
    {
        double totalFaith = 0, totalGold = 0;
        var elapsedSec = elapsedMs / 1000.0;
        var cappedSec = Math.Min(elapsedSec, GameBalance.MaxOfflineSeconds);
        foreach (var coven in state.Covens.Where(c => c.Converted))
        {
            double faithPerSec = coven.Followers * 0.012;
            foreach (var (type, count) in coven.Buildings)
                faithPerSec += count * (type switch { BuildingType.Shrine => 0.5, BuildingType.Altar => 3, BuildingType.Ossuary => 15, _ => 0 });
            if (coven.HasUpgrade(UpgradeId.Hymnal)) faithPerSec *= 2;
            if (coven.HasUpgrade(UpgradeId.Ascendance)) faithPerSec *= 1.3;
            totalFaith += faithPerSec * cappedSec;
            if (coven.HasBank) totalGold += faithPerSec * 0.5 * cappedSec;
        }
        return (totalFaith, totalGold);
    }

    public static void TickAllCovens(GameState state, WorldLocationService locations)
    {
        foreach (var coven in state.Covens.Where(c => c.Converted))
        {
            double faithPerSec = coven.Followers * 0.012;
            foreach (var (type, count) in coven.Buildings)
                faithPerSec += count * (type switch { BuildingType.Shrine => 0.5, BuildingType.Altar => 3, BuildingType.Ossuary => 15, _ => 0 });
            if (coven.HasUpgrade(UpgradeId.Hymnal)) faithPerSec *= 2;
            if (coven.HasUpgrade(UpgradeId.Ascendance)) faithPerSec *= 1.3;
            coven.Faith += faithPerSec;
            if (coven.HasBank) coven.Gold += faithPerSec * 0.5;
        }
        ShadowWarEngine.Tick(state.ShadowWarOrInit, state, locations, 1.0);
        RivalCultEngine.Tick(state, locations, 1.0);
        BattleEngine.Tick(state, locations, 1.0);
    }

    public static RankDef RankFor(int followers) =>
        GameData.Ranks.FirstOrDefault(r => followers >= r.MinFollowers) ?? GameData.Ranks[^1];
}
