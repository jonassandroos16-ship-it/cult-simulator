using System.Text.Json;
using System.Text.Json.Serialization;

namespace CultSimulator.Game;

public static class SaveLoad
{
    public const int CurrentVersion = 2;

    private static readonly JsonSerializerOptions Options = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNameCaseInsensitive = true
    };

    public static string SaveGame(GameState state)
    {
        state.SaveVersion = CurrentVersion;
        return JsonSerializer.Serialize(state, Options);
    }

    public static (GameState state, bool success) LoadGameWithBackup(string? primary, string? backup, string? backup2)
    {
        if (TryLoad(primary, out var s1)) return (s1, true);
        if (TryLoad(backup, out var s2)) return (s2, true);
        if (TryLoad(backup2, out var s3)) return (s3, true);
        return (GameEngine.InitialState(), false);
    }

    public static bool IsValidSave(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return false;
        try
        {
            var state = JsonSerializer.Deserialize<GameState>(json, Options);
            return state != null && !string.IsNullOrWhiteSpace(state.CultName);
        }
        catch { return false; }
    }

    public static bool IsCorrupted(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return true;
        try
        {
            var state = JsonSerializer.Deserialize<GameState>(json, Options);
            if (state == null) return true;
            if (string.IsNullOrWhiteSpace(state.CultName)) return true;
            if (state.Covens == null) return true;
            if (state.ActiveCovenId == null) return true;
            return false;
        }
        catch { return true; }
    }

    private static bool TryLoad(string? json, out GameState state)
    {
        state = GameEngine.InitialState();
        if (string.IsNullOrWhiteSpace(json)) return false;
        try
        {
            state = JsonSerializer.Deserialize<GameState>(json, Options);
            if (state == null)
            {
                state = GameEngine.InitialState();
                return false;
            }
            if (string.IsNullOrWhiteSpace(state.CultName))
                return false;
            EnsureCollections(state);
            Migrate(state);
            return true;
        }
        catch { return false; }
    }

    private static void Migrate(GameState state)
    {
        if (state.SaveVersion >= CurrentVersion) return;

        if (state.SaveVersion < 1)
        {
            state.ShadowWar ??= ShadowWarEngine.CreateInitialState();
            state.BattleSystem ??= BattleEngine.CreateInitialState();
            state.RivalCults ??= RivalCultEngine.CreateInitialState();
        }

        if (state.SaveVersion < 2)
        {
            state.LocalCultBattles ??= new List<LocalCultBattleState>();
            state.RivalCults ??= RivalCultEngine.CreateInitialState();
            state.RivalCults.RivalBattles ??= new List<RivalBattleState>();
        }

        state.SaveVersion = CurrentVersion;
    }

    private static void EnsureCollections(GameState state)
    {
        state.Covens ??= new List<CovenState>();
        state.RevealedFootholds ??= new List<string>();
        state.ActiveLocalCults ??= new List<LocalCultInstance>();
        state.LocalCultBattles ??= new List<LocalCultBattleState>();
        foreach (var coven in state.Covens)
        {
            coven.Buildings ??= new Dictionary<BuildingType, int>();
            coven.Upgrades ??= new List<UpgradeId>();
            coven.Occult ??= new OccultState();
            coven.Occult.Minions ??= new List<Minion>();
            coven.Occult.HighCouncil ??= new List<CovenMember>();
            coven.Occult.UnlockedTechs ??= new List<TechId>();
            coven.Occult.SocketedArtifacts ??= new List<string>();
            coven.Occult.OwnedArtifacts ??= new List<string>();
            coven.Occult.MapNodes ??= new List<MapNodeState>();
            coven.Occult.Materials ??= new Dictionary<MaterialKind, int>();
            coven.Occult.LeyLines ??= new List<string[]>();
        }
        state.ShadowWar ??= ShadowWarEngine.CreateInitialState();
        state.ShadowWar.RecruitedAgents ??= new Dictionary<AgentType, int>();
        state.BattleSystem ??= BattleEngine.CreateInitialState();
        state.BattleSystem.Battles ??= new List<BattleState>();
        state.RivalCults ??= RivalCultEngine.CreateInitialState();
        state.RivalCults.Rivals ??= new List<RivalCultState>();
        state.RivalCults.RivalBattles ??= new List<RivalBattleState>();
    }
}
