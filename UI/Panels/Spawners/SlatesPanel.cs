using System;
using System.Collections.Generic;
using daymxn.DHG.ItemSpawner.game;
using UnityEngine;
using UniverseLib.UI;
using Vector2 = UnityEngine.Vector2;

namespace daymxn.DHG.ItemSpawner.ui.Panels.Spawners;

/// <summary>
///   Panel for spawning in Support Slates.
/// </summary>
public class SlatesPanel(UIBase owner)
  : AffixSpawningPanel<SupportSlate.Name, SupportSlate>(owner) {
  public override string Name => "Support Slates";
  protected override Vector2 PivotOffset => new(700, -100);

  protected override string SelectableName => "Slate";
  protected override AffixQualityDict AffixPool => AffixPools.Slates;
  protected override List<SupportSlate.Name> Names => GameData.Slates.Names;
  protected override List<Sprite> Icons => GameData.Slates.Icons;
  protected override Action<SupportSlate> SpawnFunction => Inventory.SpawnSupportSlate;

  protected override SupportSlate CreateEmptyItem(SupportSlate.Name name, Quality quality) {
    return new SupportSlate(name, quality);
  }
}
