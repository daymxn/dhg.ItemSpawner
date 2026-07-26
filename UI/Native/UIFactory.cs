using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace daymxn.DHG.ItemSpawner.ui.Native;

/// <summary>
///   Creates the small set of native uGUI controls used by the item spawners.
/// </summary>
internal static class UIFactory {
  private const int DefaultFontSize = 14;
  private static Font _defaultFont;
  private static Sprite _whiteSprite;

  private static Font DefaultFont {
    get {
      if (_defaultFont) return _defaultFont;

      _defaultFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
      if (_defaultFont) return _defaultFont;

      foreach (var font in Resources.FindObjectsOfTypeAll<Font>()) {
        if (!font) continue;
        _defaultFont = font;
        Plugin.Logger?.LogWarning($"Built-in Arial was unavailable; using font '{font.name}'.");
        break;
      }

      return _defaultFont;
    }
  }

  private static Sprite WhiteSprite {
    get {
      if (_whiteSprite) return _whiteSprite;
      var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false) {
        name = "ItemSpawner.WhiteTexture",
        hideFlags = HideFlags.HideAndDontSave
      };
      texture.SetPixel(0, 0, Color.white);
      texture.Apply();
      _whiteSprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
      _whiteSprite.name = "ItemSpawner.WhiteSprite";
      _whiteSprite.hideFlags = HideFlags.HideAndDontSave;
      return _whiteSprite;
    }
  }

  internal static GameObject CreateCanvasRoot(string name) {
    var root = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler),
      typeof(GraphicRaycaster));
    var uiLayer = LayerMask.NameToLayer("UI");
    if (uiLayer >= 0) root.layer = uiLayer;

    var canvas = root.GetComponent<Canvas>();
    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
    canvas.overrideSorting = true;
    canvas.sortingOrder = 32_760;

    var scaler = root.GetComponent<CanvasScaler>();
    // UniverseLib used pixel-sized windows. ConstantPixelSize preserves those
    // dimensions instead of enlarging every control on resolutions above 1080p.
    scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
    scaler.scaleFactor = 1;
    scaler.referencePixelsPerUnit = 100;

    var rect = root.GetComponent<RectTransform>();
    Stretch(rect);
    Object.DontDestroyOnLoad(root);
    return root;
  }

  internal static GameObject CreateUIObject(string name, GameObject parent) {
    var child = new GameObject(name, typeof(RectTransform)) {
      layer = parent.layer
    };
    child.transform.SetParent(parent.transform, false);
    return child;
  }

  internal static Image AddImage(GameObject target, Color color, bool raycastTarget = true) {
    var image = target.GetComponent<Image>() ?? target.AddComponent<Image>();
    image.sprite = WhiteSprite;
    image.type = Image.Type.Simple;
    image.color = color;
    image.raycastTarget = raycastTarget;
    return image;
  }

  internal static Text CreateLabel(
    GameObject parent,
    string name,
    string text,
    TextAnchor alignment = TextAnchor.MiddleLeft,
    Color? color = null,
    int fontSize = DefaultFontSize
  ) {
    var root = CreateUIObject(name, parent);
    var label = root.AddComponent<Text>();
    label.font = DefaultFont;
    label.fontSize = fontSize;
    label.text = text;
    label.alignment = alignment;
    label.color = color ?? Theme.TextColor;
    label.raycastTarget = false;
    label.horizontalOverflow = HorizontalWrapMode.Wrap;
    label.verticalOverflow = VerticalWrapMode.Truncate;
    return label;
  }

  internal static Button CreateButton(
    GameObject parent,
    string name,
    string text,
    Color? color = null
  ) {
    var root = CreateUIObject(name, parent);
    var image = AddImage(root, Color.white);
    var button = root.AddComponent<Button>();
    button.targetGraphic = image;
    button.colors = ColorBlockFor(color ?? Theme.ControlColor);

    var label = CreateLabel(root, "Label", text, TextAnchor.MiddleCenter);
    Stretch(label.rectTransform);
    return button;
  }

  private static GameObject CreateDropdown(
    GameObject parent,
    string name,
    out Dropdown dropdown,
    string placeholder,
    int fontSize,
    Action<int> onChanged,
    IEnumerable<string> options
  ) {
    var root = CreateUIObject(name, parent);
    var rootImage = AddImage(root, Color.white);
    dropdown = root.AddComponent<Dropdown>();
    dropdown.targetGraphic = rootImage;
    dropdown.colors = ColorBlockFor(Theme.ControlColor);

    var caption = CreateLabel(root, "Label", placeholder, TextAnchor.MiddleLeft,
      Theme.TextColor, fontSize);
    caption.rectTransform.anchorMin = Vector2.zero;
    caption.rectTransform.anchorMax = Vector2.one;
    caption.rectTransform.offsetMin = new Vector2(10, 2);
    caption.rectTransform.offsetMax = new Vector2(-28, -2);

    var arrow = CreateLabel(root, "Arrow", "▼", TextAnchor.MiddleCenter,
      Theme.TextColor, fontSize);
    arrow.rectTransform.anchorMin = new Vector2(1, 0);
    arrow.rectTransform.anchorMax = Vector2.one;
    arrow.rectTransform.pivot = new Vector2(1, 0.5f);
    arrow.rectTransform.sizeDelta = new Vector2(24, 0);
    arrow.rectTransform.anchoredPosition = Vector2.zero;

    var template = CreateUIObject("Template", root);
    var templateRect = template.GetComponent<RectTransform>();
    templateRect.anchorMin = new Vector2(0, 0);
    templateRect.anchorMax = new Vector2(1, 0);
    templateRect.pivot = new Vector2(0.5f, 1);
    templateRect.anchoredPosition = new Vector2(0, -2);
    templateRect.sizeDelta = new Vector2(0, 180);
    AddImage(template, Theme.PanelColor);

    var viewport = CreateUIObject("Viewport", template);
    var viewportRect = viewport.GetComponent<RectTransform>();
    Stretch(viewportRect);
    viewportRect.offsetMax = new Vector2(-14, 0);
    AddImage(viewport, Theme.PanelColor);
    viewport.AddComponent<Mask>().showMaskGraphic = false;

    var content = CreateUIObject("Content", viewport);
    var contentRect = content.GetComponent<RectTransform>();
    contentRect.anchorMin = new Vector2(0, 1);
    contentRect.anchorMax = new Vector2(1, 1);
    contentRect.pivot = new Vector2(0.5f, 1);
    contentRect.anchoredPosition = Vector2.zero;
    contentRect.sizeDelta = Vector2.zero;
    var contentLayout = content.AddComponent<VerticalLayoutGroup>();
    contentLayout.childControlHeight = true;
    contentLayout.childControlWidth = true;
    contentLayout.childForceExpandHeight = false;
    contentLayout.childForceExpandWidth = true;
    contentLayout.spacing = 1;
    content.AddComponent<ContentSizeFitter>().verticalFit =
      ContentSizeFitter.FitMode.PreferredSize;

    var item = CreateUIObject("Item", content);
    AddImage(item, Theme.ControlColor);
    SetLayoutElement(item, minHeight: 26, flexibleWidth: 1);
    var itemToggle = item.AddComponent<Toggle>();
    itemToggle.targetGraphic = item.GetComponent<Image>();

    var checkmarkRoot = CreateUIObject("Item Checkmark", item);
    var checkmarkRect = checkmarkRoot.GetComponent<RectTransform>();
    checkmarkRect.anchorMin = new Vector2(0, 0.5f);
    checkmarkRect.anchorMax = new Vector2(0, 0.5f);
    checkmarkRect.pivot = new Vector2(0, 0.5f);
    checkmarkRect.anchoredPosition = new Vector2(7, 0);
    checkmarkRect.sizeDelta = new Vector2(8, 8);
    var checkmark = AddImage(checkmarkRoot, Theme.SelectedColor, false);
    itemToggle.graphic = checkmark;

    var itemLabel = CreateLabel(item, "Item Label", "Option", TextAnchor.MiddleLeft,
      Theme.TextColor, fontSize);
    Stretch(itemLabel.rectTransform);
    itemLabel.rectTransform.offsetMin = new Vector2(22, 1);
    itemLabel.rectTransform.offsetMax = new Vector2(-6, -1);

    var scrollbarRoot = CreateUIObject("Scrollbar", template);
    var scrollbarRect = scrollbarRoot.GetComponent<RectTransform>();
    scrollbarRect.anchorMin = new Vector2(1, 0);
    scrollbarRect.anchorMax = Vector2.one;
    scrollbarRect.pivot = Vector2.one;
    scrollbarRect.sizeDelta = new Vector2(14, 0);
    scrollbarRect.anchoredPosition = Vector2.zero;
    AddImage(scrollbarRoot, Theme.ControlColor);

    var slidingArea = CreateUIObject("Sliding Area", scrollbarRoot);
    Stretch(slidingArea.GetComponent<RectTransform>());
    var handleRoot = CreateUIObject("Handle", slidingArea);
    Stretch(handleRoot.GetComponent<RectTransform>());
    var handleImage = AddImage(handleRoot, Color.white);

    var scrollbar = scrollbarRoot.AddComponent<Scrollbar>();
    scrollbar.handleRect = handleRoot.GetComponent<RectTransform>();
    scrollbar.targetGraphic = handleImage;
    scrollbar.colors = ColorBlockFor(Theme.HandleColor);
    scrollbar.direction = Scrollbar.Direction.BottomToTop;

    var scrollRect = template.AddComponent<ScrollRect>();
    scrollRect.content = contentRect;
    scrollRect.viewport = viewportRect;
    scrollRect.horizontal = false;
    scrollRect.vertical = true;
    scrollRect.movementType = ScrollRect.MovementType.Clamped;
    scrollRect.verticalScrollbar = scrollbar;
    scrollRect.verticalScrollbarVisibility =
      ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;
    scrollRect.scrollSensitivity = 24;

    dropdown.template = templateRect;
    dropdown.captionText = caption;
    dropdown.itemText = itemLabel;
    dropdown.options.Clear();
    if (options != null) {
      dropdown.AddOptions([..options]);
    }

    dropdown.onValueChanged.AddListener(value => onChanged?.Invoke(value));
    dropdown.RefreshShownValue();

    var overlay = root.AddComponent<NativeDropdownOverlay>();
    var rootCanvas = root.GetComponentInParent<Canvas>()?.rootCanvas;
    overlay.Configure(dropdown, rootCanvas
      ? rootCanvas.GetComponent<RectTransform>()
      : parent.GetComponentInParent<Canvas>()?.GetComponent<RectTransform>());

    template.SetActive(false);
    return root;
  }

  internal static GameObject CreateDropdown<T>(
    GameObject parent,
    string name,
    out DropdownBinding<T> binding,
    string placeholder,
    int fontSize,
    Action<T> onChanged,
    IEnumerable<DropdownOption<T>> options,
    T selected
  ) {
    var root = CreateDropdown(
      parent,
      name,
      out var dropdown,
      placeholder,
      fontSize,
      null,
      []
    );
    binding = new DropdownBinding<T>(dropdown, onChanged);
    binding.SetOptions(options, selected);
    return root;
  }

  internal static GameObject CreateSlider(GameObject parent, string name, out Slider slider) {
    var root = CreateUIObject(name, parent);
    slider = root.AddComponent<Slider>();

    var background = CreateUIObject("Background", root);
    var backgroundRect = background.GetComponent<RectTransform>();
    backgroundRect.anchorMin = new Vector2(0, 0.5f);
    backgroundRect.anchorMax = new Vector2(1, 0.5f);
    backgroundRect.sizeDelta = new Vector2(0, 4);
    AddImage(background, Theme.SliderColor);

    var fillArea = CreateUIObject("Fill Area", root);
    var fillAreaRect = fillArea.GetComponent<RectTransform>();
    fillAreaRect.anchorMin = new Vector2(0, 0.5f);
    fillAreaRect.anchorMax = new Vector2(1, 0.5f);
    fillAreaRect.pivot = new Vector2(0.5f, 0.5f);
    fillAreaRect.anchoredPosition = Vector2.zero;
    fillAreaRect.sizeDelta = new Vector2(0, 4);

    var fill = CreateUIObject("Fill", fillArea);
    var fillRect = fill.GetComponent<RectTransform>();
    Stretch(fillRect);
    var fillImage = AddImage(fill, Theme.SelectedColor, false);

    var handleArea = CreateUIObject("Handle Slide Area", root);
    var handleAreaRect = handleArea.GetComponent<RectTransform>();
    Stretch(handleAreaRect);

    var handle = CreateUIObject("Handle", handleArea);
    var handleRect = handle.GetComponent<RectTransform>();
    handleRect.sizeDelta = new Vector2(10, 16);
    var handleImage = AddImage(handle, Color.white);

    slider.fillRect = fillRect;
    slider.handleRect = handleRect;
    slider.targetGraphic = handleImage;
    slider.colors = ColorBlockFor(Theme.HandleColor);
    slider.direction = Slider.Direction.LeftToRight;
    return root;
  }

  internal static GameObject CreateToggle(
    GameObject parent,
    string name,
    out Toggle toggle,
    out Text text
  ) {
    var root = CreateUIObject(name, parent);
    AddImage(root, Color.clear);
    var layout = root.AddComponent<HorizontalLayoutGroup>();
    layout.childAlignment = TextAnchor.MiddleLeft;
    layout.childControlHeight = true;
    layout.childControlWidth = true;
    layout.childForceExpandHeight = false;
    layout.childForceExpandWidth = false;
    layout.spacing = 6;

    var box = CreateUIObject("Background", root);
    var boxImage = AddImage(box, Color.white);
    SetLayoutElement(box, 20, 20);

    var check = CreateUIObject("Checkmark", box);
    var checkRect = check.GetComponent<RectTransform>();
    Stretch(checkRect);
    checkRect.offsetMin = new Vector2(4, 4);
    checkRect.offsetMax = new Vector2(-4, -4);
    var checkImage = AddImage(check, Theme.SelectedColor, false);

    toggle = root.AddComponent<Toggle>();
    toggle.targetGraphic = boxImage;
    toggle.graphic = checkImage;
    toggle.colors = ColorBlockFor(Theme.ControlColor);

    text = CreateLabel(root, "Label", string.Empty);
    SetLayoutElement(text.gameObject, minHeight: 20, flexibleWidth: 1);
    return root;
  }

  internal static GameObject CreateVerticalGroup(
    GameObject parent,
    string name,
    bool forceHeight,
    bool forceWidth,
    bool childControlHeight,
    bool childControlWidth,
    float spacing,
    Vector4 padding
  ) {
    var root = CreateUIObject(name, parent);
    AddImage(root, Theme.PanelColor, false);
    SetLayoutGroup<VerticalLayoutGroup>(
      root,
      forceHeight,
      forceWidth,
      childControlHeight,
      childControlWidth,
      spacing,
      (int)padding.x,
      (int)padding.y,
      (int)padding.z,
      (int)padding.w
    );
    root.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    return root;
  }

  internal static GameObject CreateGridGroup(
    GameObject parent,
    string name,
    Vector2 cellSize,
    Vector2 spacing,
    Color color
  ) {
    var root = CreateUIObject(name, parent);
    AddImage(root, color, false);
    var grid = root.AddComponent<GridLayoutGroup>();
    grid.cellSize = cellSize;
    grid.spacing = spacing;
    grid.childAlignment = TextAnchor.UpperLeft;
    grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
    grid.constraintCount = 8;
    var fitter = root.AddComponent<ContentSizeFitter>();
    fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    return root;
  }

  internal static GameObject CreateScrollView(GameObject parent, string name,
    out GameObject content) {
    var root = CreateUIObject(name, parent);
    Stretch(root.GetComponent<RectTransform>());

    var viewport = CreateUIObject("Viewport", root);
    var viewportRect = viewport.GetComponent<RectTransform>();
    Stretch(viewportRect);
    AddImage(viewport, new Color(0, 0, 0, 0.01f));
    viewport.AddComponent<RectMask2D>();

    content = CreateUIObject("Content", viewport);
    var contentRect = content.GetComponent<RectTransform>();
    contentRect.anchorMin = new Vector2(0, 1);
    contentRect.anchorMax = new Vector2(1, 1);
    contentRect.pivot = new Vector2(0.5f, 1);
    contentRect.anchoredPosition = Vector2.zero;
    contentRect.sizeDelta = Vector2.zero;

    var scroll = root.AddComponent<ScrollRect>();
    scroll.content = contentRect;
    scroll.viewport = viewportRect;
    scroll.horizontal = false;
    scroll.vertical = true;
    scroll.movementType = ScrollRect.MovementType.Clamped;
    scroll.scrollSensitivity = 30;
    return root;
  }

  internal static T SetLayoutGroup<T>(
    GameObject target,
    bool forceHeight = false,
    bool forceWidth = false,
    bool childControlHeight = false,
    bool childControlWidth = false,
    float spacing = 0,
    int padTop = 0,
    int padLeft = 0,
    int padRight = 0,
    int padBottom = 0,
    TextAnchor childAlignment = TextAnchor.UpperLeft
  ) where T : HorizontalOrVerticalLayoutGroup {
    var layout = target.GetComponent<T>() ?? target.AddComponent<T>();
    layout.childForceExpandHeight = forceHeight;
    layout.childForceExpandWidth = forceWidth;
    layout.childControlHeight = childControlHeight;
    layout.childControlWidth = childControlWidth;
    layout.spacing = spacing;
    layout.padding = new RectOffset(padLeft, padRight, padTop, padBottom);
    layout.childAlignment = childAlignment;
    return layout;
  }

  internal static LayoutElement SetLayoutElement(
    GameObject target,
    float minWidth = 0,
    float minHeight = 0,
    float flexibleWidth = 0,
    float flexibleHeight = 0
  ) {
    var layout = target.GetComponent<LayoutElement>() ?? target.AddComponent<LayoutElement>();
    if (minWidth > 0) {
      layout.minWidth = minWidth;
      layout.preferredWidth = minWidth;
    }

    if (minHeight > 0) {
      layout.minHeight = minHeight;
      layout.preferredHeight = minHeight;
    }

    layout.flexibleWidth = flexibleWidth;
    layout.flexibleHeight = flexibleHeight;
    return layout;
  }

  internal static void CloseDropdowns(GameObject root) {
    if (!root) return;
    foreach (var dropdown in root.GetComponentsInChildren<Dropdown>(true)) {
      dropdown.Hide();
    }
  }

  internal static ColorBlock ColorBlockFor(Color baseColor) {
    return new ColorBlock {
      normalColor = baseColor,
      highlightedColor = baseColor * 1.2f,
      pressedColor = baseColor * 0.75f,
      selectedColor = baseColor * 1.1f,
      disabledColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0.45f),
      colorMultiplier = 1,
      fadeDuration = 0.08f
    };
  }

  internal static void Stretch(RectTransform rect) {
    rect.anchorMin = Vector2.zero;
    rect.anchorMax = Vector2.one;
    rect.pivot = new Vector2(0.5f, 0.5f);
    rect.anchoredPosition = Vector2.zero;
    rect.sizeDelta = Vector2.zero;
  }
}
