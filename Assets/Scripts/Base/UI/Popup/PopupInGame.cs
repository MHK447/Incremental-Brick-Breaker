using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using TMPro;
using UniRx;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

using DG.Tweening;
using Unity.VisualScripting;
[UIPath("UI/Popup/PopupInGame", true)]
public class PopupInGame : CommonUIBase
{


    [SerializeField]
    private Button PauseBtn;

    public TileWeaponGroup TileWeaponGroup;

    [SerializeField]
    private TileMergeChecker TileMergeChecker;

    public TileMergeChecker GetTileMergeChecker { get { return TileMergeChecker; } }

    [SerializeField]
    private TileUnlockChecker TileUnlockChecker;

    public TileUnlockChecker GetTileUnlockChecker { get { return TileUnlockChecker; } }

    public Transform ExpIconRoot;

    public Transform SilverCoinRoot;

    public Transform ExpRoot;


    [SerializeField]
    private Slider ExpSlider;

    [SerializeField]
    private TextMeshProUGUI ExpText;

    [SerializeField]
    private InGameBtnGroup InGameBtnGroup;

    public InGameBtnGroup GetBtnGroup { get { return InGameBtnGroup; } }

    [SerializeField]
    private TextMeshProUGUI InGameSilverCoinText;

    [SerializeField]
    private TextMeshProUGUI CashText;

    [SerializeField]
    private TextMeshProUGUI WaveText;

    [SerializeField]
    private TextMeshProUGUI StageNameText;

    [HideInInspector]
    public GameObject DifficultyRoot;

    [SerializeField]
    private BossHpComponent BossHpComponent;

    public BossHpComponent GetBossHpComponent { get { return BossHpComponent; } }

    private Tweener ExpTextTweener;

    private int lastValue;

    private int NeedExpValue = 0;

    private CompositeDisposable disposables = new();

    [SerializeField]
    private Transform BtnGroup;

    [SerializeField]
    private Button AdsBtn;

    [SerializeField]
    private Transform AdsAreaTr;

    protected override void Awake()
    {
        base.Awake();

        PauseBtn.onClick.AddListener(OnClickPause);
        AdsBtn.onClick.AddListener(OnClickAds);
    }

    private void OnClickAds()
    {
        GameRoot.Instance.UISystem.OpenUI<PopupNoAdsPackages>();
    }

    public void HideBannerCheck()
    {
        ProjectUtility.SetActiveCheck(AdsAreaTr.gameObject, !GameRoot.Instance.ShopSystem.NoInterstitialAds.Value &&
        GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.InterAdOpen));
    }

    public void ActiveBannerCheck(bool active)
    {
        ProjectUtility.SetActiveCheck(AdsAreaTr.gameObject, active);
    }


    public void Init()
    {
        //temp 
        NeedExpValue = 10 * GameRoot.Instance.UserData.Playerdata.InGameUpgradeCountProperty.Value;

        ProjectUtility.SetActiveCheck(PauseBtn.gameObject, GameRoot.Instance.UserData.Stageidx.Value > 2);
        ProjectUtility.SetActiveCheck(DifficultyRoot.gameObject, false);

        HideBannerCheck();

        TileMergeChecker.Init();
        TileWeaponGroup.Init();
        TileUnlockChecker.Init();
        InGameBtnGroup.Init();

        disposables.Clear();

        GameRoot.Instance.UserData.Playerdata.InGameExpProperty.Subscribe(x =>
        {
            ExpCheck(x);
        }).AddTo(disposables);

        GameRoot.Instance.UserData.Ingamesilvercoin.Subscribe(x =>
        {
            InGameSilverCoinText.text = x.ToString();
        }).AddTo(disposables);

        GameRoot.Instance.UserData.Playerdata.IsWaveRestProperty.Subscribe(x =>
        {
            ProjectUtility.SetActiveCheck(BtnGroup.gameObject, x);
        }).AddTo(disposables);

        ProjectUtility.SetActiveCheck(ExpRoot.gameObject, GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.ExpOpen));
        ProjectUtility.SetActiveCheck(BossHpComponent.gameObject, false);

        GameRoot.Instance.UserData.Waveidx.Subscribe(SetWaveText).AddTo(disposables);

        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;

        var displayIdx = stageidx > 60 ? ((stageidx - 1) % 60) + 1 : stageidx;
        StageNameText.text = $"{stageidx}.{Tables.Instance.GetTable<Localize>().GetString($"str_stage_name_{displayIdx}")}";

    }

    public void OnClickPause()
    {
        GameRoot.Instance.UISystem.OpenUI<PopupStageGiveup>();
    }

    public void SetWaveText(int waveidx)
    {
        var stagetd = Tables.Instance.GetTable<WaveInfo>().DataList.FindAll(x => x.stage == GameRoot.Instance.UserData.Stageidx.Value).Count;

        waveidx = stagetd < waveidx ? stagetd : waveidx;

        WaveText.text = $"{waveidx}/{stagetd}";
    }


    public void ExpCheck(int value)
    {
        //exp slider tween
        ExpSlider.DOKill();
        ExpSlider.transform.DORewind();
        const float duration = 0.2f;

        if (value < ExpSlider.value)
        {
            lastValue = GameRoot.Instance.UserData.Playerdata.InGameExpProperty.Value;

            ExpSlider.value = Mathf.Clamp01((float)GameRoot.Instance.UserData.Playerdata.InGameExpProperty.Value / (float)NeedExpValue);
        }


        ExpTextTweener.Kill();
        ExpTextTweener = DOVirtual.Int(lastValue, value, duration, (x) =>
        {
            lastValue = x;
            //set text
            float expPercent = Mathf.Clamp((float)Mathf.Round((float)x / (float)NeedExpValue * 100f), 0, 100);
            ExpText.text = $"{expPercent.ToString("F0")}%";
        })
            .SetUpdate(true)
            .SetEase(Ease.OutQuad);

        ExpSlider.transform.DOScale(1.05f, 0.1f)
            .SetUpdate(true)
            .SetEase(Ease.OutCubic)
            .SetLoops(2, LoopType.Yoyo);
        ExpSlider.DOValue((float)GameRoot.Instance.UserData.Playerdata.InGameExpProperty.Value / (float)NeedExpValue, duration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true)
            .OnComplete(CheckSliderUpgradePossible)
            .OnKill(CheckSliderUpgradePossible);
    }


    private void CheckSliderUpgradePossible() => CheckUpgradePossible(true);

    public void RefreshNeedExpValue()
    {
        NeedExpValue = 10 * GameRoot.Instance.UserData.Playerdata.InGameUpgradeCountProperty.Value;
        ExpSlider.DOKill();
        ExpSlider.transform.DORewind();
        ExpSlider.value = Mathf.Clamp01((float)GameRoot.Instance.UserData.Playerdata.InGameExpProperty.Value / (float)NeedExpValue);
    }

    public void CheckUpgradePossible(bool checkSlider = false)
    {
        // 이미 팝업이 열려있으면 중복 호출 방지
        if (GameRoot.Instance.UISystem.isOpenUI<PopupLevelUpReward>()) return;

        int exp = GameRoot.Instance.UserData.Playerdata.InGameExpProperty.Value;
        if (exp < NeedExpValue) return;
        if (checkSlider && ExpSlider.value < 0.99f) return;

        GameRoot.Instance.UserData.Playerdata.InGameUpgradeCountProperty.Value += 1;
        GameRoot.Instance.UserData.Playerdata.InGameExpProperty.Value -= NeedExpValue;

        NeedExpValue = 10 * GameRoot.Instance.UserData.Playerdata.InGameUpgradeCountProperty.Value;
        GameRoot.Instance.UISystem.OpenUI<PopupLevelUpReward>(popup => popup.Init());
    }




    void OnDisable()
    {
        disposables.Clear();
    }

    void OnDestroy()
    {
        disposables.Clear();
    }

    public void StartBossStage(int bossidx, double bosshop, System.Action onComplete = null)
    {
        ProjectUtility.SetActiveCheck(ExpRoot.gameObject, false);
        ProjectUtility.SetActiveCheck(BossHpComponent.gameObject, true);
        BossHpComponent.Set(bossidx, bosshop, bosshop, onComplete);

    }

    public void BossStageClear()
    {
        ProjectUtility.SetActiveCheck(ExpRoot.gameObject, true);
        ProjectUtility.SetActiveCheck(BossHpComponent.gameObject, false);
        ProjectUtility.SetActiveCheck(DifficultyRoot.gameObject, false);

    }

}
