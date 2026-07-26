using BepInEx;
using BepInEx.Logging;
using daymxn.DHG.ItemSpawner.ui;

namespace daymxn.DHG.ItemSpawner;

[BepInProcess("Dark Hunting Ground.exe")]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin {
  internal new static ManualLogSource Logger;
  private bool _uiInitialized;

  private void Awake() {
    Logger = base.Logger;
    Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
  }

  private void Update() {
    // BepInEx plugins wake before the game's scene behaviours have necessarily
    // completed Start(). Initializing on the first Update gives game-owned UI
    // services, including an EventSystem, the first opportunity to come online.
    if (!_uiInitialized) {
      _uiInitialized = true;
      UIManager.InitUI();
      Logger.LogInfo("UI loaded. Load a save file to get started.");
    }

    UIManager.Update();
  }

  private void OnDestroy() {
    UIManager.Shutdown();
    _uiInitialized = false;
  }
}
