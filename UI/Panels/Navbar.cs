using System.Collections.Generic;
using System;
using daymxn.DHG.ItemSpawner.game;
using daymxn.DHG.ItemSpawner.ui.Native;
using UnityEngine;
using UnityEngine.UI;

namespace daymxn.DHG.ItemSpawner.ui.Panels;

/// <summary>
///   Navigation panel shown on the side of the screen for quickly opening and closing
///   the main panels.
/// </summary>
/// <remarks>
///   Panels are shown dynamically based on whether they have
///   <see cref="ItemSpawner.ui.Panels.GamePanel.IncludeInNavbar" /> set to true.
/// </remarks>
public class Navbar(GameObject owner) : GamePanel(owner) {
  private static readonly Color EnabledColor = Theme.SelectedColor;
  private static readonly Color DisabledColor = new(0.25f, 0.25f, 0.25f);

  private static readonly ColorBlock EnabledColors = UIFactory.ColorBlockFor(EnabledColor);

  private static readonly ColorBlock DisabledColors = UIFactory.ColorBlockFor(DisabledColor);

  /// <summary>
  ///   Buttons that point to panels which require game data.
  /// </summary>
  /// <remarks>
  ///   These buttons will be enabled and disabled whenever a save slot loads and unloads.
  /// </remarks>
  private readonly List<Button> _buttonsToTrack = [];
  private readonly List<(GamePanel Panel, Action<bool> Handler)> _panelHandlers = [];

  public override string Name => "Navbar";
  public override int MinWidth => 75;
  public override int MinHeight => 150;
  public override int DefaultWidth => 75;
  public override int DefaultHeight => 150;
  public override Vector2 DefaultAnchorMin => new(0f, 0.5f);
  public override Vector2 DefaultAnchorMax => new(0f, 0.5f);
  public override bool CanDragAndResize => false;
  protected override bool PivotToAnchor => true;
  protected override Vector2 PivotOffset => new(10, 0);
  protected override bool ShowTitleBar => false;

  protected override void CreateBodyContent() {
    Body.GetComponent<VerticalLayoutGroup>().childAlignment = TextAnchor.UpperCenter;
    AddButtons();

    GameData.OnPlayerDataLoaded += OnPlayerDataLoaded;
    GameData.OnPlayerDataUnloaded += OnPlayerDataUnloaded;
    UpdateButtons(GameData.IsPlayerDataLoaded());
  }

  private void AddButtons() {
    foreach (var (panelEnum, panel) in UIManager.UIPanels) {
      if (!panel.IncludeInNavbar) continue;

      var name = panelEnum.ToString();
      var button = UIFactory.CreateButton(Body, name, name);
      UIFactory.SetLayoutElement(button.gameObject, 50, 25);

      Action<bool> handler = enabled => {
        button.colors = enabled ? EnabledColors : DisabledColors;
      };
      panel.OnToggleEnabled += handler;
      _panelHandlers.Add((panel, handler));

      button.onClick.AddListener(panel.Toggle);

      if (panel.ShowByDefault) {
        button.colors = EnabledColors;
      }

      if (!panel.RequiresGameData) continue;

      _buttonsToTrack.Add(button);
      button.interactable = false;
    }
  }

  private void UpdateButtons(bool enabled) {
    foreach (var button in _buttonsToTrack) {
      button.interactable = enabled;
    }
  }

  protected override void OnDisposing() {
    GameData.OnPlayerDataLoaded -= OnPlayerDataLoaded;
    GameData.OnPlayerDataUnloaded -= OnPlayerDataUnloaded;
    foreach (var (panel, handler) in _panelHandlers) {
      panel.OnToggleEnabled -= handler;
    }

    _panelHandlers.Clear();
  }

  private void OnPlayerDataLoaded(object sender, EventArgs args) {
    UpdateButtons(true);
  }

  private void OnPlayerDataUnloaded(object sender, EventArgs args) {
    UpdateButtons(false);
  }
}
