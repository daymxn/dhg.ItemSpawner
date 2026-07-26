using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace daymxn.DHG.ItemSpawner.ui.Native;

/// <summary>
///   Supplies input for the mod's native uGUI only when the game has not supplied
///   an active EventSystem of its own.
/// </summary>
internal sealed class NativeEventSystemBridge : MonoBehaviour {
  private EventSystem _fallback;
  private GameObject _fallbackRoot;

  internal EventSystem Current => FindGameEventSystem() ?? EnsureFallback();
  internal bool IsUsingFallback => _fallback && _fallback.isActiveAndEnabled;

  private void Update() {
    var gameEventSystem = FindGameEventSystem();
    if (gameEventSystem) {
      ReleaseFallback();
      return;
    }

    EnsureFallback();
  }

  private void OnDestroy() {
    ReleaseFallback();
  }

  private EventSystem EnsureFallback() {
    if (_fallback) {
      if (!_fallbackRoot.activeSelf) _fallbackRoot.SetActive(true);
      return _fallback;
    }

    _fallbackRoot = new GameObject(
      $"{MyPluginInfo.PLUGIN_GUID}.FallbackEventSystem",
      typeof(EventSystem),
      typeof(StandaloneInputModule)
    );
    DontDestroyOnLoad(_fallbackRoot);
    _fallback = _fallbackRoot.GetComponent<EventSystem>();
    Plugin.Logger.LogWarning(
      "The game has no active EventSystem; ItemSpawner created a temporary native uGUI fallback.");
    return _fallback;
  }

  private EventSystem FindGameEventSystem() {
    return FindObjectsOfType<EventSystem>()
      .FirstOrDefault(candidate => candidate && candidate != _fallback &&
                                   candidate.isActiveAndEnabled);
  }

  private void ReleaseFallback() {
    if (!_fallbackRoot) return;

    // disable synchronously so there are never two active systems while Unity
    // waits until the end of the frame to perform the actual destruction.
    _fallbackRoot.SetActive(false);
    Destroy(_fallbackRoot);
    _fallbackRoot = null;
    _fallback = null;
    Plugin.Logger.LogInfo("ItemSpawner released its fallback EventSystem to the game.");
  }
}
