using System.Collections.Generic;
using daymxn.DHG.ItemSpawner.ui.Native;
using daymxn.DHG.ItemSpawner.util;
using UnityEngine;
using UnityEngine.UI;

namespace daymxn.DHG.ItemSpawner.ui.Panels;

/// <summary>
///   The level of a notification to display to the user.
/// </summary>
/// <remarks>
///   The level will be included as the title of the message, and will have a corresponding color.
/// </remarks>
public enum Level {
  Notice,
  Success,
  Warning,
  Error
}

/// <summary>
///   Panel for displaying notifications to the user.
/// </summary>
/// <remarks>
///   Notifications are shown at the bottom right of the screen, have a sound played when they're
///   shown, and automatically disappear after a few seconds.
/// </remarks>
/// <seealso cref="SendNotification" />
public class NotificationPanel(GameObject owner) : GamePanel(owner) {
  /// <summary>
  ///   How long to show a notification for.
  /// </summary>
  private const float ShowTime = 3f;

  /// <summary>
  ///   Notifications currently active on the screen.
  /// </summary>
  private readonly Queue<ActiveNotification> _notifications = [];

  private bool _open;
  public override string Name => "Notice";
  public override int MinWidth => 280;
  public override int MinHeight => 50;
  public override int DefaultWidth => 340;
  public override int DefaultHeight => 80;
  public override Vector2 DefaultAnchorMin => new(1f, 0f);
  public override Vector2 DefaultAnchorMax => new(1f, 0f);
  public override bool CanDragAndResize => false;
  public override bool ShowByDefault => false;

  protected override int Spacing => 20;
  protected override Padding RootPadding => Padding.Of(0);
  protected override bool PivotToAnchor => true;
  protected override Vector2 PivotOffset => new(-10, 10);
  protected override bool ShowTitleBar => false;

  protected override void CreateBodyContent() {
    MakeNonBlocking(ContentRoot.GetComponent<Image>());
    MakeNonBlocking(UIRoot.GetComponent<Image>());
  }

  /// <summary>
  ///   Sends a notification to display on the screen.
  /// </summary>
  /// <param name="level">The type of the notification.</param>
  /// <param name="text">The message to show in the notification.</param>
  public void SendNotification(Level level, string text) {
    EnqueueNotification(level, text);
    _open = true;
    ToggleVisibility();
  }

  public override void Update() {
    if (!_open) return;

    var currentNotif = _notifications.Peek();
    if (!currentNotif.IsExpired()) return;

    Object.Destroy(currentNotif.Element);
    _notifications.Dequeue();
    ResizeToContent();

    if (_notifications.Count != 0) return;

    _open = false;
    ToggleVisibility();
  }

  private void EnqueueNotification(Level level, string text) {
    var container = UIFactory.CreateVerticalGroup(
      Body,
      "Notification",
      false,
      false,
      true,
      true,
      5,
      new Vector4(10, 10, 10, 10)
    );

    var header = UIFactory.CreateLabel(container, "Header", $"{level}", TextAnchor.UpperLeft,
      LevelToColor(level));
    header.fontStyle = FontStyle.Bold;

    UIFactory.CreateLabel(container, "Notification", text, TextAnchor.UpperLeft);

    Sounds.Instance.notification.Play();
    _notifications.Enqueue(new ActiveNotification(Time.realtimeSinceStartup, container));
    ResizeToContent();
  }

  private void ToggleVisibility() {
    SetActive(_open);
  }

  private void ResizeToContent() {
    if (!Body || !Rect) return;
    Canvas.ForceUpdateCanvases();
    var bodyRect = Body.GetComponent<RectTransform>();
    LayoutRebuilder.ForceRebuildLayoutImmediate(bodyRect);
    var preferredHeight = LayoutUtility.GetPreferredHeight(bodyRect);
    Rect.sizeDelta = new Vector2(DefaultWidth,
      Mathf.Clamp(preferredHeight, MinHeight, 500));
    ForceRebuildLayout();
  }

  private static Color LevelToColor(Level level) {
    return level switch {
      Level.Notice => Color.white,
      Level.Success => Color.green,
      Level.Warning => Color.yellow,
      Level.Error => Color.red,
      _ => Color.white
    };
  }

  private static void MakeNonBlocking(Image image) {
    if (!image) return;
    image.color = Color.clear;
    image.raycastTarget = false;
  }

  /// <summary>
  ///   A notification that is actively being shown on the screen.
  /// </summary>
  /// <param name="StartTime">When the notification was first shown.</param>
  /// <param name="Element">The root object of the notification.</param>
  private readonly record struct ActiveNotification(float StartTime, GameObject Element) {
    public readonly GameObject Element = Element;
    public readonly float StartTime = StartTime;

    public bool IsExpired() {
      return Time.realtimeSinceStartup - StartTime >= ShowTime;
    }
  }
}
