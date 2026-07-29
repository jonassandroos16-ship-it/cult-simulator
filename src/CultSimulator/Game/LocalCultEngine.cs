namespace CultSimulator.Game;

/// <summary>
/// Pure functions for spawning, converting, and rewarding local cults.
/// Local cults are easier, single-step conversions that appear periodically
/// on the local map for quick rewards.
/// </summary>
public static class LocalCultEngine
{
    public const int MaxActiveLocalCults = 3;

    /// <summary>Local cults currently spawned for the player's active coven.</summary>
    public static IReadOnlyList<LocalCultInstance> ActiveForCoven(GameState state, string covenId) =>
        state.ActiveLocalCults.Where(i => LocalCultData.Find(i.CultId)?.ParentCovenId == covenId).ToList();

    /// <summary>True if a new local cult can spawn (under the cap and pool available).</summary>
    public static bool CanSpawn(GameState state, string covenId)
    {
        var active = ActiveForCoven(state, covenId);
        if (active.Count >= MaxActiveLocalCults) return false;
        var pool = LocalCultData.ForCoven(covenId);
        var available = pool.Where(d => !active.Any(a => a.CultId == d.Id)).ToList();
        return available.Count > 0;
    }

    /// <summary>Spawns a random local cult for the given coven, if under the cap.</summary>
    public static void SpawnOne(GameState state, string covenId)
    {
        if (!CanSpawn(state, covenId)) return;
        var active = ActiveForCoven(state, covenId);
        var pool = LocalCultData.ForCoven(covenId);
        var available = pool.Where(d => !active.Any(a => a.CultId == d.Id)).ToList();
        if (available.Count == 0) return;
        var pick = available[Random.Shared.Next(available.Count)];
        state.ActiveLocalCults.Add(new LocalCultInstance { CultId = pick.Id, SpawnedAt = DateTime.UtcNow });
    }

    /// <summary>True if the player has enough followers to convert this local cult.</summary>
    public static bool CanConvert(GameState state, LocalCultDef def)
    {
        return CovenProgress.TotalFollowers(state) >= def.FollowersRequired;
    }

    /// <summary>
    /// Converts a local cult: checks the follower requirement, applies the
    /// chosen reward, and removes the cult from the active list. Does NOT
    /// use the multi-step narrative siege — local cults are quick wins.
    /// Returns true on success.
    /// </summary>
    public static bool Convert(GameState state, string cultId, LocalCultReward reward)
    {
        var def = LocalCultData.Find(cultId);
        if (def == null) return false;
        if (!CanConvert(state, def)) return false;

        var instance = state.ActiveLocalCults.FirstOrDefault(i => i.CultId == cultId);
        if (instance == null) return false;

        var home = state.HomeCoven;
        if (reward == LocalCultReward.Followers)
            home.Followers += def.RewardAmount;
        else
            home.Gold += def.RewardAmount;

        state.ActiveLocalCults.Remove(instance);
        return true;
    }

    /// <summary>Removes a local cult without converting (e.g. it expires).</summary>
    public static void Expire(GameState state, string cultId)
    {
        var instance = state.ActiveLocalCults.FirstOrDefault(i => i.CultId == cultId);
        if (instance != null) state.ActiveLocalCults.Remove(instance);
    }
}
