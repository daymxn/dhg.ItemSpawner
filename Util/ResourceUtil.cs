using System.IO;
using System.Reflection;

namespace daymxn.DHG.ItemSpawner.util;

/// <summary>
///   Helper class for managing resource files.
/// </summary>
public static class ResourceUtil {
  /// <summary>
  ///   Loads the bytes from an embedded asset file.
  /// </summary>
  /// <param name="assetName">The name of the asset file, including the extension.</param>
  /// <returns>A byte array of the asset file's contents.</returns>
  public static byte[] LoadBytes(string assetName) {
    var assembly = Assembly.GetExecutingAssembly();
    var rs =
      assembly.GetManifestResourceStream($"{MyPluginInfo.PLUGIN_GUID}.assets.{assetName}");
    if (rs == null) {
      Plugin.Logger.LogError($"Missing resource: ${assetName}");
      return [];
    }

    byte[] bytes;
    using (rs)
    using (var ms = new MemoryStream()) {
      rs.CopyTo(ms);
      bytes = ms.ToArray();
    }

    return bytes;
  }
}
