using BepInEx;
using BepInEx.Logging;
using daymxn.DHG.ItemSpawner.ui;
using UniverseLib;

namespace daymxn.DHG.ItemSpawner;

[BepInProcess("Dark Hunting Ground.exe")]
[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin {
  internal new static ManualLogSource Logger;

  private void Awake() {
    Logger = base.Logger;
    Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
    Universe.Init(LateInit);
  }

  public void LateInit() {
    UIManager.InitUI();
    Logger.LogInfo("UI loaded. Load a save file to get started.");
  }
}
