namespace CultSimulator.Game;

public enum LocalCultReward { Followers, Gold }

public record LocalCultDef(
    string Id,
    string ParentCovenId,
    string Name,
    string Description,
    double Latitude,
    double Longitude,
    int FollowersRequired,
    int RewardAmount,
    double Defense = 50.0);

public class LocalCultInstance
{
    public string CultId { get; set; } = "";
    public DateTime SpawnedAt { get; set; } = DateTime.UtcNow;
}
