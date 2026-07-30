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

    /// <summary>
    /// Global prestige state. Eldritch Favor and Grand Sacrifice count are
    /// shared across all covens — they represent the cult's overall standing
    /// with the eldritch powers, not any single coven's progress.
    /// </summary>
    public double EldritchFavor { get; set; }
    public int GrandSacrificeCount { get; set; }

    /// <summary>
    /// Captures unknown JSON properties during deserialization, used to
    /// detect and migrate the legacy global Occult field from old saves.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? ExtensionData { get; set; }

    /// <summary>
    /// Returns the active coven's occult state. Each coven has its own
    /// OccultState, so this is a computed property — not a stored field.
    /// </summary>
    [JsonIgnore]
    public OccultState Occult => ActiveCoven.Occult;

    /// <summary>
    /// Active narrative-siege conversion state. Null when no conversion is
    /// in progress. Persisted so a conversion can resume after a reload.
    /// </summary>
    public ConversionState? Conversion { get; set; }

    /// <summary>
    /// Local cults currently spawned on the local map. Persisted so spawned
    /// cults survive reloads. Max 3 at a time per coven.
    /// </summary>
    public List<LocalCultInstance> ActiveLocalCults { get; set; } = new();

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
