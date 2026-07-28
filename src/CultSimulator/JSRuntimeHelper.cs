using Microsoft.JSInterop;

namespace CultSimulator;

/// <summary>
/// Static helper for invoking JavaScript functions via IJSRuntime.
/// Components inject this to call the Leaflet map initialization and marker functions.
/// </summary>
public static class JSRuntimeHelper
{
    private static IJSRuntime? _runtime;

    public static void Initialize(IJSRuntime runtime) => _runtime = runtime;

    public static async Task InvokeVoidAsync(string identifier, params object[] args)
    {
        if (_runtime == null) return;
        await _runtime.InvokeVoidAsync(identifier, args);
    }

    public static async Task<T?> InvokeAsync<T>(string identifier, params object[] args)
    {
        if (_runtime == null) return default;
        return await _runtime.InvokeAsync<T>(identifier, args);
    }
}
