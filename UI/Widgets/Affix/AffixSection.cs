using System;
using System.Linq;
using daymxn.DHG.ItemSpawner.game;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;

namespace daymxn.DHG.ItemSpawner.ui.Widgets.Affix;

/// <summary>
///   Drawable section for selecting an Affix.
///   <br /><br />
///   Created with the
///   <see cref="AffixSection(GameObject,Quality,AffixQualityDict)">constructor</see>, you can call
///   <see cref="DrawBody" /> to draw the initial elements. Make sure you call
///   <see cref="OnParentQualityChanged" /> whenever the parent item's quality changes. When you're
///   ready to use the finished product, call <see cref="BuildAffix" /> to get a matching Affix.
/// </summary>
public class AffixSection {
  private readonly GameObject _container;
  private readonly AffixQualityDict _pool;
  private RandomAffix _affix;
  private Dropdown _affixDropdown;
  private bool _deepChaos;
  private Toggle _deeplyChaosToggle;

  private bool _drawn;
  private AttributeData.NumType _numType = AttributeData.NumType.Float;

  private Quality _parentQuality;
  private Quality _quality;

  private Dropdown _qualityDropdown;
  private float _value;
  private Slider _valueSlider;
  private Text _valueText;

  /// <summary>
  ///   Create a new Affix Section.
  /// </summary>
  /// <param name="container">The parent container to attach elements to</param>
  /// <param name="parentQuality">The current quality of the parent element</param>
  /// <param name="pool">The affix pool to pull elements from</param>
  public AffixSection(GameObject container, Quality parentQuality, AffixQualityDict pool) {
    _container = container;
    _parentQuality = parentQuality;
    _pool = pool;
    _quality = parentQuality;
    _affix = pool[_quality].First();
  }

  /// <summary>
  ///   Draws the body content of this section.
  /// </summary>
  /// <remarks>
  ///   Elements will be added to the container property passed into the constructor.
  /// </remarks>
  public void DrawBody() {
    AddQualityDropdown();
    AddAffixDropdown();
    AddValueSlider();
    AddDeeplyChaosToggle();
    _drawn = true;
  }

  /// <summary>
  ///   Callback to invoke when the quality of the relic changes.
  /// </summary>
  /// <remarks>
  ///   Will ensure the quality of the affix is updated accordingly (if needed).
  /// </remarks>
  public void OnParentQualityChanged(Quality newQuality) {
    _parentQuality = newQuality;

    if (_parentQuality < GameData.Qualities.Highest) {
      _deepChaos = false;
      _deeplyChaosToggle.gameObject.SetActive(false);
    } else {
      _deepChaos = _deeplyChaosToggle.isOn;
      _deeplyChaosToggle.gameObject.SetActive(true);
    }

    UpdateValidQualities();
  }

  /// <returns>A new RandomAffix which matches the options selected</returns>
  public RandomAffix BuildAffix() {
    var newAffix = _affix.Cloned();
    newAffix.SetNumAndInit(_value);
    if (_deepChaos) {
      newAffix.DeeplyChaosed();
    }

    return newAffix;
  }

  private void AddQualityDropdown() {
    var layout = UIFactory.CreateUIObject("Container", _container);
    UIFactory.SetLayoutGroup<HorizontalLayoutGroup>(
      layout,
      forceHeight: false,
      forceWidth: false,
      childControlHeight: true,
      childControlWidth: true,
      spacing: 10
    );

    var label = UIFactory.CreateLabel(layout, "Title", "Quality");
    UIFactory.SetLayoutElement(label.gameObject, 75, 25);

    var dropdown = UIFactory.CreateDropdown(
      layout,
      "QualityDropdown",
      out _qualityDropdown,
      "Quality",
      14,
      OnQualityChanged,
      []
    );
    UIFactory.SetLayoutElement(dropdown, 200, 25, 1);

    UpdateValidQualities();
  }

  private void AddAffixDropdown() {
    var layout = UIFactory.CreateUIObject("Container", _container);
    UIFactory.SetLayoutGroup<HorizontalLayoutGroup>(
      layout,
      forceHeight: false,
      forceWidth: false,
      childControlHeight: true,
      childControlWidth: true,
      spacing: 10
    );

    var label = UIFactory.CreateLabel(layout, "Title", "Affix");
    UIFactory.SetLayoutElement(label.gameObject, 75, 25);

    var dropdown = UIFactory.CreateDropdown(
      layout,
      "AffixDropdown",
      out _affixDropdown,
      "Affix",
      14,
      OnAffixChanged,
      []
    );
    UIFactory.SetLayoutElement(dropdown, 400, 25, 1);

    UpdateValidAffixes();
  }

  private void AddValueSlider() {
    var layout = UIFactory.CreateUIObject("Layout", _container);
    UIFactory.SetLayoutGroup<HorizontalLayoutGroup>(
      layout,
      forceHeight: false,
      forceWidth: false,
      childControlHeight: true,
      childControlWidth: true,
      spacing: 10
    );

    _valueText = UIFactory.CreateLabel(layout, "ValueSlider", "0");
    UIFactory.SetLayoutElement(_valueText.gameObject, 75, 25);

    var slider = UIFactory.CreateSlider(layout, "ValueSlider", out _valueSlider);
    UIFactory.SetLayoutElement(slider, 200, 25, 1);

    _valueSlider.onValueChanged.AddListener(newValue => {
      _value = newValue;

      UpdateSliderValueText();
    });

    UpdateSliderValues();
  }

  private void AddDeeplyChaosToggle() {
    var toggle = UIFactory.CreateToggle(
      _container,
      "DeeplyChaosToggle",
      out _deeplyChaosToggle,
      out var text
    );

    text.text = "Deeply Chaos";

    _deeplyChaosToggle.isOn = false;
    _deeplyChaosToggle.onValueChanged.AddListener(newValue => {
      _deepChaos = newValue;
      UpdateSliderValueText();
    });

    UIFactory.SetLayoutElement(toggle, 20, 20);
  }

  private void OnQualityChanged(int qualityIndex) {
    if (!_drawn) return;

    _quality = GameData.Qualities.FromInt(qualityIndex);
    UpdateValidAffixes();
  }

  private void OnAffixChanged(int index) {
    if (!_drawn) return;

    _affix = _pool[_quality][index];

    UpdateSliderValues();
  }

  /// <summary>
  ///   Updates the quality dropdown options, limited to the parent quality.
  /// </summary>
  private void UpdateValidQualities() {
    _qualityDropdown.ClearOptions();

    var validQualities = GameData.Qualities.All.TakeWhile(it => it <= _parentQuality);
    _qualityDropdown.AddOptions(
      validQualities.Select(it => it.GetNameStr()).ToList()
    );
    _quality = _quality > _parentQuality ? _parentQuality : _quality;
    _qualityDropdown.SetValueWithoutNotify(_quality.GetValue());

    if (!_drawn) return;
    UpdateValidAffixes();
  }

  /// <summary>
  ///   Updates the affix dropdown options, limited to affixes that are valid for
  ///   the selected quality.
  /// </summary>
  private void UpdateValidAffixes() {
    _affixDropdown.ClearOptions();
    var affixes = _pool[_quality];
    _affixDropdown.AddOptions(
      affixes
        .Select(it => it.GetNameStr())
        .ToList()
    );

    // replace affix with the one for this quality, or fallback to the first affix if it doesn't exist in this quality
    // some affixes are quality exclusive
    _affix = affixes.FirstOrDefault(it => it.IsSameTypeAffix(_affix)) ?? affixes.First();
    _affixDropdown.SetValueWithoutNotify(affixes.IndexOf(_affix));

    if (!_drawn) return;
    UpdateSliderValues();
  }

  /// <summary>
  ///   Updates the slider values to respect the current affix.
  /// </summary>
  private void UpdateSliderValues() {
    _numType = _affix.IsCompositeAffix()
      ? AttributeData.NumType.Float
      : AttributeData.GetAttributeNumType(_affix.GetAttributeName());

    var max = _affix.GetRollMax();
    var min = _affix.GetRollMin();

    if (min < 1 && _numType == AttributeData.NumType.Float) {
      // hacky workaround for affixes that are setup wrong by the game.
      _numType = AttributeData.NumType.Percent;
    }

    _value = Math.Clamp(_value, min, max);

    _valueSlider.minValue = min;
    _valueSlider.maxValue = max;
    _valueSlider.SetValueWithoutNotify(_value);
    _valueSlider.wholeNumbers = _numType == AttributeData.NumType.Int;

    UpdateSliderValueText();
  }

  /// <summary>
  ///   Updates the slider text according to the type of value the affix represents.
  /// </summary>
  private void UpdateSliderValueText() {
    var adjustedValue = _deepChaos ? _value * 2 : _value;

    _valueText.text = _numType switch {
      AttributeData.NumType.Percent => Globals.GetPercentageStr(adjustedValue),
      AttributeData.NumType.Int => $"{adjustedValue:F0}",
      _ => $"{adjustedValue:F2}"
    };
  }
}
