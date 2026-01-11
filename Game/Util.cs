using System.Collections.Generic;
using System.Linq;

namespace daymxn.DHG.ItemSpawner.game;

public static class AbyssEyeExtension {
  /// <summary>
  ///   Add a list of powers to this eye.
  /// </summary>
  public static void AddPowers(this AbyssEye eye, AbyssPower.Name[] powers) {
    foreach (var name in powers) {
      eye.AddPower_Load(AbyssPower.LoadAbyssPower(name, 1));
    }
  }
}

public static class QualityExtension {
  /// <summary>
  ///   The translated version of this quality's enum name.
  /// </summary>
  public static string GetNameStr(this Quality quality) {
    return MultiLanguageTable.GetText(quality.ToString());
  }

  /// <summary>
  ///   The int representation of this quality.
  /// </summary>
  public static int GetValue(this Quality quality) {
    return (int)quality;
  }

  /// <summary>
  ///   How many power slots this quality supports.
  /// </summary>
  public static int GetPowerSlots(this Quality quality) {
    return quality.GetValue() + 1;
  }
}

public static class RandomAffixExtension {
  /// <summary>
  ///   The translated name of this affix.
  /// </summary>
  public static string GetNameStr(this RandomAffix affix) {
    return affix.IsCompositeAffix()
      ? affix.GetCompositeAttributeName().GetName()
      : affix.GetAttributeName().GetName();
  }

  /// <summary>
  ///   A new affix, with the same values as this one.
  /// </summary>
  public static RandomAffix Cloned(this RandomAffix affix) {
    if (affix.IsCompositeAffix()) {
      return new RandomAffix(
        affix.GetCompositeAttributeName(),
        affix.GetConvertType(),
        affix.quality,
        affix.types.ToList()
      );
    }

    return new RandomAffix(
      affix.GetAttributeName(),
      affix.GetConvertType(),
      affix.quality,
      affix.types.ToList()
    );
  }

  /// <summary>
  ///   Converts a list of affixes into a dictionary of affixes mapping to their respective qualities.
  /// </summary>
  public static AffixQualityDict ToQualityDictionary(
    this IReadOnlyList<RandomAffix> affixes
  ) {
    return affixes
      .GroupBy(a => a.quality)
      .ToDictionary(g => g.Key, g => g.ToList());
  }
}

public static class AttributeNameExtension {
  /// <summary>
  ///   The translated version of this attribute name.
  /// </summary>
  /// <remarks>
  ///   Handles mappings for attribute names that don't have supported translations yet.
  /// </remarks>
  public static string GetName(this AttributeData.Name attributeName) {
    return attributeName switch {
      AttributeData.Name.每秒生命恢复百分比 => "Life Regenerate % per Second",
      AttributeData.Name.召唤物每秒生命恢复百分比 => "Summoned Units Life Regenerate % per Second",
      AttributeData.Name.每秒能量恢复百分比 => "Mana Regenerate % per Second",
      AttributeData.Name.能量承伤比例 => "Damage taken % from mana before life",
      AttributeData.Name.承受元素伤害视作普通伤害 => "Elemental Damage % taken as Physical Damage",
      AttributeData.Name.暴击伤害穿透防御力 => "Critical damage penetrates % resistance",
      AttributeData.Name.元素异常产生额外伤害修正 => "More % damage per Elemental Ailment on enemy",
      AttributeData.Name.额外毒素概率 => "Chance to apply additional poison stack",
      AttributeData.Name.初始加剧 => "Initial aggravations for damage over time",
      AttributeData.Name.概率额外投射物 => "Chance to fire additional projectile",
      AttributeData.Name.概率额外近战攻击次数 => "Extra melee attack chance",
      _ => AttributeData.GetName(attributeName)
    };
  }
}

public static class CompositeAttributeNameExtension {
  /// <summary>
  ///   The translated version of this attribute name.
  /// </summary>
  public static string GetName(this AttributeAffix.CompositeAttributeName attributeName) {
    return AttributeData.GetName(attributeName);
  }
}
