using Microsoft.JSInterop;

namespace CultSimulator.Game;

public enum AudioGameState { Menu, Gameplay, Map, Combat }

public class AudioService
{
    private readonly IJSRuntime _js;
    private AudioGameState? _lastState;
    private string? _lastContinentId;

    public AudioService(IJSRuntime js) => _js = js;

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

        var trackName = state switch
        {
            AudioGameState.Menu => "menu",
            AudioGameState.Gameplay => "gameplay",
            AudioGameState.Map => "map",
            AudioGameState.Combat => "combat",
            _ => "gameplay"
        };

        if (!string.IsNullOrEmpty(normContinent) && (state == AudioGameState.Gameplay || state == AudioGameState.Combat))
            await _js.InvokeVoidAsync("cultAudio.playRegionalTrack", trackName, normContinent);
        else
            await _js.InvokeVoidAsync("cultAudio.playTrack", trackName);
    }

    public async Task StopMusicAsync()
    {
        _lastState = null;
        _lastContinentId = null;
        await _js.InvokeVoidAsync("cultAudio.stopMusic");
    }

    public async Task PlayClickAsync()
    {
        await _js.InvokeVoidAsync("cultAudio.playUiSound", "click");
    }

    public async Task PlayHoverAsync()
    {
        await _js.InvokeVoidAsync("cultAudio.playUiSound", "hover");
    }

    public async Task PlayErrorAsync()
    {
        await _js.InvokeVoidAsync("cultAudio.playUiSound", "error");
    }

    public async Task PlaySuccessAsync()
    {
        await _js.InvokeVoidAsync("cultAudio.playUiSound", "success");
    }

    public async Task SetMusicVolumeAsync(float v)
    {
        await _js.InvokeVoidAsync("cultAudio.setMusicVolume", v);
    }

    public async Task SetSfxVolumeAsync(float v)
    {
        await _js.InvokeVoidAsync("cultAudio.setSfxVolume", v);
    }

    public async Task ResumeAudioAsync()
    {
        await _js.InvokeVoidAsync("cultAudio.resumeAudio");
    }
}
