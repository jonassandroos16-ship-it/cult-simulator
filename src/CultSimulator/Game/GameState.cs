using System.Text.Json.Serialization;
using System.Text.Json;

namespace CultSimulator.Game;

public class GameState
{
    public string CultName { get; set; } = "";
    public long StartedAt { get; set; }
    public bool StoryShown { get; set; }
    public List<CovenState> Covens { get; set; } = new();
    public string ActiveCovenId { get; set; } = "skanor";
    public long LastSavedAt { get; set; }
    public int GrandSacrificeCount { get; set; }
    public double EldritchFavor { get; set; }
    public bool StoryStepCompleted { get; set; }
    public int StoryStep { get; set; }
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }
    public ConversionState? Conversion { get; set; }
    public List<LocalCultInstance> ActiveLocalCults { get; set; } = new();
    public ShadowWarState? ShadowWar { get; set; }

    [JsonIgnore]
    public ShadowWarState ShadowWarOrInit => ShadowWar ??= ShadowWarEngine.CreateInitialState();

    public RivalCultSystemState? RivalCults { get; set; }

    [JsonIgnore]
    public RivalCultSystemState RivalCultsOrInit => RivalCults ??= RivalCultEngine.CreateInitialState();

    public BattleSystemState? BattleSystem { get; set; }

    [JsonIgnore]
    public BattleSystemState BattleSystemOrInit => BattleSystem ??= BattleEngine.CreateInitialState();

    /// <summary>
    /// Ids of foothold covens revealed by completing the previous continent.
    /// Each entry unlocks the next continent so the player can expand into it.
    /// </summary>
    public List<string> RevealedFootholds { get; set; } = new();

    /// <summary>
    /// Continent whose completion story is currently pending display.
    /// Set when a continent is fully conquered; cleared after the player
    /// dismisses the story beat and the foothold is granted.
    /// </summary>
    public string? PendingContinentStory { get; set; }

    [JsonIgnore]
    public CovenState HomeCoven => Covens.FirstOrDefault(c => c.Id == "skanor") ?? Covens[0];
    [JsonIgnore]
    public CovenState ActiveCoven => Covens.FirstOrDefault(c => c.Id == ActiveCovenId) ?? HomeCoven;
    [JsonIgnore]
    public OccultState Occult => ActiveCoven.Occult;
    public CovenState? FindCoven(string id) => Covens.FirstOrDefault(c => c.Id == id);
}
