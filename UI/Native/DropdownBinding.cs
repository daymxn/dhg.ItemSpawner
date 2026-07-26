using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

namespace daymxn.DHG.ItemSpawner.ui.Native;

internal readonly struct DropdownOption<T> {
  internal DropdownOption(string label, T value) {
    Label = label;
    Value = value;
  }

  internal string Label { get; }
  internal T Value { get; }
}

/// <summary>
///   Keeps displayed dropdown indices separate from their underlying game values.
/// </summary>
internal sealed class DropdownBinding<T> {
  private readonly List<T> _values = [];

  internal DropdownBinding(Dropdown dropdown, Action<T> onValueChanged) {
    Component = dropdown;
    if (onValueChanged != null) ValueChanged += onValueChanged;
    Component.onValueChanged.AddListener(OnIndexChanged);
  }

  internal Dropdown Component { get; }

  internal bool HasValue => _values.Count > 0 && Component.value < _values.Count;

  internal T Value {
    get {
      if (!HasValue) throw new InvalidOperationException("The dropdown has no selected value.");
      return _values[Component.value];
    }
  }

  internal event Action<T> ValueChanged;

  internal void SetOptions(IEnumerable<DropdownOption<T>> options, T preferredValue) {
    Component.Hide();
    var materialized = options?.ToList() ?? [];
    _values.Clear();
    _values.AddRange(materialized.Select(option => option.Value));

    Component.ClearOptions();
    Component.AddOptions(materialized.Select(option => option.Label).ToList());
    Component.interactable = _values.Count > 0;

    if (_values.Count == 0) {
      Component.SetValueWithoutNotify(0);
      Component.RefreshShownValue();
      return;
    }

    var selectedIndex = _values.FindIndex(value =>
      EqualityComparer<T>.Default.Equals(value, preferredValue));
    Component.SetValueWithoutNotify(selectedIndex >= 0 ? selectedIndex : 0);
    Component.RefreshShownValue();
  }

  internal void Hide() {
    Component.Hide();
  }

  private void OnIndexChanged(int index) {
    if (index < 0 || index >= _values.Count) return;
    ValueChanged?.Invoke(_values[index]);
  }
}
