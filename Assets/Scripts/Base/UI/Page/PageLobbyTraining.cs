using System.Collections.Generic;
using System.Linq;
using TMPro;
using BanpoFri;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

[UIPath("UI/Page/PageLobbyTraining")]
public class PageLobbyTraining : CommonUIBase
{

    [SerializeField]
    private TextMeshProUGUI DescText;
    [SerializeField]
    private TextMeshProUGUI PriceText;

    //buttons
    [SerializeField]
    private GameObject BtnAreaRoot;
    [SerializeField]
    private GameObject BuyBtnRoot;
    [SerializeField]
    private GameObject AdBtnRoot;
    [SerializeField]
    private GameObject DisabledRoot;
    [SerializeField]
    private GameObject EnabledRoot;

    [SerializeField]
    private Button BuyBtn;
    [SerializeField]
    private AdsButton AdBtn;

    //toast
    [SerializeField]
    private Animator ToastAnimator;
    [SerializeField]
    private Image ToastIcon;
    [SerializeField]
    private TextMeshProUGUI ToastText;

    //tooltip
    public TextTooltip ToolTip;

    //scroll
    [SerializeField]
    private NewRecycleScrollRect scrollRect;
    private LobbyTrainingComponent firstBuyObj;

    //buy 
    private int NextBuyOrder;
    private BlockTrainingInfoData NextBuyData;

    private int AdWatchCount = 0;

    private CompositeDisposable disposables = new CompositeDisposable();

    protected override void Awake()
    {
        base.Awake();

        BuyBtn.onClick.AddListener(Buy);
       //AdBtn.AddListener(TpMaxProp.AdRewardType.TrainingUpgrade, BuyWithAd, CanBuyWithAd, false);

    }

    public void Init() { }

    public override void OnShowBefore()
    {
        base.OnShowBefore();

        disposables.Clear();

        GameRoot.Instance.UserData.Money.SkipLatestValueOnSubscribe().Subscribe(x =>
        {
            SetButtons();
            RefreshComponents();
        }).AddTo(disposables);

        ToastAnimator.gameObject.SetActive(false);
        ProjectUtility.SetActiveCheck(ToolTip.gameObject, false);

        NextBuyOrder = GameRoot.Instance.UserData.Newtrainingdatabuyorder.Value + 1;
        NextBuyData = Tables.Instance.GetTable<BlockTrainingInfo>().GetData(NextBuyOrder);
        SetButtons();
        SetTexts();
        InitScroll();
    }

    private void InitScroll()
    {
        trainingDataList = Tables.Instance.GetTable<BlockTrainingInfo>().DataList.ToList();
        scrollRect.Init(TrainingComponentPrefab, trainingDataList.Count, OnScrollListener, NextBuyOrder - 2f);
    }

    private void OnScrollListener(int index, GameObject obj)
    {
        if (index >= trainingDataList.Count) return;
        if (obj.TryGetComponent<LobbyTrainingComponent>(out var item))
        {
            item.Set(index);
            if (index == 1) firstBuyObj = item;
        }
    }

    public GameObject GetTutorialFirstBuyComponent()
    {
        return BuyBtn.gameObject;
    }

    private void Buy()
    {
        if (NextBuyData.level > GameRoot.Instance.UserData.Stageidx.Value) return;
        if (GameRoot.Instance.UserData.Money.Value >= NextBuyData.cost)
        {
            GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency, (int)Config.CurrencyID.Money, -NextBuyData.cost, true);
            OnBuy();
            SoundPlayer.Instance.PlaySound("effect_training_upgrade");
            GameRoot.Instance.GameNotification.UpdateNotification(GameNotificationSystem.NotificationCategory.Training);

            // if (GameRoot.Instance.UserData.SilverCoin.Value < NextBuyData.cost)
            //     GameRoot.Instance.UISystem.OpenUI<PopupCoinInsufficent>();
        }
        else
        {
            GameRoot.Instance.UISystem.OpenUI<PopupGoldCoinInsufficent>();
        }
    }

    private bool CanBuyWithAd()
    {
        if (AdWatchCount >= 1) return false;
        if (NextBuyData.level > GameRoot.Instance.UserData.Stageidx.Value) return false;
        return true;
    }

    private void BuyWithAd()
    {
        AdWatchCount++;
        OnBuy();
    }

    public void OnBuy()
    {
        var item = scrollRect.GetItemAt<LobbyTrainingComponent>(NextBuyOrder);
        if (item != null)
        {
            item.SetNextUpgrade();
        }

        ShowToast(NextBuyOrder);
        GameRoot.Instance.TrainingSystem.BuyTraining();
        ScrollToCurrent();
        NextBuyOrder++;
        NextBuyData = Tables.Instance.GetTable<BlockTrainingInfo>().GetData(NextBuyOrder);
        RefreshComponents();
        SetTexts();
        SetButtons();

        if (item != null)
        {
            item.OnUpgrade();
        }
    }

    private void RefreshComponents()
    {
        var allItems = scrollRect.GetAllItems<LobbyTrainingComponent>();
        foreach (var comp in allItems)
            comp.Set(0, true);
    }

    private void SetButtons()
    {
        bool isAd = false;
        // isAd |= GameRoot.Instance.UserData.Money.Value < NextBuyData.cost;
        // isAd &= AdWatchCount < 1;

        ProjectUtility.SetActiveCheck(AdBtnRoot, isAd);
        ProjectUtility.SetActiveCheck(BuyBtnRoot, !isAd);
        PriceText.text = NextBuyData.cost.ToString();
        PriceText.color = GameRoot.Instance.UserData.Money.Value < NextBuyData.cost ? Color.red : Color.white;

        //turn off
        bool canBuyNext = NextBuyData.level <= GameRoot.Instance.UserData.Stageidx.Value;
        AdBtn.interactable = canBuyNext;
        BuyBtn.interactable = canBuyNext;
        ProjectUtility.SetActiveCheck(BtnAreaRoot, true);
    }

    private void SetTexts()
    {
        DescText.text = Tables.Instance.GetTable<Localize>().GetFormat(NextBuyData.upgrade_desc, NextBuyData.value);
    }

    private void ShowToast(int buyOrder)
    {
        var trainingData = Tables.Instance.GetTable<BlockTrainingInfo>().GetData(buyOrder);
        ProjectUtility.SetActiveCheck(ToastAnimator.gameObject, true);
        ToastAnimator.Play("Show");
        ToastIcon.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, trainingData.upgrade_icon);
        ToastText.text = $"<color=#57FF00>+{trainingData.value}</color>";
    }

    private void ScrollToCurrent()
    {
        scrollRect.ScrollToPosition(NextBuyOrder - 2f, 0.3f, NewRecycleScrollRect.ScrollEase.EaseInOut, true, true);
    }

    void OnDestroy()
    {
        disposables.Clear();
    }

    void OnDisable()
    {
        disposables.Clear();
    }



    [SerializeField]
    private GameObject TrainingComponentPrefab;
    private List<BlockTrainingInfoData> trainingDataList;
}
