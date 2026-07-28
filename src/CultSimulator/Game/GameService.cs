using Microsoft.JSInterop;

namespace CultSimulator.Game;

public class GameService
{
    private readonly IJSRuntime _js;
    private GameState _state;
    private Timer? _tickTimer;
    private Timer? _eventTimer;
    private bool _eventPending;

    public GameState State => _state;
    public bool IsFirstRun => string.IsNullOrWhiteSpace(_state.CultName);
    public EventDef? ActiveEvent { get; private set; }
    public bool EventPending => _eventPending;

    public event Action? OnChange;

    public GameService(IJSRuntime js)
    {
        _js = js;
        _state = GameEngine.InitialState();
    }

    public async Task InitAsync()
    {
        try
        {
            var json = await _js.InvokeAsync<string>("localStorage.getItem", GameBalance.SaveKey);
            _state = SaveLoad.LoadGame(json);
        }
        catch { _state = GameEngine.InitialState(); }
        NotifyChanged();
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
        var (faith, gold) = GameEngine.TickIncome(_state);
        _state.Faith += faith;
        _state.Gold += gold;
        NotifyChanged();
    }

    private void TryEvent()
    {
        if (_eventPending || ActiveEvent != null) return;
        if (_state.Followers < GameBalance.EventMinFollowers) return;
        if (Random.Shared.NextDouble() > GameBalance.EventTriggerChance) return;
        var ev = GameData.Events[Random.Shared.Next(GameData.Events.Length)];
        ActiveEvent = ev;
        _eventPending = true;
        NotifyChanged();
    }

    public double Preach() { var gained = GameEngine.Preach(_state); NotifyChanged(); return gained; }
    public void Recruit() { GameEngine.Recruit(_state); NotifyChanged(); }
    public void BuyBuilding(BuildingType type) { GameEngine.BuyBuilding(_state, type); NotifyChanged(); }
    public void BuyUpgrade(UpgradeId id) { GameEngine.BuyUpgrade(_state, id); NotifyChanged(); }

    public void ChooseEvent(EventChoice choice)
    {
        choice.Apply(_state);
        if (_state.Faith < 0) _state.Faith = 0;
        if (_state.Gold < 0) _state.Gold = 0;
        if (_state.Followers < 0) _state.Followers = 0;
        ActiveEvent = null;
        _eventPending = false;
        NotifyChanged();
    }

    public void ConfirmName(string name) { _state.CultName = name.Trim(); NotifyChanged(); }

    public async Task ResetAsync()
    {
        _state = GameEngine.InitialState();
        ActiveEvent = null;
        _eventPending = false;
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
