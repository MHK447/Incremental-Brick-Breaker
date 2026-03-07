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

    private CompositeDisposable disposables = new CompositeDisposable();
    private float targetWaveProgress = 0f;
    private float waveProgressStartValue = 0f;
    private float waveProgressStartTime = 0f;

    public void Init()
    {
        disposables.Clear();

        GameRoot.Instance.UserData.InGamePlayerData.StartHpProperty.Subscribe(x => SetHpProgress(x)).AddTo(disposables);
        GameRoot.Instance.UserData.InGamePlayerData.CurHppProperty.Subscribe(x => SetHpProgress(x)).AddTo(disposables);

        StageNodeCheck();
        if (WaveProgress != null)
            WaveProgress.value = targetWaveProgress;

        GameRoot.Instance.UserData.Waveidx.Subscribe(x=> {
            StageNodeCheck();
        }).AddTo(disposables);

        GameRoot.Instance.UserData.Playerlevel.Subscribe(x=> {
            LevelText.text = $"Lv.{x}";
        }).AddTo(disposables);
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
            if(i >= StageNodeList.Count) break;

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
}

