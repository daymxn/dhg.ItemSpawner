using System;
using System.Collections.Generic;
using System.Linq;
using daymxn.DHG.ItemSpawner.game;
using daymxn.DHG.ItemSpawner.ui.Native;
using daymxn.DHG.ItemSpawner.ui.Widgets.Affix;
using daymxn.DHG.ItemSpawner.ui.Widgets.ImageSelector;
using UnityEngine;
using UnityEngine.UI;

namespace daymxn.DHG.ItemSpawner.ui.Panels.Spawners;

/// <summary>
///   Base class for panels that spawn in items with affixes.
/// </summary>
/// <typeparam name="TName">The enum type for the item names.</typeparam>
/// <typeparam name="TSpawn">The type of item.</typeparam>
public abstract class AffixSpawningPanel<TName, TSpawn>(GameObject owner)
  : GamePanel(owner) where TName : Enum where TSpawn : AffixTaker {
  private GameObject[] _affixSectionObjects;
  private AffixSection[] _affixSections;
  private bool _hasAffixes;
  private ImageSelector<TName> _imageSelector;
  private DropdownBinding<Quality> _qualityDropdown;
  private List<SelectableImage<TName>> _selectables;

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
  public override int MinHeight => 400;
  public override int DefaultWidth => 500;
  public override int DefaultHeight => 900;
  public override Vector2 DefaultAnchorMin => new(0f, 1f);
  public override Vector2 DefaultAnchorMax => new(0f, 1f);
  protected override bool UseScrollView => true;
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
    var icons = Icons;
    var names = Names;
    var selectableCount = Math.Min(icons.Count, names.Count);
    if (icons.Count != names.Count) {
      Plugin.Logger.LogWarning(
        $"{Name} has {icons.Count} icons but {names.Count} names; checking {selectableCount} matched entries."
      );
    }

    _selectables = Enumerable.Range(0, selectableCount)
      .Where(index => HasUsableIcon(icons[index]))
      .Select(index => new SelectableImage<TName>(icons[index], names[index]))
      .ToList();
    var skippedItemCount = Math.Max(0, names.Count - _selectables.Count);
    if (skippedItemCount > 0) {
      Plugin.Logger.LogWarning(
        $"{Name} skipped {skippedItemCount} item entries without usable icons."
      );
    }

    _hasAffixes = AffixPool != null &&
                  AffixPool.Any(entry => entry.Value is { Count: > 0 });

    AddQualityDropdown();
    AddSelectableDropdown();
    if (_hasAffixes) {
      AddAffixSections();
    } else {
      UIFactory.CreateLabel(Body, "MissingAffixes", "No affixes are available.",
        TextAnchor.MiddleCenter, Color.red);
    }
    AddSpawnButton();
  }

  private void AddSpawnButton() {
    var container = UIFactory.CreateUIObject("Container", Body);
    UIFactory.SetLayoutGroup<VerticalLayoutGroup>(
      container,
      forceHeight: false,
      forceWidth: true,
      childControlHeight: true,
      childControlWidth: true,
      childAlignment: TextAnchor.LowerCenter
    );
    UIFactory.SetLayoutElement(container, flexibleWidth: 1, flexibleHeight: 1);

    var button = UIFactory.CreateButton(container, "SpawnButton", "Spawn", Theme.SelectedColor);
    UIFactory.SetLayoutElement(button.gameObject, 200, 25, 1);

    button.onClick.AddListener(OnSpawnClicked);
    button.interactable = _imageSelector != null && _qualityDropdown.HasValue && _hasAffixes;
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
      GameData.Qualities.All.Select(value =>
        new DropdownOption<Quality>(value.GetNameStr(), value)),
      GameData.Qualities.Highest
    );
    UIFactory.SetLayoutElement(quality, 200, 25, 1);
  }

  private void AddSelectableDropdown() {
    if (_selectables.Count == 0) {
      UIFactory.CreateLabel(Body, "MissingItems", $"No {SelectableName} options are available.",
        TextAnchor.MiddleCenter, Color.red);
      return;
    }

    _imageSelector = new ImageSelector<TName>(Body, _selectables, SelectableName);
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

  private void OnQualityChanged(Quality quality) {
    if (!_hasAffixes) return;
    TogglePowersFromQuality(quality);

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
  /// <param name="quality">The quality that was selected.</param>
  private void TogglePowersFromQuality(Quality quality) {
    var powerSlots = quality.GetPowerSlots();

    for (var i = 0; i < _affixSectionObjects.Length; i++) {
      _affixSectionObjects[i]?.SetActive(i < powerSlots);
    }
  }

  private void OnSpawnClicked() {
    if (_imageSelector == null || !_qualityDropdown.HasValue || !_hasAffixes) {
      UIManager.SendNotification(Level.Error, $"No {SelectableName} is available to spawn.");
      return;
    }

    var quality = _qualityDropdown.Value;
    var powerSlots = quality.GetPowerSlots();
    var name = _imageSelector.Selected.Value;
    var item = CreateEmptyItem(name, quality);

    for (var i = 0; i < powerSlots; i++) {
      var affix = _affixSections[i].BuildAffix();
      item.SetAffix(affix, i);
    }

    SpawnFunction(item);
    UIManager.SendNotification(Level.Success, $"Spawned {SelectableName}!");
  }

  protected override void OnBodyDestroyed() {
    _affixSectionObjects = null;
    _affixSections = null;
    _hasAffixes = false;
    _imageSelector = null;
    _qualityDropdown = null;
    _selectables = null;
  }

  private static bool HasUsableIcon(Sprite icon) {
    return icon && icon.texture;
  }
}
