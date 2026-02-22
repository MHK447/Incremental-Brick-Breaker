using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UniRx;
using System.Linq;


public class DailyResetSystem
{
    public enum ResetType
    {
        AdEquipmentItem = 0,
    }
    public int daily_reward_reset_time { get; private set; }

    public System.DateTime ResetTime { get; private set; }
    public System.DateTime ResetStartTime { get; private set; }

    public IReactiveProperty<int> FreeDailyResetRemindTime = new ReactiveProperty<int>(-1);
    public ReactiveProperty<int> RemainingEnergyTime = new ReactiveProperty<int>(-1);

    public void Create()
    {

        daily_reward_reset_time = (int)Tables.Instance.GetTable<Define>().GetData("daily_reward_reset_time").value / 1000;
        DayInitTime();
    }

    public void DayInitTime()
    {
        var CurTime = TimeSystem.GetCurTime();
        ResetStartTime = ResetTime = new System.DateTime(CurTime.Year, CurTime.Month, CurTime.Day, daily_reward_reset_time, 0, 0);
        if (CurTime.Hour >= daily_reward_reset_time)
        {
            ResetTime = ResetTime.AddDays(1);
        }
        else
        {
            ResetStartTime = ResetTime.AddDays(-1);
        }


        if (GameRoot.Instance.UserData.Resetdaytime == default(System.DateTime))
        {
            GameRoot.Instance.UserData.Resetdaytime = ResetTime;
            DailyReset();
        }
        else
        {
            var diff = ResetStartTime.Subtract(GameRoot.Instance.UserData.Resetdaytime);
            if (diff.TotalSeconds >= 0)
            {
                GameRoot.Instance.UserData.Resetdaytime = ResetTime;
                DailyReset();
            }
        }
    }


    public void UpdateOneSecond()
    {
        if (GameRoot.Instance.UserData.Resetdaytime != default(System.DateTime))
        {
            var CurTime = TimeSystem.GetCurTime();

            var diff = GameRoot.Instance.UserData.Resetdaytime.Subtract(CurTime);

            FreeDailyResetRemindTime.Value = (int)diff.TotalSeconds;

            if (diff.TotalSeconds < 0)
            {
                DayInitTime();
            }
        }
    }

    public void DailyReset()
    {
        ResetItem(ResetType.AdEquipmentItem);
        ResetFreeCash();
    }

    public bool StarterPackageRewardCheck()
    {
        if (!GameRoot.Instance.UserData.Starterpackdata.Isbuy.Value || GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.StarterPackage) >= 3) return false;

        var packageBuyTime = (int)(GameRoot.Instance.UserData.Starterpackdata.Starterpackbuytime - TimeSystem.GetCurTime()).TotalSeconds;

        return packageBuyTime <= 0;
    }


    public void ResetFreeCash()
    {
        GameRoot.Instance.UserData.ResetRecordCount(Config.RecordCountKeys.FreeCashCount);
        GameRoot.Instance.GameNotification.UpdateNotification(GameNotificationSystem.NotificationCategory.ShopAdCash);

    }


    public void ResetItem(ResetType resetType)
    {
        switch (resetType)
        {
            case ResetType.AdEquipmentItem:
                {
                    GameRoot.Instance.UserData.ResetRecordCount(Config.RecordCountKeys.AdsEquipCount, 0);
                }
                break;
        }
    }

    public void GiveStarterPackageReward(System.Action nextaction)
    {
        // // 여기에 보상 지급 로직 구현
        var curTime = TimeSystem.GetCurTime();


        GameRoot.Instance.UserData.Starterpackdata.Starterpackbuytime = new System.DateTime(curTime.AddSeconds(86400).Ticks);


        GameRoot.Instance.UserData.AddRecordCount(Config.RecordCountKeys.StarterPackage, 1);

        var getcount = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.StarterPackage);

        var starterpackagetd = Tables.Instance.GetTable<StarerPackageRewardInfo>().GetData(getcount);

        if (starterpackagetd != null)
        {
            var rewardlist = new List<RewardData>();
            for (int i = 0; i < starterpackagetd.reward_type.Count; i++)
            {
                var newrewarddata = new RewardData(starterpackagetd.reward_type[i], starterpackagetd.reward_idx[i], starterpackagetd.reward_value[i]);
                rewardlist.Add(newrewarddata);
            }

            GameRoot.Instance.UISystem.OpenUI<PagePurchaseConfirm>(popup => popup.Set(rewardlist), nextaction);
        }
    }

}
