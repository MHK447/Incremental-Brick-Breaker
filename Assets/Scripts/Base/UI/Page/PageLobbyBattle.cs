using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using DG.Tweening;
using System;
using System.Collections.Generic;
[UIPath("UI/Page/PageLobbyBattle")]
public class PageLobbyBattle : CommonUIBase
{
    [SerializeField]

    private Button StartBtn;

    [SerializeField]
    private Button SettingBtn;

    [SerializeField]
    private Button AdsEquipBtn;

    [SerializeField]
    private Button StarterPackageBtn;

    [SerializeField]
    private LobbyBattleBoxComponent LobbyBattleBoxComponent;

    public LobbyBattleBoxComponent GetLobbyBattleBoxComponent { get { return LobbyBattleBoxComponent; } }

    [SerializeField]
    private GameObject BoxRoot;

    [SerializeField]
    private TextMeshProUGUI StageNameText;

    [SerializeField]
    private TextMeshProUGUI StageText;

    [SerializeField]
    private LobbyStageRewardGroup RewardGroup;

    [SerializeField]
    private ChapterMapComponent MapComponent;

    [SerializeField]
    private GameObject BossStageImg;

    [SerializeField]
    private GameObject HardStageImg;

    [SerializeField]
    private TextMeshProUGUI BossDiffcultlyText;

    [SerializeField]
    private Button BookBtn;

    [SerializeField]
    private Transform BookBtnIconRoot;

    [SerializeField]
    private Button AdGoldBtn;

    [SerializeField]
    private Button NoAdsBtn;

    [SerializeField]
    private List<PageIconComponent> PageIconComponents = new List<PageIconComponent>();

    override protected void Awake()
    {
        base.Awake();
        StartBtn.onClick.AddListener(OnStartBtnClick);
        SettingBtn.onClick.AddListener(OnClickSetting);
        BookBtn.onClick.AddListener(OnClickBook);
        AdGoldBtn.onClick.AddListener(OnClickAdGold);
        AdsEquipBtn.onClick.AddListener(OnClickAdsEquipBtn);
        NoAdsBtn.onClick.AddListener(OnClickNoAdsBtn);
        StarterPackageBtn.onClick.AddListener(OnClickStarterPackageBtn);
    }

    public void OnStartBtnClick()
    {
        Hide();

        StartBtn.interactable = false;

        List<TpParameter> parameters = new List<TpParameter>();
        parameters.Add(new TpParameter("Stage", GameRoot.Instance.UserData.Stageidx.Value));
        GameRoot.Instance.PluginSystem.AnalyticsProp.AllEvent(IngameEventType.None, "m_stage_start", parameters);

        GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.StartBattle();
        GameRoot.Instance.UserData.ClearInGameResumeData(false);

        if (!GameRoot.Instance.ShopSystem.NoInterstitialAds.Value)
        {
            GameRoot.Instance.PluginSystem.ShowBanner(MaxSdkBase.BannerPosition.BottomCenter);
        }
    }

    public override void OnShowBefore()
    {
        base.OnShowBefore();
        Init();
    }


    public void Init()
    {
        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;

        StartBtn.interactable = true;

        LobbyBattleBoxComponent.Init();

        RewardGroup.Init();

        SetStage();

        MapComponent.transform.localScale = Vector3.zero;
        MapComponent.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);

        MapComponent.Set(stageidx);

        ProjectUtility.SetActiveCheck(RewardGroup.gameObject,
         GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.LobbyReward));

        GameRoot.Instance.GameNotification.UpdateNotification(GameNotificationSystem.NotificationCategory.CardBook);


        foreach (var starter in PageIconComponents)
        {
            starter.Init();
        }


        AdsEquipBtnCheck();

        CheckAdGoldBtn();

        InitIcons();


        if (GameRoot.Instance.ActionQueueSystem.MainLoobyAction != null && !GameRoot.Instance.ActionQueueSystem.IsTutorialonCheck && !GameRoot.Instance.TutorialSystem.IsActive())
        {
            GameRoot.Instance.ActionQueueSystem.MainLoobyAction?.Invoke();
            GameRoot.Instance.ActionQueueSystem.MainLoobyAction = null;
        }

    }


    public void InitIcons()
    {
        ProjectUtility.SetActiveCheck(BookBtnIconRoot.gameObject, GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.EquipBook));
        ProjectUtility.SetActiveCheck(NoAdsBtn.gameObject, !GameRoot.Instance.ShopSystem.NoInterstitialAds.Value &&
        GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.InterAdOpen));
    }




    public void AdsEquipBtnCheck()
    {
        var count = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.AdsEquipCount);

        ProjectUtility.SetActiveCheck(AdsEquipBtn.gameObject, GameRoot.Instance.UserData.Herogroudata.Equipplayeridx > 0
        && count < 4);
    }

    public void OnClickSetting()
    {
        GameRoot.Instance.UISystem.OpenUI<PopupOption>(popup => popup.Set(false));
    }


    public void SetStage()
    {
        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;

        var td = Tables.Instance.GetTable<StageInfo>().DataList.ToList().FindAll(x => x.stage_idx == stageidx).LastOrDefault();

        var displayIdx = stageidx > 60 ? ((stageidx - 1) % 60) + 1 : stageidx;
        StageNameText.text = $"{stageidx}.{Tables.Instance.GetTable<Localize>().GetString($"str_stage_name_{displayIdx}")}";
        StageText.text = Tables.Instance.GetTable<Localize>().GetFormat("str_main_stage_desc", stageidx);

        ProjectUtility.SetActiveCheck(BossStageImg, td.diffiyculty == 2);
        ProjectUtility.SetActiveCheck(HardStageImg, td.diffiyculty == 1);

        BossDiffcultlyText.text = Tables.Instance.GetTable<Localize>().GetString($"str_difficultly_stage_{td.diffiyculty}");


    }

    public void OnClickOption()
    {
        GameRoot.Instance.UISystem.OpenUI<PopupOption>(popup => popup.Set(false));
    }


    public void OnClickBook()
    {
        GameRoot.Instance.UISystem.OpenUI<PopupCardBook>(popup => popup.Init());
    }

    public void OnClickAdGold()
    {
        GameRoot.Instance.PluginSystem.ADProp.ShowRewardAD(TpMaxProp.AdRewardType.AdGold, (result) =>
        {
            ProjectUtility.SetActiveCheck(AdGoldBtn.gameObject, false);

            GameRoot.Instance.EffectSystem.MultiPlay<RewardEffect>(transform.position, x =>
                                {
                                    //적에 전체체력에 퍼센트

                                    var endtr = GameRoot.Instance.UISystem.GetUI<HudCurrencyTop>().GetRewardEndTr((int)Config.RewardType.Currency, (int)Config.CurrencyID.Money);
                                    SoundPlayer.Instance.PlaySound("get_coin");

                                    x.Set((int)Config.RewardType.Currency, (int)Config.CurrencyID.Money, endtr, () =>
                                    {
                                        GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency, (int)Config.CurrencyID.Money, 300);
                                    });
                                    x.SetAutoRemove(true, 4f);
                                });
        });
    }


    public void PageIconInit(PageIconType type)
    {
        PageIconComponents[(int)type - 1].Init();
    }

    public void OnClickAdsEquipBtn()
    {
        GameRoot.Instance.UISystem.OpenUI<PopupAdsEquip>(popup => popup.Init());
    }

    public void OnClickNoAdsBtn()
    {
        GameRoot.Instance.UISystem.OpenUI<PopupNoAdsPackages>();
    }


    public void CheckAdGoldBtn()
    {
        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;

        var failedcount = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.StageFailedCount, stageidx);

        ProjectUtility.SetActiveCheck(AdGoldBtn.gameObject,
        GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.TRAININGOPEN) && failedcount > 1);
    }


    public void OnClickStarterPackageBtn()
    {
        GameRoot.Instance.UISystem.OpenUI<PopupPackageStarter>(popup => popup.Init());
    }

}
