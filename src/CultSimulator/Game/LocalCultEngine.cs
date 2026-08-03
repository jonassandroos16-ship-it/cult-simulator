using System.Linq;

namespace CultSimulator.Game;

/// <summary>
/// Local cults are always present on the map (3 per coven). After being
/// defeated or converted they enter a 1-hour recharge before the reward
/// refills. Killing has a chance to drop an artifact, converting always
/// drops one, and boss kills drop 3. When all 3 cults for the active coven
/// are on cooldown, a faith-generation buff applies.
/// </summary>
public static class LocalCultEngine
{
    public const int MaxActiveLocalCults = 3;

    public static IReadOnlyList<LocalCultInstance> ActiveForCoven(GameState state, string covenId) =>
        state.ActiveLocalCults.Where(i => LocalCultData.Find(i.CultId)?.ParentCovenId == covenId).ToList();

    public static bool AllOnCooldown(GameState state, string covenId)
    {
        var instances = ActiveForCoven(state, covenId);
        if (instances.Count == 0) return false;
        return instances.All(i => !i.IsCharged);
    }

    public static long EarliestReadyMs(GameState state, string covenId)
    {
        var instances = ActiveForCoven(state, covenId);
        if (instances.Count == 0) return 0;
        return instances.Where(i => !i.IsCharged).Select(i => i.ReadyAtMs).DefaultIfEmpty(0).Min();
    }

    public static void EnsureCultsForCoven(GameState state, string covenId)
    {
        var pool = LocalCultData.ForCoven(covenId);
        var existing = ActiveForCoven(state, covenId);
        foreach (var def in pool)
        {
            if (!existing.Any(i => i.CultId == def.Id))
                state.ActiveLocalCults.Add(new LocalCultInstance { CultId = def.Id, SpawnedAt = DateTime.UtcNow });
        }
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
        if (!instance.IsCharged) return false;

        var home = state.HomeCoven;
        double amount = def.RewardAmount;
        if (reward == LocalCultReward.Followers) home.Followers += (int)amount;
        else home.Gold += amount;

        DropArtifact(state, 1);
        instance.LastDefeatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
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

    public static void OnVictory(GameState state, string cultId)
    {
        var def = LocalCultData.Find(cultId);
        if (def == null) return;
        var instance = state.ActiveLocalCults.FirstOrDefault(i => i.CultId == cultId);
        if (instance == null) return;

        int dropCount = def.IsBoss ? 3 : (Random.Shared.NextDouble() < GameBalance.LocalCultKillArtifactChance ? 1 : 0);
        if (dropCount > 0) DropArtifact(state, dropCount);

        instance.LastDefeatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    public static void DropArtifact(GameState state, int count)
    {
        var artifactIds = OccultData.Artifacts.Select(a => a.Id).ToList();
        if (artifactIds.Count == 0) return;
        for (int i = 0; i < count; i++)
        {
            var pick = artifactIds[Random.Shared.Next(artifactIds.Count)];
            if (!state.Occult.OwnedArtifacts.Contains(pick))
                state.Occult.OwnedArtifacts.Add(pick);
        }
    }
}
