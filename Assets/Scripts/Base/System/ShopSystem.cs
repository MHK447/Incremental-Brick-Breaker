using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using UniRx;
using System.Linq;
using System;

public enum InAppPurchaseLocation
{
    none = -1,
    shop = 0,
    popup = 1,
    hud,
    nomoney,
    other,
    banner,
}

public enum ShopState
{
    None,
    Purchasing,
}

public enum ShopCurrencyIdx
{
    Gold_1 = 1,
    Gold_2 = 2,
    Gold_3 = 3,
    Gold_4 = 4,
    Gold_5 = 5,
    Gold_6 = 6,
    Cash_Free = 100,
    Cash_1 = 101,
    Cash_2 = 102,
    Cash_3 = 103,
    Cash_4 = 104,
    Cash_5 = 105,
    Cash_6 = 106,
    Material_1 = 1001,
    Material_2 = 1002,
    Material_3 = 1003,
    Material_4 = 1004,
    Material_5 = 1005,
    Material_6 = 1006,
}

public enum PackageType
{
    None = -1,

    NoAds_100 = 100,
    NoAds_101 = 101,
    StarterPackage_1001 = 1001,


}



public class ShopSystem
{
    public ShopState state { get; private set; } = ShopState.None;

    private System.IDisposable vipSubscription;

    public InAppPurchaseLocation curLocation = InAppPurchaseLocation.none;


    public System.DateTime ResetTime { get; private set; }
    public System.DateTime ResetStartTime { get; private set; }
    public enum ProductShopType
    {
        ShopCurrencyGem_01 = 101,
        ShopCurrencyGem_02 = 102,
        ShopCurrencyGem_03 = 103,
        ShopCurrencyGem_04 = 104,
        ShopCurrencyGem_05 = 105,
        ShopCurrencyGem_06 = 106,

        FreeGem = 1,
        AdGem = 2,

        GemRush_01 = 1001,
        GemRush_02 = 1002,
        GemRush_03 = 1003,
    }

    public ReactiveProperty<bool> IsVipProperty = new ReactiveProperty<bool>(false);

    public IReactiveProperty<int> FreeAdRemindTime = new ReactiveProperty<int>(-1);

    private float curdeltatime = 0f;

    private float InterAdTime = 240f; // 기본값 4분
    private float currentInterAdTimer = 0f;
    private bool isInterAdReady = false;

    public int stage_energy_consume = 0;

    public int daily_reward_reset_time = 0;

    public ReactiveProperty<bool> NoInterstitialAds = new ReactiveProperty<bool>(false);

    public ReactiveProperty<bool> NoRewardedAds = new ReactiveProperty<bool>(false);



    public void Create()
    {
        currentInterAdTimer = 0f;

        vipSubscription?.Dispose();
        vipSubscription = IsVipProperty.Subscribe(isVip =>
        {
            isInterAdReady = !isVip;
        });

        DayInitTime();
        CheckNoads();
    }

    public void UpdateOneTimeSecond()
    {
        // VIP가 아닐 때만 광고 타이머 증가
        if (isInterAdReady)
        {
            currentInterAdTimer += 1f;


        }
    }

    public void CheckNoads()
    {
        string[] noAdPackageIds = Tables.Instance.GetTable<ShopProduct>().DataList.Where(x =>
        {
            return x.idx == (int)PackageType.NoAds_100 || x.idx == (int)PackageType.NoAds_101;
        }).Select(x => x.product_id).ToArray();
        foreach (var id in noAdPackageIds)
        {
            NoInterstitialAds.Value |= GameRoot.Instance.UserData.BuyInappIds.Contains(id);
        }
    }


    // 광고 표시 시간 설정 (초 단위)
    public void SetInterAdTime(float seconds)
    {
        InterAdTime = seconds;
    }

    // 현재 타이머 리셋
    public void ResetInterAdTimer()
    {
        currentInterAdTimer = 0f;
    }

    // 광고 표시 강제 활성화/비활성화
    public void SetInterAdEnabled(bool enabled)
    {
        isInterAdReady = enabled;

        // 비활성화 시 타이머도 리셋
        if (!enabled)
        {
            currentInterAdTimer = 0f;
        }
    }

    public void UpdateOneSecond()
    {
        if (GameRoot.Instance.UserData.Dayinitialtime != default(System.DateTime))
        {
            var CurTime = TimeSystem.GetCurTime();

            var diff = GameRoot.Instance.UserData.Dayinitialtime.Subtract(CurTime);
            FreeAdRemindTime.Value = (int)diff.TotalSeconds;
            if (diff.TotalSeconds < 0)
            {
                DayInitTime();
                //TestMinuteInitTime();
            }
        }

        if (GameRoot.Instance.UserData.Starterpackdata.Isbuy.Value && GameRoot.Instance.UserData.Starterpackdata.Starterpackbuytime != default(DateTime))
        {
            GameRoot.Instance.UserData.Starterpackdata.PackageTimeProperty.Value = (int)(GameRoot.Instance.UserData.Starterpackdata.Starterpackbuytime - TimeSystem.GetCurTime()).TotalSeconds;
        }
    }


    public string OriginalPackageCost(int packageidx, bool isdouble = false)
    {
        var td = Tables.Instance.GetTable<ShopProduct>().GetData(packageidx);

        if (td != null)
        {
            var product = GameRoot.Instance.InAppPurchaseManager.GetProduct(td.product_id);
            if (product?.metadata == null)
                return string.Empty;

            var price = (float)product.metadata.localizedPrice;

            float returnvalue = 0f;

            if (!isdouble)
                returnvalue = price / ((100 - (float)td.sale) / 100f);
            else
                returnvalue = price / ((100 - (float)td.double_sale) / 100f);

            return $"{returnvalue:F1}";

        }

        return string.Empty;
    }


    public void RewardPay(int rewardtype, int rewardidx, int rewardvalue)
    {
        switch (rewardtype)
        {
            case (int)Config.RewardType.Currency:
                {
                    switch (rewardidx)
                    {
                        case (int)Config.CurrencyID.Cash:
                            {
                                GameRoot.Instance.UserData.SetReward(rewardtype, rewardidx, rewardvalue);
                            }
                            break;
                            // case (int)Config.CurrencyID.EnergyMoney:
                            //     {
                            //         GameRoot.Instance.UserData.Energycoin.Value += rewardvalue;
                            //     }
                            //     break;
                    }

                }
                break;
        }
    }


    public void InappPurchase(string product_id, int itemIdx, InAppPurchaseLocation location, System.Action OnSuccess, System.Action OnFailed = null)
    {
#if UNITY_EDITOR
        if (!GameRoot.Instance.UserData.BuyInappIds.Contains(product_id))
            GameRoot.Instance.UserData.BuyInappIds.Add(product_id);

        OnSuccess?.Invoke();
        return;
#endif
        GameRoot.Instance.Loading.Show(false);
        state = ShopState.Purchasing;

        curLocation = location;
        GameRoot.Instance.InAppPurchaseManager.BuyProductID(product_id, itemIdx, result =>
        {
            GameRoot.Instance.Loading.Hide(true);
            state = ShopState.None;

            if (result == InAppPurchaseManager.Result.Success)
            {
                if (!GameRoot.Instance.UserData.BuyInappIds.Contains(product_id))
                {
                    GameRoot.Instance.UserData.BuyInappIds.Add(product_id);
                }




                OnSuccess?.Invoke();
                //GameRoot.Instance.UserData.Save(true);
            }
            else
                OnFailed?.Invoke();
        });


        //logs
        List<TpParameter> parameters = new List<TpParameter>();
        parameters.Add(new TpParameter("idx", itemIdx));
        parameters.Add(new TpParameter("af_product_id", product_id));
        parameters.Add(new TpParameter("stage", GameRoot.Instance.UserData.Stageidx.Value));
        parameters.Add(new TpParameter("place", GameRoot.Instance.ShopSystem.curLocation.ToString()));
        GameRoot.Instance.PluginSystem.AnalyticsProp.AllEvent(IngameEventType.None,
            "m_purchase_popup", parameters);
    }





    public void PurchasePackage(int packageidx, System.Action successCallback = null, InAppPurchaseLocation location = InAppPurchaseLocation.popup, System.Action failCallback = null)
    {
        var td = Tables.Instance.GetTable<ShopProduct>().GetData(packageidx);

        if (td != null)
        {

            GameRoot.Instance.ShopSystem.InappPurchase(td.product_id, packageidx, location, () =>
            {
                var td = Tables.Instance.GetTable<ShopProduct>().GetData(packageidx);

                if (td != null)
                {
                    List<RewardData> rewarddatas = new List<RewardData>();

                    for (int i = 0; i < td.reward_type.Count; i++)
                    {
                        if (td.reward_type[i] > 0)
                        {
                            var item = new RewardData(td.reward_type[i], td.reward_idx[i], td.reward_value[i]);

                            rewarddatas.Add(item);
                        }
                    }

                    if (rewarddatas.Count > 0)
                    {
                        GameRoot.Instance.UISystem.OpenUI<PagePurchaseConfirm>(popup => popup.Set(rewarddatas));
                    }

                    if (packageidx == (int)PackageType.StarterPackage_1001)
                    {
                        GameRoot.Instance.UserData.Starterpackdata.Isbuy.Value = true;
                    }

                    successCallback?.Invoke();
                }
            }, failCallback);

        }
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


        if (GameRoot.Instance.UserData.Dayinitialtime == default(System.DateTime))
        {
            Reset();
        }
        else
        {
            var diff = ResetStartTime.Subtract(GameRoot.Instance.UserData.Dayinitialtime);
            if (diff.TotalSeconds >= 0)
            {
                Reset();
            }
        }
    }


    public void Reset()
    {

        GameRoot.Instance.UserData.Dayinitialtime = ResetTime;
        GameRoot.Instance.UserData.ResetRecordCount(Config.RecordCountKeys.AdGemCount, 0);
        GameRoot.Instance.UserData.ResetRecordCount(Config.RecordCountKeys.FreeGemCount, 0);
    }

    public void ShowInterAd()
    {
        var interopen = GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.InterAdOpen);

        var showadcout = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.ShowInterAdCount);

        if (interopen && showadcout >= 1 && GameRoot.Instance.UserData.Waveidx.Value == 4)
        {
            GameRoot.Instance.UserData.ResetRecordCount(Config.RecordCountKeys.ShowInterAdCount, 0);
            GameRoot.Instance.PluginSystem.ADProp.ShowInterstitialAD(TpMaxProp.AdInterType.None);

            GameRoot.Instance.UISystem.OpenUI<PopupNoAdsPackages>();
        }
    }



}
