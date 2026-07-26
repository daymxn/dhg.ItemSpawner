using System.Collections.Generic;
using System.Linq;
using daymxn.DHG.ItemSpawner.game;
using daymxn.DHG.ItemSpawner.ui.Native;
using daymxn.DHG.ItemSpawner.ui.Panels;
using daymxn.DHG.ItemSpawner.ui.Panels.Spawners;
using UnityEngine;

namespace daymxn.DHG.ItemSpawner.ui;

/// <summary>
///   Root controller for the native item-spawner UI.
/// </summary>
public static class UIManager {
  public enum Panels {
    Eyes,
    Relics,
    Cores,
    Slates,
    Notification,
    Navbar,
    ImageSelectorModal
  }

  internal static readonly Dictionary<Panels, GamePanel> UIPanels = new();

  private static GameObject _canvasRoot;
  private static NativeEventSystemBridge _eventSystemBridge;
  private static bool _initialized;
  private static bool _reportedCanvasState;

  public static Sounds Sounds { get; private set; }

  internal static void InitUI() {
    if (_initialized) return;
    _initialized = true;

    _canvasRoot = UIFactory.CreateCanvasRoot($"{MyPluginInfo.PLUGIN_GUID}.UI");
    _eventSystemBridge = _canvasRoot.AddComponent<NativeEventSystemBridge>();
    Sounds = _canvasRoot.AddComponent<Sounds>();

    Register(Panels.ImageSelectorModal, new ImageSelectorModal(_canvasRoot));
    Register(Panels.Notification, new NotificationPanel(_canvasRoot));
    Register(Panels.Eyes, new EyesPanel(_canvasRoot));
    Register(Panels.Relics, new RelicsPanel(_canvasRoot));
    Register(Panels.Slates, new SlatesPanel(_canvasRoot));
    Register(Panels.Cores, new CoresPanel(_canvasRoot));
    Register(Panels.Navbar, new Navbar(_canvasRoot));

    var eventSystem = _eventSystemBridge.Current;
    Plugin.Logger.LogInfo(
      _eventSystemBridge.IsUsingFallback
        ? $"Native uGUI initialized with fallback EventSystem '{eventSystem.GetType().FullName}'."
        : $"Native uGUI initialized; using EventSystem '{eventSystem.GetType().FullName}'.");

    Canvas.ForceUpdateCanvases();
  }

  internal static void Update() {
    if (!_initialized || !_canvasRoot) return;
    ReportCanvasStateOnce();
    GameData.Update();
    foreach (var panel in UIPanels.Values.ToArray()) {
      panel.RefreshCanvasLayout();
      if (panel.Enabled) panel.Update();
    }
  }

  internal static void Shutdown() {
    if (!_initialized) return;
    foreach (var panel in UIPanels.Values.ToArray()) {
      panel.Dispose();
    }

    UIPanels.Clear();
    Sounds = null;
    _eventSystemBridge = null;
    if (_canvasRoot) Object.Destroy(_canvasRoot);
    _canvasRoot = null;
    _initialized = false;
    _reportedCanvasState = false;
  }

  public static T GetPanel<T>(Panels panel) where T : GamePanel {
    return GetPanel(panel) as T;
  }

  public static void SendNotification(Level level, string message) {
    GetPanel<NotificationPanel>(Panels.Notification)?.SendNotification(level, message);
  }

  internal static void CloseAllDropdowns() {
    if (_canvasRoot) UIFactory.CloseDropdowns(_canvasRoot);
  }

  private static GamePanel GetPanel(Panels panel) {
    return UIPanels.GetValueOrDefault(panel);
  }

  private static void Register(Panels id, GamePanel panel) {
    UIPanels.Add(id, panel);
    panel.ConstructUI();
  }

  private static void ReportCanvasStateOnce() {
    if (_reportedCanvasState) return;
    _reportedCanvasState = true;

    var canvas = _canvasRoot.GetComponent<Canvas>();
    var rect = _canvasRoot.GetComponent<RectTransform>();
    var navbar = GetPanel(Panels.Navbar);
    Plugin.Logger.LogInfo(
      $"Native uGUI canvas active={_canvasRoot.activeInHierarchy}, enabled={canvas.enabled}, " +
      $"size={rect.rect.width:F0}x{rect.rect.height:F0}, screen={Screen.width}x{Screen.height}, " +
      $"navbarActive={navbar?.Enabled ?? false}, childCount={_canvasRoot.transform.childCount}.");
  }
}
