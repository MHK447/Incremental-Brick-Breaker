using System;
using System.Collections.Generic;
using UniRx;
using Google.FlatBuffers;
using BanpoFri;

public partial class UserDataSystem
{



}

public class StageRewardBoxData
{
    public int Boxidx { get; set; } = 0;
    public DateTime Boxtime { get; set; } = new DateTime();
    public bool Isopenstart { get; set; } = false;

    public int BoxOrder { get; set; } = 0;

    public int BoxGetStageIdx { get; set; } = 0;


    public IReactiveProperty<int> CurBoxTimeProperty = new ReactiveProperty<int>(0);

    public void TimeUpdate()
    {
        var curtime = TimeSystem.GetCurTime();

        CurBoxTimeProperty.Value = (int)(Boxtime - curtime).TotalSeconds;
    }

    public void TimeDeCline(int second)
    {
        if(!Isopenstart)
        {
            return;
        }

        Boxtime = Boxtime.AddSeconds(-second);

        TimeUpdate();
    }

    public int QuickUnLockValue()
    {
        if(!Isopenstart)
        {
            var time = Tables.Instance.GetTable<RewardBoxInfo>().GetData(Boxidx).time;
            return (time + 59) / 60;
        }

        // 1분당 잼 1개 (올림 처리)
        return ((CurBoxTimeProperty.Value + 59) / 60) * 3;
    }


    public void StartBox()
    {
        var curtime = TimeSystem.GetCurTime();

        var td = Tables.Instance.GetTable<RewardBoxInfo>().GetData(Boxidx);

        Boxtime = curtime + TimeSpan.FromSeconds(td.time);

        CurBoxTimeProperty.Value = (int)(Boxtime - curtime).TotalSeconds;

        Isopenstart = true;
    }

}
