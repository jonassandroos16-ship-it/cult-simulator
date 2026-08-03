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
        var continentCovens = locations.Locations
            .Where(l => string.Equals(l.Continent, continentId, StringComparison.OrdinalIgnoreCase) && l.Id != "skanor")
            .ToList();
        if (continentCovens.Count == 0) return false;
        return continentCovens.All(l => state.FindCoven(l.Id)?.Converted == true);
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
        double scale = RivalCultEngine.ScaleFor(continentId);
        var rivalHp = (rivalDef != null ? RivalBaseHp + rivalDef.AgentStrength * 20 : RivalBaseHp) * scale;

        battle = new BattleState
        {
            ContinentId = continentId,
            Phase = BattlePhase.NoThreat,
            RivalHp = rivalHp,
            RivalMaxHp = rivalHp,
            PlayerHp = PlayerBaseHp,
            PlayerMaxHp = PlayerBaseHp,
            Status = BattleStatus.NotStarted
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
        int maxForType = MaxAgentsForType(sw, state, type);
        sw.RecruitedAgents.TryGetValue(type, out int currentRecruited);
        if (type != AgentType.Initiate && currentRecruited + count > maxForType)
            return (false, $"Max {maxForType} {def.Name}s allowed (based on promoted minions). You have {currentRecruited}.");
        sw.SpentAgents += totalCost;
        sw.RecruitedAgents.TryGetValue(type, out int existing);
        sw.RecruitedAgents[type] = existing + count;
        return (true, $"Recruited {count} {def.Name}.");
    }

    public static (bool success, string message) DeployAgents(GameState state, string continentId, AgentType type, int count)
    {
        var sw = ShadowWarEngine.EnsureInitialized(state);
        sw.RecruitedAgents.TryGetValue(type, out int owned);
        var bs = EnsureInitialized(state);
        var battle = bs.GetBattle(continentId);
        if (battle == null || battle.Phase == BattlePhase.NoThreat)
            return (false, "No active battle in that continent.");
        if (battle.Phase == BattlePhase.Fighting)
            return (false, "Battle already in progress — withdraw first.");

        if (type == AgentType.Mage)
        {
            int scholars = battle.DeployedSquad.FirstOrDefault(d => d.Type == AgentType.Scholar)?.Count ?? 0;
            int mages = battle.DeployedSquad.FirstOrDefault(d => d.Type == AgentType.Mage)?.Count ?? 0;
            if (scholars + mages + count > MaxMagesPerBattle(state))
                return (false, $"Max {MaxMagesPerBattle(state)} mages/scholars per battle.");
        }

        int alreadyDeployed = battle.DeployedSquad.FirstOrDefault(d => d.Type == type)?.Count ?? 0;
        int availableToDeploy = owned - alreadyDeployed;
        if (availableToDeploy < count)
            return (false, $"Not enough {type} agents. Have {availableToDeploy} available, need {count}.");

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

        battle.DeployedSquad.Clear();
        return (true, "Agents withdrawn.");
    }

    public static (bool success, string message) ReinforceAgents(GameState state, string continentId, AgentType type, int count)
    {
        var sw = ShadowWarEngine.EnsureInitialized(state);
        var bs = EnsureInitialized(state);
        var battle = bs.GetBattle(continentId);
        if (battle == null || battle.Phase != BattlePhase.Fighting)
            return (false, "No active battle to reinforce.");

        if (type == AgentType.Initiate)
            return (false, "Cannot reinforce with Initiates mid-battle.");

        sw.RecruitedAgents.TryGetValue(type, out int owned);
        int alreadyDeployed = battle.DeployedSquad.FirstOrDefault(d => d.Type == type)?.Count ?? 0;
        int availableToDeploy = owned - alreadyDeployed;
        if (availableToDeploy < count)
            return (false, $"Not enough {type} agents. Have {availableToDeploy} available, need {count}.");

        if (type == AgentType.Mage)
        {
            int scholars = battle.DeployedSquad.FirstOrDefault(d => d.Type == AgentType.Scholar)?.Count ?? 0;
            int mages = alreadyDeployed;
            if (scholars + mages + count > MaxMagesPerBattle(state))
                return (false, $"Max {MaxMagesPerBattle(state)} mages/scholars per battle.");
        }

        var slot = battle.DeployedSquad.FirstOrDefault(d => d.Type == type);
        if (slot != null) slot.Count += count;
        else battle.DeployedSquad.Add(new DeployedAgent { Type = type, Count = count });
        return (true, $"Reinforced with {count} {type}.");
    }

    public static (bool success, string message) StartBattle(GameState state, string continentId)
    {
        var bs = EnsureInitialized(state);
        var sw = ShadowWarEngine.EnsureInitialized(state);
        var battle = bs.GetBattle(continentId);
        if (battle == null || battle.Phase != BattlePhase.Deploy)
            return (false, "Not in deploy phase.");
        if (battle.TotalDeployed == 0)
            return (false, "Deploy at least one agent before starting.");

        foreach (var slot in battle.DeployedSquad)
        {
            sw.RecruitedAgents.TryGetValue(slot.Type, out int cur);
            sw.RecruitedAgents[slot.Type] = Math.Max(0, cur - slot.Count);
        }

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

    public static int MaxMagesPerBattle(GameState state) =>
        state.Occult.Minions.Count(m => m.Role == PromotedRole.Scholar) + 1;

    public static int MaxAgentsForType(ShadowWarState sw, GameState state, AgentType type) =>
        type switch
        {
            AgentType.Initiate => (int)sw.AvailableAgents,
            AgentType.Zealot => state.Occult.Minions.Count(m => m.Role == PromotedRole.Zealot),
            AgentType.Infiltrator => state.Occult.Minions.Count(m => m.Role == PromotedRole.Infiltrator),
            AgentType.Scholar => state.Occult.Minions.Count(m => m.Role == PromotedRole.Scholar),
            AgentType.Mage => state.Occult.Minions.Count(m => m.Role == PromotedRole.Scholar),
            _ => 0
        };

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
            battle.Status = BattleStatus.NotStarted;
            var rivalDef0 = BattleData.RivalForContinent(continentId);
            if (rivalDef0 != null)
            {
                double scale0 = RivalCultEngine.ScaleFor(continentId);
                battle.EnemyUnits = EnemyCompositionBuilder.BuildComposition(rivalDef0.Archetype, scale0, 20);
                battle.EnemyArchetype = rivalDef0.Archetype;
            }
            AppendLog(battle, $"A rival cult has emerged in {continentId}!");
        }

        if (battle.Phase == BattlePhase.Cooldown)
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (now >= battle.CooldownUntil)
            {
                var rivalDef2 = BattleData.RivalForContinent(continentId);
                double scale2 = RivalCultEngine.ScaleFor(continentId);
                var rivalHp2 = (RivalBaseHp + (rivalDef2?.AgentStrength ?? 5.0) * 20) * scale2;
                battle.RivalHp = rivalHp2;
                battle.RivalMaxHp = rivalHp2;
                battle.PlayerHp = PlayerBaseHp;
                battle.Phase = BattlePhase.Deploy;
                battle.Status = BattleStatus.NotStarted;
                var rivalDef3 = BattleData.RivalForContinent(continentId);
                if (rivalDef3 != null) { double scale3 = RivalCultEngine.ScaleFor(continentId); battle.EnemyUnits = EnemyCompositionBuilder.BuildComposition(rivalDef3.Archetype, scale3, 20); battle.EnemyArchetype = rivalDef3.Archetype; }
                AppendLog(battle, "A new rival cult has risen. Prepare your agents.");
            }
            return;
        }

        if (battle.Phase != BattlePhase.Fighting) return;

        var rivalDef = BattleData.RivalForContinent(continentId);
        double scale = RivalCultEngine.ScaleFor(continentId);
        double rivalAttack = (rivalDef?.AgentStrength ?? 5.0) * scale;
        double playerAttack = BattleCommon.CalculateAttack(battle.DeployedSquad, sw, state);
        double playerDefense = BattleCommon.CalculateDefense(battle.DeployedSquad);
        double stealth = BattleCommon.CalculateStealth(battle.DeployedSquad);

        var (mitigated, playerDamage) = BattleCommon.ExchangeDamage(battle.DeployedSquad, rivalAttack, sw, state, deltaSec);

        double faithRegen = BattleCommon.CalculateFaithRegen(battle.DeployedSquad);
        battle.PlayerHp = Math.Min(battle.PlayerMaxHp, battle.PlayerHp + faithRegen * deltaSec);

        battle.RoundTimer += deltaSec;
        if (battle.RoundTimer >= 2.0)
        {
            battle.RoundTimer = 0;
            battle.RoundNumber++;

            var roundResult = BattleRoundEngine.ProcessRound(
                battle.RoundNumber, battle.DeployedSquad, battle.EnemyUnits,
                sw, state, battle.Momentum);
            battle.Momentum = roundResult.Momentum;
            battle.PlayerHp = Math.Max(0, battle.PlayerHp - roundResult.PlayerDamage);
            battle.RivalHp = Math.Max(0, battle.RivalHp - roundResult.RivalDamage);
            battle.Log.AddRange(roundResult.LogEntries);
            if (battle.Log.Count > MaxLogEntries * 2) battle.Log = battle.Log.TakeLast(MaxLogEntries).ToList();
            battle.RecentRounds.Add(roundResult);
            if (battle.RecentRounds.Count > 5) battle.RecentRounds.RemoveAt(0);

            var tactic = BattleRoundEngine.TryEnemyTactic(battle.EnemyArchetype.Value, battle.DeployedSquad, battle.RoundNumber);
            if (tactic != null)
            {
                battle.Log.AddRange(tactic.LogEntries);
                if (battle.Log.Count > MaxLogEntries * 2) battle.Log = battle.Log.TakeLast(MaxLogEntries).ToList();
            }
        }

        if (battle.RivalHp <= 0)
        {
            double faithBonus = state.Covens.Count(c => c.Converted) * 200.0;
            battle.Phase = BattlePhase.Cooldown;
            battle.Status = BattleStatus.Victory;
            battle.CooldownUntil = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + (long)(CooldownSec * 1000);
            battle.VictoryAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            battle.LastFaithReward = faithBonus;
            battle.DeployedSquad.Clear();
            battle.EnemyUnits.Clear();
            battle.RecentRounds.Clear();
            battle.RoundNumber = 0;
            battle.Momentum = 0;
            ApplyVictoryReward(state, continentId);
            AppendLog(battle, $"Victory! Rival cult defeated in {continentId}. +{NumberFormat.Fmt(faithBonus)} Faith!");
        }
        else if (battle.PlayerHp <= 0)
        {
            var lossRival = rivalDef?.Name ?? continentId;
            battle.RecentLosses.Add(new TerritoryLossEvent(continentId, lossRival, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
            if (battle.RecentLosses.Count > MaxRecentLosses) battle.RecentLosses.RemoveAt(0);
            battle.Phase = BattlePhase.Deploy;
            battle.Status = BattleStatus.Defeat;
            battle.PlayerHp = PlayerBaseHp;
            battle.RivalHp = battle.RivalMaxHp;
            battle.DeployedSquad.Clear();
            battle.RecentRounds.Clear();
            battle.RoundNumber = 0;
            battle.Momentum = 0;
            if (rivalDef != null) { battle.EnemyUnits = EnemyCompositionBuilder.BuildComposition(rivalDef.Archetype, scale, 20); }
            ApplyDefeatPenalty(state);
            AppendLog(battle, $"Defeat! Your agents were repelled in {continentId}. Suspicion rises.");
        }
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

    private static void AppendLog(BattleState battle, string message) =>
        BattleCommon.AppendLog(battle.Log, message, MaxLogEntries);
}
