using System;
using System.Collections.Generic;
using BanpoFri;
using UniRx;

public class ContentsOpenSystem
{
    public enum ContentsOpenType
    {
        TRAININGOPEN = 1,
        CARDOPEN = 2,
        INGAMESLOTCARDOPEN = 3,
        RESULTREWARDAD = 4,
        INGAMEREROLL = 5,
        LobbyReward = 6,
        ExpOpen = 7,
        TileWeaponAd = 8,
        RevivalOpen = 9,
        ReviewPopup = 10,
        EquipBook = 11,
        AttendanceReward = 12,
        HeroUpgradeOpen = 13,
        InterAdOpen = 14,
        ShopOpen = 15,
    }

    private Dictionary<ContentsOpenType, Action<bool>> OpenWaitCallbackDic = new();
    private CompositeDisposable disposables = new CompositeDisposable();
    public int revival_open_enemy_count = 0;

    public void Create()
    {
        disposables.Clear();
        //revival_open_enemy_count = Tables.Instance.GetTable<Define>().GetData("revival_open_enemy_count").value;
        GameRoot.Instance.UserData.Stageidx.SkipLatestValueOnSubscribe().Subscribe(x => RefreshContentsOpen()).AddTo(disposables);
    }

    //특수 조건은 사용하면 안됨
    public void RegisterOpenWaitContentByStage(ContentsOpenType opentype, Action<bool> openCallback)
    {
        var result = ContentsOpenCheck(opentype);
        if (!result)
        {
            if (openCallback != null)
            {
                if (!OpenWaitCallbackDic.ContainsKey(opentype))
                {
                    OpenWaitCallbackDic.Add(opentype, openCallback);
                }
            }
        }
        else
        {
            openCallback?.Invoke(true);
        }
    }

    public void UnLoad()
    {
        OpenWaitCallbackDic.Clear();
    }

    public void UnRegisterOpenWaitContentByStage(ContentsOpenType opentype, Action<bool> openCallback)
    {
        if (openCallback != null)
        {
            if (OpenWaitCallbackDic.ContainsKey(opentype))
            {
                OpenWaitCallbackDic[opentype] -= openCallback;
            }
        }
    }


    public void RefreshContentsOpen()
    {
        var listCompleteKey = new List<ContentsOpenType>();
        foreach (var callback in OpenWaitCallbackDic)
        {
            var result = ContentsOpenCheck(callback.Key);
            if (result && OpenWaitCallbackDic[callback.Key] != null)
            {
                callback.Value?.Invoke(true);
                listCompleteKey.Add(callback.Key);
            }
        }

        foreach (var key in listCompleteKey)
            OpenWaitCallbackDic.Remove(key);
    }


    public bool ContentsOpenCheck(ContentsOpenType opentype)
    {

        switch (opentype)
        {
            case ContentsOpenType.TRAININGOPEN:
                {
                    if (GameRoot.Instance.UserData.Stageidx.Value >= 4)
                    {
                        return true;
                    }
                }
                break;
            case ContentsOpenType.HeroUpgradeOpen:
                {
                    if (GameRoot.Instance.UserData.Herogroudata.Equipplayeridx > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
        }

        var failedcount = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.StageFailedCount, GameRoot.Instance.UserData.Stageidx.Value);

        var td = Tables.Instance.GetTable<ContentsOpenCheck>().GetData((int)opentype);

        if (td != null)
        {
            var stageidx = GameRoot.Instance.UserData.Stageidx.Value;


            return stageidx > td.stage_idx || (stageidx == td.stage_idx && failedcount >= td.failed_count);
        }


        return false;
    }
}
