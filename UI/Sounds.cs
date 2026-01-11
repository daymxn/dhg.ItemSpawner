using daymxn.DHG.ItemSpawner.util;
using UnityEngine;

namespace daymxn.DHG.ItemSpawner.ui;

/// <summary>
///   Controller for sounds loaded by the plugin, which can be played by the plugin.
/// </summary>
public class Sounds : MonoBehaviour {
  /// <summary>
  ///   A sound to play for notifications showing on the screen.
  /// </summary>
  public SoundAsset notification;

  public static Sounds Instance => UIManager.Sounds;

  private void Awake() {
    notification = gameObject.AddComponent<SoundAsset>();
    notification.LoadSound("notification.wav");
  }
}

/// <summary>
///   A loaded sound.
/// </summary>
/// <seealso cref="Play" />
public class SoundAsset : MonoBehaviour {
  private AudioClip _audioClip;
  private AudioSource _audioSource;

  /// <summary>
  ///   Loads an embedded audio sound from the assets folder.
  /// </summary>
  /// <param name="assetName">The name of the asset file, including the extension.</param>
  public void LoadSound(string assetName) {
    _audioSource = gameObject.AddComponent<AudioSource>();
    _audioSource.playOnAwake = false;
    _audioSource.spatialBlend = 0f;
    _audioSource.volume = 0.5f;

    StartCoroutine(EmbeddedAudio.LoadWav(assetName, OnAssetLoaded));
  }

  /// <summary>
  ///   Play the sound in a one shot manner.
  /// </summary>
  /// <remarks>
  ///   If the sound hasn't finished loading, this is a no-op.
  /// </remarks>
  public void Play() {
    if (_audioClip) {
      _audioSource.PlayOneShot(_audioClip);
    }
  }

  private void OnAssetLoaded(AudioClip audioClip) {
    _audioClip = audioClip;
  }
}
