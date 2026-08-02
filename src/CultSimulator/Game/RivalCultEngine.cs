using System.Linq;

namespace CultSimulator.Game;

public static class RivalCultEngine
{
    public const double RivalBattlePlayerBaseHp = 200;
    public const double RivalBattleRivalBaseHp = 300;
    public const int MaxLogEntries = 15;

    private static readonly Dictionary<string, double> ContinentScale = new()
    {
        ["europe"] = 1.0,
        ["north_america"] = 1.4,
        ["south_america"] = 1.8,
        ["africa"] = 2.3,
        ["middle_east"] = 2.8,
        ["asia"] = 3.4,
        ["oceania"] = 4.0
    };

    public static double ScaleFor(string continentId) =>
        ContinentScale.TryGetValue(continentId ?? "", out var s) ? s : 1.0;

    public static RivalCultSystemState CreateInitialState()
    {
        var state = new RivalCultSystemState { Rivals = new() };
        foreach (var def in RivalCultData.Rivals)
        {
            state.Rivals.Add(new RivalCultState
            {
                Id = def.Id,
                Status = RivalCultStatus.Dormant,
                Power = 0,
                TerritoryControl = 0,
                NextActionAt = 0
            });
        }
        return state;
    }

    public static RivalCultSystemState EnsureInitialized(GameState state)
    {
        if (state.RivalCults == null)
            state.RivalCults = CreateInitialState();
        return state.RivalCults;
    }

    public static bool ShouldActivateForContinent(GameState state, WorldLocationService locations, string continentId)
    {
        var continentCovens = locations.Locations
            .Where(l => string.Equals(l.Continent, continentId, StringComparison.OrdinalIgnoreCase) && l.Id != "skanor")
            .ToList();
        if (continentCovens.Count == 0) return false;
        return continentCovens.All(l => state.FindCoven(l.Id)?.Converted == true);
    }

    public static void ActivateForContinent(GameState state, string continentId)
    {
        var rs = EnsureInitialized(state);
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        foreach (var rival in rs.Rivals)
        {
            var def = RivalCultData.Find(rival.Id);
            if (def == null || def.PreferredTerritoryId != continentId) continue;
            if (rival.Status != RivalCultStatus.Dormant) continue;
            if (rival.Defeated) continue;

            rival.Status = RivalCultStatus.Active;
            rival.Power = 20;
            rival.NextActionAt = now + (long)(Random.Shared.Next(30, 60) * 1000);
        }
    }

    public static void Tick(GameState state, WorldLocationService locations, double deltaSec)
    {
        var rs = EnsureInitialized(state);
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        foreach (var def in RivalCultData.Rivals)
        {
            var rival = rs.GetRival(def.Id);
            if (rival == null) continue;
            if (rival.Defeated) continue;

            bool shouldActive = ShouldActivateForContinent(state, locations, def.PreferredTerritoryId);

            if (shouldActive && rival.Status == RivalCultStatus.Dormant)
            {
                ActivateForContinent(state, def.PreferredTerritoryId);
            }
            else if (!shouldActive && rival.Status != RivalCultStatus.Dormant)
            {
                rival.Status = RivalCultStatus.Dormant;
                rival.Power = 0;
                rival.ControlledInstitutions.Clear();
                rival.TerritoryControl = 0;
            }

            if (rival.Status == RivalCultStatus.Dormant) continue;

            rival.Power += def.GrowthRate * deltaSec * (1.0 + rival.TerritoryControl * 0.5);

            if (now >= rival.NextActionAt)
            {
                TakeAction(state, rival, def, now);
                rival.NextActionAt = now + (long)(Random.Shared.Next(20, 45) * 1000);
            }
        }

        TickRivalBattles(state, locations, deltaSec);
    }

    private static void TakeAction(GameState state, RivalCultState rival, RivalCultDef def, long now)
    {
        var sw = state.ShadowWarOrInit;

        var targetInst = FindTargetInstitution(sw, def, rival);
        if (targetInst != null)
        {
            AttackInstitution(sw, rival, def, targetInst);
            return;
        }

        rival.TerritoryControl = Math.Min(1.0, rival.TerritoryControl + 0.05);
        rival.Status = rival.TerritoryControl > 0.5 ? RivalCultStatus.Expanding : RivalCultStatus.Active;
    }

    private static InstitutionState? FindTargetInstitution(ShadowWarState sw, RivalCultDef def, RivalCultState rival)
    {
        var territoryInsts = ShadowWarData.InstitutionsForTerritory(def.PreferredTerritoryId);

        var playerControlled = territoryInsts
            .FirstOrDefault(i => sw.GetInstitution(i.Id)?.Status == InstitutionStatus.Controlled);
        if (playerControlled != null && Random.Shared.NextDouble() < def.Aggression)
        {
            rival.Status = RivalCultStatus.AtWar;
            return sw.GetInstitution(playerControlled.Id);
        }

        var unlocked = territoryInsts
            .Where(i => sw.GetInstitution(i.Id)?.Status == InstitutionStatus.Unlocked)
            .ToList();
        if (unlocked.Count > 0)
            return sw.GetInstitution(unlocked[Random.Shared.Next(unlocked.Count)].Id);

        return null;
    }

    private static void AttackInstitution(ShadowWarState sw, RivalCultState rival, RivalCultDef def, InstitutionState inst)
    {
        if (inst.Status == InstitutionStatus.Controlled)
        {
            double damage = rival.Power * 0.1 * def.AgentStrength;
            inst.InvestigationDefense = Math.Max(0, inst.InvestigationDefense - damage);
            if (inst.InvestigationDefense <= 0 && inst.Status == InstitutionStatus.Controlled)
            {
                inst.Status = InstitutionStatus.Investigated;
                inst.InvestigationDefense = 30;
                rival.ControlledInstitutions.Add(inst.Id);
            }
            rival.Power -= 5;
        }
        else if (inst.Status == InstitutionStatus.Unlocked)
        {
            if (Random.Shared.NextDouble() < 0.3 * def.AgentStrength)
            {
                inst.Status = InstitutionStatus.Alerted;
                inst.CooldownUntil = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + 20_000;
                rival.ControlledInstitutions.Add(inst.Id);
                rival.TerritoryControl = Math.Min(1.0, rival.TerritoryControl + 0.1);
            }
        }
    }

    public static int TotalRivalControlled(GameState state)
    {
        var rs = EnsureInitialized(state);
        return rs.Rivals.Where(r => r.Status != RivalCultStatus.Dormant && !r.Defeated).Sum(r => r.ControlledInstitutions.Count);
    }

    public static IReadOnlyList<(RivalCultDef def, RivalCultState state)> ActiveRivals(GameState state)
    {
        var rs = EnsureInitialized(state);
        return rs.Rivals
            .Where(r => r.Status != RivalCultStatus.Dormant && !r.Defeated)
            .Select(r => (RivalCultData.Find(r.Id)!, r))
            .ToList();
    }

    public static RivalBattleState GetOrCreateRivalBattle(GameState state, string rivalId)
    {
        var rs = EnsureInitialized(state);
        var existing = rs.GetRivalBattle(rivalId);
        if (existing != null) return existing;

        var def = RivalCultData.Find(rivalId);
        if (def == null) throw new ArgumentException($"Unknown rival: {rivalId}");

        var rival = rs.GetRival(rivalId);
        if (rival == null || rival.Defeated) throw new InvalidOperationException("Rival not available");

        double scale = ScaleFor(def.PreferredTerritoryId);
        double rivalHp = (RivalBattleRivalBaseHp + rival.Power * 2) * scale;
        var battle = new RivalBattleState
        {
            RivalId = rivalId,
            ContinentId = def.PreferredTerritoryId,
            Phase = RivalBattlePhase.Deploy,
            RivalHp = rivalHp,
            RivalMaxHp = rivalHp,
            PlayerHp = RivalBattlePlayerBaseHp,
            PlayerMaxHp = RivalBattlePlayerBaseHp,
            LastTickAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
            EnemyArchetype = def.Archetype
        };
        battle.EnemyUnits = EnemyCompositionBuilder.BuildComposition(def.Archetype, scale, rival.Power);
        rs.RivalBattles.Add(battle);
        return battle;
    }

    public static (bool success, string message) DeployRivalBattleAgents(GameState state, string rivalId, AgentType type, int count)
    {
        var sw = ShadowWarEngine.EnsureInitialized(state);
        sw.RecruitedAgents.TryGetValue(type, out int owned);

        try
        {
            var battle = GetOrCreateRivalBattle(state, rivalId);
            if (battle.Phase != RivalBattlePhase.Deploy && battle.Phase != RivalBattlePhase.Fighting)
                return (false, "Battle is not in deploy or fighting phase.");

            if (battle.Phase == RivalBattlePhase.Fighting && type == AgentType.Initiate)
                return (false, "Cannot deploy Initiates mid-battle.");

            if (type == AgentType.Mage)
            {
                int scholars = battle.DeployedSquad.FirstOrDefault(d => d.Type == AgentType.Scholar)?.Count ?? 0;
                int mages = battle.DeployedSquad.FirstOrDefault(d => d.Type == AgentType.Mage)?.Count ?? 0;
                if (scholars <= mages + count)
                    return (false, "Each Mage requires at least 1 Scholar in the squad.");
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
        catch (Exception ex) { return (false, ex.Message); }
    }

    public static (bool success, string message) WithdrawRivalBattleAgents(GameState state, string rivalId)
    {
        var sw = ShadowWarEngine.EnsureInitialized(state);
        var rs = EnsureInitialized(state);
        var battle = rs.GetRivalBattle(rivalId);
        if (battle == null) return (false, "No battle found.");
        if (battle.Phase == RivalBattlePhase.Fighting)
            return (false, "Cannot withdraw during an active battle.");

        battle.DeployedSquad.Clear();
        return (true, "Agents withdrawn.");
    }

    public static (bool success, string message) ReinforceRivalBattleAgents(GameState state, string rivalId, AgentType type, int count)
    {
        var rs = EnsureInitialized(state);
        var battle = rs.GetRivalBattle(rivalId);
        if (battle == null || battle.Phase != RivalBattlePhase.Fighting)
            return (false, "No active battle to reinforce.");

        if (type == AgentType.Initiate)
            return (false, "Cannot reinforce with Initiates mid-battle.");

        var sw = ShadowWarEngine.EnsureInitialized(state);
        sw.RecruitedAgents.TryGetValue(type, out int owned);
        int alreadyDeployed = battle.DeployedSquad.FirstOrDefault(d => d.Type == type)?.Count ?? 0;
        int availableToDeploy = owned - alreadyDeployed;
        if (availableToDeploy < count)
            return (false, $"Not enough {type} agents. Have {availableToDeploy} available, need {count}.");

        if (type == AgentType.Mage)
        {
            int scholars = battle.DeployedSquad.FirstOrDefault(d => d.Type == AgentType.Scholar)?.Count ?? 0;
            int mages = alreadyDeployed;
            if (scholars <= mages + count)
                return (false, "Each Mage requires at least 1 Scholar in the squad.");
        }

        var slot = battle.DeployedSquad.FirstOrDefault(d => d.Type == type);
        if (slot != null) slot.Count += count;
        else battle.DeployedSquad.Add(new DeployedAgent { Type = type, Count = count });

        sw.RecruitedAgents[type] = Math.Max(0, owned - count);
        AppendLog(battle, $"Reinforced with {count} {type}.");
        return (true, $"Reinforced with {count} {type}!");
    }

    public static (bool success, string message) StartRivalBattle(GameState state, string rivalId)
    {
        var rs = EnsureInitialized(state);
        var battle = rs.GetRivalBattle(rivalId);
        if (battle == null || battle.Phase != RivalBattlePhase.Deploy)
            return (false, "Not in deploy phase.");
        if (battle.TotalDeployed == 0)
            return (false, "Deploy at least one agent before starting.");

        var sw = ShadowWarEngine.EnsureInitialized(state);
        foreach (var slot in battle.DeployedSquad)
        {
            sw.RecruitedAgents.TryGetValue(slot.Type, out int cur);
            sw.RecruitedAgents[slot.Type] = Math.Max(0, cur - slot.Count);
        }

        battle.Phase = RivalBattlePhase.Fighting;
        AppendLog(battle, $"Assault on {rivalId} begun with {battle.TotalDeployed} agents!");
        return (true, "Battle started!");
    }

    public static void TickRivalBattles(GameState state, WorldLocationService locations, double deltaSec)
    {
        var rs = EnsureInitialized(state);
        var sw = ShadowWarEngine.EnsureInitialized(state);

        foreach (var battle in rs.RivalBattles.ToList())
        {
            if (battle.Phase != RivalBattlePhase.Fighting) continue;

            var def = RivalCultData.Find(battle.RivalId);
            var rival = rs.GetRival(battle.RivalId);
            if (def == null || rival == null || rival.Defeated) continue;

            double scale = ScaleFor(def.PreferredTerritoryId);
            double rivalAttack = (def.AgentStrength * 3.0 + rival.Power * 0.05) * scale;
            double playerAttack = BattleCommon.CalculateAttack(battle.DeployedSquad, sw, state);
            double playerDefense = BattleCommon.CalculateDefense(battle.DeployedSquad);
            double stealth = BattleCommon.CalculateStealth(battle.DeployedSquad);
            var (mitigated, playerDamage) = BattleCommon.ExchangeDamage(battle.DeployedSquad, rivalAttack, sw, state, deltaSec);

            double faithRegen = BattleCommon.CalculateFaithRegen(battle.DeployedSquad);
            battle.PlayerHp = Math.Min(battle.PlayerMaxHp, battle.PlayerHp + faithRegen * deltaSec);

            battle.RivalHp = Math.Max(0, battle.RivalHp - playerDamage);
            battle.PlayerHp = Math.Max(0, battle.PlayerHp - mitigated);

            battle.RoundTimer += deltaSec;
            battle.Momentum = Math.Clamp(battle.Momentum + (playerDamage - mitigated) * 0.01, -100, 100);

            if (battle.RoundTimer >= BattleRoundEngine.RoundIntervalSec)
            {
                battle.RoundTimer = 0;
                battle.RoundNumber++;
                var round = BattleRoundEngine.ExecuteRound(
                    battle.RoundNumber, battle.DeployedSquad, battle.EnemyUnits,
                    playerAttack, rivalAttack, playerDefense, stealth, BattleRoundEngine.RoundIntervalSec);
                var tactic = BattleRoundEngine.TryEnemyTactic(def.Archetype, battle.DeployedSquad, battle.RoundNumber);
                if (tactic != null) round.EnemyAction = tactic;
                var (reinforced, action) = BattleRoundEngine.TryEnemyReinforce(battle.EnemyUnits, def.Archetype, scale, battle.RoundNumber);
                if (reinforced) { round.EnemyReinforced = true; round.EnemyAction = action; }
                battle.RecentRounds.Add(round);
                if (battle.RecentRounds.Count > 6) battle.RecentRounds.RemoveAt(0);
                AppendLog(battle, round.Summary);
            }

            if (battle.RivalHp <= 0)
            {
                double faithReward = (5000 + rival.Power * 50) * scale;
                battle.Phase = RivalBattlePhase.Victory;
                rival.Defeated = true;
                rival.Status = RivalCultStatus.Dormant;
                rival.Power = 0;
                rival.ControlledInstitutions.Clear();
                rival.TerritoryControl = 0;

                foreach (var instId in rival.ControlledInstitutions.ToList())
                {
                    var inst = sw.GetInstitution(instId);
                    if (inst != null && inst.Status == InstitutionStatus.Alerted)
                        inst.Status = InstitutionStatus.Unlocked;
                }

                state.ActiveCoven.Faith += faithReward;
                state.Occult.LifetimeFaith += faithReward;
                battle.EnemyUnits.Clear();
                battle.RecentRounds.Clear();
                battle.RoundNumber = 0;
                battle.Momentum = 0;
                AppendLog(battle, $"VICTORY! {def.Name} has been destroyed! +{NumberFormat.Fmt(faithReward)} Faith!");
            }
            else if (battle.PlayerHp <= 0)
            {
                battle.Phase = RivalBattlePhase.Defeat;
                battle.PlayerHp = battle.PlayerMaxHp;
                battle.RivalHp = battle.RivalMaxHp;
                battle.DeployedSquad.Clear();
                battle.RecentRounds.Clear();
                battle.RoundNumber = 0;
                battle.Momentum = 0;
                state.Occult.Suspicion = Math.Min(OccultBalance.SuspicionMax, state.Occult.Suspicion + 20);
                AppendLog(battle, $"DEFEAT! Your assault force was annihilated. Suspicion rises.");
            }
        }

        rs.RivalBattles.RemoveAll(b => b.Phase == RivalBattlePhase.Victory);
    }

    private static void AppendLog(RivalBattleState battle, string message) =>
        BattleCommon.AppendLog(battle.Log, message, MaxLogEntries);
}
