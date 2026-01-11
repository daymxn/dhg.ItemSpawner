using System;
using System.Collections.Generic;
using System.Linq;

namespace daymxn.DHG.ItemSpawner.util;

/// <summary>
///   Generic utility functions which aren't specific to the game.
/// </summary>
public static class Util {
  /// <summary>
  ///   Gets all the values of an enum, in order.
  /// </summary>
  /// <typeparam name="T">The enum whose values to get.</typeparam>
  public static List<T> GetEnumValues<T>() where T : Enum {
    return Enum.GetValues(typeof(T)).Cast<T>().ToList();
  }
}
