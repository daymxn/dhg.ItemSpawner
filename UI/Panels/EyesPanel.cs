using System.Linq;
using daymxn.DHG.ItemSpawner.game;
using daymxn.DHG.ItemSpawner.ui.Native;
using UnityEngine;
using UnityEngine.UI;
using Vector2 = UnityEngine.Vector2;

namespace daymxn.DHG.ItemSpawner.ui.Panels;

/// <summary>
///   Panel for spawning in Abyss Eyes.
/// </summary>
public class EyesPanel(GameObject owner) : GamePanel(owner) {
  private DropdownBinding<AbyssPower.Name>[] _powerDropdowns;

  private DropdownBinding<Quality> _qualityDropdown;
  public override string Name => "Abyss Eyes";
  public override int MinWidth => 300;
  public override int MinHeight => 450;
  public override Vector2 DefaultAnchorMin => new(1f, .5f);
  public override Vector2 DefaultAnchorMax => new(1f, .5f);
  public override bool CanDragAndResize => true;
  public override bool IncludeInNavbar => true;
  protected override bool PivotToAnchor => true;
  protected override Vector2 PivotOffset => new(-100, 0);

  public override bool RequiresGameData => true;

  protected override void CreateBodyContent() {
    _powerDropdowns =
      new DropdownBinding<AbyssPower.Name>[GameData.Qualities.Highest.GetPowerSlots()];

    AddQualityDropdown();

    AddPowerDropdowns();
    AddSpawnButton();
  }

  private void AddSpawnButton() {
    var container = UIFactory.CreateUIObject("Container", Body);
    UIFactory.SetLayoutGroup<VerticalLayoutGroup>(
      container,
      false,
      true,
      true,
      true,
      childAlignment: TextAnchor.LowerCenter
    );
    UIFactory.SetLayoutElement(container, flexibleWidth: 1, flexibleHeight: 1);

    var button = UIFactory.CreateButton(container, "SpawnButton", "Spawn", Theme.SelectedColor);
    UIFactory.SetLayoutElement(button.gameObject, 200, 25, 1);

    button.onClick.AddListener(OnSpawnClicked);
  }

  private void AddQualityDropdown() {
    var container = UIFactory.CreateUIObject("Container", Body);
    UIFactory.SetLayoutGroup<VerticalLayoutGroup>(
      container,
      false,
      false,
      true,
      true,
      10,
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
      GameData.Qualities.All.Select(value =>
        new DropdownOption<Quality>(value.GetNameStr(), value)),
      GameData.Qualities.Highest
    );
    UIFactory.SetLayoutElement(quality, 200, 25, 1);
  }

  private void AddPowerDropdowns() {
    var label = UIFactory.CreateLabel(Body, "Title", "Powers");
    UIFactory.SetLayoutElement(label.gameObject, 75, 25);

    var container = UIFactory.CreateUIObject("Container", Body);
    UIFactory.SetLayoutGroup<VerticalLayoutGroup>(
      container,
      false,
      false,
      true,
      true,
      10,
      padBottom: 40
    );

    var powers = AbyssPower.prefabs.Select(power =>
      new DropdownOption<AbyssPower.Name>(power.GetNameStr(), power.name)).ToArray();
    for (var i = 0; i < _powerDropdowns.Length; i++) {
      var dropdown = UIFactory.CreateDropdown(
        container,
        $"PowerDropdown_{i}",
        out _powerDropdowns[i],
        "Power",
        14,
        null,
        powers,
        powers.Length > 0 ? powers[0].Value : default
      );
      UIFactory.SetLayoutElement(dropdown, 200, 25, 1);
    }
  }

  private void OnQualityChanged(Quality quality) {
    TogglePowersFromQuality(quality);
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
  /// <param name="quality">The quality that was selected.</param>
  private void TogglePowersFromQuality(Quality quality) {
    var powerSlots = quality.GetPowerSlots();

    for (var i = 0; i < _powerDropdowns.Length; i++) {
      _powerDropdowns[i]?.Component.gameObject.SetActive(i < powerSlots);
    }
  }

  private void OnSpawnClicked() {
    if (!_qualityDropdown.HasValue ||
        _powerDropdowns.Take(_qualityDropdown.Value.GetPowerSlots())
          .Any(binding => binding == null || !binding.HasValue)) {
      UIManager.SendNotification(Level.Error, "No valid abyss powers are available.");
      return;
    }

    var quality = _qualityDropdown.Value;
    var eye = AbyssEye.MakeEmptyAbyssEye(quality);

    var powers = _powerDropdowns
      .Take(quality.GetPowerSlots())
      .Where(it => it is { HasValue: true })
      .Select(it => it.Value)
      .ToArray();

    eye.AddPowers(powers);

    Inventory.SpawnAbyssEye(eye);
    UIManager.SendNotification(Level.Success, "Spawned abyss eye!");
  }

  protected override void OnBodyDestroyed() {
    _powerDropdowns = null;
    _qualityDropdown = null;
  }
}
