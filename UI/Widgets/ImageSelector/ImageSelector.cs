using System;
using System.Collections.Generic;
using daymxn.DHG.ItemSpawner.ui.Native;
using daymxn.DHG.ItemSpawner.ui.Panels;
using UnityEngine;
using UnityEngine.UI;

namespace daymxn.DHG.ItemSpawner.ui.Widgets.ImageSelector;

/// <summary>
///   Helper element for opening an <see cref="ImageSelectorModal" />.
/// </summary>
public class ImageSelector<T> {
  private readonly Button _button;

  /// <summary>
  ///   The currently selected image.
  /// </summary>
  public SelectableImage<T> Selected;

  /// <summary>
  ///   Creates a UI element for opening an <see cref="ImageSelectorModal" />.
  /// </summary>
  /// <param name="parent">The UI object to attach this element to.</param>
  /// <param name="selectableImages">A list of selectable images to show.</param>
  /// <param name="text">Text to show next to the element, articulating what the selectable is for.</param>
  public ImageSelector(
    GameObject parent,
    IReadOnlyList<SelectableImage<T>> selectableImages,
    string text
  ) {
    if (selectableImages == null || selectableImages.Count == 0) {
      throw new ArgumentException("At least one selectable image is required.",
        nameof(selectableImages));
    }

    var selectableImages1 = selectableImages;
    Selected = selectableImages[0];

    var container = UIFactory.CreateUIObject("ImageSelectorContainer", parent);
    UIFactory.SetLayoutGroup<VerticalLayoutGroup>(
      container,
      false,
      false,
      true,
      true,
      10
    );

    var label = UIFactory.CreateLabel(container, "ImageSelectorTitle", text);
    UIFactory.SetLayoutElement(label.gameObject, 75, 25);

    _button = UIFactory.CreateButton(container, "OpenImageSelectorButton", "");
    _button.image.sprite = Selected.Image;
    _button.image.color = Color.white;
    _button.colors = UIFactory.ColorBlockFor(Color.white);
    _button.onClick.AddListener(() => {
      ImageSelectorModal.Instance.Open(Selected, selectableImages1, OnImageSelected);
    });

    UIFactory.SetLayoutElement(_button.gameObject, 50, 50);
  }

  private void OnImageSelected(SelectableImage<T> selectable) {
    Selected = selectable;
    _button.image.sprite = Selected.Image;
  }
}
