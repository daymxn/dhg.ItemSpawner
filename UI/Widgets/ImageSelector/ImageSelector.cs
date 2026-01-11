using System.Collections.Generic;
using daymxn.DHG.ItemSpawner.ui.Panels;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Models;

namespace daymxn.DHG.ItemSpawner.ui.Widgets.ImageSelector;

/// <summary>
///   Helper element for opening an <see cref="ImageSelectorModal" />.
/// </summary>
public class ImageSelector {
  private readonly ButtonRef _button;

  /// <summary>
  ///   The currently selected image.
  /// </summary>
  public SelectableImage Selected;

  /// <summary>
  ///   Creates a UI element for opening an <see cref="ImageSelectorModal" />.
  /// </summary>
  /// <param name="parent">The UI object to attach this element to.</param>
  /// <param name="selectableImages">A list of selectable images to show.</param>
  /// <param name="text">Text to show next to the element, articulating what the selectable is for.</param>
  public ImageSelector(GameObject parent, List<SelectableImage> selectableImages, string text) {
    Selected = selectableImages[0];

    var container = UIFactory.CreateUIObject("ImageSelectorContainer", parent);
    UIFactory.SetLayoutGroup<VerticalLayoutGroup>(
      container,
      forceHeight: false,
      forceWidth: false,
      childControlHeight: true,
      childControlWidth: true,
      spacing: 10
    );

    var label = UIFactory.CreateLabel(container, "ImageSelectorTitle", text);
    UIFactory.SetLayoutElement(label.gameObject, 75, 25);

    _button = UIFactory.CreateButton(container, "OpenImageSelectorButton", "");
    _button.Component.GetComponent<Image>().sprite = Selected.Image;
    _button.OnClick += () => {
      ImageSelectorModal.Instance.Open(Selected, selectableImages, OnImageSelected);
    };

    UIFactory.SetLayoutElement(_button.GameObject, 50, 50);
  }

  private void OnImageSelected(SelectableImage selectable) {
    Selected = selectable;
    _button.Component.image.sprite = Selected.Image;
  }
}
