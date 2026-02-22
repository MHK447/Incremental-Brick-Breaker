using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using UniRx;

[UIPath("UI/Popup/PopupBoxRewardTime")]
public class PopupBoxRewardTime : UIBase
{
    private StageRewardBoxData BoxData = null;


    [SerializeField]
    private List<RewardIconComponent> RewardIconComponents = new List<RewardIconComponent>();

    [SerializeField]
    private Button SpeedUpBtn;

    [SerializeField]
    private Button QuickBuyBtn;

    [SerializeField]
    private Button QuickBuyBtn2;

    [SerializeField]
    private Button StartBtn;


    [SerializeField]
    private Transform TimeRoot;

    [SerializeField]
    private Transform StartBtnRoot;

    [SerializeField]
    private Transform WaitTimeRoot;


    [SerializeField]
    private Image RewardImg;

    [SerializeField]
    private TextMeshProUGUI TimeText;

    [SerializeField]
    private List<TextMeshProUGUI> QuickBuyTexts = new List<TextMeshProUGUI>();

    [SerializeField]
    private TextMeshProUGUI RewardBoxNameText;

    [SerializeField]
    private TextMeshProUGUI RewardBoxTimeText;


    private CompositeDisposable disposables = new CompositeDisposable();


    override protected void Awake()
    {
        base.Awake();
        SpeedUpBtn.onClick.AddListener(OnClickAdReward);
        QuickBuyBtn.onClick.AddListener(OnClickQuickReward);
        QuickBuyBtn2.onClick.AddListener(OnClickAdQuickBtn);

        StartBtn.onClick.AddListener(OnClickStart);
    }

    public void Set(StageRewardBoxData boxdata)
    {
        BoxData = boxdata;

        var td = Tables.Instance.GetTable<RewardBoxInfo>().GetData(BoxData.Boxidx);

        if (td != null)
        {
            var boxtime = ProjectUtility.GetTimeStringFormattingShort(td.time);

            RewardBoxNameText.text = Tables.Instance.GetTable<Localize>().GetString($"reward_box_name_0{BoxData.Boxidx}");
            RewardBoxTimeText.text = Tables.Instance.GetTable<Localize>().GetFormat("reward_box_time", boxtime);

            foreach (var reward in RewardIconComponents)
            {
                ProjectUtility.SetActiveCheck(reward.gameObject, false);
            }

            for (int i = 0; i < td.reward_type.Count; ++i)
            {
                string rewardstring = "";
                if (td.reward_idx[i] == (int)Config.CurrencyID.Money)
                {
                    rewardstring = $"{ProjectUtility.CalculateMoneyToString(GameRoot.Instance.StageRewardSystem.GetStageRewardMoney(td.reward_value_min[i]) / 2)} ~ {ProjectUtility.CalculateMoneyToString(GameRoot.Instance.StageRewardSystem.GetStageRewardMoney(td.reward_value_max[i]) / 2)}";
                }
                else
                {
                    if (td.reward_value_min[i] == td.reward_value_max[i])
                    {
                        rewardstring = $"{(int)td.reward_value_min[i]}";
                    }
                    else
                    {
                        rewardstring = $"{(int)td.reward_value_min[i]} ~ {(int)td.reward_value_max[i]}";
                    }
                }
                RewardIconComponents[i].Set(td.reward_type[i], td.reward_idx[i], rewardstring);
                ProjectUtility.SetActiveCheck(RewardIconComponents[i].gameObject, true);
            }

            var boxopencheck = GameRoot.Instance.UserData.Stagerewardboxgroup.IsBoxUnlockStart();


            ProjectUtility.SetActiveCheck(StartBtnRoot.gameObject, !BoxData.Isopenstart && !boxopencheck);

            ProjectUtility.SetActiveCheck(TimeRoot.gameObject, BoxData.Isopenstart);

            ProjectUtility.SetActiveCheck(WaitTimeRoot.gameObject, !BoxData.Isopenstart && BoxData.Boxtime == default(System.DateTime) && boxopencheck);


            RewardImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, td.image);

            SetBuyText();

            disposables.Clear();

            boxdata.CurBoxTimeProperty.Subscribe(x =>
            {
                TimeText.text = ProjectUtility.GetTimeStringFormattingShort(x);

                if (x <= 0 && boxdata.Isopenstart)
                {
                    Hide();
                }

                SetBuyText();
            }).AddTo(disposables);
        }
    }

    public void SetBuyText()
    {

        foreach (var quicktext in QuickBuyTexts)
        {
            if (GameRoot.Instance.TutorialSystem.IsActive(TutorialSystem.Tuto_3))
            {
                quicktext.text = Tables.Instance.GetTable<Localize>().GetString("str_free");
                quicktext.color = Color.white;
            }
            else
            {
                quicktext.text = BoxData.QuickUnLockValue().ToString();

                quicktext.color = BoxData.QuickUnLockValue() <= GameRoot.Instance.UserData.Cash.Value ? Color.white : Color.red;
            }
        }
    }



    public void OnClickAdReward()
    {
        // GameRoot.Instance.PluginSystem.ADProp.ShowRewardAD(TpMaxProp.AdRewardType.BoxRewardTime, (bool success) =>
        // {
        //     BoxData.TimeDeCline(3600);
        // });
    }

    public void OnClickQuickReward()
    {

        if (BoxData.QuickUnLockValue() <= GameRoot.Instance.UserData.Cash.Value || GameRoot.Instance.TutorialSystem.IsActive(TutorialSystem.Tuto_3))
        {
            Hide();

            if (!GameRoot.Instance.TutorialSystem.IsActive(TutorialSystem.Tuto_3))
            {
                GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency, (int)Config.CurrencyID.Cash, -BoxData.QuickUnLockValue());
            }

            GameRoot.Instance.UserData.Stagerewardboxgroup.GetBoxReward(BoxData.Boxidx);
            GameRoot.Instance.UserData.Stagerewardboxgroup.RemoveBox(BoxData);
        }

    }


    public void OnClickAdQuickBtn()
    {
        // GameRoot.Instance.PluginSystem.ADProp.ShowRewardAD(TpMaxProp.AdRewardType.BoxRewardTime, (bool success) =>
        // {
        //     Hide();

        //     if (!GameRoot.Instance.TutorialSystem.IsActive(TutorialSystem.Tuto_3))
        //     {
        //         GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency, (int)Config.CurrencyID.Cash, -BoxData.QuickUnLockValue());
        //     }

        //     GameRoot.Instance.UserData.Stagerewardboxgroup.GetBoxReward(BoxData.Boxidx);
        //     GameRoot.Instance.UserData.Stagerewardboxgroup.RemoveBox(BoxData);
        // });
    }


    public void OnClickStart()
    {
        ProjectUtility.Vibrate();

        BoxData.StartBox();

        GameRoot.Instance.UISystem.GetUI<PageLobbyBattle>()?.GetLobbyBattleBoxComponent.SetRewardBox();

        Hide();
    }



}

