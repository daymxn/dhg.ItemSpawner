using System.Collections.Generic;
using daymxn.DHG.ItemSpawner.game;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace daymxn.DHG.ItemSpawner.ui.Panels;

/// <summary>
///   Navigation panel shown on the side of the screen for quickly opening and closing
///   the main panels.
/// </summary>
/// <remarks>
///   Panels are shown dynamically based on whether they have
///   <see cref="ItemSpawner.ui.Panels.GamePanel.IncludeInNavbar" /> set to true.
/// </remarks>
public class Navbar(UIBase owner) : GamePanel(owner) {
  private static readonly Color EnabledColor = Theme.SelectedColor;
  private static readonly Color DisabledColor = new(0.25f, 0.25f, 0.25f);

  private static readonly ColorBlock EnabledColors =
    new() {
      normalColor = EnabledColor,
      highlightedColor = EnabledColor * 1.2f,
      pressedColor = EnabledColor * 0.7f,
      colorMultiplier = 1.0f
    };

  private static readonly ColorBlock DisabledColors =
    new() {
      normalColor = DisabledColor,
      highlightedColor = DisabledColor * 1.2f,
      pressedColor = DisabledColor * 0.7f,
      colorMultiplier = 1.0f
    };

  /// <summary>
  ///   Buttons that point to panels which require game data.
  /// </summary>
  /// <remarks>
  ///   These buttons will be enabled and disabled whenever a save slot loads and unloads.
  /// </remarks>
  private readonly List<ButtonRef> _buttonsToTrack = [];

  public override string Name => "Navbar";
  public override int MinWidth => 300;
  public override int MinHeight => 450;
  public override Vector2 DefaultAnchorMin => new(0f, 0.5f);
  public override Vector2 DefaultAnchorMax => new(0f, 0.5f);
  public override bool CanDragAndResize => false;
  protected override bool PivotToAnchor => true;
  protected override Vector2 PivotOffset => new(10, 0);
  protected override bool IncludeBodyFitter => true;

  protected override void CreateBodyContent() {
    AddButtons();

    GameData.OnPlayerDataLoaded += (_, _) => { UpdateButtons(true); };
    GameData.OnPlayerDataUnloaded += (_, _) => { UpdateButtons(false); };
  }

  private void AddButtons() {
    foreach (var (panelEnum, panel) in UIManager.UIPanels) {
      if (!panel.IncludeInNavbar) continue;

      var name = panelEnum.ToString();
      var button = UIFactory.CreateButton(Body, name, name);
      UIFactory.SetLayoutElement(button.Component.gameObject, 50, 25);

      panel.OnToggleEnabled += enabled => {
        button.Component.colors = enabled ? EnabledColors : DisabledColors;
      };

      button.OnClick += panel.Toggle;

      if (panel.ShowByDefault) {
        button.Component.colors = EnabledColors;
      }

      if (!panel.RequiresGameData) continue;

      _buttonsToTrack.Add(button);
      button.Component.interactable = false;
    }
  }

  private void UpdateButtons(bool enabled) {
    foreach (var button in _buttonsToTrack) {
      button.Component.interactable = enabled;
    }
  }
}
