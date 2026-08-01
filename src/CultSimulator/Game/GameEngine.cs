using System.Linq;

namespace CultSimulator.Game;

public static class GameEngine
{
    public static GameState InitialState()
    {
        var state = new GameState
        {
            StartedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            LastSavedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            CultName = "",
            StoryShown = false
        };
        var skanor = new CovenState { Id = "skanor", Converted = true, TakenOver = true };
        state.Covens.Add(skanor);
        return state;
    }

    public static (double fps, double gps) TickIncome(CovenState coven)
    {
        double faithPerSec = coven.Followers * GameBalance.FollowerFaithPerSec;
        double goldPerSec = coven.Followers * GameBalance.FollowerGoldPerSec;

        foreach (var (building, count) in coven.Buildings)
        {
            switch (building)
            {
                case BuildingType.Shrine: faithPerSec += GameBalance.ShrineFaithPerSec * count; break;
                case BuildingType.Cathedral: goldPerSec += GameBalance.CathedralGoldPerSec * count; break;
            }
        }

        foreach (var up in coven.Upgrades)
        {
            switch (up)
            {
                case UpgradeId.Monolith: faithPerSec *= 1 + GameBalance.MonolithFaithBonus * coven.Upgrades.Count(u => u == UpgradeId.Monolith); break;
                case UpgradeId.Treasury: goldPerSec *= 1 + GameBalance.TreasuryGoldBonus * coven.Upgrades.Count(u => u == UpgradeId.Treasury); break;
                case UpgradeId.Observatory: faithPerSec *= 1 + GameBalance.ObservatoryFaithBonus * coven.Upgrades.Count(u => u == UpgradeId.Observatory); break;
                case UpgradeId.Reliquary: goldPerSec *= 1 + GameBalance.ReliquaryGoldBonus * coven.Upgrades.Count(u => u == UpgradeId.Reliquary); break;
            }
        }

        return (faithPerSec, goldPerSec);
    }

    public static double IdleCapSeconds(CovenState s)
    {
        int bankLevel = s.Buildings.GetValueOrDefault(BuildingType.Bank);
        if (bankLevel == 0) return GameBalance.BankCapHours[0] * 3600.0;
        int tier = Math.Min(bankLevel, GameBalance.BankCapHours.Length - 1);
        double mult = 1.0 + s.Upgrades.Count(u => u == UpgradeId.Treasury) * GameBalance.TreasuryGoldBonus;
        if (s.HasUpgrade(UpgradeId.BankVault)) mult *= 1.5;
        if (s.HasUpgrade(UpgradeId.OffshoreAccounts)) mult *= 1.5;
        if (s.HasUpgrade(UpgradeId.DarkLedger)) mult *= 2.0;
        if (s.HasUpgrade(UpgradeId.SoulEndowment)) mult *= 2.0;
        return GameBalance.BankCapHours[tier] * 3600.0 * mult;
    }

    public static string IdleCapDisplay(CovenState s)
    {
        double hours = IdleCapSeconds(s) / 3600.0;
        if (hours < 1) return $"{Math.Floor(hours * 60)}m";
        if (hours < 24) return $"{hours:F1}h";
        return $"{Math.Floor(hours / 24)}d {Math.Floor(hours % 24)}h";
    }

    public static (double faith, double gold, double lostFaith, double lostGold) ApplyOfflineIncome(GameState state, long elapsedMs)
    {
        double elapsedSec = elapsedMs / 1000.0;
        if (elapsedSec <= 0) return (0, 0, 0, 0);
        double totalFaith = 0, totalGold = 0, totalLostFaith = 0, totalLostGold = 0;

        foreach (var coven in state.Covens)
        {
            if (!coven.TakenOver) continue;
            var (fps, gps) = TickIncome(coven);
            double cap = IdleCapSeconds(coven);
            double eff = Math.Min(elapsedSec, cap);
            double f = fps * eff; double g = gps * eff;
            coven.Faith += f; coven.Gold += g;
            totalFaith += f; totalGold += g;

            double occultFps = OccultEngine.TotalFaithPerSecForCoven(state, coven) + OccultEngine.TotalMapFaithPerSecForCoven(coven.Occult, state);
            double occultFaith = occultFps * eff;
            coven.Faith += occultFaith;
            coven.Occult.LifetimeFaith += occultFaith;
            totalFaith += occultFaith;

            if (elapsedSec > cap)
            {
                double lostSec = elapsedSec - cap;
                totalLostFaith += (fps + occultFps) * lostSec;
                totalLostGold += gps * lostSec;
            }
        }

        return (totalFaith, totalGold, totalLostFaith, totalLostGold);
    }

    public static int BankBuildingCost(int owned) => (int)Math.Ceiling(GameBalance.BankBaseCost * Math.Pow(GameBalance.BankCostGrowth, owned));

    public static bool CanAfford(CovenState s, int faithCost, int goldCost) => s.Faith >= faithCost && s.Gold >= goldCost;

    public static bool UpgradeUnlocked(CovenState s, UpgradeDef def) => s.Followers >= def.UnlockFollowers;

    public static void BuyUpgrade(CovenState s, UpgradeId id)
    {
        var def = Upgrades.First(u => u.Id == id);
        if (!UpgradeUnlocked(s, def) || s.HasUpgrade(id) || !CanAfford(s, def.FaithCost, def.GoldCost)) return;
        s.Faith -= def.FaithCost; s.Gold -= def.GoldCost; s.Upgrades.Add(id);
    }

    public static void BuyBank(CovenState s) { int owned = s.Buildings.GetValueOrDefault(BuildingType.Bank); int cost = BankBuildingCost(owned); if (s.Gold < cost) return; s.Gold -= cost; s.Buildings[BuildingType.Bank] = owned + 1; }

    public static void BuyBank(GameState s) => BuyBank(s.ActiveCoven);

    public static (double faith, double gold) TotalIncomePerSec(GameState state)
    {
        double f = 0, g = 0;
        foreach (var coven in state.Covens) { if (!coven.TakenOver) continue; var (fps, gps) = TickIncome(coven); f += fps; g += gps; }
        return (f, g);
    }
}
