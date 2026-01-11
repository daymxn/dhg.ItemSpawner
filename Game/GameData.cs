using System;
using System.Collections.Generic;
using System.Linq;
using daymxn.DHG.ItemSpawner.util;
using UnityEngine;

namespace daymxn.DHG.ItemSpawner.game;

/// <summary>
///   Data relevant to the game.
/// </summary>
public static class GameData {
  private static bool _playerDataLoaded;

  /// <summary>
  ///   Event fired when player data is loaded (ie; a save is loaded).
  /// </summary>
  public static event EventHandler OnPlayerDataLoaded;

  /// <summary>
  ///   Event fired when player data is unloaded (ie; going to the main menu).
  /// </summary>
  public static event EventHandler OnPlayerDataUnloaded;

  public static void Update() {
    var isLoaded = IsPlayerDataLoaded();
    if (isLoaded == _playerDataLoaded) return;

    _playerDataLoaded = isLoaded;
    if (_playerDataLoaded) {
      OnPlayerDataLoaded?.Invoke(null, EventArgs.Empty);
    } else {
      OnPlayerDataUnloaded?.Invoke(null, EventArgs.Empty);
    }
  }

  /// <summary>
  ///   Whether player data is actively loaded.
  /// </summary>
  /// <remarks>
  ///   To get a notification whenever data is loaded and unloaded, use the events instead:
  ///   <see cref="OnPlayerDataLoaded" />, <see cref="OnPlayerDataUnloaded" />.
  /// </remarks>
  public static bool IsPlayerDataLoaded() {
    return MainScene.Instance?.playerData != null;
  }

  /// <summary>
  ///   Game data related to Qualities.
  /// </summary>
  public static class Qualities {
    /// <summary>
    ///   All the qualities in the game.
    /// </summary>
    public static readonly List<Quality> All = Util.GetEnumValues<Quality>();

    /// <summary>
    ///   The highest (max) quality.
    /// </summary>
    public static readonly Quality Highest = All.Max();

    /// <summary>
    ///   The english names of all the qualities.
    /// </summary>
    public static readonly string[] Names = All.Select(it => it.GetNameStr()).ToArray();

    /// <summary>
    ///   Converts a raw int into a Quality instance.
    /// </summary>
    /// <param name="quality">The int value to convert from</param>
    /// <returns>The Quality enum instance that matches the given value.</returns>
    /// <exception cref="Exception">If the index isn't a valid quality.</exception>
    public static Quality FromInt(int quality) {
      if (quality < 0 || quality >= All.Count) {
        throw new Exception(
          $"Invalid quality passed to 'QualityFromInt': {quality} (must be between 0 and {All.Count})"
        );
      }

      return All[quality];
    }
  }

  /// <summary>
  ///   Game data related to Relics (badges).
  /// </summary>
  public static class Relics {
    private static readonly Lazy<List<Sprite>> LazyIcons = new(() =>
      Names.Where(it => it != Badge.Name.Max && it != Badge.Name.None)
        .Select(it => IconManager.Instance.GetBadgeIcon(it)).ToList()
    );

    /// <summary>
    ///   A list of all the Relic names.
    /// </summary>
    public static readonly List<Badge.Name> Names = Util.GetEnumValues<Badge.Name>();

    /// <summary>
    ///   A list of icons for each Relic.
    /// </summary>
    public static List<Sprite> Icons => LazyIcons.Value;
  }

  /// <summary>
  ///   Game data related to Support Slates.
  /// </summary>
  public static class Slates {
    private static readonly Lazy<List<Sprite>> LazyIcons = new(() =>
      Names.Select(it => IconManager.Instance.GetSupportSlateIcon(it)).ToList()
    );

    /// <summary>
    ///   A list of all the Support Slate names.
    /// </summary>
    public static readonly List<SupportSlate.Name> Names = Util.GetEnumValues<SupportSlate.Name>();

    /// <summary>
    ///   A list of icons for each Support Slate.
    /// </summary>
    public static List<Sprite> Icons => LazyIcons.Value;
  }

  /// <summary>
  ///   Game data related to Hunting Cores.
  /// </summary>
  public static class Cores {
    private static readonly Lazy<List<Sprite>> LazyIcons = new(() =>
      Names.Select(it => IconManager.Instance.GetHuntingCoreIcon(it)).ToList()
    );

    /// <summary>
    ///   A list of all the Hunting Core names.
    /// </summary>
    public static readonly List<HuntingCore.Name> Names = Util.GetEnumValues<HuntingCore.Name>();

    /// <summary>
    ///   A list of icons for each Hunting Core.
    /// </summary>
    public static List<Sprite> Icons => LazyIcons.Value;
  }
}
