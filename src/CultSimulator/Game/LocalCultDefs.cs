namespace CultSimulator.Game;

/// <summary>
/// The reward the player picks after converting a local cult.
/// </summary>
public enum LocalCultReward { Followers, Gold }

/// <summary>
/// A local rival cult that spawns periodically on the local map.
/// Easier to convert than full covens — single-step, lower follower cost.
/// Each is tied to a parent coven (e.g. Skanör has Falsterbo, Malmö, Lund).
/// </summary>
public record LocalCultDef(
    string Id,
    string ParentCovenId,
    string Name,
    string Description,
    double Latitude,
    double Longitude,
    int FollowersRequired,
    int RewardAmount);

/// <summary>
/// A live instance of a local cult currently spawned on the map.
/// Persisted so spawned cults survive reloads.
/// </summary>
public class LocalCultInstance
{
    public string CultId { get; set; } = "";
    public DateTime SpawnedAt { get; set; } = DateTime.UtcNow;
}
