using System.Linq;

namespace CultSimulator.Game;

public static class LocalCultBattleEngine
{
    public const double PlayerBaseHp = 60;
    public const int MaxLogEntries = 10;

    public static LocalCultBattleState CreateBattle(LocalCultDef def)
    {
        double rivalHp = 40 + def.FollowersRequired * 0.3;
        if (def.IsBoss) rivalHp *= 2.0;
        return new LocalCultBattleState
        {
            CultId = def.Id,
            RivalName = def.Name,
            RivalHp = rivalHp,
            RivalMaxHp = rivalHp,
            PlayerHp = PlayerBaseHp,
            PlayerMaxHp = PlayerBaseHp,
            Phase = LocalCultBattlePhase.Deploy,
            LastTickAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
    }

    public static LocalCultBattleState? GetBattle(GameState state, string cultId)
    {
        state.LocalCultBattles ??= new();
        return state.LocalCultBattles.FirstOrDefault(b => b.CultId == cultId);
    }

    public static LocalCultBattleState GetOrCreateBattle(GameState state, LocalCultDef def)
    {
        var existing = GetBattle(state, def.Id);
        if (existing != null) return existing;
        var battle = CreateBattle(def);
        state.LocalCultBattles ??= new();
        state.LocalCultBattles.Add(battle);
        return battle;
    }

    public static (bool success, string message) DeployAgents(GameState state, string cultId, AgentType type, int count)
    {
        var sw = ShadowWarEngine.EnsureInitialized(state);
        sw.RecruitedAgents.TryGetValue(type, out int owned);

        var def = LocalCultData.Find(cultId);
        if (def == null) return (false, "Local cult not found.");

        var battle = GetOrCreateBattle(state, def);
        if (battle.Phase != LocalCultBattlePhase.Deploy && battle.Phase != LocalCultBattlePhase.Fighting)
            return (false, "Battle is not in deploy or fighting phase.");

        if (battle.Phase == LocalCultBattlePhase.Fighting && type == AgentType.Initiate)
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

    public static (bool success, string message) WithdrawAgents(GameState state, string cultId)
    {
        var sw = ShadowWarEngine.EnsureInitialized(state);
        var battle = GetBattle(state, cultId);
        if (battle == null) return (false, "No battle found.");
        if (battle.Phase == LocalCultBattlePhase.Fighting)
            return (false, "Cannot withdraw during an active battle.");

        battle.DeployedSquad.Clear();
        return (true, "Agents withdrawn.");
    }

    public static (bool success, string message) ReinforceAgents(GameState state, string cultId, AgentType type, int count)
    {
        var sw = ShadowWarEngine.EnsureInitialized(state);
        var battle = GetBattle(state, cultId);
        if (battle == null || battle.Phase != LocalCultBattlePhase.Fighting)
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

    public static (bool success, string message) StartBattle(GameState state, string cultId)
    {
        var battle = GetBattle(state, cultId);
        if (battle == null || battle.Phase != LocalCultBattlePhase.Deploy)
            return (false, "Not in deploy phase.");
        if (battle.TotalDeployed == 0)
            return (false, "Deploy at least one agent before starting.");

        var sw = ShadowWarEngine.EnsureInitialized(state);
        foreach (var slot in battle.DeployedSquad)
        {
            sw.RecruitedAgents.TryGetValue(slot.Type, out int cur);
            sw.RecruitedAgents[slot.Type] = Math.Max(0, cur - slot.Count);
        }

        battle.Phase = LocalCultBattlePhase.Fighting;
        var localDef = LocalCultData.Find(cultId);
        if (localDef != null)
        {
            double localScale = 0.5 + localDef.FollowersRequired / 40.0;
            if (localDef.IsBoss) localScale *= 1.5;
            battle.EnemyUnits = EnemyCompositionBuilder.BuildComposition(RivalCultArchetype.TheCrimsonConclave, localScale, battle.RivalMaxHp / 10);
            battle.EnemyArchetype = RivalCultArchetype.TheCrimsonConclave;
        }
        AppendLog(battle, $"Battle started with {battle.TotalDeployed} agents.");
        return (true, "Battle started!");
    }

    public static void Tick(GameState state, double deltaSec)
    {
        state.LocalCultBattles ??= new();
        CleanupVictories(state);
        foreach (var battle in state.LocalCultBattles.ToList())
        {
            if (battle.Phase != LocalCultBattlePhase.Fighting) continue;

            var sw = ShadowWarEngine.EnsureInitialized(state);
            var def = LocalCultData.Find(battle.CultId);
            double rivalAttack = def != null ? 2.0 + def.FollowersRequired * 0.01 : 3.0;
            if (def != null && def.IsBoss) rivalAttack *= 1.5;
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
                if (battle.EnemyArchetype != null)
                {
                    var tactic = BattleRoundEngine.TryEnemyTactic(battle.EnemyArchetype.Value, battle.DeployedSquad, battle.RoundNumber);
                    if (tactic != null) round.EnemyAction = tactic;
                    var (reinforced, action) = BattleRoundEngine.TryEnemyReinforce(battle.EnemyUnits, battle.EnemyArchetype.Value, 0.5, battle.RoundNumber);
                    if (reinforced) { round.EnemyReinforced = true; round.EnemyAction = action; }
                }
                battle.RecentRounds.Add(round);
                if (battle.RecentRounds.Count > 6) battle.RecentRounds.RemoveAt(0);
                AppendLog(battle, round.Summary);
            }

            if (battle.RivalHp <= 0)
            {
                battle.Phase = LocalCultBattlePhase.Victory;
                battle.Status = LocalCultBattleStatus.Victory;
                battle.VictoryAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                battle.EnemyUnits.Clear();
                battle.RecentRounds.Clear();
                battle.RoundNumber = 0;
                battle.Momentum = 0;
                AppendLog(battle, "Victory! The local cult has been defeated.");
                ApplyVictory(state, battle);
            }
            else if (battle.PlayerHp <= 0)
            {
                battle.Phase = LocalCultBattlePhase.Deploy;
                battle.Status = LocalCultBattleStatus.Defeat;
                battle.PlayerHp = battle.PlayerMaxHp;
                battle.RivalHp = battle.RivalMaxHp;
                battle.DeployedSquad.Clear();
                battle.RecentRounds.Clear();
                battle.RoundNumber = 0;
                battle.Momentum = 0;
                AppendLog(battle, "Defeat! Your agents were lost in battle. Recruit new ones to try again.");
            }
        }
    }

    public static void ClearBattle(GameState state, string cultId)
    {
        state.LocalCultBattles ??= new();
        var battle = GetBattle(state, cultId);
        if (battle != null) state.LocalCultBattles.Remove(battle);
    }

    private static void ApplyVictory(GameState state, LocalCultBattleState battle)
    {
        var def = LocalCultData.Find(battle.CultId);
        if (def == null) return;
        double faithBonus = def.RewardAmount;
        state.ActiveCoven.Faith += faithBonus;
        state.Occult.LifetimeFaith += faithBonus;
        state.TotalLifetimeFaith += faithBonus;
        battle.LastFaithReward = faithBonus;
        LocalCultEngine.OnVictory(state, battle.CultId);
    }

    public static void CleanupVictories(GameState state)
    {
        if (state.LocalCultBattles == null) return;
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        state.LocalCultBattles.RemoveAll(b => b.Phase == LocalCultBattlePhase.Victory && now - b.VictoryAt > 5000);
    }

    private static void AppendLog(LocalCultBattleState battle, string message) =>
        BattleCommon.AppendLog(battle.Log, message, MaxLogEntries);
}
