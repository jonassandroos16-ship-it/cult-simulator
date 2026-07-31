using System.Linq;
using System.Text.Json;

namespace CultSimulator.Game;

public class GameService
{
    private GameState _state;
    public GameState State => _state;
    public ShadowWarState ShadowWar => _state.ShadowWarOrInit;

    public event Action? OnChange;

    public CovenEventData? ActiveEvent { get; private set; }
    public bool EventPending => ActiveEvent != null;
    public string? ConvertedCovenName { get; private set; }
    public string? PendingLocalCultId { get; private set; }
    public string? SpawnedLocalCultId { get; private set; }
    public string? PendingFoothold { get; private set; }

    public string? PopupMessage { get; private set; }
    public string? PopupTitle { get; private set; }
    public bool PopupPending => PopupMessage != null;
    public double OfflineFaith { get; private set; }
    public double OfflineGold { get; private set; }
    public double OfflineSeconds { get; private set; }
    public double OfflineLostFaith { get; private set; }
    public double OfflineLostGold { get; private set; }
    public bool HasOfflineReport => OfflineFaith > 0 || OfflineGold > 0;
    public bool OfflinePopupPending { get; private set; }

    public List<(string RivalName, string ContinentId)> RecentTerritoryLosses => _state.RivalCultsOrInit.RecentTerritoryLosses;

    public GameService()
    {
        _state = GameEngine.InitialState();
        _ = LoadAsync();
    }

    public async Task LoadAsync()
    {
        var (state, _) = SaveLoad.LoadGameWithBackup(
            await SaveLoad.ReadSaveAsync(),
            await SaveLoad.ReadBackupAsync(),
            await SaveLoad.ReadBackup2Async());
        _state = state ?? GameEngine.InitialState();
        ApplyOfflineIncome();
        NotifyChanged();
    }

    private void ApplyOfflineIncome()
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long elapsed = now - _state.LastSavedAt;
        if (elapsed <= 0) return;
        var (faith, gold, lostFaith, lostGold) = GameEngine.ApplyOfflineIncome(_state, elapsed);
        OfflineFaith = faith; OfflineGold = gold; OfflineSeconds = elapsed / 1000.0;
        OfflineLostFaith = lostFaith; OfflineLostGold = lostGold;
        OfflinePopupPending = faith > 0 || gold > 0 || lostFaith > 0 || lostGold > 0;
        _state.LastSavedAt = now;
    }

    public async Task SaveAsync()
    {
        _state.LastSavedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        await SaveLoad.WriteSaveAsync(SaveLoad.SaveGame(_state));
    }

    public void NotifyChanged() => OnChange?.Invoke();

    public void DismissEvent()
    {
        ActiveEvent = null;
        NotifyChanged();
    }

    public void DismissPopup() { PopupMessage = null; PopupTitle = null; NotifyChanged(); }

    public void DismissOfflineReport() { OfflineFaith = 0; OfflineGold = 0; OfflineSeconds = 0; OfflineLostFaith = 0; OfflineLostGold = 0; OfflinePopupPending = false; NotifyChanged(); }

    public bool CanConvert(string covenId)
    {
        if (covenId == "skanor") return false;
        var loc = WorldLocationService.FindStatic(covenId);
        if (loc == null) return false;
        if (!HasCovenInContinent(loc.Continent)) { PopupTitle = "No Foothold"; PopupMessage = $"You need a coven in {loc.Continent} before you can convert covens there. Expand to a neighboring continent first."; return false; }
        var coven = _state.FindCoven(covenId);
        if (coven != null && coven.Converted) return false;
        var totalFollowers = CovenProgress.TotalFollowers(_state);
        if (totalFollowers < loc.FollowersRequired) { PopupTitle = "Not Ready"; PopupMessage = $"You need {loc.FollowersRequired - totalFollowers} more followers before you can convert this coven."; return false; }
        return true;
    }

    public void StartConversion(string covenId)
    {
        if (!CanConvert(covenId)) { NotifyChanged(); return; }
        var def = ConversionDataService.FindStatic(covenId);
        if (def == null) { PopupTitle = "No Conversion Available"; PopupMessage = "This coven has no conversion sequence. It may not have event data yet."; NotifyChanged(); return; }
        _state.Conversion = new ConversionState { CovenId = covenId, StepIndex = 0, StartTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() };
        ConvertedCovenName = WorldLocationService.FindStatic(covenId)?.Name ?? covenId;
        NotifyChanged();
    }

    public void ChooseConversionOption(int optionIndex)
    {
        if (_state.Conversion == null) return;
        var def = ConversionDataService.FindStatic(_state.Conversion.CovenId);
        if (def == null || _state.Conversion.StepIndex >= def.Steps.Count) return;
        var step = def.Steps[_state.Conversion.StepIndex];
        if (optionIndex < 0 || optionIndex >= step.Options.Count) return;
        var option = step.Options[optionIndex];
        var outcome = option.Outcome ?? "";
        if (!string.IsNullOrWhiteSpace(outcome)) { PopupTitle = "Outcome"; PopupMessage = outcome; }
        _state.Conversion.StepIndex++;
        if (_state.Conversion.StepIndex >= def.Steps.Count)
        {
            CompleteConversion(_state.Conversion.CovenId);
            _state.Conversion = null;
        }
        NotifyChanged();
    }

    private void CompleteConversion(string covenId)
    {
        var loc = WorldLocationService.FindStatic(covenId);
        if (loc == null) return;
        var existing = _state.FindCoven(covenId);
        if (existing == null)
        {
            existing = new CovenState { Id = covenId, Converted = true, TakenOver = true };
            _state.Covens.Add(existing);
        }
        else
        {
            existing.Converted = true;
            existing.TakenOver = true;
        }
        var home = _state.HomeCoven;
        double faithTransfer = home.Faith * GameBalance.CovenTakeoverFaithPercent;
        double goldTransfer = home.Gold * GameBalance.CovenTakeoverGoldPercent;
        double followerTransfer = home.Followers * GameBalance.CovenTakeoverFollowerPercent;
        home.Faith -= faithTransfer; home.Gold -= goldTransfer; home.Followers -= (int)followerTransfer;
        existing.Faith = faithTransfer; existing.Gold = goldTransfer; existing.Followers = (int)followerTransfer;
        PendingFoothold = loc.Continent;
    }

    public void CancelConversion()
    {
        _state.Conversion = null;
        ConvertedCovenName = null;
        NotifyChanged();
    }

    public string? ConsumePendingFoothold()
    {
        var f = PendingFoothold;
        PendingFoothold = null;
        return f;
    }

    public bool CanConquerNode(string nodeId)
    {
        var def = OccultData.MapNode(nodeId);
        if (def == null) return false;
        if (def.CovenId != _state.ActiveCovenId) { PopupTitle = "Wrong Coven"; PopupMessage = "This node belongs to a different coven. Switch active coven first."; return false; }
        var coven = _state.ActiveCoven;
        if (coven.Faith < def.FaithCost) { PopupTitle = "Cannot Claim Node"; PopupMessage = $"Not enough Faith. Need {NumberFormat.Fmt(def.FaithCost)} but have {NumberFormat.Fmt(coven.Faith)}."; return false; }
        if (_state.Occult.ArmyPower < def.ArmyPowerRequired) { PopupTitle = "Cannot Claim Node"; PopupMessage = $"Not enough Army Power. Need {NumberFormat.Fmt(def.ArmyPowerRequired)} but have {NumberFormat.Fmt(_state.Occult.ArmyPower)}."; return false; }
        if (WorldMapSystem.IsConquered(_state.Occult, def.Id)) { PopupTitle = "Cannot Claim Node"; PopupMessage = "Cannot conquer this node right now."; return false; }
        return true;
    }

    public void ConquerNode(string nodeId)
    {
        if (!CanConquerNode(nodeId)) { NotifyChanged(); return; }
        WorldMapSystem.Conquer(_state, OccultData.MapNode(nodeId)!);
        NotifyChanged();
    }

    public void SetNodeStance(string nodeId, NodeStance stance)
    {
        WorldMapSystem.SetStance(_state.Occult, nodeId, stance);
        NotifyChanged();
    }

    public void SwitchActiveCoven(string id)
    {
        var coven = _state.FindCoven(id);
        if (coven == null || !coven.Converted) return;
        _state.ActiveCovenId = id;
        NotifyChanged();
    }

    public void RecruitAgent(AgentType type, int count)
    {
        ShadowWarEngine.RecruitAgent(_state, type, count);
        NotifyChanged();
    }

    public void DeployAgents(string continentId, AgentType type, int count)
    {
        BattleEngine.DeployAgents(_state, continentId, type, count);
        NotifyChanged();
    }

    public void StartBattle(string continentId)
    {
        BattleEngine.StartBattle(_state, continentId);
        NotifyChanged();
    }

    public void WithdrawBattleAgents(string continentId)
    {
        BattleEngine.WithdrawAgents(_state, continentId);
        NotifyChanged();
    }

    public void InfiltrateInstitution(string id)
    {
        ShadowWarEngine.Infiltrate(_state, id);
        NotifyChanged();
    }

    public void DefendInstitution(string id)
    {
        ShadowWarEngine.Defend(_state, id);
        NotifyChanged();
    }

    public void DeployShadowWarAgents(string institutionId, int count)
    {
        ShadowWarEngine.DeployAgents(_state, institutionId, count);
        NotifyChanged();
    }

    public void Tick(double deltaSec)
    {
        OccultEngine.Tick(_state, deltaSec);
        ShadowWarEngine.Tick(_state, deltaSec);
        BattleEngine.Tick(_state, deltaSec);
        RivalCultEngine.Tick(_state, deltaSec);
        LocalCultEngine.Tick(_state, deltaSec);
        LocalCultEngine.SpawnTick(_state, deltaSec);
        NotifyChanged();
    }

    public void TriggerEvent(CovenEventData evt)
    {
        ActiveEvent = evt;
        NotifyChanged();
    }

    public void ResolveEvent(int optionIndex)
    {
        if (ActiveEvent == null) return;
        var option = ActiveEvent.Options[optionIndex];
        var outcome = option.Resolve?.Invoke(_state) ?? "";
        if (!string.IsNullOrWhiteSpace(outcome)) { PopupTitle = "Outcome"; PopupMessage = outcome; }
        ActiveEvent = null;
        NotifyChanged();
    }

    public bool CanStartLocalCultBattle(string cultId)
    {
        var def = LocalCultData.Find(cultId);
        if (def == null) return false;
        return CovenProgress.TotalFollowers(_state) >= def.FollowersRequired;
    }

    public void RequestLocalCultConversion(string cultId)
    {
        var def = LocalCultData.Find(cultId);
        if (def == null) return;
        var totalFollowers = CovenProgress.TotalFollowers(_state);
        if (totalFollowers < def.FollowersRequired)
        {
            PopupTitle = "Not Ready";
            PopupMessage = $"You need {def.FollowersRequired - totalFollowers} more followers to convert this local cult.";
            NotifyChanged();
            return;
        }
        PendingLocalCultId = cultId;
        NotifyChanged();
    }

    public void SpawnLocalCult(string cultId)
    {
        SpawnedLocalCultId = cultId;
        NotifyChanged();
    }

    public string? ConsumeSpawnedLocalCult()
    {
        var id = SpawnedLocalCultId;
        SpawnedLocalCultId = null;
        return id;
    }

    public string? ConsumePendingLocalCult()
    {
        var id = PendingLocalCultId;
        PendingLocalCultId = null;
        return id;
    }

    public LocalCultBattleState? GetLocalCultBattle(string cultId) => LocalCultBattleEngine.GetBattle(_state, cultId);

    public void DeployLocalCultAgents(string cultId, AgentType type, int count)
    {
        LocalCultBattleEngine.DeployAgents(_state, cultId, type, count);
        NotifyChanged();
    }

    public void WithdrawLocalCultAgents(string cultId)
    {
        LocalCultBattleEngine.WithdrawAgents(_state, cultId);
        NotifyChanged();
    }

    public void StartLocalCultBattle(string cultId)
    {
        LocalCultBattleEngine.StartBattle(_state, cultId);
        NotifyChanged();
    }

    public List<LocalCultInstance> ActiveLocalCultsForCurrentCoven => LocalCultEngine.ActiveForCoven(_state, _state.ActiveCovenId);

    public bool HasCovenInContinent(string continent)
    {
        return _state.Covens.Any(c => c.Converted &&
            string.Equals(WorldLocationService.FindStatic(c.Id)?.Continent, continent, StringComparison.OrdinalIgnoreCase));
    }

    public async Task ResetAsync() { _state = GameEngine.InitialState(); ActiveEvent = null; _eventPending = false; ConvertedCovenName = null; PopupMessage = null; PopupTitle = null; OfflineFaith = 0; OfflineGold = 0; OfflineSeconds = 0; OfflineLostFaith = 0; OfflineLostGold = 0; OfflinePopupPending = false; PendingLocalCultId = null; SpawnedLocalCultId = null; PendingFoothold = null; await SaveAsync(); NotifyChanged(); }
}
