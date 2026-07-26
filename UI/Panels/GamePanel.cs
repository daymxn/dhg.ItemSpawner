using System;
using daymxn.DHG.ItemSpawner.game;
using daymxn.DHG.ItemSpawner.ui.Native;
using daymxn.DHG.ItemSpawner.util;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace daymxn.DHG.ItemSpawner.ui.Panels;

/// <summary>
///   Common native-uGUI window lifecycle for item-spawner panels.
/// </summary>
public abstract class GamePanel(GameObject owner) : IDisposable {
  private const float MaximumDefaultHeightRatio = 0.8f;
  private const float MaximumDefaultWidthRatio = 0.9f;
  private const float TitleBarHeight = 30;

  private GameObject _bodyRoot;
  private bool _constructed;
  private bool _disposed;
  private bool _enabled;
  private Vector2 _lastCanvasSize;

  protected GameObject Body;
  protected GameObject ContentRoot;
  protected RectTransform Rect;
  protected GameObject UIRoot;

  public virtual bool IncludeInNavbar => false;
  public virtual bool RequiresGameData => false;
  public virtual bool ShowByDefault => true;
  public virtual bool CanDragAndResize => false;
  public virtual int MinWidth => 250;
  public virtual int MinHeight => 200;
  public virtual int DefaultWidth => MinWidth;
  public virtual int DefaultHeight => MinHeight;
  public virtual Vector2 DefaultAnchorMin => new(0.5f, 0.5f);
  public virtual Vector2 DefaultAnchorMax => DefaultAnchorMin;
  public abstract string Name { get; }

  protected virtual int Spacing => 10;
  protected virtual Padding RootPadding => Padding.Of(10);
  protected virtual bool PivotToAnchor => false;
  protected virtual Vector2 PivotOffset => Vector2.zero;
  protected virtual bool UseScrollView => false;
  protected virtual bool ShowTitleBar => true;

  public bool Enabled {
    get => _enabled;
    private set {
      if (_enabled == value) return;
      _enabled = value;
      if (UIRoot) UIRoot.SetActive(value);
      if (value && Rect) Rect.SetAsLastSibling();
      OnToggleEnabled?.Invoke(value);
    }
  }

  protected GameObject Owner => owner;

  public virtual void Dispose() {
    if (_disposed) return;
    _disposed = true;
    GameData.OnPlayerDataLoaded -= OnPlayerDataLoaded;
    GameData.OnPlayerDataUnloaded -= OnPlayerDataUnloaded;
    OnDisposing();
    if (UIRoot) Object.Destroy(UIRoot);
    UIRoot = null;
    ContentRoot = null;
    Body = null;
    _bodyRoot = null;
  }

  public event Action<bool> OnToggleEnabled;

  internal void ConstructUI() {
    if (_constructed || _disposed) return;
    _constructed = true;
    ConstructWindow();

    if (RequiresGameData) {
      GameData.OnPlayerDataLoaded += OnPlayerDataLoaded;
      GameData.OnPlayerDataUnloaded += OnPlayerDataUnloaded;
      if (GameData.IsPlayerDataLoaded()) {
        ConstructPanelContent();
        SetActive(ShowByDefault);
      } else {
        SetActive(false);
      }
    } else {
      ConstructPanelContent();
      SetActive(ShowByDefault);
    }
  }

  public void Toggle() {
    SetActive(!Enabled);
  }

  public virtual void SetActive(bool active) {
    if (_disposed) return;
    if (!active && UIRoot) UIFactory.CloseDropdowns(UIRoot);
    if (active) ClampToCanvas();
    Enabled = active;
  }

  public virtual void Update() {
  }

  internal void RefreshCanvasLayout() {
    ResizeForCanvasChange();
    ClampToCanvas();
  }

  protected virtual void OnDisposing() {
  }

  protected virtual void ConstructPanelContent() {
    if (_disposed || Body || (RequiresGameData && !GameData.IsPlayerDataLoaded())) return;

    if (UseScrollView) {
      _bodyRoot = UIFactory.CreateScrollView(ContentRoot, "BodyScrollView", out Body);
    } else {
      _bodyRoot = UIFactory.CreateUIObject("BodyRoot", ContentRoot);
      var bodyRootRect = _bodyRoot.GetComponent<RectTransform>();
      UIFactory.Stretch(bodyRootRect);
      Body = UIFactory.CreateUIObject("Body", _bodyRoot);
      UIFactory.Stretch(Body.GetComponent<RectTransform>());
    }

    UIFactory.SetLayoutGroup<VerticalLayoutGroup>(
      Body,
      false,
      false,
      true,
      true,
      Spacing,
      RootPadding.Top,
      RootPadding.Left,
      RootPadding.Right,
      RootPadding.Bottom
    );

    if (UseScrollView) {
      Body.AddComponent<ContentSizeFitter>().verticalFit =
        ContentSizeFitter.FitMode.PreferredSize;
    }

    CreateBodyContent();
    ForceRebuildLayout();
  }

  protected abstract void CreateBodyContent();

  protected void ForceRebuildLayout() {
    if (!Rect) return;
    Canvas.ForceUpdateCanvases();
    if (Body) LayoutRebuilder.ForceRebuildLayoutImmediate(Body.GetComponent<RectTransform>());
    LayoutRebuilder.ForceRebuildLayoutImmediate(Rect);
    ClampToCanvas();
  }

  private void ConstructWindow() {
    UIRoot = UIFactory.CreateUIObject(Name, owner);
    Rect = UIRoot.GetComponent<RectTransform>();
    Rect.anchorMin = DefaultAnchorMin;
    Rect.anchorMax = DefaultAnchorMax;
    Rect.pivot = PivotToAnchor ? DefaultAnchorMin : new Vector2(0.5f, 0.5f);
    Rect.anchoredPosition = PivotOffset;
    var parent = owner.GetComponent<RectTransform>();
    _lastCanvasSize = GetCanvasSize(parent);
    Rect.sizeDelta = CanDragAndResize
      ? GetResponsiveDefaultSize(_lastCanvasSize)
      : new Vector2(DefaultWidth, DefaultHeight);
    UIFactory.AddImage(UIRoot, Theme.WindowColor);

    var focus = UIRoot.AddComponent<NativeWindowFocusHandler>();
    focus.Configure(Rect);

    var contentTop = ShowTitleBar ? -TitleBarHeight : 0;
    ContentRoot = UIFactory.CreateUIObject("Content", UIRoot);
    var contentRect = ContentRoot.GetComponent<RectTransform>();
    UIFactory.Stretch(contentRect);
    contentRect.offsetMax = new Vector2(0, contentTop);
    UIFactory.AddImage(ContentRoot, Theme.PanelColor);

    if (ShowTitleBar) ConstructTitleBar();
    if (CanDragAndResize) ConstructResizeHandle();

    UIRoot.SetActive(false);
    ClampToCanvas();
  }

  private void ConstructTitleBar() {
    var titleBar = UIFactory.CreateUIObject("TitleBar", UIRoot);
    var titleRect = titleBar.GetComponent<RectTransform>();
    titleRect.anchorMin = new Vector2(0, 1);
    titleRect.anchorMax = Vector2.one;
    titleRect.pivot = new Vector2(0.5f, 1);
    titleRect.anchoredPosition = Vector2.zero;
    titleRect.sizeDelta = new Vector2(0, TitleBarHeight);
    UIFactory.AddImage(titleBar, Theme.HeaderColor);

    var title = UIFactory.CreateLabel(titleBar, "Title", Name);
    UIFactory.Stretch(title.rectTransform);
    title.rectTransform.offsetMin = new Vector2(10, 0);
    title.rectTransform.offsetMax = new Vector2(-34, 0);

    var close = UIFactory.CreateButton(titleBar, "CloseButton", "−", Theme.HandleColor);
    var closeRect = close.GetComponent<RectTransform>();
    closeRect.anchorMin = new Vector2(1, 0.5f);
    closeRect.anchorMax = new Vector2(1, 0.5f);
    closeRect.pivot = new Vector2(1, 0.5f);
    closeRect.anchoredPosition = new Vector2(-3, 0);
    closeRect.sizeDelta = new Vector2(25, 24);
    close.onClick.AddListener(() => SetActive(false));

    if (!CanDragAndResize) return;
    var canvas = owner.GetComponentInParent<Canvas>();
    var drag = titleBar.AddComponent<NativeWindowDragHandler>();
    drag.Configure(Rect, owner.GetComponent<RectTransform>(), canvas);
  }

  private void ConstructResizeHandle() {
    var handle = UIFactory.CreateUIObject("ResizeHandle", UIRoot);
    var handleRect = handle.GetComponent<RectTransform>();
    handleRect.anchorMin = new Vector2(1, 0);
    handleRect.anchorMax = new Vector2(1, 0);
    handleRect.pivot = new Vector2(1, 0);
    handleRect.anchoredPosition = Vector2.zero;
    handleRect.sizeDelta = new Vector2(16, 16);
    UIFactory.AddImage(handle, Theme.HandleColor);

    var resize = handle.AddComponent<NativeWindowResizeHandler>();
    resize.Configure(
      Rect,
      owner.GetComponent<RectTransform>(),
      owner.GetComponentInParent<Canvas>(),
      new Vector2(MinWidth, MinHeight)
    );
  }

  private void OnPlayerDataLoaded(object sender, EventArgs args) {
    if (_disposed) return;
    ConstructPanelContent();
    SetActive(ShowByDefault);
  }

  private void OnPlayerDataUnloaded(object sender, EventArgs args) {
    if (_disposed) return;
    SetActive(false);
    if (_bodyRoot) Object.Destroy(_bodyRoot);
    _bodyRoot = null;
    Body = null;
    OnBodyDestroyed();
  }

  protected virtual void OnBodyDestroyed() {
  }

  private void ClampToCanvas() {
    if (!Rect || !owner) return;
    var parent = owner.GetComponent<RectTransform>();
    NativeWindowLayout.ClampSize(Rect, parent, new Vector2(MinWidth, MinHeight));
    NativeWindowLayout.ClampToParent(Rect, parent);
  }

  private void ResizeForCanvasChange() {
    if (!CanDragAndResize || !Rect || !owner) return;

    var parent = owner.GetComponent<RectTransform>();
    var canvasSize = GetCanvasSize(parent);
    if (canvasSize.x <= 1 || canvasSize.y <= 1) return;

    if (_lastCanvasSize is { x: > 1, y: > 1 } &&
        Mathf.Approximately(canvasSize.x, _lastCanvasSize.x) &&
        Mathf.Approximately(canvasSize.y, _lastCanvasSize.y)) {
      return;
    }

    var responsiveMaximum = GetResponsiveDefaultSize(canvasSize);
    Rect.sizeDelta = new Vector2(
      Mathf.Min(Rect.sizeDelta.x, responsiveMaximum.x),
      Mathf.Min(Rect.sizeDelta.y, responsiveMaximum.y)
    );
    _lastCanvasSize = canvasSize;
  }

  private Vector2 GetResponsiveDefaultSize(Vector2 canvasSize) {
    if (canvasSize.x <= 1 || canvasSize.y <= 1) {
      return new Vector2(DefaultWidth, DefaultHeight);
    }

    var maximumWidth = canvasSize.x * MaximumDefaultWidthRatio;
    var maximumHeight = canvasSize.y * MaximumDefaultHeightRatio;
    return new Vector2(
      Mathf.Min(DefaultWidth, maximumWidth),
      Mathf.Min(DefaultHeight, maximumHeight)
    );
  }

  private static Vector2 GetCanvasSize(RectTransform canvas) {
    var canvasSize = canvas ? canvas.rect.size : Vector2.zero;
    return canvasSize is { x: > 1, y: > 1 } ? canvasSize : new Vector2(Screen.width, Screen.height);
  }
}
