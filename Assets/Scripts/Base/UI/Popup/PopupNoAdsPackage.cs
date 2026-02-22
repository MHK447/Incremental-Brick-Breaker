using UnityEngine;
using TMPro;
using BanpoFri;
using UnityEngine.UI;

[UIPath("UI/Popup/NoAdsPackages")]
public class PopupNoAdsPackages : CommonUIBase
{
    [Header("First Package")]
    [SerializeField]
    private Button FirstBtn;
    [SerializeField]
    private CommonPackageComponent FirstPackageComp;

    [Header("Second Package")]
    [SerializeField]
    private Button SecondBtn;
    [SerializeField]
    private CommonPackageComponent SecondPackageComp;

    [SerializeField]
    private Button CloseBtn;

    private bool canInteract = true;

    protected override void Awake()
    {
        base.Awake();
        FirstBtn.onClick.AddListener(OnBuyFirst);
        SecondBtn.onClick.AddListener(OnBuySecond);
        CloseBtn.onClick.AddListener(Hide);
    }

    public override void OnShowBefore()
    {
        if (GameRoot.Instance.UserData.Playerdata.IsGameStartProperty.Value)
        {
            GameRoot.Instance.GameSpeedSystem.StopGameSpeed(true);
        }

        base.OnShowBefore();

        canInteract = true;
        SetFirstPackage();
        SetSecondPackage();
    }

    public override void OnHideAfter()
    {
        base.OnHideAfter();

        GameRoot.Instance.GameSpeedSystem.StopGameSpeed(false);
    }

    private void SetFirstPackage()
    {
        if (FirstPackageComp != null)
            FirstPackageComp.Set(PackageType.NoAds_100);
    }

    private void SetSecondPackage()
    {
        if (SecondPackageComp != null)
            SecondPackageComp.Set(PackageType.NoAds_101);
        var td = Tables.Instance.GetTable<ShopProduct>().GetData((int)PackageType.NoAds_101);

    }

    private void OnBuyFirst()
    {
        if (!canInteract) return;
        canInteract = false;
        PackageType packageType = PackageType.NoAds_100;
        GameRoot.Instance.ShopSystem.PurchasePackage((int)packageType, OnBuySuccess, InAppPurchaseLocation.popup, OnBuyCancel);
    }

    private void OnBuySecond()
    {
        if (!canInteract) return;
        canInteract = false;
        GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.ActiveBannerCheck(false);
        GameRoot.Instance.PluginSystem.HideBanner();
        PackageType packageType = PackageType.NoAds_101;
        GameRoot.Instance.ShopSystem.PurchasePackage((int)packageType, OnBuySuccess, InAppPurchaseLocation.popup, OnBuyCancel);
    }

    private void OnBuySuccess()
    {
        GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.ActiveBannerCheck(false);
        var battle = GameRoot.Instance.UISystem.GetUI<PageLobbyBattle>();
        GameRoot.Instance.ShopSystem.CheckNoads();
        GameRoot.Instance.PluginSystem.HideBanner();
        battle.InitIcons();
        Hide();
    }

    private void OnBuyCancel()
    {
        canInteract = true;
    }


    public override void Hide()
    {
        base.Hide();
    }
}