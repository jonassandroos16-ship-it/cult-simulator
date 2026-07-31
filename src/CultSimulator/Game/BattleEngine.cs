using System.Linq;

namespace CultSimulator.Game;

public static class BattleEngine
{
    public const double PlayerBaseHp = 100;
    public const double RivalBaseHp = 100;
    public const double CooldownSec = 30;
    public const int MaxLogEntries = 20;
    public const int MaxRecentLosses = 5;

    public static BattleSystemState CreateInitialState() => new();

    public static BattleSystemState EnsureInitialized(GameState state)
    {
        state.BattleSystem ??= CreateInitialState();
        return state.BattleSystem;
    }

    public static bool IsTheaterActive(GameState state, WorldLocationService locations, string continentId)
    {
        int count = state.Covens.Count(c => c.Converted &&
            string.Equals(locations.Find(c.Id)?.Continent, continentId, StringComparison.OrdinalIgnoreCase));
        return count >= 2;
    }

    public static bool HasCovenInContinent(GameState state, WorldLocationService locations, string continentId)
    {
        return state.Covens.Any(c => c.Converted &&
            string.Equals(locations.Find(c.Id)?.Continent, continentId, StringComparison.OrdinalIgnoreCase));
    }

    public static BattleState GetOrCreateBattle(GameState state, WorldLocationService locations, string continentId)
    {
        var bs = EnsureInitialized(state);
        var battle = bs.GetBattle(continentId);
        if (battle != null) return battle;

        var rivalDef = BattleData.RivalForContinent(continentId);
        var rivalHp = rivalDef != null ? RivalBaseHp + rivalDef.AgentStrength * 20 : RivalBaseHp;

        battle = new BattleState
        {
            ContinentId = continentId,
            Phase = BattlePhase.NoThreat,
            RivalHp = rivalHp,
            RivalMaxHp = rivalHp,
            PlayerHp = PlayerBaseHp,
            PlayerMaxHp = PlayerBaseHp,
            LastTickAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
        bs.Battles.Add(battle);
        return battle;
    }

    public static (bool success, string message) RecruitAgent(GameState state, AgentType type, int count)
    {
        var sw = ShadowWarEngine.EnsureInitialized(state);
        var def = BattleData.AgentDef(type);
        if (def == null) return (false, "Unknown agent type.");
        int totalCost = def.AgentCost * count;
        if (sw.AvailableAgents < totalCost)
            return (false, $"Not enough agents. Need {totalCost}, have {(int)sw.AvailableAgents}.");
        sw.TotalAgents -= totalCost;
        sw.RecruitedAgents.TryGetValue(type, out int existing);
        sw.RecruitedAgents[type] = existing + count;
        return (true, $"Recruited {count} {def.Name}.");
    }

    public static (bool success, string message) DeployAgents(GameState state, string continentId, AgentType type, int count)
    {
        var sw = ShadowWarEngine.EnsureInitialized(state);
        sw.RecruitedAgents.TryGetValue(type, out int available);
        if (available < count)
            return (false, $"Not enough {type} agents. Have {available}, need {count}.");

        var bs = EnsureInitialized(state);
        var battle = bs.GetBattle(continentId);
        if (battle == null || battle.Phase == BattlePhase.NoThreat)
            return (false, "No active battle in that continent.");
        if (battle.Phase == BattlePhase.Fighting)
            return (false, "Battle already in progress — withdraw first.");

        sw.RecruitedAgents[type] = available - count;
        var slot = battle.DeployedSquad.FirstOrDefault(d => d.Type == type);
        if (slot != null) slot.Count += count;
        else battle.DeployedSquad.Add(new DeployedAgent { Type = type, Count = count });
        return (true, $"Deployed {count} {type}.");
    }

    public static (bool success, string message) WithdrawAgents(GameState state, string continentId)
    {
        var sw = ShadowWarEngine.EnsureInitialized(state);
        var bs = EnsureInitialized(state);
        var battle = bs.GetBattle(continentId);
        if (battle == null) return (false, "No battle found.");
        if (battle.Phase == BattlePhase.Fighting)
            return (false, "Cannot withdraw during an active battle.");

        foreach (var slot in battle.DeployedSquad)
        {
            sw.RecruitedAgents.TryGetValue(slot.Type, out int cur);
            sw.RecruitedAgents[slot.Type] = cur + slot.Count;
        }
        battle.DeployedSquad.Clear();
        return (true, "Agents withdrawn.");
    }

    public static (bool success, string message) StartBattle(GameState state, string continentId)
    {
        var bs = EnsureInitialized(state);
        var battle = bs.GetBattle(continentId);
        if (battle == null || battle.Phase != BattlePhase.Deploy)
            return (false, "Not in deploy phase.");
        if (battle.TotalDeployed == 0)
            return (false, "Deploy at least one agent before starting.");
        battle.Phase = BattlePhase.Fighting;
        battle.Status = BattleStatus.Active;
        AppendLog(battle, $"Battle started in {continentId} with {battle.TotalDeployed} agents.");
        return (true, "Battle started!");
    }

    public static List<TerritoryLossEvent> GetRecentLosses(GameState state)
    {
        var bs = EnsureInitialized(state);
        return bs.Battles.SelectMany(b => b.RecentLosses)
            .OrderByDescending(e => e.OccurredAt)
            .Take(MaxRecentLosses)
            .ToList();
    }

    public static void Tick(GameState state, WorldLocationService locations, double deltaSec)
    {
        foreach (var theater in BattleData.Theaters)
            TickBattle(state, locations, theater.ContinentId, deltaSec);
    }

    public static void TickBattle(GameState state, WorldLocationService locations, string continentId, double deltaSec)
    {
        var sw = ShadowWarEngine.EnsureInitialized(state);
        var battle = GetOrCreateBattle(state, locations, continentId);

        if (battle.Phase == BattlePhase.NoThreat && IsTheaterActive(state, locations, continentId))
        {
            battle.Phase = BattlePhase.Deploy;
            AppendLog(battle, $"A rival cult has emerged in {continentId}!");
        }

        if (battle.Phase == BattlePhase.Cooldown)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (now >= battle.CooldownUntil)
            {
                var rivalDef2 = BattleData.RivalForContinent(continentId);
                var rivalHp2 = RivalBaseHp + (rivalDef2?.AgentStrength ?? 5.0) * 20;
                battle.RivalHp = rivalHp2;
                battle.RivalMaxHp = rivalHp2;
                battle.PlayerHp = PlayerBaseHp;
                battle.Phase = BattlePhase.Deploy;
                AppendLog(battle, "A new rival cult has risen. Prepare your agents.");
            }
            return;
        }

        if (battle.Phase != BattlePhase.Fighting) return;

        var rivalDef = BattleData.RivalForContinent(continentId);
        double rivalAttack = rivalDef?.AgentStrength ?? 5.0;
        double playerAttack = CalculatePlayerAttack(battle, sw, state);
        double playerDefense = CalculatePlayerDefense(battle, sw, state);
        double stealth = CalculatePlayerStealth(battle);

        double rivalDamage = rivalAttack * (1.0 - stealth * 0.3) * deltaSec;
        double playerDamage = playerAttack * deltaSec;

        battle.RivalHp = Math.Max(0, battle.RivalHp - playerDamage);
        battle.PlayerHp = Math.Max(0, battle.PlayerHp - Math.Max(0, rivalDamage - playerDefense * 0.1 * deltaSec));

        if (battle.RivalHp <= 0)
        {
            battle.Phase = BattlePhase.Cooldown;
            battle.Status = BattleStatus.Victory;
            battle.CooldownUntil = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + (long)(CooldownSec * 1000);
            battle.DeployedSquad.Clear();
            ApplyVictoryReward(state, continentId);
            AppendLog(battle, $"Victory! Rival cult defeated in {continentId}.");
        }
        else if (battle.PlayerHp <= 0)
        {
            var lossRival = rivalDef?.Name ?? continentId;
            battle.RecentLosses.Add(new TerritoryLossEvent(continentId, lossRival, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
            if (battle.RecentLosses.Count > MaxRecentLosses) battle.RecentLosses.RemoveAt(0);
            battle.Phase = BattlePhase.Deploy;
            battle.Status = BattleStatus.Defeat;
            battle.PlayerHp = PlayerBaseHp;
            battle.DeployedSquad.Clear();
            ApplyDefeatPenalty(state);
            AppendLog(battle, $"Defeat! Your agents were repelled in {continentId}.");
        }
    }

    public static int MaxAgentsForType(ShadowWarState sw, GameState state, AgentType type) =>
        type switch
        {
            AgentType.Initiate => (int)Math.Floor(sw.TotalAgents),
            AgentType.Zealot => state.Occult.Minions.Count(m => m.Role == PromotedRole.Zealot),
            AgentType.Infiltrator => state.Occult.Minions.Count(m => m.Role == PromotedRole.Infiltrator),
            _ => 0
        };

    private static double CalculatePlayerAttack(BattleState battle, ShadowWarState sw, GameState state)
    {
        double strength = ShadowWarEngine.AgentStrength(sw, state);
        double attack = 0;
        foreach (var slot in battle.DeployedSquad)
        {
            var def = BattleData.AgentDef(slot.Type);
            if (def != null) attack += def.Attack * slot.Count * strength;
        }
        return attack;
    }

    private static double CalculatePlayerDefense(BattleState battle, ShadowWarState sw, GameState state)
    {
        double defense = 0;
        foreach (var slot in battle.DeployedSquad)
        {
            var def = BattleData.AgentDef(slot.Type);
            if (def != null) defense += def.Defense * slot.Count;
        }
        return defense;
    }

    private static double CalculatePlayerStealth(BattleState battle)
    {
        if (battle.TotalDeployed == 0) return 0;
        double stealth = 0;
        foreach (var slot in battle.DeployedSquad)
        {
            var def = BattleData.AgentDef(slot.Type);
            if (def != null) stealth += def.Stealth * slot.Count;
        }
        return stealth / battle.TotalDeployed;
    }

    private static void ApplyVictoryReward(GameState state, string continentId)
    {
        double faithBonus = state.Covens.Count(c => c.Converted) * 200.0;
        state.ActiveCoven.Faith += faithBonus;
        state.Occult.LifetimeFaith += faithBonus;
    }

    private static void ApplyDefeatPenalty(GameState state)
    {
        state.Occult.Suspicion = Math.Min(OccultBalance.SuspicionMax, state.Occult.Suspicion + 10);
    }

    private static void AppendLog(BattleState battle, string message)
    {
        battle.Log.Add($"[{DateTime.UtcNow:HH:mm:ss}] {message}");
        if (battle.Log.Count > MaxLogEntries)
            battle.Log.RemoveAt(0);
    }
}
