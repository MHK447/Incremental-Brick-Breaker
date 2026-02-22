using UnityEngine;
using UniRx;
using BanpoFri;


public class AttendanceSystem
{
    public IReactiveProperty<int> AttendanceTimeProperty = new ReactiveProperty<int>(0);
    public void Create()
    {
        // if (GameRoot.Instance.UserData.Attendancetime == new System.DateTime())
        // {
        //     GameRoot.Instance.UserData.Attendancetime = TimeSystem.GetCurTime();
        //     GameRoot.Instance.UserData.ResetRecordCount(Config.RecordCountKeys.AttendanceCount);
        // }
    }


    public void Reset()
    {
        if (!GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.AttendanceReward)) return;

        var curTime = TimeSystem.GetCurTime();

        GameRoot.Instance.UserData.Attendancetime = new System.DateTime(curTime.AddSeconds(86400).Ticks);

        AttendanceTimeProperty.Value = (int)(GameRoot.Instance.UserData.Attendancetime - curTime).TotalSeconds;

        GameRoot.Instance.UserData.AddRecordCount(Config.RecordCountKeys.AttendanceCount, 1);

        GameRoot.Instance.UserData.ResetRecordCount(Config.RecordCountKeys.AdAttendanceCount);


        if (GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.AttendanceCount) >= 7)
        {
            GameRoot.Instance.UserData.ResetRecordCount(Config.RecordCountKeys.AttendanceCount, 0);
        }
    }

    public bool IsAttendanceRewardCheck()
    {
        return AttendanceTimeProperty.Value <= 0 || GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.AdAttendanceCount) == 0;
    }


    public void FirstStartCheck()
    {
        if (GameRoot.Instance.UserData.Attendancetime == new System.DateTime() && GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.AttendanceReward))
        {
            Reset();
        }
    }
    public void UpdateOneSecond()
    {
        // var curTime = TimeSystem.GetCurTime();


        // AttendanceTimeProperty.Value = (int)(GameRoot.Instance.UserData.Attendancetime - curTime).TotalSeconds;


        // if (AttendanceTimeProperty.Value <= 0)
        // {
        //     GameRoot.Instance.UserData.ResetRecordCount(Config.RecordCountKeys.AdAttendanceCount);
        // }
    }
}
