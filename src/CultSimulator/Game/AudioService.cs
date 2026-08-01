using Microsoft.JSInterop;

namespace CultSimulator.Game;

public enum AudioGameState { Menu, Gameplay, Map, Combat }

public class AudioService
{
    private readonly IJSRuntime _js;
    private IJSObjectReference? _module;
    private AudioGameState? _lastState;
    private string? _lastContinentId;

    public AudioService(IJSRuntime js) => _js = js;

    private async Task<IJSObjectReference> GetModuleAsync()
    {
        if (_module is not null) return _module;
        _module = await _js.InvokeAsync<IJSObjectReference>("import", "./js/audio.js?v=1");
        return _module;
    }

    /// <summary>
    /// Sets the music state. Only triggers a track change if the resulting
    /// track is different from what's currently playing, so this is safe to
    /// call on every state update without restarting music.
    /// </summary>
    public async Task SetGameStateAsync(AudioGameState state, string? continentId = null)
    {
        var normContinent = continentId?.ToLowerInvariant();
        if (_lastState == state && _lastContinentId == normContinent) return;
        _lastState = state;
        _lastContinentId = normContinent;

        var mod = await GetModuleAsync();
        var trackName = state switch
        {
            AudioGameState.Menu => "menu",
            AudioGameState.Gameplay => "gameplay",
            AudioGameState.Map => "map",
            AudioGameState.Combat => "combat",
            _ => "gameplay"
        };

        if (!string.IsNullOrEmpty(normContinent) && (state == AudioGameState.Gameplay || state == AudioGameState.Combat))
            await mod.InvokeVoidAsync("playRegionalTrack", trackName, normContinent);
        else
            await mod.InvokeVoidAsync("playTrack", trackName);
    }

    public async Task StopMusicAsync()
    {
        _lastState = null;
        _lastContinentId = null;
        var mod = await GetModuleAsync();
        await mod.InvokeVoidAsync("stopMusic");
    }

    public async Task PlayClickAsync()
    {
        var mod = await GetModuleAsync();
        await mod.InvokeVoidAsync("playUiSound", "click");
    }

    public async Task PlayHoverAsync()
    {
        var mod = await GetModuleAsync();
        await mod.InvokeVoidAsync("playUiSound", "hover");
    }

    public async Task PlayErrorAsync()
    {
        var mod = await GetModuleAsync();
        await mod.InvokeVoidAsync("playUiSound", "error");
    }

    public async Task PlaySuccessAsync()
    {
        var mod = await GetModuleAsync();
        await mod.InvokeVoidAsync("playUiSound", "success");
    }

    public async Task SetMusicVolumeAsync(float v)
    {
        var mod = await GetModuleAsync();
        await mod.InvokeVoidAsync("setMusicVolume", v);
    }

    public async Task SetSfxVolumeAsync(float v)
    {
        var mod = await GetModuleAsync();
        await mod.InvokeVoidAsync("setSfxVolume", v);
    }

    public async Task ResumeAudioAsync()
    {
        var mod = await GetModuleAsync();
        await mod.InvokeVoidAsync("resumeAudio");
    }
}
