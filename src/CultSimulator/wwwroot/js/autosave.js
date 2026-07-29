window.setupAutoSave = function (dotNetRef) {
  const save = () => { try { dotNetRef.invokeMethodAsync("SaveOnExit"); } catch (e) {} };
  window.addEventListener("pagehide", save, { once: true });
  document.addEventListener("visibilitychange", () => {
    if (document.visibilityState === "hidden") save();
  });
  window.addEventListener("beforeunload", () => save());
};
