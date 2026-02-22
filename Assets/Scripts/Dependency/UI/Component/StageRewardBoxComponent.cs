using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using UniRx;
public class StageRewardBoxComponent : MonoBehaviour
{
    public enum Status
    {
        None,
        Ready,
        TimeWait,
        Open,
    }

    [SerializeField]
    private Transform ReadyRoot;

    [SerializeField]
    private Transform TimeWaitRoot;

    [SerializeField]
    private Transform OpenRoot;


    [SerializeField]
    private TextMeshProUGUI TimeText;

    [SerializeField]
    private TextMeshProUGUI CashText;

    [SerializeField]
    private TextMeshProUGUI DayText;

    [SerializeField]
    private TextMeshProUGUI StageText;


    [SerializeField]
    private Button BoxBtn;


    [SerializeField]
    private List<Image> BoxImgList = new List<Image>();

    private int BoxIdx = 0;

    private Status CurStatus = Status.None;

    private StageRewardBoxData BoxData = null;

    private CompositeDisposable disposables = new CompositeDisposable();

    private int Order = 0;

    void Awake()
    {
        BoxBtn.onClick.AddListener(OnClickBox);
    }

    public void Clear()
    {
        ProjectUtility.SetActiveCheck(ReadyRoot.gameObject, false);
        ProjectUtility.SetActiveCheck(TimeWaitRoot.gameObject, false);
        ProjectUtility.SetActiveCheck(OpenRoot.gameObject, false);

        BoxIdx = -1;
        Order = -1;
    }

    public void Set(int boxidx, int order)
    {
        BoxIdx = boxidx;

        Order = order;

        BoxData = GameRoot.Instance.UserData.Stagerewardboxgroup.Stagerewardboxdatas[order];

        var td = Tables.Instance.GetTable<RewardBoxInfo>().GetData(BoxIdx);

        if (td != null)
        {
            SetStatus();

            DayText.text = ProjectUtility.GetTimeStringFormattingShort(td.time);

            StageText.text = Tables.Instance.GetTable<Localize>().GetFormat("unlock_stage_desc" , BoxData.BoxGetStageIdx);

            disposables.Clear();

            BoxData.CurBoxTimeProperty.Subscribe(x => { SetTimeText(x); }).AddTo(disposables);

            foreach (var boximg in BoxImgList)
            {
                boximg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, td.image);
            }

        }
    }

    public void SetStatus()
    {
        if (BoxData == null) return;

        BoxData.TimeUpdate();

        CurStatus = Status.None;

        if (BoxData.Isopenstart)
        {
            if (BoxData.CurBoxTimeProperty.Value <= 0)
            {
                CurStatus = Status.Open;
            }
            else
            {
                CurStatus = Status.TimeWait;
            }
        }
        else
        {
            CurStatus = Status.Ready;
        }



        ProjectUtility.SetActiveCheck(ReadyRoot.gameObject, CurStatus == Status.Ready);
        ProjectUtility.SetActiveCheck(TimeWaitRoot.gameObject, CurStatus == Status.TimeWait);
        ProjectUtility.SetActiveCheck(OpenRoot.gameObject, CurStatus == Status.Open);

    }

    public void SetTimeText(int time)
    {
        TimeText.text = ProjectUtility.GetTimeStringFormattingShort(time);

        CashText.text = BoxData.QuickUnLockValue().ToString();

        if (time <= 0)
        {
            SetStatus();
        }
    }


    public void OnClickBox()
    {
        var td = Tables.Instance.GetTable<RewardBoxInfo>().GetData(BoxIdx);

        if (td != null)
        {
            if (CurStatus == Status.Open)
            {
                GameRoot.Instance.UserData.Stagerewardboxgroup.GetBoxReward(BoxIdx);
                GameRoot.Instance.UserData.Stagerewardboxgroup.RemoveBox(BoxData);
            }
            else if (CurStatus == Status.TimeWait || CurStatus == Status.Ready)
            {
                GameRoot.Instance.UISystem.OpenUI<PopupBoxRewardTime>(page => page.Set(BoxData));
            }
        }

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

