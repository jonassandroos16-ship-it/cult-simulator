using System.Text.Json;
using System.Collections.Concurrent;

namespace CultSimulator.Game;

public class GameService : IDisposable
{
    private GameState _state = new();
    private DateTime _lastTick = DateTime.UtcNow;
    private DateTime _lastSave = DateTime.UtcNow;
    private Timer? _tickTimer;
    private Timer? _saveTimer;
    public event Action? OnChange;
    public bool IsFirstRun => _state.IsFirstRun;
    public bool NeedsStory => _state.NeedsStory;
    public GameState State => _state;

    public CovenState ActiveCoven => _state.ActiveCoven;
    public OccultState Occult => _state.Occult;

    public async Task InitAsync()
    {
        _state = await SaveLoad.Load();
        _lastTick = DateTime.UtcNow;
    }

    public void StartTimers()
    {
        _tickTimer?.Dispose();
        _saveTimer?.Dispose();
        _tickTimer = new Timer(_ => Tick(), null, 0, 100);
        _saveTimer = new Timer(_ => _ = SaveAsync(), null, 30000, 30000);
    }

    private void Tick()
    {
        var now = DateTime.UtcNow;
        var delta = (now - _lastTick).TotalSeconds;
        _lastTick = now;
        if (delta > 0 && delta < 60)
        {
            OccultEngine.Tick(_state, delta);
            OnChange?.Invoke();
        }
    }

    public void Preach()
    {
        OccultEngine.Tap(_state);
        NotifyChanged();
    }

    public void Recruit()
    {
        if (GameEngine.RecruitFollower(_state)) NotifyChanged();
    }

    public void HireAcolyte()
    {
        if (OccultEngine.HireAcolyte(_state)) NotifyChanged();
    }

    public void BuySermonPower()
    {
        if (OccultEngine.BuySermonPower(_state)) NotifyChanged();
    }

    public void PromoteAcolyte()
    {
        if (GameEngine.PromoteAcolyte(_state)) NotifyChanged();
    }

    public void PromoteToCouncil(string minionId, CouncilRole role)
    {
        if (GameEngine.PromoteToCouncil(_state, minionId, role)) NotifyChanged();
    }

    public void RemoveFromCouncil(string minionId)
    {
        if (GameEngine.RemoveFromCouncil(_state, minionId)) NotifyChanged();
    }

    public void SacrificeMinion(string minionId)
    {
        if (GameEngine.SacrificeMinion(_state, minionId)) NotifyChanged();
    }

    public void UnlockTech(TechId id)
    {
        if (TechTree.Unlock(_state, id)) NotifyChanged();
    }

    public void SocketArtifact(string artifactId)
    {
        if (Grimoire.Socket(_state.Occult, artifactId)) NotifyChanged();
    }

    public void UnsocketArtifact(string artifactId)
    {
        if (Grimoire.Unsocket(_state.Occult, artifactId)) NotifyChanged();
    }

    public void CraftRecipe(CauldronRecipeId id)
    {
        Cauldron.Craft(_state.Occult, id);
        NotifyChanged();
    }

    public void ConquerNode(string nodeId)
    {
        var def = OccultData.MapNode(nodeId);
        if (def != null) WorldMapSystem.Conquer(_state, def);
        NotifyChanged();
    }

    public void SetNodeStance(string nodeId, NodeStance stance) { WorldMapSystem.SetStance(_state.Occult, nodeId, stance); NotifyChanged(); }
    public bool ConnectLeyLine(string nodeA, string nodeB) { var ok = WorldMapSystem.ConnectLeyLine(_state.Occult, nodeA, nodeB); NotifyChanged(); return ok; }

    public void ActivateFrenzy()
    {
        if (OccultEngine.ActivateFrenzy(_state.Occult)) NotifyChanged();
    }

    public void ActivateMassHysteria()
    {
        if (OccultEngine.ActivateMassHysteria(_state.Occult)) NotifyChanged();
    }

    public void GrandSacrifice()
    {
        GrandSacrifice.Perform(_state);
        NotifyChanged();
    }

    public void SetName(string name)
    {
        _state.CovenName = name;
        _state.IsFirstRun = false;
        _state.NeedsStory = true;
        NotifyChanged();
        _ = SaveAsync();
    }

    public void DismissStory()
    {
        _state.NeedsStory = false;
        NotifyChanged();
        _ = SaveAsync();
    }

    public void SwitchCoven(string covenId)
    {
        _state.SwitchCoven(covenId);
        NotifyChanged();
        _ = SaveAsync();
    }

    public void PurchaseCoven(string covenId)
    {
        _state.PurchaseCoven(covenId);
        NotifyChanged();
        _ = SaveAsync();
    }

    public void NotifyChanged() => OnChange?.Invoke();

    public async Task SaveAsync()
    {
        _lastSave = DateTime.UtcNow;
        await SaveLoad.Save(_state);
    }

    public async Task ResetAsync()
    {
        _state = new GameState { IsFirstRun = true };
        await SaveLoad.Save(_state);
        NotifyChanged();
    }

    public void Dispose()
    {
        _tickTimer?.Dispose();
        _saveTimer?.Dispose();
    }
}
