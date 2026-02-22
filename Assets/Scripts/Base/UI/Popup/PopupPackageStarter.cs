using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UniRx;

[UIPath("UI/Popup/PopupPackageStarter")]
public class PopupPackageStarter : CommonUIBase
{
    [SerializeField]
    private TextMeshProUGUI PriceText;

    [SerializeField]
    private TextMeshProUGUI OriginalPriceText;

    [SerializeField]
    private List<GameObject> LockObjList = new List<GameObject>();


    [SerializeField]
    private Button PurchaseBtn;

    [SerializeField]
    private Button ClaimBtn;

    [SerializeField]
    private TextMeshProUGUI SaleText;

    [SerializeField]
    private TextMeshProUGUI TimerText;

    [SerializeField]
    private GameObject TimerRoot;

    private CompositeDisposable disposables = new CompositeDisposable();


    protected override void Awake()
    {
        base.Awake();
        PurchaseBtn.onClick.AddListener(OnClickPurchaseBtn);
        ClaimBtn.onClick.AddListener(OnClickClaimBtn);
    }

    public override void OnShowBefore()
    {
        base.OnShowBefore();
        Init();
    }

    public void Init()
    {
        var td = Tables.Instance.GetTable<ShopProduct>().GetData((int)PackageType.StarterPackage_1001);

        if (td != null)
        {
            SaleText.text = $"{td.sale}%";

            var product = GameRoot.Instance.InAppPurchaseManager.GetProduct(td.product_id);
            if (product != null)
            {
                PriceText.text = product.metadata.localizedPriceString;

                if (OriginalPriceText != null)
                {
                    OriginalPriceText.text = GameRoot.Instance.ShopSystem.OriginalPackageCost((int)PackageType.StarterPackage_1001);
                }
            }

            for (int i = 0; i < LockObjList.Count; ++i)
            {
                ProjectUtility.SetActiveCheck(LockObjList[i], i == 0 ? GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.StarterPackage) >= 1 : GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.StarterPackage) >= i + 1);
            }



            ProjectUtility.SetActiveCheck(PurchaseBtn.gameObject, !GameRoot.Instance.UserData.Starterpackdata.Isbuy.Value);

            disposables.Clear();

            ClaimBtn.interactable = GameRoot.Instance.UserData.Starterpackdata.PackageTimeProperty.Value <= 0 && GameRoot.Instance.UserData.Starterpackdata.Isbuy.Value;


            GameRoot.Instance.UserData.Starterpackdata.PackageTimeProperty.Subscribe(x =>
            {
                ClaimBtnCheck();
            }).AddTo(disposables);
        }
    }

    public void OnClickClaimBtn()
    {
        ProjectUtility.SetActiveCheck(PurchaseBtn.gameObject, false);
        ProjectUtility.SetActiveCheck(ClaimBtn.gameObject, false);

        if (GameRoot.Instance.UserData.Starterpackdata.Isbuy.Value)
        {
            GameRoot.Instance.DailyResetSystem.GiveStarterPackageReward(null);

            GameRoot.Instance.WaitTimeAndCallback(0.5f, () => //대기용 
            {
                Init();
            });
            ClaimBtn.interactable = false;


            if (GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.StarterPackage) >= 3)
            {
                Hide();
                GameRoot.Instance.UISystem.GetUI<PageLobbyBattle>().PageIconInit(PageIconType.StarterPackage);
            }
        }
    }


    public void ClaimBtnCheck()
    {
        bool climbtncheck = GameRoot.Instance.UserData.Starterpackdata.PackageTimeProperty.Value <= 0 && GameRoot.Instance.UserData.Starterpackdata.Isbuy.Value;

        if (climbtncheck && !ClaimBtn.interactable)
        {
            ClaimBtn.interactable = true;
        }
        else if (!climbtncheck && ClaimBtn.interactable)
        {
            ClaimBtn.interactable = false;
        }

        ProjectUtility.SetActiveCheck(ClaimBtn.gameObject, GameRoot.Instance.UserData.Starterpackdata.Isbuy.Value);

        ProjectUtility.SetActiveCheck(TimerRoot.gameObject, GameRoot.Instance.UserData.Starterpackdata.Starterpackbuytime != default(System.DateTime) && GameRoot.Instance.UserData.Starterpackdata.PackageTimeProperty.Value > 0);

        TimerText.text = ProjectUtility.GetTimeStringFormattingShort(GameRoot.Instance.UserData.Starterpackdata.PackageTimeProperty.Value);
    }

    public void OnClickPurchaseBtn()
    {
        GameRoot.Instance.ShopSystem.PurchasePackage((int)PackageType.StarterPackage_1001, OnClickClaimBtn, InAppPurchaseLocation.hud);
    }


    void OnDestroy()
    {
        disposables.Clear();
    }
}
