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

  // Track whether we have unsaved changes
  window.__cultSaveDirty = false;
  window.__cultSaveMarkDirty = function() { window.__cultSaveDirty = true; };

  // Write the cached JSON to all three localStorage slots synchronously.
  // Only rotates backups if the previous data is valid (not corrupted).
  const writeCachedSave = () => {
    var json = window.__cultSaveJson;
    if (!json) return;
    try {
      var prev = localStorage.getItem("cult_simulator_save_v2");
      if (prev && prev.length > 10) {
        // Only rotate backups if previous save looks valid
        var prevBackup = localStorage.getItem("cult_simulator_save_v2_backup");
        if (prevBackup && prevBackup.length > 10)
          localStorage.setItem("cult_simulator_save_v2_backup2", prevBackup);
        localStorage.setItem("cult_simulator_save_v2_backup", prev);
      }
      localStorage.setItem("cult_simulator_save_v2", json);
      window.__cultSaveDirty = false;
    } catch (e) {}
  };

  // pagehide is the reliable cross-browser event for page close / tab close.
  window.addEventListener("pagehide", () => {
    writeCachedSave();
    // Also trigger a cloud save via .NET if the runtime is still alive.
    try { dotNetRef.invokeMethodAsync("SaveOnExit"); } catch (e) {}
  }, { once: true });

  // visibilitychange fires when the tab is hidden (mobile background, tab switch).
  // Write synchronously here too, since mobile browsers may kill the page
  // without firing beforeunload.
  document.addEventListener("visibilitychange", () => {
    if (document.visibilityState === "hidden") writeCachedSave();
  });

  // beforeunload: show a warning if we have unsaved data, giving the save system
  // time to finish writing. The browser shows its own dialog.
  window.addEventListener("beforeunload", (e) => {
    writeCachedSave();
    // If we still have unsaved changes (cloud save pending), ask user to stay
    if (window.__cultSaveDirty) {
      e.preventDefault();
      e.returnValue = "Your save is still being written. Please wait a moment and try closing again.";
      return e.returnValue;
    }
  });
};
