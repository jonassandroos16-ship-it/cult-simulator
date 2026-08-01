// Supabase auth + cloud save interop for Blazor
// Loaded via <script> tag in index.html

window.supabaseAuth = (function () {
  const SUPABASE_URL = "https://bwjreumsfxzmkbttsiqd.supabase.co";
  const SUPABASE_ANON_KEY = "sb_publishable_UD_XJQg5a7kocofsBXC8kw_o-8a5GjS";

  let supabaseClient = null;
  let currentUser = null;
  let dotNetRef = null;
  let initPromise = null;

  async function init() {
    if (initPromise) return initPromise;
    initPromise = doInit();
    return initPromise;
  }

  async function doInit() {
    if (supabaseClient) return;
    const { createClient } = await import("https://cdn.jsdelivr.net/npm/@supabase/supabase-js@2/+esm");
    supabaseClient = createClient(SUPABASE_URL, SUPABASE_ANON_KEY, {
      auth: {
        persistSession: true,
        autoRefreshToken: true,
        detectSessionInUrl: true
      }
    });

    const { data: { session } } = await supabaseClient.auth.getSession();
    if (session) {
      currentUser = session.user;
    }

    supabaseClient.auth.onAuthStateChange((event, session) => {
      (async () => {
        if (event === "SIGNED_IN" && session) {
          currentUser = session.user;
          if (dotNetRef) {
            try { await dotNetRef.invokeMethodAsync("OnAuthStateChanged", true, getUserInfo()); } catch (e) {}
          }
        } else if (event === "SIGNED_OUT") {
          currentUser = null;
          if (dotNetRef) {
            try { await dotNetRef.invokeMethodAsync("OnAuthStateChanged", false, null); } catch (e) {}
          }
        }
      })();
    });
  }

  // Auto-initialize immediately so detectSessionInUrl can process the OAuth
  // callback hash before Blazor finishes loading. Without this, the tokens in
  // the URL expire by the time the Settings panel calls init().
  init();

  function getUserInfo() {
    if (!currentUser) return null;
    return {
      id: currentUser.id,
      email: currentUser.email || "",
      name: (currentUser.user_metadata && currentUser.user_metadata.full_name) || currentUser.email || "Player",
      avatarUrl: (currentUser.user_metadata && currentUser.user_metadata.avatar_url) || ""
    };
  }

  return {
    init: init,

    ready: function () {
      return init();
    },

    setDotNetRef: function (ref) {
      dotNetRef = ref;
    },

    getUser: function () {
      return getUserInfo();
    },

    signInWithGoogle: async function () {
      await init();
      const { data, error } = await supabaseClient.auth.signInWithOAuth({
        provider: "google",
        options: {
          redirectTo: window.location.origin
        }
      });
      if (error) throw error;
      return data;
    },

    signOut: async function () {
      await init();
      const { error } = await supabaseClient.auth.signOut();
      if (error) throw error;
      currentUser = null;
    },

    getSession: async function () {
      await init();
      const { data: { session } } = await supabaseClient.auth.getSession();
      if (session) {
        currentUser = session.user;
        return getUserInfo();
      }
      return null;
    },

    loadSave: async function () {
      await init();
      if (!currentUser) return null;
      const { data, error } = await supabaseClient
        .from("saves")
        .select("save_data")
        .eq("user_id", currentUser.id)
        .maybeSingle();
      if (error) throw error;
      return data ? data.save_data : null;
    },

    saveToCloud: async function (saveJson) {
      await init();
      if (!currentUser) return false;
      const { error } = await supabaseClient
        .from("saves")
        .upsert({
          user_id: currentUser.id,
          save_data: saveJson,
          updated_at: new Date().toISOString()
        }, { onConflict: "user_id" });
      if (error) throw error;
      return true;
    },

    deleteSave: async function () {
      await init();
      if (!currentUser) return false;
      const { error } = await supabaseClient
        .from("saves")
        .delete()
        .eq("user_id", currentUser.id);
      if (error) throw error;
      return true;
    }
  };
})();