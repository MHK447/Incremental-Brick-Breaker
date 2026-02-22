using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using TMPro;
using UniRx;
using DG.Tweening;
public class StarterPackageIconComponent : PageIconComponent
{
    [SerializeField]
    private Button StarterPackageBtn;

    [SerializeField]
    private TextMeshProUGUI TimeText;

    [SerializeField]
    private GameObject TimeRoot;

    [SerializeField]
    private GameObject SaleTagObj;




    private void Awake()
    {
        StarterPackageBtn.onClick.AddListener(OnClickStarterPackage);

        GameRoot.Instance.UserData.BuyInappIds.ObserveAdd().Subscribe(x =>
        {
            Init();
        }).AddTo(this);


        GameRoot.Instance.UserData.Starterpackdata.PackageTimeProperty.Subscribe(x =>
        {
            TimeText.text = ProjectUtility.GetTimeStringFormattingShort(x);

            if (x > 0 && !TimeRoot.activeSelf)
            {
                ProjectUtility.SetActiveCheck(TimeRoot, true);
            }

            if (x < 0 && TimeRoot.activeSelf)
            {
                ProjectUtility.SetActiveCheck(TimeRoot, false);
            }

        }).AddTo(this);

        GameRoot.Instance.UserData.Starterpackdata.Isbuy.Subscribe(x =>
        {
            ProjectUtility.SetActiveCheck(TimeRoot, GameRoot.Instance.UserData.Starterpackdata.PackageTimeProperty.Value > 0);
            ProjectUtility.SetActiveCheck(SaleTagObj, !x);
        }).AddTo(this);
    }


    public override void Init()
    {
        bool canShow = true;

        var td = Tables.Instance.GetTable<ShopProduct>().GetData((int)PackageType.StarterPackage_1001);

        if (td != null)
        {
            canShow = GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.ShopOpen) &&
            GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.StarterPackage) < 3;
        }

        ProjectUtility.SetActiveCheck(this.gameObject, canShow);

        ProjectUtility.SetActiveCheck(SaleTagObj, !GameRoot.Instance.UserData.Starterpackdata.Isbuy.Value);
        ProjectUtility.SetActiveCheck(TimeRoot, GameRoot.Instance.UserData.Starterpackdata.PackageTimeProperty.Value > 0);

    }


    public void OnClickStarterPackage()
    {
        GameRoot.Instance.UISystem.OpenUI<PopupPackageStarter>();
    }


}
