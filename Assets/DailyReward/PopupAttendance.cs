using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UniRx;

[UIPath("UI/Popup/PopupAttendance")]
public class PopupAttendance : CommonUIBase
{
    [SerializeField]
    private List<AttendanceComponent> AttendanceComponentList = new List<AttendanceComponent>();

    [SerializeField]
    private Button BaseRewardBtn;


    [SerializeField]
    private AdsButton AdsRewardBtn;

    [SerializeField]
    private TextMeshProUGUI RewardAdText;
    [SerializeField]
    private TextMeshProUGUI TicketRewardAdText;

    [SerializeField]
    private TextMeshProUGUI AttendanceTimeText;

    [SerializeField]
    private Transform ButtonRoot;

    [SerializeField]
    private Transform TimeRoot;

    private CompositeDisposable disposables = new CompositeDisposable();



    protected override void Awake()
    {
        base.Awake();

        BaseRewardBtn.onClick.AddListener(OnClickBaseReward);
        AdsRewardBtn.AddListener(TpMaxProp.AdRewardType.Attendance, OnClickAdsReward);
    }


    public void Init()
    {
        for (int i = 0; i < AttendanceComponentList.Count; i++)
        {
            AttendanceComponentList[i].Set(i + 1);
        }

        var adcount = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.AdAttendanceCount);
        var attendancecount = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.AttendanceCount);


        bool attendancecheck = true;
        if (attendancecount == 0)
        {
            attendancecheck = attendancecount == 0 && GameRoot.Instance.AttendanceSystem.AttendanceTimeProperty.Value <= 0;
        }
        ProjectUtility.SetActiveCheck(AdsRewardBtn.gameObject, adcount < 1 && attendancecount < 6 && attendancecheck);
        ProjectUtility.SetActiveCheck(BaseRewardBtn.gameObject, GameRoot.Instance.AttendanceSystem.AttendanceTimeProperty.Value <= 0);


        ProjectUtility.SetActiveCheck(ButtonRoot.gameObject, AdsRewardBtn.gameObject.activeSelf || BaseRewardBtn.gameObject.activeSelf);

        ProjectUtility.SetActiveCheck(TimeRoot.gameObject, GameRoot.Instance.AttendanceSystem.AttendanceTimeProperty.Value > 0);


        var count = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.AttendanceCount);

        RewardAdText.text = TicketRewardAdText.text = GameRoot.Instance.AttendanceSystem.AttendanceTimeProperty.Value <= 0 ? "str_get_double" : "str_retry";


        disposables.Clear();

        GameRoot.Instance.AttendanceSystem.AttendanceTimeProperty.Subscribe(x =>
        {
            AttendanceTimeText.text = ProjectUtility.GetTimeStringFormattingShort(x);
        }).AddTo(disposables);
    }


    public void OnClickBaseReward()
    {
        GetReward(false, GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.AttendanceCount) + 1);
        GameRoot.Instance.AttendanceSystem.Reset();
        Init();
        GameRoot.Instance.WaitTimeAndCallback(1f, () => Hide());
    }


    public void OnClickAdsReward()
    {
        if (GameRoot.Instance.AttendanceSystem.AttendanceTimeProperty.Value <= 0)
        {
            GetReward(true, GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.AttendanceCount) + 1, true);
            GameRoot.Instance.AttendanceSystem.Reset();
        }
        else
        {
            GetReward(false, GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.AttendanceCount), true);
        }

        GameRoot.Instance.UserData.AddRecordCount(Config.RecordCountKeys.AdAttendanceCount, 1);

        Init();
        GameRoot.Instance.WaitTimeAndCallback(1f, () => Hide());
    }


    public void GetReward(bool doublereward, int rewardorder, bool isad = false)
    {
        var td = Tables.Instance.GetTable<AttendanceInfo>().GetData(rewardorder);

        int multi = doublereward ? 2 : 1;

        if (td != null)
        {
            for (int i = 0; i < td.reward_type.Count; i++)
            {
                ProjectUtility.SetRewardAndEffect(td.reward_type[i], td.reward_idx[i], td.reward_value[i] * multi);
            }
        }

        int adcheck = isad ? 1 : 0;
    }

    void OnDestroy()
    {
        disposables.Clear();
    }


}
