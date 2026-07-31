using System.Text.Json;
using System.Text.Json.Serialization;

namespace CultSimulator.Game;

public static class SaveLoad
{
    public static readonly JsonSerializerOptions JsonOptions = new() { Converters = { new JsonStringEnumConverter() }, WriteIndented = false };

    public static GameState LoadGame(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return GameEngine.InitialState();
        try
        {
            var state = JsonSerializer.Deserialize<GameState>(json, JsonOptions);
            if (state == null) return GameEngine.InitialState();
            if (!IsValid(state)) return GameEngine.InitialState();
            Migrate(state);
            return state;
        }
        catch
        {
            return GameEngine.InitialState();
        }
    }

    /// <summary>
    /// Tries each save slot (primary, backup, backup2) and returns the first
    /// that deserializes and validates. Returns a fresh initial state only if
    /// all three are missing or corrupt.
    /// </summary>
    public static (GameState state, bool loaded) LoadGameWithBackup(string? primary, string? backup, string? backup2)
    {
        foreach (var (json, slot) in new[] { (primary, "primary"), (backup, "backup"), (backup2, "backup2") })
        {
            if (string.IsNullOrWhiteSpace(json)) continue;
            try
            {
                var s = JsonSerializer.Deserialize<GameState>(json, JsonOptions);
                if (s != null && IsValid(s)) { Migrate(s); return (s, true); }
            }
            catch { }
        }

        return (GameEngine.InitialState(), false);
    }

    /// <summary>
    /// Lightweight structural validation to reject truncated or corrupted saves.
    /// </summary>
    public static bool IsValid(GameState state)
    {
        if (state.Covens == null) return false;
        if (state.Covens.Count == 0) return false;
        if (string.IsNullOrEmpty(state.ActiveCovenId)) return false;
        var active = state.Covens.FirstOrDefault(c => c.Id == state.ActiveCovenId);
        if (active == null) return false;
        if (active.Occult == null) return false;
        foreach (var c in state.Covens)
        {
            if (string.IsNullOrEmpty(c.Id)) return false;
            if (c.Buildings == null) return false;
            if (c.Upgrades == null) return false;
            if (c.Occult == null) return false;
        }
        return true;
    }

    private static void Migrate(GameState state)
    {
        state.Covens ??= new List<CovenState>();
        if (state.Covens.Count == 0) { state.Covens.Add(new CovenState { Id = "skanor", Converted = true }); state.StoryShown = false; }
        if (string.IsNullOrEmpty(state.ActiveCovenId)) state.ActiveCovenId = "skanor";
        if (state.LastSavedAt == 0) state.LastSavedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        if (state.ExtensionData != null && state.ExtensionData.TryGetValue("Occult", out var legacyOccultJson))
        {
            if (legacyOccultJson.TryGetProperty("EldritchFavor", out var favorVal))
                state.EldritchFavor = favorVal.GetDouble();
            if (legacyOccultJson.TryGetProperty("GrandSacrificeCount", out var countVal))
                state.GrandSacrificeCount = countVal.GetInt32();

            var legacy = legacyOccultJson.Deserialize<OccultState>(SaveLoad.JsonOptions);
            if (legacy != null)
            {
                var home = state.Covens.FirstOrDefault(c => c.Id == "skanor");
                if (home != null) home.Occult = legacy;
            }
            state.ExtensionData.Remove("Occult");
        }

        foreach (var c in state.Covens)
        {
            c.Buildings ??= new Dictionary<BuildingType, int>();
            c.Upgrades ??= new List<UpgradeId>();
            c.Occult ??= new OccultState();
            c.Occult.Minions ??= new List<Minion>();
            c.Occult.HighCouncil ??= new List<CovenMember>();
            c.Occult.MapNodes ??= new List<MapNodeState>();
            c.Occult.UnlockedTechs ??= new List<TechId>();
            c.Occult.SocketedArtifacts ??= new List<string>();
            c.Occult.OwnedArtifacts ??= new List<string>();
            c.Occult.Materials ??= new Dictionary<MaterialKind, int>();
        }
        state.Conversion ??= null;
        state.ActiveLocalCults ??= new List<LocalCultInstance>();
        state.ShadowWar ??= ShadowWarEngine.CreateInitialState();
        state.ShadowWar.Institutions ??= new List<InstitutionState>();
        foreach (var def in ShadowWarData.Institutions)
            if (state.ShadowWar.GetInstitution(def.Id) == null)
                state.ShadowWar.Institutions.Add(new InstitutionState
                {
                    Id = def.Id,
                    Status = def.Prerequisites == null || def.Prerequisites.Length == 0
                        ? InstitutionStatus.Unlocked
                        : InstitutionStatus.Locked,
                    DefenseRemaining = def.Defense
                });
        state.BattleSystem ??= BattleEngine.CreateInitialState();
    }

    public static string SaveGame(GameState s) => JsonSerializer.Serialize(s, JsonOptions);
}
