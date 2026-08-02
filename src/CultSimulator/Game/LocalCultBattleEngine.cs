using System.Linq;

namespace CultSimulator.Game;

/// <summary>
/// Data-driven battle system for local cult takeovers.
/// Local cults use the same agent-based combat as continent battles,
/// but are simpler: single encounter, no cooldown, lower HP pools.
/// </summary>
public static class LocalCultBattleEngine
{
    public const double PlayerBaseHp = 60;
    public const int MaxLogEntries = 10;

    public static LocalCultBattleState CreateBattle(LocalCultDef def)
    {
        double rivalHp = 40 + def.FollowersRequired * 0.3;
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
        return DeployAgents(state, cultId, type, count);
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
        AppendLog(battle, $"Battle started with {battle.TotalDeployed} agents.");
        return (true, "Battle started!");
    }

    public static void Tick(GameState state, double deltaSec)
    {
        state.LocalCultBattles ??= new();
        foreach (var battle in state.LocalCultBattles.ToList())
        {
            if (battle.Phase != LocalCultBattlePhase.Fighting) continue;

            var sw = ShadowWarEngine.EnsureInitialized(state);
            var def = LocalCultData.Find(battle.CultId);
            double rivalAttack = def != null ? 2.0 + def.FollowersRequired * 0.01 : 3.0;

            var (mitigated, playerDamage) = BattleCommon.ExchangeDamage(battle.DeployedSquad, rivalAttack, sw, state, deltaSec);

            double faithRegen = BattleCommon.CalculateFaithRegen(battle.DeployedSquad);
            battle.PlayerHp = Math.Min(battle.PlayerMaxHp, battle.PlayerHp + faithRegen * deltaSec);

            battle.RivalHp = Math.Max(0, battle.RivalHp - playerDamage);
            battle.PlayerHp = Math.Max(0, battle.PlayerHp - mitigated);

            if (battle.RivalHp <= 0)
            {
                battle.Phase = LocalCultBattlePhase.Victory;
                battle.Status = LocalCultBattleStatus.Victory;
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
        var instance = state.ActiveLocalCults.FirstOrDefault(i => i.CultId == battle.CultId);
        if (instance != null) state.ActiveLocalCults.Remove(instance);
        ClearBattle(state, battle.CultId);
    }

    private static void AppendLog(LocalCultBattleState battle, string message) =>
        BattleCommon.AppendLog(battle.Log, message, MaxLogEntries);
}
