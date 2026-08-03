using System.Text.Json.Serialization;

namespace CultSimulator.Game;

public class RivalCultState
{
    public string Id { get; set; } = "";
    public RivalCultStatus Status { get; set; } = RivalCultStatus.Dormant;
    public double Power { get; set; }
    public long NextActionAt { get; set; }
    public bool Defeated { get; set; }
}

public class RivalCultSystemState
{
    public List<RivalCultState> Rivals { get; set; } = new();
    public long ActivatedAt { get; set; }
    public bool IsActive { get; set; }
    public List<RivalBattleState> RivalBattles { get; set; } = new();

    public RivalCultState? GetRival(string id) =>
        Rivals.FirstOrDefault(r => r.Id == id);

    public RivalBattleState? GetRivalBattle(string rivalId) =>
        RivalBattles.FirstOrDefault(b => b.RivalId == rivalId);
}
