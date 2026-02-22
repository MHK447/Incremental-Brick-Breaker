using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using BanpoFri;
using UniRx;
using UnityEngine;
using AppsFlyerSDK;

#if !UNITY_EDITOR
//using AmazonAds;
#endif

public class TpMaxProp
{
    private CompositeDisposable adDisposables = new CompositeDisposable();
#if UNITY_ANDROID
    string adInterstitialUnitId = "f9595ccc7cde339f";
    string adRewardUnitId = "0557a51ec404faa8";
    string adBannerUnitId = "461e933ab4a5c97c";
    string adAppOpenUnitId = "b0c92a35413261f4";
    string amazone_inter_id = "dc92b8fc-e3c2-4865-90a1-3b92c4f0bdc2";
    string amazone_reward_id = "7641dcdf-6189-4988-ad12-14821ea6099f";
#else
    string adInterstitialUnitId = "34359bb4201da8ae";
    string adRewardUnitId = "60338cb76b5409e0";
    string adBannerUnitId = "7a7e99ef635bd866";
    string adAppOpenUnitId = "02d72dd41598886f";
    string amazone_inter_id = "d4a26539-a502-4433-9f84-7ebf11dcea0a";
    string amazone_reward_id = "fb6bc6e8-5e23-424e-a2a9-d33099968607";
#endif
    System.Action<bool> OnRewardResult;

    public enum AdSkipType
    {
        Ads = 0,
        Ticket = 1,
        NoAds = 2,
    }
    public enum AdRewardType
    {
        None = -1,
        Revival = 0,
        StageResult = 1,
        LevelUpReroll = 2,
        LuckyChoice = 3,
        InSufficientSilverCoint = 4,
        AdWeapon = 5,
        AdTile = 6,
        AdGold = 7,
        InGameSilverCoin = 8,
        AddKing = 9,
        StageClearAdReward = 10,
        DailyEquipmentAdReward = 11,

        InSufficientGoldCoin = 12,

        Attendance = 9999,

    }

    public enum AdInterType
    {
        None = -1,
        Stage,
    }

    private AdRewardType adType = AdRewardType.None;

    DateTime adLoadedTime = default(DateTime);

    public bool showingAd { get; private set; } = false;
    private UnityEngine.Coroutine waitCo = null;
    private UnityEngine.Coroutine showingCo = null;

    private Action cb_loadRewardedAd = null;

    private bool isInterstitialAds = false;
    private bool isRewardAdsinit = false;
    public bool isBanner = false;
    public bool isShowBanner = false;
    private bool isInterstitialAdsLoad = false;

    // Button Dim
    public Action<bool> listenerRewardedAd;
    public void AddRewardedAdEvent(Action<bool> cb) { listenerRewardedAd += cb; }
    public void RemoveRewardedAdEvent(Action<bool> cb) { listenerRewardedAd -= cb; }
    public bool IsRewardedAd() { return MaxSdk.IsRewardedAdReady(adRewardUnitId); }
    //

    private int appear_ad_skip_purchase_popup = -1;

    public System.Action OnInterPaidAction = null;

    public bool IsInterstitialAds() { return isInterstitialAds; }
    public bool IsRewardAdsInit() { return isRewardAdsinit; }

    public void InitializeInterstitialAds()
    {
        // Attach callback
        Observable.FromEvent<Action<string, MaxSdkBase.AdInfo>, (string adUnitId, MaxSdkBase.AdInfo adInfo)>(
            h => (adUnitId, adInfo) => h((adUnitId, adInfo)),
            h => MaxSdkCallbacks.Interstitial.OnAdLoadedEvent += h,
            h => MaxSdkCallbacks.Interstitial.OnAdLoadedEvent -= h).ObserveOnMainThread().Subscribe(x => { OnInterstitialLoadedEvent(x.adUnitId, x.adInfo); }).AddTo(adDisposables);
        Observable.FromEvent<Action<string, MaxSdkBase.ErrorInfo>, (string adUnitId, MaxSdkBase.ErrorInfo adInfo)>(
            h => (adUnitId, adInfo) => h((adUnitId, adInfo)),
            h => MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent += h,
            h => MaxSdkCallbacks.Interstitial.OnAdLoadFailedEvent -= h).ObserveOnMainThread().Subscribe(x => { OnInterstitialFailedEvent(x.adUnitId, x.adInfo); }).AddTo(adDisposables);
        Observable.FromEvent<Action<string, MaxSdkBase.ErrorInfo, MaxSdkBase.AdInfo>, (string adUnitId, MaxSdkBase.ErrorInfo adError, MaxSdkBase.AdInfo adInfo)>(
            h => (adUnitId, adError, adInfo) => h((adUnitId, adError, adInfo)),
            h => MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent += h,
            h => MaxSdkCallbacks.Interstitial.OnAdDisplayFailedEvent -= h).ObserveOnMainThread().Subscribe(x => { InterstitialFailedToDisplayEvent(x.adUnitId, x.adError, x.adInfo); }).AddTo(adDisposables);
        Observable.FromEvent<Action<string, MaxSdkBase.AdInfo>, (string adUnitId, MaxSdkBase.AdInfo adInfo)>(
            h => (adUnitId, adInfo) => h((adUnitId, adInfo)),
            h => MaxSdkCallbacks.Interstitial.OnAdHiddenEvent += h,
            h => MaxSdkCallbacks.Interstitial.OnAdHiddenEvent -= h).ObserveOnMainThread().Subscribe(x => { OnInterstitialDismissedEvent(x.adUnitId, x.adInfo); }).AddTo(adDisposables);
        Observable.FromEvent<Action<string, MaxSdkBase.AdInfo>, (string adUnitId, MaxSdkBase.AdInfo adInfo)>(
            h => (adUnitId, adInfo) => h((adUnitId, adInfo)),
            h => MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent += h,
            h => MaxSdkCallbacks.Interstitial.OnAdRevenuePaidEvent -= h).ObserveOnMainThread().Subscribe(x => { OnInterstitialRevenuePaidEvent(x.adUnitId, x.adInfo); }).AddTo(adDisposables);

        // Load the first interstitial
#if !UNITY_EDITOR
        // var interstitialAd = new APSInterstitialAdRequest(amazone_inter_id);
        // interstitialAd.onSuccess += (adResponse) =>
        // {
        //     MaxSdk.SetInterstitialLocalExtraParameter(adInterstitialUnitId, "amazon_ad_response", adResponse.GetResponse());
        //     MaxSdk.LoadInterstitial(adInterstitialUnitId);
        // };
        // interstitialAd.onFailedWithError += (adError) =>
        // {
        //     MaxSdk.SetInterstitialLocalExtraParameter(adInterstitialUnitId, "amazon_ad_error", adError.GetAdError());
        //     MaxSdk.LoadInterstitial(adInterstitialUnitId);
        // };

        // interstitialAd.LoadAd();
        if(!GameRoot.Instance.ShopSystem.NoInterstitialAds.Value)
            LoadInterstitial();
#else
#endif
        isInterstitialAds = true;
    }


    public void Init()
    {
        InitializeRewardedAds();
        InitializeInterstitialAds();
        InitializeBannerAds(MaxSdkBase.BannerPosition.BottomCenter);

        bool preload = true;
#if UNITY_ANDROID && !UNITY_EDITOR
            if (SystemInfo.systemMemorySize < 1024 * 4)
            {
                preload = false;
                BpLog.Log("[AdManager] 메모리 용량이 너무 작아서 광고사전로드를 사용하지 않습니다.");
            }
#endif

        // if (preload)
        // {
        //     Observable.EveryUpdate().ThrottleFirst(TimeSpan.FromSeconds(PERIODIC_REFRESH_INTERVAL)).Subscribe(x =>
        //     {
        //         TpLog.Log("[TpMaxProp] Ads refresh");
        //         PrepareAds();
        //     });
        // }
    }

    // private void PrepareAds()
    // {
    //     var adLoadIdList = new List<(string unitId, AdType adType)>()
    //     {
    //         (unitId: adRewardUnit_MainId, adType: AdType.Rewarded),
    //         (unitId: adInterstitialUnit_MainId, adType: AdType.Interstitial),
    //     };
    //     adLoadIdList.AddRange(adRewardUnit_StepIds.Select(x => (unitId: x, adType: AdType.Rewarded)));
    //     adLoadIdList.AddRange(adInterstitialUnit_StepIds.Select(x => (unitId: x, adType: AdType.Interstitial)));

    //     Dictionary<string, DateTime> updateTimes = new Dictionary<string, DateTime>();
    //     foreach (var it in adLoadIdList)
    //     {
    //         if (it.adType == AdType.Rewarded && dic_RewardedAdInfos.TryGetValue(it.unitId, out var rewardedInfo))
    //         {
    //             updateTimes[it.unitId] = rewardedInfo.UpdateTime;
    //         }
    //         else if (it.adType == AdType.Interstitial && dic_InterstitialAdInfos.TryGetValue(it.unitId, out var interstitialInfo))
    //         {
    //             updateTimes[it.unitId] = interstitialInfo.UpdateTime;
    //         }
    //         else
    //         {
    //             updateTimes[it.unitId] = DateTime.MinValue;
    //         }
    //     }

    //     var adLoadList = adLoadIdList
    //         .Where(x => DateTime.Now - updateTimes[x.unitId] > TimeSpan.FromMinutes(PERIODIC_REFRESH_INTERVAL)) // 로드한지 n분 초과되면 업데이트
    //         .OrderBy(x => updateTimes[x.unitId]).ToArray(); // 오래된 광고부터 업데이트

    //     foreach (var adLoadInfo in adLoadList)
    //     {
    //         scheduler.Schedule(adLoadInfo.unitId, adLoadInfo.adType);
    //         TpLog.Log($"[TpMaxProp] Ad load scheduled - ID: {adLoadInfo.unitId}, Type: {adLoadInfo.adType}");
    //     }
    // }

    public void InitializeRewardedAds()
    {

        // Attach callback
        Observable.FromEvent<Action<string, MaxSdkBase.AdInfo>, (string adUnitId, MaxSdkBase.AdInfo adInfo)>(
            h => (adUnitId, adInfo) => h((adUnitId, adInfo)),
            h => MaxSdkCallbacks.Rewarded.OnAdLoadedEvent += h,
            h => MaxSdkCallbacks.Rewarded.OnAdLoadedEvent -= h).ObserveOnMainThread().Subscribe(x => { OnRewardedAdLoadedEvent(x.adUnitId, x.adInfo); }).AddTo(adDisposables);
        Observable.FromEvent<Action<string, MaxSdkBase.ErrorInfo>, (string adUnitId, MaxSdkBase.ErrorInfo adInfo)>(
            h => (adUnitId, adInfo) => h((adUnitId, adInfo)),
            h => MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent += h,
            h => MaxSdkCallbacks.Rewarded.OnAdLoadFailedEvent -= h).ObserveOnMainThread().Subscribe(x => { OnRewardedAdFailedEvent(x.adUnitId, x.adInfo); }).AddTo(adDisposables);
        Observable.FromEvent<Action<string, MaxSdkBase.ErrorInfo, MaxSdkBase.AdInfo>, (string adUnitId, MaxSdkBase.ErrorInfo adError, MaxSdkBase.AdInfo adInfo)>(
            h => (adUnitId, adError, adInfo) => h((adUnitId, adError, adInfo)),
            h => MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent += h,
            h => MaxSdkCallbacks.Rewarded.OnAdDisplayFailedEvent -= h).ObserveOnMainThread().Subscribe(x => { OnRewardedAdFailedToDisplayEvent(x.adUnitId, x.adError, x.adInfo); }).AddTo(adDisposables);
        Observable.FromEvent<Action<string, MaxSdkBase.AdInfo>, (string adUnitId, MaxSdkBase.AdInfo adInfo)>(
            h => (adUnitId, adInfo) => h((adUnitId, adInfo)),
            h => MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent += h,
            h => MaxSdkCallbacks.Rewarded.OnAdDisplayedEvent -= h).ObserveOnMainThread().Subscribe(x => { OnRewardedAdDisplayedEvent(x.adUnitId, x.adInfo); }).AddTo(adDisposables);
        Observable.FromEvent<Action<string, MaxSdkBase.AdInfo>, (string adUnitId, MaxSdkBase.AdInfo adInfo)>(
            h => (adUnitId, adInfo) => h((adUnitId, adInfo)),
            h => MaxSdkCallbacks.Rewarded.OnAdClickedEvent += h,
            h => MaxSdkCallbacks.Rewarded.OnAdClickedEvent -= h).ObserveOnMainThread().Subscribe(x => { OnRewardedAdClickedEvent(x.adUnitId, x.adInfo); }).AddTo(adDisposables);
        Observable.FromEvent<Action<string, MaxSdkBase.AdInfo>, (string adUnitId, MaxSdkBase.AdInfo adInfo)>(
            h => (adUnitId, adInfo) => h((adUnitId, adInfo)),
            h => MaxSdkCallbacks.Rewarded.OnAdHiddenEvent += h,
            h => MaxSdkCallbacks.Rewarded.OnAdHiddenEvent -= h).ObserveOnMainThread().Subscribe(x => { OnRewardedAdDismissedEvent(x.adUnitId, x.adInfo); }).AddTo(adDisposables);
        Observable.FromEvent<Action<string, MaxSdkBase.Reward, MaxSdkBase.AdInfo>, (string adUnitId, MaxSdkBase.Reward adReward, MaxSdkBase.AdInfo adInfo)>(
            h => (adUnitId, adReward, adInfo) => h((adUnitId, adReward, adInfo)),
            h => MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent += h,
            h => MaxSdkCallbacks.Rewarded.OnAdReceivedRewardEvent -= h).ObserveOnMainThread().Subscribe(x => { OnRewardedAdReceivedRewardEvent(x.adUnitId, x.adReward, x.adInfo); }).AddTo(adDisposables);
        Observable.FromEvent<Action<string, MaxSdkBase.AdInfo>, (string adUnitId, MaxSdkBase.AdInfo adInfo)>(
            h => (adUnitId, adInfo) => h((adUnitId, adInfo)),
            h => MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent += h,
            h => MaxSdkCallbacks.Rewarded.OnAdRevenuePaidEvent -= h).ObserveOnMainThread().Subscribe(x => { OnRewardedAdRevenuePaidEvent(x.adUnitId, x.adInfo); }).AddTo(adDisposables);

#if !UNITY_EDITOR
        // var rewardedVideoAd = new APSVideoAdRequest(320, 480, amazone_reward_id);
        // rewardedVideoAd.onSuccess += (adResponse) =>
        // {
        //     MaxSdk.SetRewardedAdLocalExtraParameter(adRewardUnitId, "amazon_ad_response", adResponse.GetResponse());
        //     MaxSdk.LoadRewardedAd(adRewardUnitId);
        // };
        // rewardedVideoAd.onFailedWithError += (adError) =>
        // {
        //     MaxSdk.SetRewardedAdLocalExtraParameter(adRewardUnitId, "amazon_ad_error", adError.GetAdError());
        //     MaxSdk.LoadRewardedAd(adRewardUnitId);
        // };

        // rewardedVideoAd.LoadAd();
        if(!GameRoot.Instance.ShopSystem.NoRewardedAds.Value)
            LoadRewardedAd();
#else
#endif
        isRewardAdsinit = true;
    }

    public void InitializeBannerAds(MaxSdkBase.BannerPosition bPos)
    {
        BpLog.Log($"[InitializeBannerAds] Starting banner initialization at position: {bPos}");

        Observable.FromEvent<Action<string, MaxSdkBase.AdInfo>, (string adUnitId, MaxSdkBase.AdInfo adInfo)>(
            h => (adUnitId, adInfo) => h((adUnitId, adInfo)),
            h => MaxSdkCallbacks.Banner.OnAdClickedEvent += h,
            h => MaxSdkCallbacks.Banner.OnAdClickedEvent -= h).ObserveOnMainThread().Subscribe(x => { OnBannerAdsClickedEvent(x.adUnitId, x.adInfo); }).AddTo(adDisposables);
        Observable.FromEvent<Action<string, MaxSdkBase.AdInfo>, (string adUnitId, MaxSdkBase.AdInfo adInfo)>(
            h => (adUnitId, adInfo) => h((adUnitId, adInfo)),
            h => MaxSdkCallbacks.Banner.OnAdCollapsedEvent += h,
            h => MaxSdkCallbacks.Banner.OnAdCollapsedEvent -= h).ObserveOnMainThread().Subscribe(x => { OnBannerAdsCollapsedEvent(x.adUnitId, x.adInfo); }).AddTo(adDisposables);
        Observable.FromEvent<Action<string, MaxSdkBase.AdInfo>, (string adUnitId, MaxSdkBase.AdInfo adInfo)>(
            h => (adUnitId, adInfo) => h((adUnitId, adInfo)),
            h => MaxSdkCallbacks.Banner.OnAdExpandedEvent += h,
            h => MaxSdkCallbacks.Banner.OnAdExpandedEvent -= h).ObserveOnMainThread().Subscribe(x => { OnBannerAdsExpandedEvent(x.adUnitId, x.adInfo); }).AddTo(adDisposables);
        Observable.FromEvent<Action<string, MaxSdkBase.AdInfo>, (string adUnitId, MaxSdkBase.AdInfo adInfo)>(
            h => (adUnitId, adInfo) => h((adUnitId, adInfo)),
            h => MaxSdkCallbacks.Banner.OnAdLoadedEvent += h,
            h => MaxSdkCallbacks.Banner.OnAdLoadedEvent -= h).ObserveOnMainThread().Subscribe(x => { OnBannerAdLoadedEvent(x.adUnitId, x.adInfo); }).AddTo(adDisposables);
        Observable.FromEvent<Action<string, MaxSdkBase.ErrorInfo>, (string adUnitId, MaxSdkBase.ErrorInfo adInfo)>(
            h => (adUnitId, adInfo) => h((adUnitId, adInfo)),
            h => MaxSdkCallbacks.Banner.OnAdLoadFailedEvent += h,
            h => MaxSdkCallbacks.Banner.OnAdLoadFailedEvent -= h).ObserveOnMainThread().Subscribe(x => { OnBannerAdLoadFailedEvent(x.adUnitId, x.adInfo); }).AddTo(adDisposables);
        Observable.FromEvent<Action<string, MaxSdkBase.AdInfo>, (string adUnitId, MaxSdkBase.AdInfo adInfo)>(
            h => (adUnitId, adInfo) => h((adUnitId, adInfo)),
            h => MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent += h,
            h => MaxSdkCallbacks.Banner.OnAdRevenuePaidEvent -= h).ObserveOnMainThread().Subscribe(x => { OnRewardedAdRevenuePaidEvent(x.adUnitId, x.adInfo); }).AddTo(adDisposables);

        //#if !UNITY_EDITOR
        //        int width , height;
        //        if (MaxSdkUtils.IsTablet()){ width = 728; height = 90; }
        //        else { width = 320; height = 50; }

        //        var apsBanner = new APSBannerAdRequest(width, height, amazone_banner_id);
        //        apsBanner.onSuccess += (adResponse) =>
        //        {
        //            MaxSdk.SetBannerLocalExtraParameter(adBannerUnitId, "amazon_ad_response", adResponse.GetResponse());
        //            MaxSdk.CreateBanner(adBannerUnitId, bPos);

        //        };
        //        apsBanner.onFailedWithError += (adError) =>
        //        {
        //            MaxSdk.SetBannerLocalExtraParameter(adBannerUnitId, "amazon_ad_error", adError.GetAdError());
        //            MaxSdk.CreateBanner(adBannerUnitId, bPos);

        //        };
        //        apsBanner.LoadAd();
        //#else
        MaxSdk.CreateBanner(adBannerUnitId, bPos);
        //#endif

        isBanner = true;

    }

    private void OnBannerAdsClickedEvent(string id, MaxSdkBase.AdInfo adInfo)
    {
#if UNITY_EDITOR
        BpLog.Log("OnBannerAds_Click _ " + id);
#endif
    }
    private void OnBannerAdsCollapsedEvent(string id, MaxSdkBase.AdInfo adInfo)
    {
#if UNITY_EDITOR
        BpLog.Log("OnBannerAds_Collapsed _ " + id);
#endif
    }
    private void OnBannerAdsExpandedEvent(string id, MaxSdkBase.AdInfo adInfo)
    {
#if UNITY_EDITOR
        BpLog.Log("OnBannerAds_Expanded _ " + id);
#endif
    }
    private void OnBannerAdLoadedEvent(string id, MaxSdkBase.AdInfo adInfo)
    {
        BpLog.Log("OnBannerAds_Loaded _ " + id);

        // 배너가 로드되면 자동으로 표시 (이미 ShowBanner가 호출되었다면)
        if (isShowBanner)
        {
            MaxSdk.ShowBanner(adBannerUnitId);
        }
    }
    private void OnBannerAdLoadFailedEvent(string id, MaxSdkBase.ErrorInfo errorInfo)
    {
        BpLog.Log("OnBannerAds_LoadFailed _ " + id + " _ " + errorInfo.Code + " _ msg = " + errorInfo.Message);
    }



    public void ShowBannerAD(MaxSdkBase.BannerPosition bannerPos)
    {
        MaxSdk.StartBannerAutoRefresh(adBannerUnitId);

        MaxSdk.UpdateBannerPosition(adBannerUnitId, bannerPos);

        MaxSdk.ShowBanner(adBannerUnitId);

        isShowBanner = true;
    }


    public bool IsRewardAdReady()
    {
        return MaxSdk.IsRewardedAdReady(adRewardUnitId);
    }

    public void HideBannerAD()
    {
        MaxSdk.HideBanner(adBannerUnitId);
        isShowBanner = false;
    }

    public void ShowInterstitialAD(AdInterType idx)
    {

#if BANPOFRI_LOG
        BpLog.Log("ShowInterstitialAD");
        return;
#endif


        // if (GameRoot.Instance.ShopSystem.NoInterstitialAds.Value || GameRoot.Instance.ABTestSystem.CheckABTest(ABTestSystem.removeadsIdx))
        //     return;
        //{
        //    return;
        //}
        GameRoot.Instance.PluginSystem.InitMax(() =>
        {
            if (MaxSdk.IsInterstitialReady(adInterstitialUnitId))
            {

                MaxSdk.ShowInterstitial(adInterstitialUnitId);
                isInterstitialAdsLoad = false;
                if (showingCo != null) { GameRoot.Instance.StopCoroutine(showingCo); showingCo = null; }
                showingAd = true;

                var adTypeStr = idx.ToString().ToLower(new CultureInfo("en-US", false));

                //logs
                // List<TpParameter> parameters = new List<TpParameter>();
                // parameters.Add(new TpParameter("idx", adTypeStr));
                // GameRoot.Instance.PluginSystem.AnalyticsProp.AllEvent(IngameEventType.None, "m_ads_inter", parameters);

                GameRoot.Instance.UserData.Save();
            }
            else
            {
                LoadInterstitial();
            }
        });
    }

    public void ShowRewardAD(AdRewardType type, System.Action<bool> OnSuccess)
    {
        if (GameRoot.Instance.TutorialSystem.IsActive())
        {
            OnSuccess?.Invoke(true);
            return;
        }

        // switch (type)
        // {
        //     case AdRewardType.InGameMoney:
        //     case AdRewardType.Revival:
        //     case AdRewardType.StageResultReward:
        //     case AdRewardType.TowerRevival:
        //     case AdRewardType.SelectAdTowerUpgrade:
        //     case AdRewardType.AllAdTowerUpgrade:
        //     case AdRewardType.TowerResultReward:
        //         GameRoot.Instance.ShopSystem.InterAdData.SetAdCntValue(-1);
        //         break;
        // }
        OnRewardResult = OnSuccess;
        GameRoot.Instance.Loading.Show(false);
        adType = type;
        // GameRoot.Instance.WaitTimeAndCallback(10f, () => {
        //     if(OnRewardResult != null)
        //     {
        //         onRewardResult(false);
        //     }
        // });
#if UNITY_EDITOR || BANPOFRI_LOG
        listenerRewardedAd?.Invoke(true);
        GameRoot.Instance.Loading.Hide(true);
        onRewardResult(true);
        return;
#endif




        GameRoot.Instance.PluginSystem.InitMax(() =>
        {
            var diffLoadedTime = TimeSystem.GetCurTime() - adLoadedTime;
            UnityEngine.Debug.Log($"ShowRewardAd DiffTotalSec: ${diffLoadedTime.TotalSeconds}");
            if (MaxSdk.IsRewardedAdReady(adRewardUnitId) && diffLoadedTime.TotalSeconds < 3300)
            {
                listenerRewardedAd?.Invoke(false);
                MaxSdk.ShowRewardedAd(adRewardUnitId);
                if (showingCo != null) { GameRoot.Instance.StopCoroutine(showingCo); showingCo = null; }
                showingAd = true;
                if (waitCo != null)
                {
                    GameRoot.Instance.StopCoroutine(waitCo);
                    waitCo = null;
                }
                waitCo = GameRoot.Instance.WaitTimeAndCallback(10f, () =>
                {
                    if (!showingAd) onRewardResult(false);

                    waitCo = null;
                });
            }
            else
            {
                UnityEngine.Debug.Log($"Reload");
                cb_loadRewardedAd = () =>
                {
                    MaxSdk.ShowRewardedAd(adRewardUnitId);
                };
                LoadRewardedAd();
            }
        });
    }

    private void onRewardResult(bool value)
    {
        System.Action ResultCb = () =>
        {
            if (OnRewardResult == null) return;



            GameRoot.Instance.UserData.AddRecordCount(Config.RecordCountKeys.AdWatchCount, 1);

            GameRoot.Instance.UserData.AddRecordCount(Config.RecordCountKeys.ShowInterAdCount, -1);

            List<TpParameter> parameters = new List<TpParameter>();
            parameters.Add(new TpParameter("stage", GameRoot.Instance.UserData.Stageidx.Value));
            parameters.Add(new TpParameter("waveidx", GameRoot.Instance.UserData.Waveidx.Value));
            parameters.Add(new TpParameter("idx", (int)adType));
            GameRoot.Instance.PluginSystem.AnalyticsProp.AllEvent(IngameEventType.None, "m_ads_reward", parameters);

            


            OnRewardResult?.Invoke(value);
            OnRewardResult = null;
            GameRoot.Instance.Loading.Hide(true);
            adType = AdRewardType.None;
        };



#if UNITY_ANDROID || UNITY_EDITOR
        GameRoot.Instance.MainThreadDispatcher.Enqueue(ResultCb);
#else
        ResultCb.Invoke();
#endif
    }


    public void ShowAdTicketRecommand()
    {
        if (appear_ad_skip_purchase_popup < 0)
        {
            // var define_data = Tables.Instance.GetTable<Define>().GetData("appear_ad_skip_purchase_popup");
            // if (define_data != null)
            //     appear_ad_skip_purchase_popup = define_data.value;
            // else
            //     appear_ad_skip_purchase_popup = 999999999;
        }

        //if (ProjectUtility.ContentsOpenCheck(Config.ContentsOpenType.adsticket)
        //    && !GameRoot.Instance.TutorialSystem.IsActive())
        //{
        //    var count = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.AdWatchCount);

        //    if (count == 0) return;
        //    if (appear_ad_skip_purchase_popup == 0) return;

        //    if (count > 0 && count % appear_ad_skip_purchase_popup == 0)
        //    {
        //        GameRoot.Instance.UISystem.OpenUI<PopupTicketRecommend>();
        //    }
        //}

    }

    private void LoadInterstitial()
    {
        isInterstitialAdsLoad = true;
        MaxSdk.LoadInterstitial(adInterstitialUnitId);
    }

    private void OnInterstitialLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad is ready to be shown. MaxSdk.IsInterstitialReady(interstitialAdUnitId) will now return 'true'        
        // Reset retry attempt
        isInterstitialAdsLoad = true;
    }

    private void OnInterstitialFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Interstitial ad failed to load in
        // We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds)

        // GameRoot.Instance.WaitTimeAndCallback(64f, () =>
        // {
        //     LoadInterstitial();
        // });

        isInterstitialAdsLoad = false;
    }

    private void InterstitialFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad failed to display. We recommend loading the next ad
        //LoadInterstitial();
    }

    private void OnInterstitialDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad is hidden. Pre-load the next ad
        LoadInterstitial();
        showingCo = GameRoot.Instance.WaitTimeAndCallback(3f, () => { showingAd = false; });

    }

    private void OnInterstitialRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Interstitial ad revenue paid. Use this callback to track user revenue.
        BpLog.Log("Interstitial revenue paid");


        OnInterPaidAction?.Invoke();

        OnInterPaidAction = null;


        double revenue = adInfo.Revenue;
        List<TpParameter> parameters = new List<TpParameter>();
        parameters.Add(new TpParameter("ad_platform", "AppLovin"));
        parameters.Add(new TpParameter("ad_source", adInfo.NetworkName));
        //parameters.Add(new TpParameter("ad_unit_name", adInfo.AdUnitIdentifier));
        parameters.Add(new TpParameter("ad_format", adInfo.AdFormat));
        parameters.Add(new TpParameter("value", Math.Truncate(revenue * 10000) / 10000));
        parameters.Add(new TpParameter("currency", "USD"));
        parameters.Add(new TpParameter("stage", GameRoot.Instance.UserData.Stageidx.Value));
        GameRoot.Instance.PluginSystem.AnalyticsProp.AllEvent(IngameEventType.None,
            "ad_impression", parameters);

        // Ad revenue
        // Miscellaneous data
        // string countryCode = MaxSdk.GetSdkConfiguration().CountryCode; // "US" for the United States, etc - Note: Do not confuse this with currency code which is "USD" in most cases!
        // string networkName = adInfo.NetworkName; // Display name of the network that showed the ad (e.g. "AdColony")
        // string adUnitIdentifier = adInfo.AdUnitIdentifier; // The MAX Ad Unit ID
        // string placement = adInfo.Placement; // The placement this ad's postbacks are tied to
    }

    public void LoadRewardedAd()
    {
        MaxSdk.LoadRewardedAd(adRewardUnitId);
    }

    private void OnRewardedAdLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad is ready to be shown. MaxSdk.IsRewardedAdReady(adUnitId) will now return 'true'
        adLoadedTime = TimeSystem.GetCurTime();
        UnityEngine.Debug.Log($"AdLoaded : {adLoadedTime.ToLongTimeString()}");

        if (GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.FirstEcpm) == 0
        && GameRoot.Instance.UserData.Stageidx.Value == 1)
        {
            List<TpParameter> parameters = new List<TpParameter>();
            parameters.Add(new TpParameter("ecpm", adInfo.Revenue));
            GameRoot.Instance.PluginSystem.AnalyticsProp.AllEvent(IngameEventType.None, "first_ecpm", parameters);
            GameRoot.Instance.UserData.AddRecordCount(Config.RecordCountKeys.FirstEcpm, 1);
        }

        listenerRewardedAd?.Invoke(true);

        // Reset retry attempt
        // if (OnRewardResult != null)
        // {
        //     MaxSdk.ShowRewardedAd(adRewardUnitId);
        // }

        cb_loadRewardedAd?.Invoke();
        cb_loadRewardedAd = null;
    }

    private void OnRewardedAdFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    {
        // Rewarded ad failed to load 
        // We recommend retrying with exponentially higher delays up to a maximum delay (in this case 64 seconds)
        onRewardResult(false);
        GameRoot.Instance.WaitTimeAndCallback(64f, () =>
        {
            LoadRewardedAd();
        });

        UnityEngine.Debug.Log($"---------------OnRewardedAdFailedEvent : {errorInfo.ToString()}");
    }

    private void OnRewardedAdFailedToDisplayEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad failed to display. We recommend loading the next ad
        onRewardResult(false);
        LoadRewardedAd();

        UnityEngine.Debug.Log($"---------------OnRewardedAdFailedToDisplayEvent : {errorInfo.ToString()}");
    }

    private void OnRewardedAdDisplayedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        showingAd = true;
        UnityEngine.Debug.Log($"---------------OnRewardedAdDisplayedEvent");
    }

    private void OnRewardedAdClickedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        UnityEngine.Debug.Log($"---------------OnRewardedAdClickedEvent");
    }

    private void OnRewardedAdDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        if (GameRoot.Instance.Loading.gameObject.activeSelf)
        {
            GameRoot.Instance.Loading.Hide(true);
        }
        LoadRewardedAd();

        if (waitCo != null && showingAd)
        {
            GameRoot.Instance.StopCoroutine(waitCo);
            waitCo = null;
        }
        showingAd = false;

        if (showingCo != null) { GameRoot.Instance.StopCoroutine(showingCo); showingCo = null; }
        showingCo = GameRoot.Instance.WaitTimeAndCallback(4f, () => { showingAd = false; });
        UnityEngine.Debug.Log($"---------------OnRewardedAdDismissedEvent");

    }

    private void OnRewardedAdReceivedRewardEvent(string adUnitId, MaxSdk.Reward reward, MaxSdkBase.AdInfo adInfo)
    {
        onRewardResult(true);

        UnityEngine.Debug.Log($"---------------OnRewardedAdReceivedRewardEvent");
    }

    private void OnRewardedAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    {
        // Rewarded ad revenue paid. Use this callback to track user revenue.
        BpLog.Log("Rewarded ad revenue paid");


        double revenue = adInfo.Revenue;
        List<TpParameter> parameters = new List<TpParameter>();
        parameters.Add(new TpParameter("ad_platform", "AppLovin"));
        parameters.Add(new TpParameter("ad_source", adInfo.NetworkName));
        //parameters.Add(new TpParameter("ad_unit_name", adInfo.AdUnitIdentifier));
        parameters.Add(new TpParameter("ad_format", adInfo.AdFormat));
        parameters.Add(new TpParameter("value", Math.Truncate(revenue * 10000) / 10000));
        parameters.Add(new TpParameter("currency", "USD"));
        parameters.Add(new TpParameter("stage", GameRoot.Instance.UserData.Stageidx.Value));
        GameRoot.Instance.PluginSystem.AnalyticsProp.AllEvent(IngameEventType.None,
            "ad_impression", parameters);

        Dictionary<string, string> additionalParams = new Dictionary<string, string>();
        additionalParams.Add(AdRevenueScheme.COUNTRY, MaxSdk.GetSdkConfiguration().CountryCode);
        additionalParams.Add(AdRevenueScheme.AD_UNIT, adInfo.AdUnitIdentifier);
        additionalParams.Add(AdRevenueScheme.AD_TYPE, adInfo.AdFormat);
        additionalParams.Add(AdRevenueScheme.PLACEMENT, adInfo.Placement);
        additionalParams.Add("ad_platform", "AppLovin");
        additionalParams.Add("ad_source", adInfo.NetworkName);
        additionalParams.Add("currency", "USD"); // All AppLovin revenue is sent in USD
        additionalParams.Add("ad_country_code", MaxSdk.GetSdkConfiguration().CountryCode);
        //additionalParams.Add("value", toStringRevenue);
        additionalParams.Add(AFInAppEvents.CURRENCY, "USD");// All AppLovin revenue is sent in USD

        var logRevenue = new AFAdRevenueData(adInfo.NetworkName, MediationNetwork.ApplovinMax, "USD", adInfo.Revenue);
        AppsFlyer.logAdRevenue(logRevenue, additionalParams);



        if (revenue * 10000 < int.MaxValue - 5)
        {
            Check_m_Rev_05((int)Math.Truncate(revenue * 10000));
        }
    }

    void Check_m_Rev_05(int addRev)
    {
        var recordValue = GameRoot.Instance.UserData.GetRecordValue(Config.RecordKeys.M_Rev_05);

        recordValue += addRev;

        var rev = recordValue / 10000f;

        if (rev > 0.5f)
        {
            GameRoot.Instance.UserData.SetRecordValue(Config.RecordKeys.M_Rev_05, 0);

            List<TpParameter> param = new List<TpParameter>();
            param.Add(new TpParameter("stage", GameRoot.Instance.UserData.Stageidx.Value));
            GameRoot.Instance.PluginSystem.AnalyticsProp.AllEvent(IngameEventType.None, "m_rev_05", param);
        }
        else
        {
            GameRoot.Instance.UserData.SetRecordValue(Config.RecordKeys.M_Rev_05, recordValue);
        }
    }

    public void Dispose()
    {
        adDisposables.Clear();
        listenerRewardedAd = null;
        OnInterPaidAction = null;
        OnRewardResult = null;
        cb_loadRewardedAd = null;
    }

    private void OnBannerAdsClickedEvent(string id)
    {
#if UNITY_EDITOR
        BpLog.Log("OnBannerAds_Click _ " + id);
#endif
    }
    private void OnBannerAdsCollapsedEvent(string id)
    {
#if UNITY_EDITOR
        BpLog.Log("OnBannerAds_Collapsed _ " + id);
#endif
    }
    private void OnBannerAdsExpandedEvent(string id)
    {
#if UNITY_EDITOR
        BpLog.Log("OnBannerAds_Expanded _ " + id);
#endif
    }
    private void OnBannerAdLoadedEvent(string id)
    {
#if UNITY_EDITOR
        BpLog.Log("OnBannerAds_Loaded _ " + id);
#endif
    }
    private void OnBannerAdLoadFailedEvent(string id, int errorCode)
    {
#if UNITY_EDITOR
        BpLog.Log("OnBannerAds_LoadFailed _ " + id + " _ " + errorCode);
#endif
    }

    // public void OnAppOpenDismissedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    // {
    //     LoadAppOpenAd();
    // }

    // public void OnAppOpenLoadedEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    // {
    //     TpLog.Log($"AppOpenAdLoaded {adUnitId}");
    // }

    // public void OnAppOpenLoadFailedEvent(string adUnitId, MaxSdkBase.ErrorInfo errorInfo)
    // {
    //     TpLog.Log($"AppOpenAdLoadFailed {errorInfo.Code} {errorInfo.Message}");

    //     onRewardResult(false);
    //     GameRoot.Instance.WaitTimeAndCallback(64f, () =>
    //     {
    //         LoadAppOpenAd();
    //     });
    // }

    // private void OnAppOpenAdRevenuePaidEvent(string adUnitId, MaxSdkBase.AdInfo adInfo)
    // {
    //     // Rewarded ad revenue paid. Use this callback to track user revenue.
    //     TpLog.Log("AppOpen ad revenue paid");

    //     double revenue = adInfo.Revenue;
    //     List<TpParameter> parameters = new List<TpParameter>();
    //     //parameters.Add(new TpParameter("ad_platform", "AppLovin"));
    //     parameters.Add(new TpParameter("ad_source", adInfo.NetworkName));
    //     //parameters.Add(new TpParameter("ad_unit_name", adInfo.AdUnitIdentifier));
    //     parameters.Add(new TpParameter("ad_format", adInfo.AdFormat));
    //     parameters.Add(new TpParameter("value", Math.Truncate(revenue * 10000) / 10000));
    //     parameters.Add(new TpParameter("currency", "USD"));
    //     GameRoot.Instance.PluginSystem.AnalyticsProp.AllEvent(IngameEventType.None,
    //         "ad_impression", parameters);
    // }


    // public void ShowAppOpenAd()
    // {
    //     TpLog.Log("ShowAppOpenAd");

    //     if (MaxSdk.IsAppOpenAdReady(adAppOpenUnitId))
    //     {
    //         TpLog.Log("AppOpenAdNotReady");
    //         MaxSdk.ShowAppOpenAd(adAppOpenUnitId);
    //     }
    //     else
    //     {
    //         LoadAppOpenAd();
    //     }
    // }

    // public void LoadAppOpenAd()
    // {
    //     TpLog.Log("LoadAppOpenAd");
    //     MaxSdk.LoadAppOpenAd(adAppOpenUnitId);
    // }
}
