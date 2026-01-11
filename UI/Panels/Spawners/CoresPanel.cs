using System;
using System.Collections.Generic;
using daymxn.DHG.ItemSpawner.game;
using UnityEngine;
using UniverseLib.UI;
using Vector2 = UnityEngine.Vector2;

namespace daymxn.DHG.ItemSpawner.ui.Panels.Spawners;

/// <summary>
///   Panel for spawning in Hunting Cores.
/// </summary>
public class CoresPanel(UIBase owner)
  : AffixSpawningPanel<HuntingCore.Name, HuntingCore>(owner) {
  public override string Name => "Hunting Cores";
  protected override Vector2 PivotOffset => new(1300, -100);

  protected override string SelectableName => "Core";
  protected override AffixQualityDict AffixPool => AffixPools.Cores;
  protected override List<HuntingCore.Name> Names => GameData.Cores.Names;
  protected override List<Sprite> Icons => GameData.Cores.Icons;
  protected override Action<HuntingCore> SpawnFunction => Inventory.SpawnHuntingCore;

  protected override HuntingCore CreateEmptyItem(HuntingCore.Name name, Quality quality) {
    return new HuntingCore(name, quality);
  }
}
