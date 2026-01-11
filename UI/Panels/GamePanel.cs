using daymxn.DHG.ItemSpawner.game;
using daymxn.DHG.ItemSpawner.util;
using UnityEngine;
using UnityEngine.UI;
using UniverseLib.UI;
using UniverseLib.UI.Panels;

namespace daymxn.DHG.ItemSpawner.ui.Panels;

/// <summary>
///   Common base panel that provides additional defaults and functionality over PanelBase.
/// </summary>
public abstract class GamePanel(UIBase owner) : PanelBase(owner) {
  protected GameObject Body;
  public virtual bool IncludeInNavbar => false;
  public virtual bool RequiresGameData => false;

  public virtual bool ShowByDefault => true;
  protected virtual int Spacing => 10;
  protected virtual Padding RootPadding => Padding.Of(10);
  protected virtual bool PivotToAnchor => false;
  protected virtual Vector2 PivotOffset => new(0, 0);
  protected virtual bool IncludeBodyFitter => false;

  public override void ConstructUI() {
    base.ConstructUI();
    if (RequiresGameData) {
      GameData.OnPlayerDataLoaded += (_, _) => {
        ConstructPanelContent();
        SetActive(ShowByDefault);
      };
      GameData.OnPlayerDataUnloaded += (_, _) => {
        Object.Destroy(Body);
        SetActive(false);
      };

      SetActive(false);
    } else {
      SetActive(ShowByDefault);
    }
  }

  public override void SetActive(bool active) {
    // ensure the OnEnableToggled callback is invoked
    Enabled = active;
  }

  protected override void ConstructPanelContent() {
    if (RequiresGameData && !GameData.IsPlayerDataLoaded()) return;

    Body = UIFactory.CreateUIObject("Body", ContentRoot);
    UIFactory.SetLayoutGroup<VerticalLayoutGroup>(
      Body,
      forceHeight: false,
      forceWidth: false,
      childControlHeight: true,
      childControlWidth: true,
      spacing: Spacing,
      padTop: RootPadding.Top,
      padLeft: RootPadding.Left,
      padRight: RootPadding.Right,
      padBottom: RootPadding.Bottom
    );

    if (IncludeBodyFitter) {
      var fitter = Rect.gameObject.AddComponent<ContentSizeFitter>();
      fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
      fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }

    CreateBodyContent();
  }

  /// <summary>
  ///   Use this instead of "ConstructPanelContent".
  /// </summary>
  protected abstract void CreateBodyContent();

  protected override void LateConstructUI() {
    base.LateConstructUI();
    if (!PivotToAnchor) return;

    Rect.pivot = DefaultAnchorMin;
    Rect.anchoredPosition = PivotOffset;
  }

  /// <summary>
  ///   Forcibly rebuild the layout.
  /// </summary>
  protected void ForceRebuildLayout() {
    Canvas.ForceUpdateCanvases();
    LayoutRebuilder.ForceRebuildLayoutImmediate(Rect);
  }
}
