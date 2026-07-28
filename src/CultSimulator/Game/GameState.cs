using System.Text.Json.Serialization;

namespace CultSimulator.Game;

public class GameState
{
    public string CultName { get; set; } = "";
    public long StartedAt { get; set; }
    public long LastSavedAt { get; set; }
    public bool StoryShown { get; set; }
    public string ActiveCovenId { get; set; } = "";
    public List<CovenState> Covens { get; set; } = new();
    public OccultState Occult { get; set; } = new();

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