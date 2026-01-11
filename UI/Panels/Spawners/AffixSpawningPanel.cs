using System;
using System.Collections.Generic;
using System.Linq;
using daymxn.DHG.ItemSpawner.game;
using daymxn.DHG.ItemSpawner.ui.Widgets.Affix;
using daymxn.DHG.ItemSpawner.ui.Widgets.ImageSelector;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;

namespace daymxn.DHG.ItemSpawner.ui.Panels.Spawners;

/// <summary>
///   Base class for panels that spawn in items with affixes.
/// </summary>
/// <typeparam name="TName">The enum type for the item names.</typeparam>
/// <typeparam name="TSpawn">The type of item.</typeparam>
public abstract class AffixSpawningPanel<TName, TSpawn>(UIBase owner)
  : GamePanel(owner) where TName : Enum where TSpawn : AffixTaker {
  private GameObject[] _affixSectionObjects;
  private AffixSection[] _affixSections;
  private ImageSelector _imageSelector;
  private Dropdown _qualityDropdown;
  private List<SelectableImage> _selectables;

  /// <summary>
  ///   Name of the spawnable item.
  /// </summary>
  /// <remarks>
  ///   Will be used in logs, notifications, and for the selectable text.
  /// </remarks>
  protected abstract string SelectableName { get; }

  /// <summary>
  ///   The affix pool mapping to the spawnable item.
  /// </summary>
  protected abstract AffixQualityDict AffixPool { get; }

  /// <summary>
  ///   A cached list of spawnable item name enums.
  /// </summary>
  /// <remarks>
  ///   Should have the same order as <see cref="Icons" />
  /// </remarks>
  protected abstract List<TName> Names { get; }

  /// <summary>
  ///   A cached list of icons for each spawnable item name.
  /// </summary>
  /// <remarks>
  ///   Should have the same order as <see cref="Names" />
  /// </remarks>
  protected abstract List<Sprite> Icons { get; }

  /// <summary>
  ///   Function to call with the item when ready to spawn.
  /// </summary>
  protected abstract Action<TSpawn> SpawnFunction { get; }

  public override int MinWidth => 400;
  public override int MinHeight => 1200;
  public override Vector2 DefaultAnchorMin => new(0f, 1f);
  public override Vector2 DefaultAnchorMax => new(0f, 1f);
  protected override bool IncludeBodyFitter => true;
  protected override bool PivotToAnchor => true;
  public override bool CanDragAndResize => true;
  public override bool IncludeInNavbar => true;
  public override bool RequiresGameData => true;

  /// <summary>
  ///   Function which creates a new empty version of the item.
  /// </summary>
  /// <param name="name">The enum name of the item to spawn.</param>
  /// <param name="quality">The quality of the item to spawn.</param>
  protected abstract TSpawn CreateEmptyItem(TName name, Quality quality);

  protected override void CreateBodyContent() {
    _affixSections = new AffixSection[GameData.Qualities.Highest.GetPowerSlots()];
    _affixSectionObjects = new GameObject[_affixSections.Length];
    _selectables = Icons.Select((image, index) => new SelectableImage(image, index)).ToList();

    AddQualityDropdown();
    AddSelectableDropdown();
    AddAffixSections();
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
      padBottom: 10
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

    _qualityDropdown.SetValueWithoutNotify(GameData.Qualities.Highest.GetValue());
  }

  private void AddSelectableDropdown() {
    _imageSelector = new ImageSelector(Body, _selectables, SelectableName);
  }

  private void AddAffixSections() {
    for (var i = 0; i < _affixSections.Length; i++) {
      var container = UIFactory.CreateUIObject("Container", Body);
      UIFactory.SetLayoutGroup<VerticalLayoutGroup>(
        container,
        forceHeight: false,
        forceWidth: false,
        childControlHeight: true,
        childControlWidth: true,
        spacing: 10,
        padBottom: 10
      );

      var label = UIFactory.CreateLabel(container, "Title", $"Affix #{i + 1}");
      UIFactory.SetLayoutElement(label.gameObject, 75, 25);

      var section = new AffixSection(container, GameData.Qualities.Highest, AffixPool);
      _affixSections[i] = section;
      _affixSectionObjects[i] = container;

      section.DrawBody();
    }
  }

  private void OnQualityChanged(int qualityIndex) {
    TogglePowersFromQuality(qualityIndex);

    var quality = GameData.Qualities.FromInt(qualityIndex);
    foreach (var section in _affixSections) {
      if (section == null) break;

      section.OnParentQualityChanged(quality);
    }
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

    for (var i = 0; i < _affixSectionObjects.Length; i++) {
      _affixSectionObjects[i]?.SetActive(i < powerSlots);
    }
  }

  private void OnSpawnClicked() {
    var quality = GameData.Qualities.FromInt(_qualityDropdown.value);
    var powerSlots = quality.GetPowerSlots();
    var name = Names[_imageSelector.Selected.Value];
    var item = CreateEmptyItem(name, quality);

    for (var i = 0; i < powerSlots; i++) {
      var affix = _affixSections[i].BuildAffix();
      item.SetAffix(affix, i);
    }

    SpawnFunction(item);
    UIManager.SendNotification(Level.Success, $"Spawned {SelectableName}!");
  }
}
