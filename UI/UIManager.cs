using System.Collections.Generic;
using daymxn.DHG.ItemSpawner.game;
using daymxn.DHG.ItemSpawner.ui.Panels;
using daymxn.DHG.ItemSpawner.ui.Panels.Spawners;
using UnityEngine;
using UniverseLib.UI;

namespace daymxn.DHG.ItemSpawner.ui;

/// <summary>
///   Root controller for UI related functions.
/// </summary>
public static class UIManager {
  /// <summary>
  ///   All the panels shown in the UI.
  /// </summary>
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

  private static UIBase UiBase { get; set; }
  private static GameObject UIRoot => UiBase?.RootObject;

  /// <summary>
  ///   Sounds loaded by the plugin, which can be played.
  /// </summary>
  public static Sounds Sounds { get; private set; }


  internal static void InitUI() {
    UiBase = UniversalUI.RegisterUI(MyPluginInfo.PLUGIN_GUID, Update);
    UiBase.RootObject.GetComponent<RectTransform>();
    Sounds = UiBase.RootObject.AddComponent<Sounds>();

    UIPanels.Add(Panels.ImageSelectorModal, new ImageSelectorModal(UiBase));

    UIPanels.Add(Panels.Notification, new NotificationPanel(UiBase));
    UIPanels.Add(Panels.Eyes, new EyesPanel(UiBase));
    UIPanels.Add(Panels.Relics, new RelicsPanel(UiBase));
    UIPanels.Add(Panels.Slates, new SlatesPanel(UiBase));
    UIPanels.Add(Panels.Cores, new CoresPanel(UiBase));

    UIPanels.Add(Panels.Navbar, new Navbar(UiBase));
  }

  /// <summary>
  ///   Get a loaded panel, typecasting it go its specific type.
  /// </summary>
  /// <param name="panel">The panel to get.</param>
  /// <typeparam name="T">The class of the panel to get.</typeparam>
  /// <returns>The currently loaded panel for the corresponding name and type.</returns>
  public static T GetPanel<T>(Panels panel) where T : GamePanel {
    return GetPanel(panel) as T;
  }

  private static GamePanel GetPanel(Panels panel) {
    return UIPanels[panel];
  }

  /// <summary>
  ///   Sends a notification to display to the user.
  /// </summary>
  /// <param name="level">The type of notification to send.</param>
  /// <param name="message">The text to show in the notification.</param>
  /// <seealso cref="NotificationPanel" />
  public static void SendNotification(Level level, string message) {
    GetPanel<NotificationPanel>(Panels.Notification)?.SendNotification(level, message);
  }

  private static void Update() {
    if (!UIRoot)
      return;

    GameData.Update();
  }
}
