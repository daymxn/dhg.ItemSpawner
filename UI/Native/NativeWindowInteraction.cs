using UnityEngine;
using UnityEngine.EventSystems;

namespace daymxn.DHG.ItemSpawner.ui.Native;

internal static class NativeWindowLayout {
  internal static void ClampSize(RectTransform window, RectTransform parent, Vector2 minimum) {
    var parentSize = parent.rect.size;
    if (parentSize.x <= 1 || parentSize.y <= 1) return;
    var maxWidth = Mathf.Max(1, parentSize.x);
    var maxHeight = Mathf.Max(1, parentSize.y);
    var minWidth = Mathf.Min(minimum.x, maxWidth);
    var minHeight = Mathf.Min(minimum.y, maxHeight);
    window.sizeDelta = new Vector2(
      Mathf.Clamp(window.sizeDelta.x, minWidth, maxWidth),
      Mathf.Clamp(window.sizeDelta.y, minHeight, maxHeight)
    );
  }

  internal static void ClampToParent(RectTransform window, RectTransform parent) {
    var parentSize = parent.rect.size;
    if (parentSize.x <= 1 || parentSize.y <= 1) return;
    var windowSize = window.rect.size;
    var anchor = window.anchorMin;
    var pivot = window.pivot;

    var minX = -anchor.x * parentSize.x + pivot.x * windowSize.x;
    var maxX = (1 - anchor.x) * parentSize.x - (1 - pivot.x) * windowSize.x;
    var minY = -anchor.y * parentSize.y + pivot.y * windowSize.y;
    var maxY = (1 - anchor.y) * parentSize.y - (1 - pivot.y) * windowSize.y;

    var position = window.anchoredPosition;
    position.x = minX <= maxX ? Mathf.Clamp(position.x, minX, maxX) : (minX + maxX) * 0.5f;
    position.y = minY <= maxY ? Mathf.Clamp(position.y, minY, maxY) : (minY + maxY) * 0.5f;
    window.anchoredPosition = position;
  }
}

internal sealed class NativeWindowFocusHandler : MonoBehaviour, IPointerDownHandler {
  private RectTransform _window;

  internal void Configure(RectTransform window) {
    _window = window;
  }

  public void OnPointerDown(PointerEventData eventData) {
    if (_window) _window.SetAsLastSibling();
  }
}

internal sealed class NativeWindowDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler,
  IEndDragHandler {
  private Canvas _canvas;
  private RectTransform _parent;
  private RectTransform _window;

  internal void Configure(RectTransform window, RectTransform parent, Canvas canvas) {
    _window = window;
    _parent = parent;
    _canvas = canvas;
  }

  public void OnBeginDrag(PointerEventData eventData) {
    if (_window) _window.SetAsLastSibling();
  }

  public void OnDrag(PointerEventData eventData) {
    if (!_window || !_parent || !_canvas) return;
    _window.anchoredPosition += eventData.delta / Mathf.Max(0.01f, _canvas.scaleFactor);
    NativeWindowLayout.ClampToParent(_window, _parent);
  }

  public void OnEndDrag(PointerEventData eventData) {
    if (_window && _parent) NativeWindowLayout.ClampToParent(_window, _parent);
  }
}

internal sealed class NativeWindowResizeHandler : MonoBehaviour, IBeginDragHandler, IDragHandler,
  IEndDragHandler {
  private Canvas _canvas;
  private Vector2 _minimum;
  private RectTransform _parent;
  private RectTransform _window;

  internal void Configure(
    RectTransform window,
    RectTransform parent,
    Canvas canvas,
    Vector2 minimum
  ) {
    _window = window;
    _parent = parent;
    _canvas = canvas;
    _minimum = minimum;
  }

  public void OnBeginDrag(PointerEventData eventData) {
    if (_window) _window.SetAsLastSibling();
  }

  public void OnDrag(PointerEventData eventData) {
    if (!_window || !_parent || !_canvas) return;
    var delta = eventData.delta / Mathf.Max(0.01f, _canvas.scaleFactor);
    var oldSize = _window.sizeDelta;
    _window.sizeDelta += new Vector2(delta.x, -delta.y);
    NativeWindowLayout.ClampSize(_window, _parent, _minimum);

    var actualDelta = _window.sizeDelta - oldSize;
    _window.anchoredPosition += new Vector2(
      _window.pivot.x * actualDelta.x,
      -(1 - _window.pivot.y) * actualDelta.y
    );
    NativeWindowLayout.ClampToParent(_window, _parent);
  }

  public void OnEndDrag(PointerEventData eventData) {
    if (!_window || !_parent) return;
    NativeWindowLayout.ClampSize(_window, _parent, _minimum);
    NativeWindowLayout.ClampToParent(_window, _parent);
  }
}
