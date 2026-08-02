using Microsoft.JSInterop;
using System.Text.Json;

namespace CultSimulator.Game;

public class GameService
{
    private readonly IJSRuntime _js;
    private readonly WorldLocationService _locations;
    private readonly ConversionDataService _conversions;
    private readonly CloudSaveService _cloud;
    private GameState _state;
    private Timer? _tickTimer, _eventTimer, _occultTimer, _periodicSaveTimer, _localCultTimer;
    private bool _eventPending;
    private DateTime _lastOccultTick;
    private DateTime _lastSave = DateTime.UtcNow;
    private DateTime _lastCloudSave = DateTime.UtcNow;
    private readonly SemaphoreSlim _saveLock = new(1, 1);
    private bool _isCloudSaving;
    public CloudSaveService Cloud => _cloud;

    public GameState State => _state;
    public WorldLocationService Locations => _locations;
    public bool IsFirstRun => string.IsNullOrWhiteSpace(_state.CultName);
    public bool NeedsStory => !IsFirstRun && !_state.StoryShown;
    public bool LoadSucceeded { get; private set; }
    public EventDef? ActiveEvent { get; private set; }
    public bool EventPending => _eventPending;
    public string? ConvertedCovenName { get; private set; }
    public bool ConversionCompletePending => ConvertedCovenName != null;

    public FootholdDef? PendingFoothold { get; private set; }
    public bool ContinentStoryPending => PendingFoothold != null;
    public string? PopupMessage { get; private set; }
    public string? PopupTitle { get; private set; }
    public bool PopupPending => PopupMessage != null;
    public double OfflineFaith { get; private set; }
    public double OfflineGold { get; private set; }
    public double OfflineSeconds { get; private set; }
    public double OfflineLostFaith { get; private set; }
    public double OfflineLostGold { get; private set; }
    public bool OfflinePopupPending { get; private set; }
    public bool HasOfflineReport => OfflineFaith > 0 || OfflineGold > 0;
    public