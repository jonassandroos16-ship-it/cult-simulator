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
    double Defense = 50.0,
    bool IsBoss = false);

public class LocalCultInstance
{
    public string CultId { get; set; } = "";
    public DateTime SpawnedAt { get; set; } = DateTime.UtcNow;
    public long LastDefeatedAt { get; set; }
    public bool IsCharged => ChargeFraction >= 1.0;
    public double ChargeFraction
    {
        get
        {
            if (LastDefeatedAt == 0) return 1.0;
            var elapsed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - LastDefeatedAt;
            return Math.Clamp((double)elapsed / (GameBalance.LocalCultRechargeMs), 0.0, 1.0);
        }
    }
    public long ReadyAtMs => LastDefeatedAt == 0 ? 0 : LastDefeatedAt + GameBalance.LocalCultRechargeMs;
}
