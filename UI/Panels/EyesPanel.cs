using System.Linq;
using daymxn.DHG.ItemSpawner.game;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using Vector2 = UnityEngine.Vector2;

namespace daymxn.DHG.ItemSpawner.ui.Panels;

/// <summary>
///   Panel for spawning in Abyss Eyes.
/// </summary>
public class EyesPanel(UIBase owner) : GamePanel(owner) {
  private Dropdown[] _powerDropdowns;

  private Dropdown _qualityDropdown;
  public override string Name => "Abyss Eyes";
  public override int MinWidth => 300;
  public override int MinHeight => 450;
  public override Vector2 DefaultAnchorMin => new(1f, .5f);
  public override Vector2 DefaultAnchorMax => new(1f, .5f);
  public override bool CanDragAndResize => true;
  public override bool IncludeInNavbar => true;
  protected override bool IncludeBodyFitter => true;

  protected override bool PivotToAnchor => true;
  protected override Vector2 PivotOffset => new(-100, 0);

  public override bool RequiresGameData => true;

  protected override void CreateBodyContent() {
    _powerDropdowns = new Dropdown[GameData.Qualities.Highest.GetPowerSlots()];

    AddQualityDropdown();

    AddPowerDropdowns();
    AddSpawnButton();
  }

  private void AddSpawnButton() {
    var container = UIFactory.CreateUIObject("Container", Body);
    UIFactory.SetLayoutGroup<VerticalLayoutGroup>(
      container,
      forceHeight: false,
      forceWidth: false,
      childControlHeight: true,
      childControlWidth: true,
      childAlignment: TextAnchor.LowerCenter
    );
    UIFactory.SetLayoutElement(container, flexibleHeight: 1);

    var button = UIFactory.CreateButton(container, "SpawnButton", "Spawn", Theme.SelectedColor);
    UIFactory.SetLayoutElement(button.Component.gameObject, 200, 25, 1);

    button.OnClick += OnSpawnClicked;
  }

  private void AddQualityDropdown() {
    var container = UIFactory.CreateUIObject("Container", Body);
    UIFactory.SetLayoutGroup<VerticalLayoutGroup>(
      container,
      forceHeight: false,
      forceWidth: false,
      childControlHeight: true,
      childControlWidth: true,
      spacing: 10,
      padBottom: 20
    );

    var label = UIFactory.CreateLabel(container, "Title", "Quality");
    UIFactory.SetLayoutElement(label.gameObject, 75, 25);

    var quality = UIFactory.CreateDropdown(
      container,
      "QualityDropdown",
      out _qualityDropdown,
      "Quality",
      14,
      OnQualityChanged,
      GameData.Qualities.Names
    );
    UIFactory.SetLayoutElement(quality, 200, 25, 1);

    _qualityDropdown.value = GameData.Qualities.Highest.GetValue();
  }

  private void AddPowerDropdowns() {
    var label = UIFactory.CreateLabel(Body, "Title", "Powers");
    UIFactory.SetLayoutElement(label.gameObject, 75, 25);

    var container = UIFactory.CreateUIObject("Container", Body);
    UIFactory.SetLayoutGroup<VerticalLayoutGroup>(
      container,
      forceHeight: false,
      forceWidth: false,
      childControlHeight: true,
      childControlWidth: true,
      spacing: 10,
      padBottom: 40
    );

    var powers = AbyssPower.prefabs.Select(power => power.GetNameStr()).ToArray();
    for (var i = 0; i < _powerDropdowns.Length; i++) {
      var dropdown = UIFactory.CreateDropdown(
        container,
        $"PowerDropdown_{i}",
        out _powerDropdowns[i],
        "Power",
        14,
        _ => { },
        powers
      );
      UIFactory.SetLayoutElement(dropdown, 200, 25, 1);
    }
  }

  private void OnQualityChanged(int qualityIndex) {
    TogglePowersFromQuality(qualityIndex);
  }

  /// <summary>
  ///   Updates power dropdowns to show only the amount that are valid for the given Quality.
  ///   <br /> <br />
  ///   For example, if the Quality was changed to "Common" and "Common" only has 2 power slots,
  ///   then only 2 power dropdowns should be shown.
  /// </summary>
  /// <remarks>
  ///   Called when the selected Quality changes.
  /// </remarks>
  /// <param name="qualityIndex">The int value of the quality that was selected.</param>
  private void TogglePowersFromQuality(int qualityIndex) {
    var quality = GameData.Qualities.FromInt(qualityIndex);
    var powerSlots = quality.GetPowerSlots();

    for (var i = 0; i < _powerDropdowns.Length; i++) {
      _powerDropdowns[i]?.gameObject.SetActive(i < powerSlots);
    }
  }

  private void OnSpawnClicked() {
    var quality = GameData.Qualities.FromInt(_qualityDropdown.value);
    var eye = AbyssEye.MakeEmptyAbyssEye(quality);

    var powers = _powerDropdowns
      .Take(quality.GetPowerSlots())
      .Select(it => AbyssPower.prefabs[it.value].name)
      .ToArray();

    eye.AddPowers(powers);

    Inventory.SpawnAbyssEye(eye);
    UIManager.SendNotification(Level.Success, "Spawned abyss eye!");
  }
}
