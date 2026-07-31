// Keeps the latest save JSON cached in JS so that page-close events can
// write it to localStorage synchronously, even if the Blazor .NET runtime
// is being torn down and async JSInterop calls never complete.
window.setupAutoSave = function (dotNetRef) {
  // Ask .NET for the latest JSON every 5 seconds (matches the periodic save timer).
  const syncJson = () => {
    try { dotNetRef.invokeMethodAsync("SyncSaveJsonToJS"); } catch (e) {}
  };
  syncJson();
  setInterval(syncJson, 5000);

  // Write the cached JSON to all three localStorage slots synchronously.
  // This runs during beforeunload / pagehide where async JSInterop would
  // be killed mid-flight.
  const writeCachedSave = () => {
    var json = window.__cultSaveJson;
    if (!json) return;
    try {
      var prev = localStorage.getItem("cult_simulator_save_v2");
      if (prev) {
        var prevBackup = localStorage.getItem("cult_simulator_save_v2_backup");
        if (prevBackup)
          localStorage.setItem("cult_simulator_save_v2_backup2", prevBackup);
        localStorage.setItem("cult_simulator_save_v2_backup", prev);
      }
      localStorage.setItem("cult_simulator_save_v2", json);
    } catch (e) {}
  };

  // pagehide is the reliable cross-browser event for page close / tab close.
  // We use it to write the cached save synchronously.
  window.addEventListener("pagehide", writeCachedSave, { once: true });

  // visibilitychange fires when the tab is hidden (mobile background, tab switch).
  // Write synchronously here too, since mobile browsers may kill the page
  // without firing beforeunload.
  document.addEventListener("visibilitychange", () => {
    if (document.visibilityState === "hidden") writeCachedSave();
  });

  // beforeunload is a last-resort fallback. On some browsers async
  // JSInterop won't complete, so we rely on the cached JSON write above.
  window.addEventListener("beforeunload", writeCachedSave);
};
