using System.Text.Json.Serialization;
using System.Text.Json;

namespace CultSimulator.Game;

public class GameState
{
    public string CultName { get; set; } = "";
    public long StartedAt { get; set; }
    public bool StoryShown { get; set; }
    public int SaveVersion { get; set; } = 2;
    public List<CovenState> Covens { get; set; } = new();
    public string ActiveCovenId { get; set; } = "skanor";
    public long LastSavedAt { get; set; }
    public int GrandSacrificeCount { get; set; }
    public double EldritchFavor { get; set; }
    public double TotalLifetimeFaith { get; set; }
    public string? PendingContinentStory { get; set; }
    public List<string> RevealedFootholds { get; set; } = new();
    public List<LocalCultInstance> ActiveLocalCults { get; set; } = new();
    public List<LocalCultBattleState> LocalCultBattles { get; set; } = new();
    public ShadowWarState? ShadowWar { get; set; }
    public BattleSystemState? BattleSystem { get; set; }
    public RivalCultSystemState? RivalCults { get; set; }
    public ConversionState? Conversion { get; set; }

    [JsonIgnore]
    public CovenState HomeCoven => Covens.FirstOrDefault(c => c.Id == "skanor") ?? Covens[0];
    [JsonIgnore]
    public CovenState ActiveCoven => FindCoven(ActiveCovenId) ?? Covens[0];
    [JsonIgnore]
    public OccultState Occult => ActiveCoven.Occult;
    [JsonIgnore]
    public ShadowWarState ShadowWarOrInit => ShadowWar ??= ShadowWarEngine.CreateInitialState();
    [JsonIgnore]
    public RivalCultSystemState RivalCultsOrInit => RivalCults ??= RivalCultEngine.CreateInitialState();

    public CovenState? FindCoven(string id) => Covens.FirstOrDefault(c => c.Id == id);
}
