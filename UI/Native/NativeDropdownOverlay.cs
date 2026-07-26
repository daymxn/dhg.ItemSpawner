using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace daymxn.DHG.ItemSpawner.ui.Native;

/// <summary>
///   Moves Unity's generated dropdown list out of scroll views/masks while it's open.
///   Dropdown still owns and destroys the generated list normally.
/// </summary>
internal sealed class NativeDropdownOverlay : MonoBehaviour {
  private RectTransform _blocker;
  private RectTransform _canvasRoot;
  private Dropdown _dropdown;
  private RectTransform _popup;

  private void LateUpdate() {
    if (!_dropdown || !_canvasRoot) {
      _popup = null;
      return;
    }

    if (!_popup) {
      _popup = FindGeneratedPopup();
      if (!_popup) {
        _blocker = null;
        return;
      }

      _popup.SetParent(_canvasRoot, true);
      PromoteToTopmostCanvas(_popup.gameObject);
    }

    if (!_blocker) {
      _blocker = FindGeneratedBlocker();
      if (_blocker) PromoteCloseBlocker(_blocker.gameObject);
    }

    // windows may change sibling order when focused. keep the active popup above
    // every window for as long as the dropdown owns it.
    _popup.SetAsLastSibling();
  }

  internal void Configure(Dropdown dropdown, RectTransform canvasRoot) {
    _dropdown = dropdown;
    _canvasRoot = canvasRoot;
  }

  private RectTransform FindGeneratedPopup() {
    return (from Transform child in transform
      where child.name.StartsWith("Dropdown List")
      select child as RectTransform).FirstOrDefault();
  }

  private RectTransform FindGeneratedBlocker() {
    return (from Transform child in _canvasRoot
      where child.name == "Blocker" && child.gameObject.activeInHierarchy
      select child as RectTransform).FirstOrDefault();
  }

  private static void PromoteToTopmostCanvas(GameObject popup) {
    var popupCanvas = popup.GetComponent<Canvas>() ?? popup.AddComponent<Canvas>();
    popupCanvas.overrideSorting = true;
    popupCanvas.sortingOrder = short.MaxValue;

    if (!popup.GetComponent<GraphicRaycaster>()) {
      popup.AddComponent<GraphicRaycaster>();
    }
  }

  private static void PromoteCloseBlocker(GameObject blocker) {
    var blockerCanvas = blocker.GetComponent<Canvas>() ?? blocker.AddComponent<Canvas>();
    blockerCanvas.overrideSorting = true;
    blockerCanvas.sortingOrder = short.MaxValue - 1;

    if (!blocker.GetComponent<GraphicRaycaster>()) {
      blocker.AddComponent<GraphicRaycaster>();
    }
  }
}
