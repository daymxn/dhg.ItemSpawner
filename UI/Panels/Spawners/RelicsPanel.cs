using System;
using System.Collections.Generic;
using daymxn.DHG.ItemSpawner.game;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

namespace daymxn.DHG.ItemSpawner.ui.Panels.Spawners;

/// <summary>
///   Panel for spawning in Relics (badges).
/// </summary>
public class RelicsPanel(GameObject owner)
  : AffixSpawningPanel<Badge.Name, Badge>(owner) {
  public override string Name => "Relics";
  protected override Vector2 PivotOffset => new(100, -100);

  protected override string SelectableName => "Relic";
  protected override AffixQualityDict AffixPool => AffixPools.Relics;
  protected override List<Badge.Name> Names => GameData.Relics.Names;
  protected override List<Sprite> Icons => GameData.Relics.Icons;
  protected override Action<Badge> SpawnFunction => Inventory.SpawnBadge;

  protected override Badge CreateEmptyItem(Badge.Name name, Quality quality) {
    return new Badge(name, quality);
  }
}
