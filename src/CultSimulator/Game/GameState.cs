using System.Text.Json.Serialization;
using System.Text.Json;
using System.Collections.Generic;

namespace CultSimulator.Game;

public class GameState
{
    public string CultName { get; set; } = "";
    public long StartedAt { get; set; }
    public long LastSavedAt { get; set; }
    public bool StoryShown { get; set; }
    public string ActiveCovenId { get; set; } = "";
    public List<CovenState> Covens { get; set; } = new();

    public double EldritchFavor { get; set; }
    public int GrandSacrificeCount { get; set; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    [JsonIgnore]
    public OccultState Occult => ActiveCoven.Occult;

    public ConversionState? Conversion { get; set; }

    public List<LocalCultInstance> ActiveLocalCults { get; set; } = new();

    public ShadowWarState? ShadowWar { get; set; }

    [JsonIgnore]
    public ShadowWarState ShadowWarOrInit => ShadowWar ??= ShadowWarEngine.CreateInitialState();

    public RivalCultSystemState? RivalCults { get; set; }

    [JsonIgnore]
    public RivalCultSystemState RivalCultsOrInit => RivalCults ??= RivalCultEngine.CreateInitialState();

    [JsonIgnore]
    public string? ActiveEventId { get; set; }

    public CovenState HomeCoven => Covens.First(c => c.Id == "skanor");

    public CovenState ActiveCoven
    {
        get
        {
            if (Covens.Count == 0) return new CovenState { Id = "skanor" };
            var id = string.IsNullOrEmpty(ActiveCovenId) ? "skanor" : ActiveCovenId;
            return Covens.FirstOrDefault(c => c.Id == id) ?? HomeCoven;
        }
    }

    public CovenState? FindCoven(string id) =>
        Covens.FirstOrDefault(c => c.Id == id);
}
