namespace CultSimulator.Game;

/// <summary>
/// Pure functions for spawning, converting, and rewarding local cults.
/// Local cults are easier, single-step conversions that appear periodically
/// on the local map for quick rewards.
/// </summary>
public static class LocalCultEngine
{
    public const int MaxActiveLocalCults = 3;

    public static IReadOnlyList<LocalCultInstance> ActiveForCoven(GameState state, string covenId) =>
        state.ActiveLocalCults.Where(i => LocalCultData.Find(i.CultId)?.ParentCovenId == covenId).ToList();

    public static bool CanSpawn(GameState state, string covenId)
    {
        var active = ActiveForCoven(state, covenId);
        if (active.Count >= MaxActiveLocalCults) return false;
        var pool = LocalCultData.ForCoven(covenId);
        var available = pool.Where(d => !active.Any(a => a.CultId == d.Id)).ToList();
        return available.Count > 0;
    }

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

    public static bool CanConvert(GameState state, LocalCultDef def)
    {
        return CovenProgress.TotalFollowers(state) >= def.FollowersRequired;
    }

    public static bool Convert(GameState state, string cultId, LocalCultReward reward)
    {
        var def = LocalCultData.Find(cultId);
        if (def == null) return false;
        if (!CanConvert(state, def)) return false;
        var instance = state.ActiveLocalCults.FirstOrDefault(i => i.CultId == cultId);
        if (instance == null) return false;
        var home = state.HomeCoven;
        if (reward == LocalCultReward.Followers) home.Followers += def.RewardAmount;
        else home.Gold += def.RewardAmount;
        state.ActiveLocalCults.Remove(instance);
        return true;
    }

    public static bool CanStartBattle(GameState state, LocalCultDef def)
    {
        return CovenProgress.TotalFollowers(state) >= def.FollowersRequired;
    }

    public static LocalCultBattleState StartBattle(GameState state, string cultId)
    {
        var def = LocalCultData.Find(cultId);
        if (def == null) throw new ArgumentException($"Local cult {cultId} not found.");
        return LocalCultBattleEngine.GetOrCreateBattle(state, def);
    }

    public static void Expire(GameState state, string cultId)
    {
        var instance = state.ActiveLocalCults.FirstOrDefault(i => i.CultId == cultId);
        if (instance != null) state.ActiveLocalCults.Remove(instance);
    }
}
