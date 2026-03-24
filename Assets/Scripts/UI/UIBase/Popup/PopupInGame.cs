using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine.Rendering;
using UniRx;
[UIPath("UI/Popup/PopupInGame")]
public class PopupInGame : UIBase
{
    [SerializeField]
    private TextMeshProUGUI LevelText;

    [SerializeField]
    private Slider HpProgress;

    [SerializeField]
    private Slider WaveProgress;

    [SerializeField]
    [Tooltip("웨이브 진행도가 목표치까지 차오르는 시간(초). 0이면 맵 이동 시간(InGameBaseStage.WaveMoveDuration)과 동기화")]
    private float waveProgressFillDuration = 0f;

    [SerializeField]
    private List<Image> StageNodeList = new List<Image>();

    [SerializeField]
    private TextMeshProUGUI BombCountText;

    [SerializeField]
    private Image CoolTimeBgImg;

    [SerializeField]
    private UpgradeImgComponent UpgradeImgComponent;

    [SerializeField]
    private IncreaseUpgradeGroup InCreaseUpgradeGroup;

    [SerializeField]
    private Transform StageAreaRoot;

    [SerializeField]
    private IncreaBtnGroupComponent InCreaBtnGroupComponent;

    [SerializeField]
    private GameObject ArrowObj;


    public IncreaBtnGroupComponent GetInCreaBtnGroupComponent { get { return InCreaBtnGroupComponent; } }

    private CompositeDisposable disposables = new CompositeDisposable();
    private float targetWaveProgress = 0f;
    private float waveProgressStartValue = 0f;
    private float waveProgressStartTime = 0f;

    public void Init()
    {
        disposables.Clear();

        GameRoot.Instance.UserData.InGamePlayerData.WeaponFallCountProperty.Subscribe(x =>
        {
            BombCountText.text = x.ToString();
        }).AddTo(disposables);

        // 낙하 무기 쿨타임: fillAmount 1 → 0 (쿨 진행에 따라 감소)
        GameRoot.Instance.UserData.InGamePlayerData.WeaponFallCooldownProgressProperty.Subscribe(progress =>
        {
            if (CoolTimeBgImg != null)
                CoolTimeBgImg.fillAmount = 1f - progress;
        }).AddTo(disposables);

        GameRoot.Instance.UserData.InGamePlayerData.StartHpProperty.Subscribe(x => SetHpProgress(x)).AddTo(disposables);
        GameRoot.Instance.UserData.InGamePlayerData.CurHppProperty.Subscribe(x => SetHpProgress(x)).AddTo(disposables);

        StageNodeCheck();
        if (WaveProgress != null)
            WaveProgress.value = targetWaveProgress;

        GameRoot.Instance.UserData.Waveidx.Subscribe(x =>
        {
            StageNodeCheck();
        }).AddTo(disposables);

        GameRoot.Instance.UserData.Playerlevel.Subscribe(x =>
        {
            LevelText.text = $"Lv.{x}";
        }).AddTo(disposables);

        InCreaseUpgradeGroup.Init();

        ProjectUtility.SetActiveCheck(UpgradeImgComponent.gameObject, false);

        ProjectUtility.SetActiveCheck(StageAreaRoot.gameObject, GameRoot.Instance.IncreaMentalSystem.IsUnlocked(IncreaMentalType.TruckUnlock));

        var finddata = GameRoot.Instance.IncreaMentalSystem.FindData(2);
        if (finddata != null)
        {
            finddata.Level
                .Subscribe(level => ProjectUtility.SetActiveCheck(ArrowObj, level == 0))
                .AddTo(disposables);
        }
        else
        {
            ProjectUtility.SetActiveCheck(ArrowObj, false);
        }
    }

    public void SetHpProgress(int hp)
    {
        var starthp = GameRoot.Instance.UserData.InGamePlayerData.StartHpProperty.Value;

        HpProgress.value = (float)hp / (float)starthp;
    }


    public void StageNodeCheck()
    {
        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;
        var waveidx = GameRoot.Instance.UserData.Waveidx.Value;

        var tdlist = Tables.Instance.GetTable<WaveInfo>().DataList.Where(x => x.stage == stageidx).ToList();

        for (int i = 0; i < tdlist.Count; i++)
        {
            if (i >= StageNodeList.Count) break;

            StageNodeList[i].sprite = waveidx >= i + 1 ? AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, "Common_Icon_LevelupFill")
            : AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, "Common_Icon_LevelupEmpty");
        }

        float newTarget = tdlist.Count > 0 ? Mathf.Clamp01((float)waveidx / (float)tdlist.Count) : 0f;
        // 목표가 바뀔 때마다 항상 시작값/시작시간 갱신 (안 하면 waveProgressStartTime=0 때문에 t가 바로 1이 되어 바로 꽉 참)
        if (WaveProgress != null)
        {
            waveProgressStartValue = WaveProgress.value;
            waveProgressStartTime = Time.time;
        }
        targetWaveProgress = newTarget;
    }

    void Update()
    {
        if (WaveProgress == null) return;
        if (Mathf.Approximately(WaveProgress.value, targetWaveProgress)) return;

        // 목표까지 정확히 duration 초 걸리도록 Lerp (맵 이동 시간과 동기화)
        float duration = InGameBaseStage.WaveMoveDuration;
        float t = duration > 0f ? Mathf.Clamp01((Time.time - waveProgressStartTime) / duration) : 1f;
        WaveProgress.value = Mathf.Lerp(waveProgressStartValue, targetWaveProgress, t);
    }

    void OnDestroy()
    {
        disposables.Clear();
    }

    void OnDisable()
    {
        disposables.Clear();
    }


    public void RefreshTruckUI()
    {
        ProjectUtility.SetActiveCheck(StageAreaRoot.gameObject, GameRoot.Instance.IncreaMentalSystem.IsUnlocked(IncreaMentalType.TruckUnlock));
    }

    public void UpgradeImgHover(int upgradeidx , Vector3 pos)
    {
        ProjectUtility.SetActiveCheck(UpgradeImgComponent.gameObject, true);
        UpgradeImgComponent.Set(upgradeidx);
        UpgradeImgComponent.transform.position = new Vector3(pos.x, pos.y + 100f, 0);
    }

    public void UpgradeImgHoverExit()
    {
        ProjectUtility.SetActiveCheck(UpgradeImgComponent.gameObject, false);
    }
}

