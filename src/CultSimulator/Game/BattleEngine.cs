using System.Linq;

namespace CultSimulator.Game;

public static class BattleEngine
{
    public const double PlayerBaseHp = 100;
    public const double RivalBaseHp = 100;
    public const double CooldownSec = 30;
    public const int MaxLogEntries = 20;

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

    public static bool ShouldSpawnRival(GameState state, WorldLocationService locations, string continentId)
    {
        return IsTheaterActive(state, locations, continentId);
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
            PlayerMaxHp = PlayerBaseHp
        };
        bs.Battles.Add(battle);
        return battle;
    }

    public static void EnsureBattleForContinent(GameState state, WorldLocationService locations, string continentId)
    {
        var battle = GetOrCreateBattle(state, locations, continentId);

        if (battle.Phase == BattlePhase.NoThreat && ShouldSpawnRival(state, locations, continentId))
        {
            var rivalDef = BattleData.RivalForContinent(continentId);
            if (rivalDef != null)
            {
                battle.Phase = BattlePhase.Deploy;
                battle.Status = BattleStatus.Active;
                battle.RivalHp = battle.RivalMaxHp;
                battle.PlayerHp = battle.PlayerMaxHp;
                AddLog(battle, $"{rivalDef.Icon} {rivalDef.Name} has emerged in {BattleData.Theater(continentId)?.Name}!");
            }
        }
    }

    public static (bool success, string message) RecruitAgent(GameState state, AgentType type, int count)
    {
        var def = BattleData.AgentDef(type);
        if (def == null) return (false, "Unknown agent type.");
        int totalCost = def.AgentCost * count;
        var sw = state.ShadowWarOrInit;
        if (sw.AvailableAgents < totalCost)
            return (false, $"Need {totalCost} agents to recruit {count} {def.Name}(s).");

        if (type == AgentType.Zealot)
        {
            int availableZealots = state.Occult.Minions.Count(m => m.Role == PromotedRole.Zealot);
            int deployedZealots = CountDeployedAgents(state, AgentType.Zealot);
            if (deployedZealots + count > availableZealots)
                return (false, $"Only {availableZealots - deployedZealots} Zealot(s) available. Promote more in the Sanctum.");
        }
        if (type == AgentType.Infiltrator)
        {
            int availableInfiltrators = state.Occult.Minions.Count(m => m.Role == PromotedRole.Infiltrator);
            int deployedInfiltrators = CountDeployedAgents(state, AgentType.Infiltrator);
            if (deployedInfiltrators + count > availableInfiltrators)
                return (false, $"Only {availableInfiltrators - deployedInfiltrators} Infiltrator(s) available. Promote more in the Sanctum.");
        }

        sw.TotalAgents -= totalCost;
        sw.TotalAgents += count;
        return (true, $"Recruited {count} {def.Name}(s).");
    }

    public static int CountDeployedAgents(GameState state, AgentType type)
    {
        if (state.BattleSystem == null) return 0;
        return state.BattleSystem.Battles
            .SelectMany(b => b.DeployedSquad)
            .Where(d => d.Type == type)
            .Sum(d => d.Count);
    }

    public static int TotalUnitsOfType(GameState state, AgentType type)
    {
        var sw = state.ShadowWarOrInit;
        return type switch
        {
            AgentType.Acolyte => (int)Math.Floor(sw.TotalAgents),
            AgentType.Zealot => state.Occult.Minions.Count(m => m.Role == PromotedRole.Zealot),
            AgentType.Infiltrator => state.Occult.Minions.Count(m => m.Role == PromotedRole.Infiltrator),
            _ => 0
        };
    }

    public static int AvailableUnitsOfType(GameState state, AgentType type)
    {
        return TotalUnitsOfType(state, type) - CountDeployedAgents(state, type);
    }

    public static (bool success, string message) DeployAgents(GameState state, string continentId, AgentType type, int count)
    {
        var sw = state.ShadowWarOrInit;
        if (sw.AvailableAgents < count)
            return (false, "Not enough available agents.");

        var battle = state.BattleSystem?.GetBattle(continentId);
        if (battle == null || (battle.Phase != BattlePhase.Deploy && battle.Phase != BattlePhase.Fighting))
            return (false, "No active battle in this continent.");

        if (type == AgentType.Zealot)
        {
            int available = state.Occult.Minions.Count(m => m.Role == PromotedRole.Zealot) - CountDeployedAgents(state, AgentType.Zealot);
            if (count > available)
                return (false, "Not enough Zealots available.");
        }
        if (type == AgentType.Infiltrator)
        {
            int available = state.Occult.Minions.Count(m => m.Role == PromotedRole.Infiltrator) - CountDeployedAgents(state, AgentType.Infiltrator);
            if (count > available)
                return (false, "Not enough Infiltrators available.");
        }

        sw.DeployedAgents += count;
        var existing = battle.DeployedSquad.FirstOrDefault(d => d.Type == type);
        if (existing != null)
            existing.Count += count;
        else
            battle.DeployedSquad.Add(new DeployedAgent { Type = type, Count = count });

        var def = BattleData.AgentDef(type);
        AddLog(battle, $"Deployed {count} {def?.Name}(s) to the battle.");
        return (true, $"Deployed {count} {def?.Name}(s).");
    }

    public static (bool success, string message) WithdrawAgents(GameState state, string continentId)
    {
        var sw = state.ShadowWarOrInit;
        var battle = state.BattleSystem?.GetBattle(continentId);
        if (battle == null) return (false, "No battle found.");
        if (battle.TotalDeployed == 0) return (false, "No agents deployed.");

        sw.DeployedAgents -= battle.TotalDeployed;
        battle.DeployedSquad.Clear();
        AddLog(battle, "Agents withdrawn from the battle.");
        return (true, "Agents withdrawn.");
    }

    public static (bool success, string message) StartBattle(GameState state, string continentId)
    {
        var battle = state.BattleSystem?.GetBattle(continentId);
        if (battle == null || battle.Phase != BattlePhase.Deploy)
            return (false, "Battle is not ready to start.");
        if (battle.TotalDeployed == 0)
            return (false, "Deploy at least one agent before starting the battle.");

        battle.Phase = BattlePhase.Fighting;
        AddLog(battle, "Battle commenced!");
        return (true, "Battle started!");
    }

    public static void Tick(GameState state, WorldLocationService locations, double deltaSec)
    {
        var bs = EnsureInitialized(state);
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        foreach (var continent in new[] { "europe", "north_america", "south_america", "asia", "oceania", "africa", "middle_east" })
        {
            if (IsTheaterActive(state, locations, continent))
                EnsureBattleForContinent(state, locations, continent);
        }

        foreach (var battle in bs.Battles)
        {
            if (battle.Phase == BattlePhase.Fighting)
                ProcessBattle(state, battle, deltaSec, now);
            else if (battle.Phase == BattlePhase.Cooldown && now >= battle.CooldownUntil)
            {
                var rivalDef = BattleData.RivalForContinent(battle.ContinentId);
                if (rivalDef != null && ShouldSpawnRival(state, locations, battle.ContinentId))
                {
                    battle.Phase = BattlePhase.Deploy;
                    battle.Status = BattleStatus.Active;
                    battle.RivalHp = battle.RivalMaxHp;
                    battle.PlayerHp = battle.PlayerMaxHp;
                    battle.DeployedSquad.Clear();
                    AddLog(battle, $"{rivalDef.Icon} {rivalDef.Name} has returned!");
                }
                else
                {
                    battle.Phase = BattlePhase.NoThreat;
                    battle.Status = BattleStatus.NotStarted;
                }
            }
        }
    }

    private static void ProcessBattle(GameState state, BattleState battle, double deltaSec, long now)
    {
        if (battle.TotalDeployed == 0)
        {
            battle.Phase = BattlePhase.Deploy;
            AddLog(battle, "All agents lost. Redeploy to continue.");
            return;
        }

        var sw = state.ShadowWarOrInit;
        double strengthMult = ShadowWarEngine.AgentStrength(sw, state);

        double playerAttack = 0;
        double totalStealth = 0;
        foreach (var deployed in battle.DeployedSquad)
        {
            var def = BattleData.AgentDef(deployed.Type);
            if (def == null) continue;
            playerAttack += deployed.Count * def.Attack * strengthMult;
            totalStealth += deployed.Count * def.Stealth;
        }

        double rivalDefDamage = playerAttack * deltaSec;
        battle.RivalHp = Math.Max(0, battle.RivalHp - rivalDefDamage);

        var rivalDef = BattleData.RivalForContinent(battle.ContinentId);
        double rivalAttack = rivalDef != null ? rivalDef.AgentStrength * 5 * deltaSec : 3 * deltaSec;

        double stealthReduction = Math.Min(0.7, totalStealth * 0.05);
        double effectiveRivalAttack = rivalAttack * (1.0 - stealthReduction);
        battle.PlayerHp = Math.Max(0, battle.PlayerHp - effectiveRivalAttack);

        if (battle.PlayerHp < battle.PlayerMaxHp * 0.3 && Random.Shared.NextDouble() < 0.1 * deltaSec)
        {
            var firstSquad = battle.DeployedSquad.FirstOrDefault();
            if (firstSquad != null && firstSquad.Count > 0)
            {
                firstSquad.Count--;
                sw.DeployedAgents--;
                if (firstSquad.Count <= 0)
                    battle.DeployedSquad.Remove(firstSquad);
                AddLog(battle, "An agent was lost in battle!");
            }
        }

        if (battle.RivalHp <= 0)
        {
            battle.Phase = BattlePhase.Cooldown;
            battle.Status = BattleStatus.Victory;
            battle.CooldownUntil = now + (long)(CooldownSec * 1000);

            sw.DeployedAgents -= battle.TotalDeployed;
            battle.DeployedSquad.Clear();

            sw.Heat = Math.Max(0, sw.Heat - 20);

            var rival = state.RivalCultsOrInit.Rivals.FirstOrDefault(r => r.Id == rivalDef?.Id);
            if (rival != null)
            {
                rival.Status = RivalCultStatus.Dormant;
                rival.Power = 0;
                rival.ControlledInstitutions.Clear();
            }

            AddLog(battle, $"Victory! {rivalDef?.Name} has been defeated. Agents returning home.");
        }
        else if (battle.PlayerHp <= 0)
        {
            battle.Phase = BattlePhase.Cooldown;
            battle.Status = BattleStatus.Defeat;
            battle.CooldownUntil = now + (long)(CooldownSec * 1000);

            sw.DeployedAgents -= battle.TotalDeployed;
            sw.TotalAgents -= battle.TotalDeployed;
            battle.DeployedSquad.Clear();

            var continentInsts = ShadowWarData.InstitutionsForTerritory(battle.ContinentId);
            var playerControlled = continentInsts
                .FirstOrDefault(i => sw.GetInstitution(i.Id)?.Status == InstitutionStatus.Controlled);
            if (playerControlled != null)
            {
                var inst = sw.GetInstitution(playerControlled.Id);
                if (inst != null)
                {
                    inst.Status = InstitutionStatus.Investigated;
                    inst.InvestigationDefense = 30;
                    var rival = state.RivalCultsOrInit.Rivals.FirstOrDefault(r => r.Id == rivalDef?.Id);
                    if (rival != null)
                        rival.ControlledInstitutions.Add(inst.Id);
                    AddLog(battle, $"Defeat! {rivalDef?.Name} seized {playerControlled.Name}!");
                }
            }
            else
            {
                AddLog(battle, $"Defeat! Your forces were overwhelmed by {rivalDef?.Name}.");
            }
        }
    }

    private static void AddLog(BattleState battle, string msg)
    {
        battle.Log.Add(msg);
        if (battle.Log.Count > MaxLogEntries)
            battle.Log.RemoveAt(0);
    }

    public static List<TerritoryLossEvent> GetRecentLosses(GameState state, int maxCount = 5)
    {
        var losses = new List<TerritoryLossEvent>();
        if (state.BattleSystem == null) return losses;

        foreach (var battle in state.BattleSystem.Battles)
        {
            if (battle.Status == BattleStatus.Defeat)
            {
                var theater = BattleData.Theater(battle.ContinentId);
                var rival = BattleData.RivalForContinent(battle.ContinentId);
                losses.Add(new TerritoryLossEvent(
                    battle.ContinentId,
                    theater?.Name ?? battle.ContinentId,
                    rival?.Name ?? "Unknown",
                    battle.CooldownUntil
                ));
            }
        }
        return losses.Take(maxCount).ToList();
    }
}

public record TerritoryLossEvent(string ContinentId, string ContinentName, string RivalName, long Timestamp);
