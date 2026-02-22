using UnityEngine;
using UnityEngine.UI;
using BanpoFri;

[UIPath("UI/Popup/NoAds")]
public class PopupNoAds : CommonUIBase
{
    [SerializeField]
    private Button SkipBtn;
    [SerializeField]
    private Button BuyBtn;

    protected override void Awake()
    {
        base.Awake();
        SkipBtn.onClick.AddListener(OnSkip);
        BuyBtn.onClick.AddListener(OnBuy);
    }

    private void OnSkip() => Hide();

    private void OnBuy()
    {
        BuyBtn.interactable = false;
        GameRoot.Instance.UISystem.OpenUI<PopupNoAdsPackages>(x => BuyBtn.interactable = true,CheckHide);
    }

    private void CheckHide()
    {
        if (GameRoot.Instance.ShopSystem.NoInterstitialAds.Value) Hide();
    }

    public override void OnHideAfter()
    {
        base.OnHideAfter();
        BuyBtn.interactable = true;
    }
}