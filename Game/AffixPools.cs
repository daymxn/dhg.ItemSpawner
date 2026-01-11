using System;

namespace daymxn.DHG.ItemSpawner.game;

/// <summary>
///   Class for tracking pools of affixes.
/// </summary>
public static class AffixPools {
  private static readonly Lazy<AffixQualityDict> LazyBadges =
    new(() => BadgeAffix.pool.affixPrefabs.ToQualityDictionary());

  private static readonly Lazy<AffixQualityDict> LazySlates =
    new(() => SupportSlateAffix.pool.affixPrefabs.ToQualityDictionary());

  private static readonly Lazy<AffixQualityDict> LazyCores =
    new(() => HuntingCoreAffix.pool.affixPrefabs.ToQualityDictionary());

  /// <summary>
  ///   Pool of affixes valid for relics.
  /// </summary>
  public static AffixQualityDict Relics => LazyBadges.Value;

  /// <summary>
  ///   Pool of affixes valid for relics.
  /// </summary>
  public static AffixQualityDict Slates => LazySlates.Value;

  /// <summary>
  ///   Pool of affixes valid for relics.
  /// </summary>
  public static AffixQualityDict Cores => LazyCores.Value;
}
