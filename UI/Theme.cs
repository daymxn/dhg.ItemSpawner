using UnityEngine;

namespace daymxn.DHG.ItemSpawner.ui;

/// <summary>
///   Common theme for the UI.
/// </summary>
public static class Theme {
  /// <summary>
  ///   Color for selected buttons.
  /// </summary>
  public static Color SelectedColor = new(45 / 255f, 75 / 255f, 80 / 255f);

  public static readonly Color WindowColor = new(0.055f, 0.055f, 0.055f, 0.98f);
  public static readonly Color PanelColor = new(0.08f, 0.08f, 0.08f, 0.98f);
  public static readonly Color HeaderColor = new(0.035f, 0.035f, 0.035f, 1f);
  public static readonly Color ControlColor = new(0.035f, 0.035f, 0.035f, 1f);
  public static readonly Color SliderColor = new(0.14f, 0.14f, 0.14f, 1f);
  public static readonly Color HandleColor = new(0.22f, 0.22f, 0.22f, 1f);
  public static readonly Color TextColor = new(0.92f, 0.92f, 0.92f, 1f);
}
