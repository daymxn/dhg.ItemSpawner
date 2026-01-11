using System;
using System.Collections.Generic;
using UnityEngine;
using UniverseLib;
using UniverseLib.UI;
using Object = UnityEngine.Object;

namespace daymxn.DHG.ItemSpawner.ui.Panels;

/// <summary>
///   An Image that is selectable via a <see cref="ImageSelectorModal" />.
/// </summary>
/// <param name="Image">The sprite image to display.</param>
/// <param name="Value">The underlying value (typically an index).</param>
public record struct SelectableImage(Sprite Image, int Value);

/// <summary>
///   Popup modal which presents a grid of clickable images.
/// </summary>
/// <remarks>
///   This modal shares a common instance, so only one can be open at once.
/// </remarks>
/// <seealso cref="ItemSpawner.ui.Widgets.ImageSelector.ImageSelector" />
/// <seealso cref="Open" />
public class ImageSelectorModal(UIBase owner) : GamePanel(owner) {
  private static readonly Color SelectedColor = new(45 / 255f, 75 / 255f, 80 / 255f);
  private static readonly Color InactiveColor = new(1f, 1f, 1f);
  private readonly List<GameObject> _buttons = [];

  private GameObject _grid;

  public static ImageSelectorModal Instance =>
    UIManager.GetPanel<ImageSelectorModal>(UIManager.Panels.ImageSelectorModal);

  public override string Name => "Select";
  public override int MinWidth => 500;
  public override int MinHeight => 500;
  public override Vector2 DefaultAnchorMin => new(0.5f, 0.5f);
  public override Vector2 DefaultAnchorMax => new(0.5f, 0.5f);
  public override bool ShowByDefault => false;

  protected override bool PivotToAnchor => true;

  protected override bool IncludeBodyFitter => true;

  /// <summary>
  ///   Open the modal with the selected values.
  /// </summary>
  /// <param name="selectedImage">The currently selected image.</param>
  /// <param name="images">A list of selectable images to present.</param>
  /// <param name="onSelect">Callback invoked when the user clicks an image.</param>
  public void Open(SelectableImage selectedImage, List<SelectableImage> images,
    Action<SelectableImage> onSelect) {
    UIRoot.SetActive(true);
    UIRoot.transform.SetAsLastSibling();

    Clear();

    foreach (var selectable in images) {
      var button = UIFactory.CreateButton(_grid, $"Button_{selectable.Value}", "");
      button.Component.image.sprite = selectable.Image;
      button.OnClick += () => {
        onSelect(selectable);
        Close();
      };
      RuntimeHelper.SetColorBlock(button.Component,
        selectable == selectedImage ? SelectedColor : InactiveColor);
      UIFactory.SetLayoutElement(button.GameObject, 50, 50);

      _buttons.Add(button.GameObject);
    }
  }

  private void Close() {
    Clear();
    UIRoot.SetActive(false);
  }

  private void Clear() {
    foreach (var row in _buttons) {
      Object.Destroy(row);
    }

    _buttons.Clear();
  }

  protected override void CreateBodyContent() {
    _grid = UIFactory.CreateGridGroup(
      Body,
      "Grid",
      new Vector2(50, 50),
      new Vector2(2, 2),
      new Color(1, 1, 1, 0)
    );
    UIFactory.SetLayoutElement(_grid, 580, 25, 0);

    UIRoot.SetActive(false);
  }
}
