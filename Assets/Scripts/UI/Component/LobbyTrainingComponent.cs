using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System.Linq;
using UniRx;

public class LobbyTrainingComponent : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI LevelText;
    public Image IconImg;
    [SerializeField]
    private GameObject UnlockedBox;

    [SerializeField]
    private TextMeshProUGUI AmountText;

    [SerializeField]
    private GameObject UpgradeIconObj;

    [SerializeField]
    private GameObject LevelObj;

    [SerializeField]
    private GameObject LevelLineObj;

    [SerializeField]
    private GameObject BgTopObj;

    [SerializeField]
    private GameObject BgObj;

    [SerializeField]
    private GameObject LineObj;

    [SerializeField]
    private GameObject NormalObj;

    [SerializeField]
    private Image SliderGuageImg;

    [SerializeField]
    private GameObject FxUpgrade;

    [SerializeField]
    private Button Btn;

    private CompositeDisposable disposables = new CompositeDisposable();

    private PageLobbyTraining Training;
    private BlockTrainingInfoData TrainingData;

    private bool isCap;
    private bool isLevelChange;
    private bool isClaimed;

    private void Awake()
    {
        Btn.onClick.AddListener(OnClick);
    }

    public void Set(int trainingOrder, bool isRefresh = false)
    {
        if (isRefresh && TrainingData != null)
        {
            trainingOrder = TrainingData.upgrade_order;
        }
        Training = GameRoot.Instance.UISystem.GetUI<PageLobbyTraining>();
        ProjectUtility.SetActiveCheck(FxUpgrade, false);
        ProjectUtility.SetActiveCheck(NormalObj, true);

        if (trainingOrder == 0)
        {
            ProjectUtility.SetActiveCheck(BgObj, true);
            ProjectUtility.SetActiveCheck(BgTopObj, false);
            ProjectUtility.SetActiveCheck(NormalObj, false);
            ProjectUtility.SetActiveCheck(LineObj, false);
            ProjectUtility.SetActiveCheck(LevelLineObj, false);
            ProjectUtility.SetActiveCheck(LevelObj, false);
            return;
        }

        TrainingData = Tables.Instance.GetTable<BlockTrainingInfo>().GetData(trainingOrder);
        var nextData = Tables.Instance.GetTable<BlockTrainingInfo>().GetData(trainingOrder + 1);
        isCap = false;
        isLevelChange = false;
        bool isLevelOver = TrainingData.level > GameRoot.Instance.UserData.Stageidx.Value;
        isClaimed = GameRoot.Instance.UserData.Newtrainingdatabuyorder.Value >= TrainingData.upgrade_order;
        bool canUpgrade = trainingOrder - 1 == GameRoot.Instance.UserData.Newtrainingdatabuyorder.Value;
        canUpgrade &= TrainingData.cost <= GameRoot.Instance.UserData.Money.Value;
        canUpgrade &= !isLevelOver;
        if (nextData != null && nextData.level != TrainingData.level) isLevelChange = true;
        if (isLevelChange && nextData.level > GameRoot.Instance.UserData.Stageidx.Value && !isLevelOver) isCap = true;

        //set basic info
        IconImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, TrainingData.upgrade_icon);
        int chapter = 0, stage = 0;
        LevelText.text = $"Level {TrainingData.level}";
        ProjectUtility.SetActiveCheck(LevelText.gameObject, isLevelChange);
        ProjectUtility.SetActiveCheck(BgTopObj, isCap);
        ProjectUtility.SetActiveCheck(BgObj, !isCap && !isLevelOver);
        ProjectUtility.SetActiveCheck(UpgradeIconObj, canUpgrade);
        AmountText.text = "+" + TrainingData.value.ToString();

        ProjectUtility.SetActiveCheck(LevelObj, isLevelChange);
        ProjectUtility.SetActiveCheck(LevelLineObj, isLevelChange);
        ProjectUtility.SetActiveCheck(LineObj, isCap);

        SliderGuageImg.DOKill();
        SliderGuageImg.fillAmount = !isClaimed ? 0 : isCap ? 0.7f : 1f;

        ProjectUtility.SetActiveCheck(UnlockedBox, isClaimed);
    }

    public void OnUpgrade()
    {
        ProjectUtility.SetActiveCheck(FxUpgrade, true);
        isClaimed = true;

        SliderGuageImg.DOKill();
        SliderGuageImg.DOFillAmount(!isClaimed ? 0 : isCap ? 0.7f : 1f, 0.2f)
            .SetEase(Ease.OutCubic)
            .SetUpdate(true);
        ProjectUtility.SetActiveCheck(UnlockedBox, isClaimed);
        ProjectUtility.SetActiveCheck(UpgradeIconObj, false);
    }

    public void SetNextUpgrade()
    {
        ProjectUtility.SetActiveCheck(UpgradeIconObj, true);
    }

    private void OnClick()
    {
        if (Training == null || TrainingData == null) return;

        Training.ToolTip.Set(Tables.Instance.GetTable<Localize>().GetString(TrainingData.upgrade_name), transform.position + Vector3.up * 20, transform.parent, 1.5f);
    }

    // public void Set(int level, int upgradeorder, LobbyTrainingBuyComponent trainingbuycomponent)
    // {
    //     Level = level;

    //     UpgradeOrder = upgradeorder;

    //     TpUtility.SetActiveCheck(NormalObj, true);
    //     TpUtility.SetActiveCheck(FxUpgrade, false);

    //     if (level == 1 && upgradeorder == 0)
    //     {
    //         TpUtility.SetActiveCheck(BgObj, true);
    //         TpUtility.SetActiveCheck(BgTopObj, false);
    //         TpUtility.SetActiveCheck(NormalObj, false);
    //         TpUtility.SetActiveCheck(LineObj, false);
    //         TpUtility.SetActiveCheck(LevelObj, false);
    //         TpUtility.SetActiveCheck(LevelLineObj, false);
    //         return;
    //     }

    //     BuyComponent = trainingbuycomponent;

    //     var curtraniniglevel = ProjectUtility.GetAbsoluteStageNumber(GameRoot.Instance.UserData.Chapteridx, GameRoot.Instance.UserData.Stageidx.Value);

    //     var lastorder = Tables.Instance.GetTable<BlockTrainingInfo>().DataList.Where(x => x.level == level).Max(x => x.upgrade_order);

    //     var td = Tables.Instance.GetTable<BlockTrainingInfo>().GetData(upgradeorder);

    //     if (td != null)
    //     {

    //         bool islastorder = curtraniniglevel == level && UpgradeOrder == lastorder;

    //         BaseIcon.Set(level, upgradeorder, TrainingAction);

    //         bool isupgrade = GameRoot.Instance.UserData.Newtrainingdatabuyorder.Value >= upgradeorder;

    //         //PremiumIcon.Set(level, upgradeorder, TrainingAction, true);

    //         TpUtility.SetActiveCheck(PremiumIcon.gameObject, td.special_type > 0);

    //         TpUtility.SetActiveCheck(LevelObj, UpgradeOrder == lastorder);
    //         TpUtility.SetActiveCheck(LevelLineObj, UpgradeOrder == lastorder);

    //         TpUtility.SetActiveCheck(BgTopObj, islastorder);
    //         TpUtility.SetActiveCheck(LineObj, islastorder);
    //         TpUtility.SetActiveCheck(BgObj, curtraniniglevel >= level && !islastorder);

    //         if (SliderGuageImg != null)
    //         {
    //             if (isupgrade && islastorder)
    //             {
    //                 SliderGuageImg.fillAmount = 0.7f;
    //             }
    //             else
    //             {
    //                 SliderGuageImg.fillAmount = isupgrade ? 1 : 0;
    //             }
    //         }


    //         disposables.Clear();

    //         GameRoot.Instance.UserData.Newtrainingdatabuyorder.Subscribe(x =>
    //         {
    //             if (x >= upgradeorder && islastorder)
    //             {
    //                 SliderGuageImg.fillAmount = 0.7f;
    //             }
    //             else
    //             {
    //                 SliderGuageImg.fillAmount = x >= upgradeorder ? 1 : 0;
    //             }
    //         }).AddTo(disposables);


    //     }
    //     else
    //     {
    //         var test = 1;
    //     }
    // }


    // public void TrainingAction(Transform targettrans, bool ispremium)
    // {
    //     TpUtility.SetActiveCheck(BuyComponent.gameObject, true);
    //     BuyComponent.Set(Level, UpgradeOrder, ispremium, this);
    //     BuyComponent.transform.position = targettrans.position;

    // }

    void OnDestroy()
    {
        disposables.Clear();
    }
}
