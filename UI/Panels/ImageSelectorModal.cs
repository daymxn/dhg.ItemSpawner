using System;
using System.Collections.Generic;
using daymxn.DHG.ItemSpawner.ui.Native;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace daymxn.DHG.ItemSpawner.ui.Panels;

/// <summary>
///   An Image that is selectable via a <see cref="ImageSelectorModal" />.
/// </summary>
public readonly struct SelectableImage<T>(Sprite image, T value) {
  public Sprite Image { get; } = image;
  public T Value { get; } = value;
}

/// <summary>
///   Popup modal which presents a grid of clickable images.
/// </summary>
/// <remarks>
///   This modal shares a common instance, so only one can be open at once.
/// </remarks>
/// <seealso cref="ItemSpawner.ui.Widgets.ImageSelector.ImageSelector" />
/// <seealso cref="Open" />
public class ImageSelectorModal(GameObject owner) : GamePanel(owner) {
  private const int ColumnCount = 8;
  private const float CellSize = 50;
  private const float GridSpacing = 4;
  private const float ModalWidth = ColumnCount * CellSize + (ColumnCount - 1) * GridSpacing + 20;
  private const float TitleBarHeight = 30;
  private const float VerticalPadding = 20;

  private static readonly Color SelectedColor = new(45 / 255f, 75 / 255f, 80 / 255f);
  private static readonly Color InactiveColor = new(1f, 1f, 1f);
  private readonly List<GameObject> _buttons = [];

  private GameObject _blocker;
  private GameObject _grid;

  public static ImageSelectorModal Instance =>
    UIManager.GetPanel<ImageSelectorModal>(UIManager.Panels.ImageSelectorModal);

  public override string Name => "Select";
  public override int MinWidth => (int)ModalWidth;
  public override int MinHeight => 200;
  public override int DefaultWidth => (int)ModalWidth;
  public override int DefaultHeight => 600;
  public override Vector2 DefaultAnchorMin => new(0.5f, 0.5f);
  public override Vector2 DefaultAnchorMax => new(0.5f, 0.5f);
  public override bool ShowByDefault => false;

  protected override bool PivotToAnchor => true;

  protected override bool UseScrollView => true;

  public override void SetActive(bool active) {
    base.SetActive(active);
    if (!active && _blocker) _blocker.SetActive(false);
  }

  /// <summary>
  ///   Open the modal with the selected values.
  /// </summary>
  /// <param name="selectedImage">The currently selected image.</param>
  /// <param name="images">A list of selectable images to present.</param>
  /// <param name="onSelect">Callback invoked when the user clicks an image.</param>
  public void Open<T>(
    SelectableImage<T> selectedImage,
    IReadOnlyList<SelectableImage<T>> images,
    Action<SelectableImage<T>> onSelect
  ) {
    UIManager.CloseAllDropdowns();
    ResizeForItemCount(images.Count);
    _blocker.SetActive(true);
    _blocker.transform.SetAsLastSibling();
    SetActive(true);
    UIRoot.transform.SetAsLastSibling();

    Clear();

    foreach (var selectable in images) {
      var button = UIFactory.CreateButton(_grid, $"Button_{selectable.Value}", "");
      button.image.sprite = selectable.Image;
      button.onClick.AddListener(() => {
        onSelect(selectable);
        Close();
      });
      button.colors = UIFactory.ColorBlockFor(
        EqualityComparer<T>.Default.Equals(selectable.Value, selectedImage.Value)
          ? SelectedColor
          : InactiveColor);
      UIFactory.SetLayoutElement(button.gameObject, 50, 50);

      _buttons.Add(button.gameObject);
    }
  }

  private void Close() {
    Clear();
    SetActive(false);
    if (_blocker) _blocker.SetActive(false);
  }

  private void Clear() {
    foreach (var row in _buttons) {
      Object.Destroy(row);
    }

    _buttons.Clear();
  }

  protected override void CreateBodyContent() {
    _blocker = UIFactory.CreateUIObject("ImageSelectorBlocker", Owner);
    UIFactory.Stretch(_blocker.GetComponent<RectTransform>());
    var blockerImage = UIFactory.AddImage(_blocker, new Color(0, 0, 0, 0.4f));
    var blockerButton = _blocker.AddComponent<Button>();
    blockerButton.targetGraphic = blockerImage;
    blockerButton.onClick.AddListener(Close);
    _blocker.SetActive(false);

    _grid = UIFactory.CreateGridGroup(
      Body,
      "Grid",
      new Vector2(CellSize, CellSize),
      new Vector2(GridSpacing, GridSpacing),
      new Color(1, 1, 1, 0)
    );
    _grid.GetComponent<GridLayoutGroup>().constraintCount = ColumnCount;
    UIFactory.SetLayoutElement(_grid, ModalWidth - 20, CellSize);

    SetActive(false);
  }

  protected override void OnDisposing() {
    if (_blocker) Object.Destroy(_blocker);
    _blocker = null;
  }

  private void ResizeForItemCount(int itemCount) {
    var rows = Mathf.Max(1, Mathf.CeilToInt(itemCount / (float)ColumnCount));
    var gridHeight = rows * CellSize + Mathf.Max(0, rows - 1) * GridSpacing;
    var desiredHeight = TitleBarHeight + VerticalPadding + gridHeight;
    var canvasHeight = Owner.GetComponent<RectTransform>().rect.height;
    var maxHeight = canvasHeight > 1 ? Mathf.Max(MinHeight, canvasHeight - 40) : DefaultHeight;

    Rect.sizeDelta = new Vector2(ModalWidth,
      Mathf.Clamp(desiredHeight, MinHeight, maxHeight));
    Rect.anchoredPosition = Vector2.zero;
    ForceRebuildLayout();
  }
}
