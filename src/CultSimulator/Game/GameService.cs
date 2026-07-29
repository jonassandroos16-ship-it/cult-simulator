using Microsoft.JSInterop;

namespace CultSimulator.Game;

public class GameService
{
    private readonly IJSRuntime _js;
    private readonly WorldLocationService _locations;
    private GameState _state;
    private Timer? _tickTimer, _eventTimer, _occultTimer, _saveDebounceTimer, _periodicSaveTimer;
    private bool _eventPending;
    private DateTime _lastOccultTick;
    private DateTime _lastSave = DateTime.UtcNow;

    public GameState State => _state;
    public WorldLocationService Locations => _locations;
    public bool IsFirstRun => string.IsNullOrWhiteSpace(_state.CultName);
    public bool NeedsStory => !IsFirstRun && !_state.StoryShown;
    public EventDef? ActiveEvent { get; private set; }
    public bool EventPending => _eventPending;
    public string? TakeoverCovenName { get; private set; }
    public bool TakeoverPending => TakeoverCovenName != null;
    public string? PopupMessage { get; private set; }
    public string? PopupTitle { get; private set; }
    public bool PopupPending => PopupMessage != null;
    public double OfflineFaith { get; private set; }
    public double OfflineGold { get; private set; }
    public double OfflineSeconds { get; private set; }
    public bool HasOfflineReport => OfflineFaith > 0 || OfflineGold > 0;
    public event Action? OnChange;

    public GameService(IJSRuntime js, WorldLocationService locations) { _js = js; _locations = locations; _state = GameEngine.InitialState(); }

    public async Task InitAsync()
    {
        await _locations.LoadAsync();
        try { var json = await _js.InvokeAsync<string>("localStorage.getItem", GameBalance.SaveKey); _state = SaveLoad.LoadGame(json); }
        catch { _state = GameEngine.InitialState(); }
        EnsureHomeCoven();
        ApplyOfflineIncome();
        NotifyChanged();
    }

    private void EnsureHomeCoven() { if (_state.Covens.Count == 0) { _state.Covens.Add(new CovenState { Id = "skanor", TakenOver = true }); _state.ActiveCovenId = "skanor"; } }

    private void ApplyOfflineIncome()
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var elapsed = now - _state.LastSavedAt;
        if (elapsed <= 0) { _state.LastSavedAt = now; return; }
        var (faith, gold) = GameEngine.ApplyOfflineIncome(_state, elapsed);
        OfflineFaith = faith; OfflineGold = gold; OfflineSeconds = elapsed / 1000.0;
        _state.LastSavedAt = now;
    }

    public void DismissOfflineReport() { OfflineFaith = 0; OfflineGold = 0; OfflineSeconds = 0; NotifyChanged(); }

    public void StartTimers()
    {
        _tickTimer?.Dispose(); _eventTimer?.Dispose(); _occultTimer?.Dispose(); _saveDebounceTimer?.Dispose(); _periodicSaveTimer?.Dispose();
        _tickTimer = new Timer(_ => Tick(), null, 1000, 1000);
        _eventTimer = new Timer(_ => TryEvent(), null, GameBalance.EventIntervalSeconds * 1000, GameBalance.EventIntervalSeconds * 1000);
        _lastOccultTick = DateTime.UtcNow;
        _occultTimer = new Timer(_ => OccultTick(), null, 100, 100);
        _periodicSaveTimer = new Timer(async _ => await SaveAsync(), null, 5000, 5000);
    }

    private void OccultTick() { var now = DateTime.UtcNow; var delta = (now - _lastOccultTick).TotalSeconds; _lastOccultTick = now; OccultEngine.Tick(_state, delta); NotifyChanged(); }
    private void Tick() { GameEngine.TickAllCovens(_state); NotifyChanged(); }

    private void TryEvent()
    {
        if (_eventPending || ActiveEvent != null) return;
        if (_state.ActiveCoven.Followers < GameBalance.EventMinFollowers) return;
        if (Random.Shared.NextDouble() > GameBalance.EventTriggerChance) return;
        ActiveEvent = GameData.Events[Random.Shared.Next(GameData.Events.Length)];
        _eventPending = true; NotifyChanged();
    }

    public double Preach() { var gained = GameEngine.Preach(_state.ActiveCoven); NotifyChanged(); return gained; }
    public void Recruit() { GameEngine.Recruit(_state.ActiveCoven); NotifyChanged(); }
    public void BuyBuilding(BuildingType type) { GameEngine.BuyBuilding(_state.ActiveCoven, type); NotifyChanged(); }
    public void BuyBank() { GameEngine.BuyBank(_state.ActiveCoven); NotifyChanged(); }
    public void BuyUpgrade(UpgradeId id) { GameEngine.BuyUpgrade(_state.ActiveCoven, id); NotifyChanged(); }

    public double OccultTap() { var gained = OccultEngine.Tap(_state); NotifyChanged(); return gained; }
    public void BuySermonPower() { OccultEngine.BuySermonPower(_state); NotifyChanged(); }
    public void HireAcolyte() { OccultEngine.HireAcolyte(_state); NotifyChanged(); }
    public void PromoteMinion() { CultistHierarchy.Promote(_state.Occult); NotifyChanged(); }
    public void SacrificeMinion(string minionId) { CultistHierarchy.Sacrifice(_state, minionId); NotifyChanged(); }
    public void AppointCouncil(CouncilRole role, string minionId) { CultistHierarchy.AppointCouncil(_state.Occult, role, minionId); NotifyChanged(); }
    public void RemoveCouncil(CouncilRole role) { CultistHierarchy.RemoveCouncil(_state.Occult, role); NotifyChanged(); }
    public void UnlockTech(TechId id) { TechTree.Unlock(_state, id); NotifyChanged(); }
    public void SocketArtifact(string artifactId) { Grimoire.Socket(_state.Occult, artifactId); NotifyChanged(); }
    public void UnsocketArtifact(string artifactId) { Grimoire.Unsocket(_state.Occult, artifactId); NotifyChanged(); }
    public void ConquerNode(string nodeId) { var def = OccultData.MapNode(nodeId); if (def != null) WorldMapSystem.Conquer(_state, def); NotifyChanged(); }
    public void SetNodeStance(string nodeId, NodeStance stance) { WorldMapSystem.SetStance(_state.Occult, nodeId, stance); NotifyChanged(); }
    public void CraftRecipe(CauldronRecipeId id) { Cauldron.Craft(_state.Occult, id); NotifyChanged(); }
    public void ActivateFrenzy() { OccultEngine.ActivateFrenzy(_state.Occult); NotifyChanged(); }
    public void ActivateMassHysteria() { OccultEngine.ActivateMassHysteria(_state.Occult); NotifyChanged(); }
    public double PerformGrandSacrifice() { var favor = GrandSacrifice.PerformSacrifice(_state); NotifyChanged(); return favor; }

    public void ChooseEvent(EventChoice choice)
    {
        var outcome = choice.Apply(_state.ActiveCoven);
        Clamp(_state.ActiveCoven); ActiveEvent = null; _eventPending = false;
        if (!string.IsNullOrWhiteSpace(outcome)) { PopupTitle = "Outcome"; PopupMessage = outcome; }
        NotifyChanged();
    }

    public void DismissPopup() { PopupMessage = null; PopupTitle = null; NotifyChanged(); }
    private static void Clamp(CovenState c) { if (c.Faith < 0) c.Faith = 0; if (c.Gold < 0) c.Gold = 0; if (c.Followers < 0) c.Followers = 0; }
    public void ConfirmName(string name) { _state.CultName = name.Trim(); NotifyChanged(); }
    public void MarkStoryShown() { _state.StoryShown = true; NotifyChanged(); }
    public bool CanTakeover(string covenId) { var loc = _locations.Find(covenId); return loc != null && CovenProgress.CanTakeover(_state, loc); }
    public void TakeoverCoven(string covenId) { var loc = _locations.Find(covenId); if (loc == null || !CovenProgress.CanTakeover(_state, loc)) return; CovenProgress.Takeover(_state, loc); TakeoverCovenName = loc.Name; NotifyChanged(); }
    public void DismissTakeover() { TakeoverCovenName = null; NotifyChanged(); }
    public void SwitchActiveCoven(string covenId) { CovenProgress.SwitchActive(_state, covenId); NotifyChanged(); }

    public async Task ResetAsync() { _state = GameEngine.InitialState(); ActiveEvent = null; _eventPending = false; TakeoverCovenName = null; PopupMessage = null; PopupTitle = null; OfflineFaith = 0; OfflineGold = 0; OfflineSeconds = 0; await SaveAsync(); NotifyChanged(); }

    private void NotifyChanged()
    {
        OnChange?.Invoke();
        var now = DateTime.UtcNow;
        if (now - _lastSave < TimeSpan.FromSeconds(3)) return;
        _lastSave = now;
        _ = SaveAsync();
    }
    public async Task SaveAsync() { try { _state.LastSavedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(); var json = SaveLoad.SaveGame(_state); await _js.InvokeVoidAsync("localStorage.setItem", GameBalance.SaveKey, json); _lastSave = DateTime.UtcNow; } catch { } }
}
