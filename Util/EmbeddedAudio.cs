using System;
using System.Collections;
using System.IO;
using BepInEx;
using UnityEngine;
using UnityEngine.Networking;

namespace daymxn.DHG.ItemSpawner.util;

/// <summary>
///   Helper class for embedded audio files.
/// </summary>
public static class EmbeddedAudio {
  /// <summary>
  ///   Loads an audio wav file from the assets folder.
  /// </summary>
  /// <param name="assetName">The name of the asset, including the wav extension.</param>
  /// <param name="onLoaded">Callback to invoke when the asset is loaded.</param>
  /// <remarks>
  ///   Should be launched in a coroutine.
  /// </remarks>
  public static IEnumerator LoadWav(string assetName, Action<AudioClip> onLoaded) {
    var bytes = ResourceUtil.LoadBytes(assetName);

    var tmpPath = Path.Combine(Paths.CachePath, $"{MyPluginInfo.PLUGIN_GUID}.{assetName}");
    File.WriteAllBytes(tmpPath, bytes);

    using var request = UnityWebRequestMultimedia.GetAudioClip(tmpPath, AudioType.WAV);
    yield return request.SendWebRequest();

    if (request.result != UnityWebRequest.Result.Success) {
      Plugin.Logger.LogError($"Failed to load audio: {request.error}");
      yield break;
    }

    onLoaded(DownloadHandlerAudioClip.GetContent(request));
  }
}
