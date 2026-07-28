using Microsoft.JSInterop;

namespace CultSimulator.Game;

public class GameService
{
    private readonly IJSRuntime _js;
    private readonly WorldLocationService _locations;
    private GameState _state;
    private Timer? _tickTimer;
    private Timer? _eventTimer;
    private bool _eventPending;

    public GameState State => _state;
    public WorldLocationService Locations => _locations;
    public bool IsFirstRun => string.IsNullOrWhiteSpace(_state.CultName);
    public bool NeedsStory => !IsFirstRun && !_state.StoryShown;
    public EventDef? ActiveEvent { get; private set; }
    public bool EventPending => _eventPending;

    public string? TakeoverCovenName { get; private set; }
    public bool TakeoverPending => TakeoverCovenName != null;

    public event Action? OnChange;

    public GameService(IJSRuntime js, WorldLocationService locations)
    {
        _js = js;
        _locations = locations;
        _state = GameEngine.InitialState();
    }

    public async Task InitAsync()
    {
        await _locations.LoadAsync();
        try
        {
            var json = await _js.InvokeAsync<string>("localStorage.getItem", GameBalance.SaveKey);
            _state = SaveLoad.LoadGame(json);
        }
        catch { _state = GameEngine.InitialState(); }
        EnsureHomeCoven();
        NotifyChanged();
    }

    private void EnsureHomeCoven()
    {
        if (_state.Covens.Count == 0)
        {
            _state.Covens.Add(new CovenState { Id = "skanor", TakenOver = true });
            _state.ActiveCovenId = "skanor";
        }
    }

    public void StartTimers()
    {
        _tickTimer?.Dispose();
        _eventTimer?.Dispose();
        _tickTimer = new Timer(_ => Tick(), null, 1000, 1000);
        _eventTimer = new Timer(_ => TryEvent(), null, GameBalance.EventIntervalSeconds * 1000, GameBalance.EventIntervalSeconds * 1000);
    }

    private void Tick()
    {
        GameEngine.TickAllCovens(_state);
        NotifyChanged();
    }

    private void TryEvent()
    {
        if (_eventPending || ActiveEvent != null) return;
        if (_state.ActiveCoven.Followers < GameBalance.EventMinFollowers) return;
        if (Random.Shared.NextDouble() > GameBalance.EventTriggerChance) return;
        var ev = GameData.Events[Random.Shared.Next(GameData.Events.Length)];
        ActiveEvent = ev;
        _eventPending = true;
        NotifyChanged();
    }

    public double Preach() { var gained = GameEngine.Preach(_state.ActiveCoven); NotifyChanged(); return gained; }
    public void Recruit() { GameEngine.Recruit(_state.ActiveCoven); NotifyChanged(); }
    public void BuyBuilding(BuildingType type) { GameEngine.BuyBuilding(_state.ActiveCoven, type); NotifyChanged(); }
    public void BuyUpgrade(UpgradeId id) { GameEngine.BuyUpgrade(_state.ActiveCoven, id); NotifyChanged(); }

    public void ChooseEvent(EventChoice choice)
    {
        choice.Apply(_state.ActiveCoven);
        Clamp(_state.ActiveCoven);
        ActiveEvent = null;
        _eventPending = false;
        NotifyChanged();
    }

    private static void Clamp(CovenState c)
    {
        if (c.Faith < 0) c.Faith = 0;
        if (c.Gold < 0) c.Gold = 0;
        if (c.Followers < 0) c.Followers = 0;
    }

    public void ConfirmName(string name) { _state.CultName = name.Trim(); NotifyChanged(); }

    public void MarkStoryShown() { _state.StoryShown = true; NotifyChanged(); }

    public bool CanTakeover(string covenId)
    {
        var loc = _locations.Find(covenId);
        return loc != null && CovenProgress.CanTakeover(_state, loc);
    }

    public void TakeoverCoven(string covenId)
    {
        var loc = _locations.Find(covenId);
        if (loc == null || !CovenProgress.CanTakeover(_state, loc)) return;
        CovenProgress.Takeover(_state, loc);
        TakeoverCovenName = loc.Name;
        NotifyChanged();
    }

    public void DismissTakeover()
    {
        TakeoverCovenName = null;
        NotifyChanged();
    }

    public void SwitchActiveCoven(string covenId)
    {
        CovenProgress.SwitchActive(_state, covenId);
        NotifyChanged();
    }

    public async Task ResetAsync()
    {
        _state = GameEngine.InitialState();
        ActiveEvent = null;
        _eventPending = false;
        TakeoverCovenName = null;
        await SaveAsync();
        NotifyChanged();
    }

    private Timer? _saveTimer;
    private void NotifyChanged()
    {
        _saveTimer?.Dispose();
        _saveTimer = new Timer(async _ => await SaveAsync(), null, 500, Timeout.Infinite);
        OnChange?.Invoke();
    }

    public async Task SaveAsync()
    {
        try
        {
            var json = SaveLoad.SaveGame(_state);
            await _js.InvokeVoidAsync("localStorage.setItem", GameBalance.SaveKey, json);
        }
        catch { }
    }
}
