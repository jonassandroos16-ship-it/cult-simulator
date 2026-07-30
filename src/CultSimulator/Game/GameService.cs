using Microsoft.JSInterop;

namespace CultSimulator.Game;

public class GameService
{
    private readonly IJSRuntime _js;
    private readonly WorldLocationService _locations;
    private GameState _state;
    private Timer? _tickTimer, _eventTimer, _occultTimer, _saveDebounceTimer, _periodicSaveTimer, _localCultTimer;
    private bool _eventPending;
    private DateTime _lastOccultTick;
    private DateTime _lastSave = DateTime.UtcNow;

    public GameState State => _state;
    public WorldLocationService Locations => _locations;
    public bool IsFirstRun => string.IsNullOrWhiteSpace(_state.CultName);
    public bool NeedsStory => !IsFirstRun && !_state.StoryShown;
    public EventDef? ActiveEvent { get; private set; }
    public bool EventPending => _eventPending;
    public string? ConvertedCovenName { get; private set; }
    public bool ConversionCompletePending => ConvertedCovenName != null;
    public string? PopupMessage { get; private set; }
    public string? PopupTitle { get; private set; }
    public bool PopupPending => PopupMessage != null;
    public double OfflineFaith { get; private set; }
    public double OfflineGold { get; private set; }
    public double OfflineSeconds { get; private set; }
    public bool HasOfflineReport => OfflineFaith > 0 || OfflineGold > 0;
    public event Action? OnChange;

    public string? PendingLocalCultId { get; private set; }
    public bool LocalCultRewardPending => PendingLocalCultId != null;

    public string? SpawnedLocalCultId { get; private set; }
    public bool LocalCultSpawnPending => SpawnedLocalCultId != null;
    public LocalCultDef? SpawnedLocalCultDef =>
        SpawnedLocalCultId == null ? null : LocalCultData.Find(SpawnedLocalCultId);

    public GameService(IJSRuntime js, WorldLocationService locations) { _js = js; _locations = locations; _state = GameEngine.InitialState(); }

    public async Task InitAsync()
    {
        await _locations.LoadAsync();
        try
        {
            var primary = await _js.InvokeAsync<string>("localStorage.getItem", GameBalance.SaveKey);
            var backup = await _js.InvokeAsync<string>("localStorage.getItem", GameBalance.BackupSaveKey);
            var (loaded, success) = SaveLoad.LoadGameWithBackup(primary, backup);
            _state = loaded;
        }
        catch { _state = GameEngine.InitialState(); }
        EnsureHomeCoven();
        ApplyOfflineIncome();
        NotifyChanged();
    }

    private void EnsureHomeCoven() { if (_state.Covens.Count == 0) { _state.Covens.Add(new CovenState { Id = "skanor", Converted = true }); _state.ActiveCovenId = "skanor"; } }

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
        _tickTimer?.Dispose(); _eventTimer?.Dispose(); _occultTimer?.Dispose(); _saveDebounceTimer?.Dispose(); _periodicSaveTimer?.Dispose(); _localCultTimer?.Dispose();
        _tickTimer = new Timer(_ => Tick(), null, 1000, 1000);
        _eventTimer = new Timer(_ => TryEvent(), null, GameBalance.EventIntervalSeconds * 1000, GameBalance.EventIntervalSeconds * 1000);
        _lastOccultTick = DateTime.UtcNow;
        _occultTimer = new Timer(_ => OccultTick(), null, 100, 100);
        _periodicSaveTimer = new Timer(async _ => await SaveAsync(), null, 5000, 5000);
        _localCultTimer = new Timer(_ => TrySpawnLocalCult(), null, GameBalance.LocalCultSpawnIntervalSeconds * 1000, GameBalance.LocalCultSpawnIntervalSeconds * 1000);
    }

    private void OccultTick() { var now = DateTime.UtcNow; var delta = (now - _lastOccultTick).TotalSeconds; _lastOccultTick = now; OccultEngine.Tick(_state, delta); NotifyChanged(); }
    private void Tick() { GameEngine.TickAllCovens(_state, _locations); NotifyChanged(); }

    private void TryEvent()
    {
        if (_eventPending || ActiveEvent != null) return;
        if (ConversionEngine.IsActive(_state)) return;
        if (_state.ActiveCoven.Followers < GameBalance.EventMinFollowers) return;
        if (Random.Shared.NextDouble() > GameBalance.EventTriggerChance) return;

        var loc = _locations.Find(_state.ActiveCovenId);
        if (loc != null && loc.Events != null && loc.Events.Count > 0)
        {
            var def = loc.Events[Random.Shared.Next(loc.Events.Count)];
            ActiveEvent = def.ToEventDef();
        }
        else
        {
            ActiveEvent = GameData.Events[Random.Shared.Next(GameData.Events.Length)];
        }
        _eventPending = true; NotifyChanged();
    }

    private void TrySpawnLocalCult()
    {
        if (ConversionEngine.IsActive(_state)) return;
        var before = LocalCultEngine.ActiveForCoven(_state, _state.ActiveCovenId).Count;
        LocalCultEngine.SpawnOne(_state, _state.ActiveCovenId);
        var after = LocalCultEngine.ActiveForCoven(_state, _state.ActiveCovenId);
        if (after.Count > before)
        {
            var spawned = after[^1];
            SpawnedLocalCultId = spawned.CultId;
        }
        NotifyChanged();
    }

    public void DismissLocalCultSpawn() { SpawnedLocalCultId = null; NotifyChanged(); }

    public double Preach() { var gained = GameEngine.Preach(_state); NotifyChanged(); return gained; }
    public void Recruit() { GameEngine.Recruit(_state.ActiveCoven); NotifyChanged(); }
    public void BuyBuilding(BuildingType type) { GameEngine.BuyBuilding(_state.ActiveCoven, type); NotifyChanged(); }
    public void BuyBank() { GameEngine.BuyBank(_state.ActiveCoven); NotifyChanged(); }
    public void BuyUpgrade(UpgradeId id) { GameEngine.BuyUpgrade(_state.ActiveCoven, id); NotifyChanged(); }

    public double OccultTap() { var gained = OccultEngine.Tap(_state); NotifyChanged(); return gained; }
    public void BuySermonPower() { OccultEngine.BuySermonPower(_state); NotifyChanged(); }
    public void HireAcolyte() { OccultEngine.HireAcolyte(_state); NotifyChanged(); }
    public void PromoteMinion() { CultistHierarchy.Promote(_state.Occult); NotifyChanged(); }
    public void SacrificeMinion(string minionId) { CultistHierarchy.Sacrifice(_state, minionId); NotifyChanged(); }
    public void AppointCouncil(CouncilRole role, string minionId) { CultistHierarchy.AppointCouncil(_state, role, minionId); NotifyChanged(); }
    public void RemoveCouncil(CouncilRole role) { CultistHierarchy.RemoveCouncil(_state.Occult, role); NotifyChanged(); }
    public void UnlockTech(TechId id) { TechTree.Unlock(_state, id); NotifyChanged(); }
    public void SocketArtifact(string artifactId) { Grimoire.Socket(_state.Occult, artifactId); NotifyChanged(); }
    public void UnsocketArtifact(string artifactId) { Grimoire.Unsocket(_state.Occult, artifactId); NotifyChanged(); }
    public void ConquerNode(string nodeId)
    {
        var def = OccultData.MapNode(nodeId);
        if (def == null) return;
        if (!WorldMapSystem.CanConquer(_state, def))
        {
            var coven = _state.ActiveCoven;
            if (def.CovenId != _state.ActiveCovenId)
                PopupMessage = "This node belongs to a different coven. Switch active coven first.";
            else if (coven.Faith < def.FaithCost)
                PopupMessage = $"Not enough Faith. Need {NumberFormat.Fmt(def.FaithCost)} but have {NumberFormat.Fmt(coven.Faith)}.";
            else if (_state.Occult.ArmyPower < def.ArmyPowerRequired)
                PopupMessage = $"Not enough Army Power. Need {NumberFormat.Fmt(def.ArmyPowerRequired)} but have {NumberFormat.Fmt(_state.Occult.ArmyPower)}.";
            else
                PopupMessage = "Cannot conquer this node right now.";
            PopupTitle = "Cannot Claim Node";
            NotifyChanged();
            return;
        }
        WorldMapSystem.Conquer(_state, def);
        NotifyChanged();
    }
    public void SetNodeStance(string nodeId, NodeStance stance) { WorldMapSystem.SetStance(_state.Occult, nodeId, stance); NotifyChanged(); }
    public bool ConnectLeyLine(string nodeA, string nodeB)
    {
        if (!WorldMapSystem.CanConnectLeyLine(_state.Occult, nodeA, nodeB))
        {
            PopupTitle = "Cannot Connect";
            PopupMessage = "Both nodes must be conquered to connect them.";
            NotifyChanged();
            return false;
        }
        if (_state.Occult.LeyLines.Any(l => l.Contains(nodeA) && l.Contains(nodeB)))
        {
            PopupTitle = "Already Connected";
            PopupMessage = "These two nodes are already linked by a Ley Line.";
            NotifyChanged();
            return false;
        }
        var ok = WorldMapSystem.ConnectLeyLine(_state.Occult, nodeA, nodeB);
        NotifyChanged();
        return ok;
    }
    public void CraftRecipe(CauldronRecipeId id) { Cauldron.Craft(_state.Occult, id); NotifyChanged(); }
    public void ActivateFrenzy() { OccultEngine.ActivateFrenzy(_state.Occult); NotifyChanged(); }
    public void ActivateMassHysteria() { OccultEngine.ActivateMassHysteria(_state.Occult); NotifyChanged(); }
    public void SacrificeAcolyte() { OccultEngine.SacrificeAcolyte(_state); NotifyChanged(); }
    public void ActivateBloodOffering() { OccultEngine.ActivateBloodOffering(_state); NotifyChanged(); }
    public void ActivateDarkVigil() { OccultEngine.ActivateDarkVigil(_state.Occult); NotifyChanged(); }
    public void ActivateWhisperChoir() { OccultEngine.ActivateWhisperChoir(_state.Occult); NotifyChanged(); }
    public void ActivateCovenBlessing() { OccultEngine.ActivateCovenBlessing(_state.Occult); NotifyChanged(); }
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

    public bool CanConvert(string covenId) { var loc = _locations.Find(covenId); return loc != null && ConversionEngine.CanStartConversion(_state, loc); }

    public void StartConversion(string covenId)
    {
        var loc = _locations.Find(covenId);
        if (loc == null) return;
        if (!ConversionEngine.CanStartConversion(_state, loc))
        {
            var needed = loc.FollowersRequired - CovenProgress.TotalFollowers(_state);
            PopupTitle = "Not Ready";
            PopupMessage = $"You need {needed} more followers before you can convert this coven.";
            NotifyChanged();
            return;
        }
        if (ConversionEngine.DefinitionFor(covenId) == null)
        {
            PopupTitle = "No Conversion Available";
            PopupMessage = "This coven cannot be converted through the narrative siege system yet. Try expanding your reach to other covens first.";
            NotifyChanged();
            return;
        }
        ConversionEngine.StartConversion(_state, loc);
        NotifyChanged();
    }

    public string? ApplyConversionChoice(ConversionChoice choice)
    {
        var outcome = ConversionEngine.ApplyChoice(_state, choice);
        if (_state.Conversion != null && _state.Conversion.Completed)
        {
            var loc = _locations.Find(_state.Conversion.CovenId);
            if (loc != null) ConvertedCovenName = loc.Name;
        }
        NotifyChanged();
        return outcome;
    }

    public void CancelConversion() { ConversionEngine.Cancel(_state); NotifyChanged(); }
    public void DismissConversionComplete() { ConversionEngine.ClearCompleted(_state); ConvertedCovenName = null; NotifyChanged(); }
    public bool IsConversionActive => ConversionEngine.IsActive(_state);
    public ConversionStep? CurrentConversionStep => ConversionEngine.CurrentStep(_state);
    public ConversionDef? ActiveConversion => _state.Conversion == null ? null : ConversionData.Find(_state.Conversion.CovenId);
    public double ConversionProgressValue => _state.Conversion?.Progress ?? 0.0;

    public IReadOnlyList<LocalCultInstance> ActiveLocalCultsForCurrentCoven =>
        LocalCultEngine.ActiveForCoven(_state, _state.ActiveCovenId);

    public bool CanConvertLocalCult(string cultId)
    {
        var def = LocalCultData.Find(cultId);
        return def != null && LocalCultEngine.CanConvert(_state, def);
    }

    public void RequestLocalCultConversion(string cultId)
    {
        if (!CanConvertLocalCult(cultId))
        {
            var def = LocalCultData.Find(cultId);
            if (def != null)
            {
                var needed = def.FollowersRequired - CovenProgress.TotalFollowers(_state);
                PopupTitle = "Not Ready";
                PopupMessage = $"You need {needed} more followers to convert this local cult.";
                NotifyChanged();
            }
            return;
        }
        PendingLocalCultId = cultId;
        NotifyChanged();
    }

    public void ConfirmLocalCultReward(LocalCultReward reward)
    {
        if (PendingLocalCultId == null) return;
        LocalCultEngine.Convert(_state, PendingLocalCultId, reward);
        PendingLocalCultId = null;
        NotifyChanged();
    }

    public void CancelLocalCultReward()
    {
        PendingLocalCultId = null;
        NotifyChanged();
    }

    public LocalCultDef? PendingLocalCultDef =>
        PendingLocalCultId == null ? null : LocalCultData.Find(PendingLocalCultId);

    public void TakeoverCoven(string covenId) { var loc = _locations.Find(covenId); if (loc == null || !CovenProgress.CanConvert(_state, loc)) return; CovenProgress.Takeover(_state, loc); ConvertedCovenName = loc.Name; NotifyChanged(); }
    public void DismissTakeover() { ConvertedCovenName = null; NotifyChanged(); }
    public void SwitchActiveCoven(string covenId) { CovenProgress.SwitchActive(_state, covenId); NotifyChanged(); }

    // ── Shadow War ──
    public ShadowWarState ShadowWar => _state.ShadowWarOrInit;
    public bool ShadowWarVictory => ShadowWar.VictoryAchieved;

    public (bool success, string message) StartRecon(string institutionId, int agentCount)
    {
        var r = ShadowWarEngine.StartRecon(ShadowWar, _state, _locations, institutionId, agentCount);
        NotifyChanged();
        return r;
    }

    public (bool success, string message) SendInfiltrationWave(string institutionId, int waveSize)
    {
        var r = ShadowWarEngine.SendInfiltrationWave(ShadowWar, _state, _locations, institutionId, waveSize);
        NotifyChanged();
        return r;
    }

    public (bool success, string message) WithdrawAgents(string institutionId)
    {
        var r = ShadowWarEngine.WithdrawAgents(ShadowWar, institutionId);
        NotifyChanged();
        return r;
    }

    public (bool success, string message) AssignDefenders(string institutionId, int count)
    {
        var r = ShadowWarEngine.AssignDefenders(ShadowWar, institutionId, count);
        NotifyChanged();
        return r;
    }

    public async Task ResetAsync() { _state = GameEngine.InitialState(); ActiveEvent = null; _eventPending = false; ConvertedCovenName = null; PopupMessage = null; PopupTitle = null; OfflineFaith = 0; OfflineGold = 0; OfflineSeconds = 0; PendingLocalCultId = null; SpawnedLocalCultId = null; await SaveAsync(); NotifyChanged(); }

    private void NotifyChanged()
    {
        OnChange?.Invoke();
        var now = DateTime.UtcNow;
        if (now - _lastSave < TimeSpan.FromSeconds(3)) return;
        _lastSave = now;
        _ = SaveAsync();
    }
    public async Task SaveAsync()
    {
        _state.LastSavedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var json = SaveLoad.SaveGame(_state);
        try
        {
            var prev = await _js.InvokeAsync<string>("localStorage.getItem", GameBalance.SaveKey);
            if (!string.IsNullOrWhiteSpace(prev))
                await _js.InvokeVoidAsync("localStorage.setItem", GameBalance.BackupSaveKey, prev);
            await _js.InvokeVoidAsync("localStorage.setItem", GameBalance.SaveKey, json);
        }
        catch { }
        _lastSave = DateTime.UtcNow;
    }
}
