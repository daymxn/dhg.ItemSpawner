namespace daymxn.DHG.ItemSpawner.util;

/// <summary>
///   Padding to apply to some UI element.
/// </summary>
/// <param name="top">Padding applied to the top of the element.</param>
/// <param name="bottom">Padding applied to the bottom of the element.</param>
/// <param name="left">Padding applied to the left of the element.</param>
/// <param name="right">Padding applied to the right of the element.</param>
public struct Padding(int top = 0, int bottom = 0, int left = 0, int right = 0) {
  public readonly int Top = top;
  public readonly int Bottom = bottom;
  public readonly int Left = left;
  public readonly int Right = right;

  /// <summary>
  ///   Creates a new <see cref="Padding" /> instance with all the directions initialized to a single
  ///   value.
  /// </summary>
  /// <param name="value">The value to initialize all the padding directions to.</param>
  public static Padding Of(int value) {
    return new Padding(value, value, value, value);
  }
}
