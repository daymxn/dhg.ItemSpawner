using JetBrains.Annotations;

namespace daymxn.DHG.ItemSpawner.game;

/// <summary>
///   Class for managing player inventory related actions.
/// </summary>
public static class Inventory {
  /// <summary>
  ///   Adds a relic to the player's inventory.
  /// </summary>
  /// <param name="badge">The relic to spawn.</param>
  public static void SpawnBadge(Badge badge) {
    GetPackage()?.PickUpBadge(badge, true);
  }

  /// <summary>
  ///   Adds a hunting core to the player's inventory.
  /// </summary>
  /// <param name="core">The core to spawn.</param>
  public static void SpawnHuntingCore(HuntingCore core) {
    GetPackage()?.PickUpHuntingCore(core, true);
  }

  /// <summary>
  ///   Adds a support slate to the player's inventory.
  /// </summary>
  /// <param name="slate">The slate to spawn.</param>
  public static void SpawnSupportSlate(SupportSlate slate) {
    GetPackage()?.PickUpSupportSlate(slate, true);
  }

  /// <summary>
  ///   Adds an abyss eye to the player's inventory.
  /// </summary>
  /// <param name="eye">The eye to spawn.</param>
  public static void SpawnAbyssEye(AbyssEye eye) {
    GetPackage()?.PickUpAbyssEye(eye, true);
  }

  [CanBeNull]
  private static Package GetPackage() {
    return MainScene.Instance?.playerData?.package;
  }
}
