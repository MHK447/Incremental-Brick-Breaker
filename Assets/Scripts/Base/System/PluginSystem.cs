using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Text;
#if !UNITY_EDITOR
//using AmazonAds;
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
using Google.Play.Review;
//using Google.Android.PerformanceTuner;
#elif UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
using Unity.Advertisement.IosSupport;
#endif
using BanpoFri;
using Facebook.Unity;
using AppsFlyerSDK;
using UniRx.Async;
using UnityEngine.Networking;
using System;



public class PluginSystem
{
    public static readonly string googleStoreURL = "https://play.google.com/store/apps/details?id=com.banpofri.tinycastleguardians";
    public static readonly string InviteFriendServerURL = "https://us-central1-cat-snack-bar.cloudfunctions.net/InviteFriendLumber";

    public TpMaxProp ADProp = new TpMaxProp();
    public TpAnalyticsProp AnalyticsProp = new TpAnalyticsProp();
    public TpPlatformLoginProp LoginProp = new TpPlatformLoginProp();
    public TpFirebaseDataProp DataProp = new TpFirebaseDataProp();


    public static bool IsInitAppsflyer = false;

#if UNITY_ANDROID && !UNITY_EDITOR
    private string AmazonAppID = "13308a0b-8a7f-477f-99ad-c111c2bd9d46";
    private ReviewManager _reviewManager;
    private PlayReviewInfo _playReviewInfo = null;
    //private AndroidPerformanceTuner<FidelityParams, Annotation> tuner =
    //    new AndroidPerformanceTuner<FidelityParams, Annotation>();
#elif UNITY_IOS && !UNITY_EDITOR
    private string AmazonAppID = "7177ec60-451d-4bed-8a6e-70cb3b6ead44";
    [DllImport("__Internal")]
    public static extern void IOSCallReview();
#else
    private string AmazonAppID = "7177ec60-451d-4bed-8a6e-70cb3b6ead44";
#endif
    public bool IsInitFacebook { get; private set; } = false;
    private bool initMax = false;

    private string strLocalPushEnable = "LocalPushInit";


    const string APPSFLYER_DEV_KEY = "B3E3DvVKsFLfGWrNg4WtWU";
    const string APPSFLYER_APP_ID = "6757511198";

    public void Init()
    {
        InitAppOpenAvailable();

#if UNITY_IOS && !UNITY_EDITOR
        if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() != ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
        {
            InitMax();
        }
#endif
        //review
#if UNITY_ANDROID && !UNITY_EDITOR
        _reviewManager = new ReviewManager();
        GameRoot.Instance.StartCoroutine(ReviewInProgressBefore());
        //ErrorCode startErrorCode = tuner.Start();
        //TpLog.Log("Android Performance Tuner started with code: " + startErrorCode);

        //tuner.onReceiveUploadLog += request =>
        //{
        //    TpLog.Log("Telemetry uploaded with request name: " + request.name);
        //};
#endif
        //localnoti
        //LocalNotification.Init();

#if !UNITY_EDITOR
        DataProp.Init();
        Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
        Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;

        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                // Crashlytics will use the DefaultInstance, as well;
                // this ensures that Crashlytics is initialized.
                Firebase.FirebaseApp app = Firebase.FirebaseApp.DefaultInstance;

                // Set a flag here for indicating that your project is ready to use Firebase.
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
            }
        });

        //facebook
        if (!FB.IsInitialized)
        {
            // Initialize the Facebook SDK
            FB.Init(InitCallback, OnHideUnity);
        }
        else
        {
            // Already initialized, signal an app activation App Event
            FB.ActivateApp();
        }
#endif

        //BackEndProp.Init();
    }

    public void InitMax(System.Action InitAction = null)
    {
        if (initMax)
        {
            InitAction?.Invoke();
            return;
        }
#if !UNITY_EDITOR
        //Amazon.Initialize(AmazonAppID);
        //Amazon.SetAdNetworkInfo(new AdNetworkInfo(DTBAdNetwork.MAX));
#endif

        MaxSdkCallbacks.OnSdkInitializedEvent += sdkConfiguration =>
        {
            BpLog.Log("MAX SDK Initialized");
#if UNITY_IOS || UNITY_IPHONE || UNITY_EDITOR
            // AdSettings.setDataProcessingOptions( new String[] {} );
            if (MaxSdkUtils.CompareVersions(UnityEngine.iOS.Device.systemVersion, "14.5") != MaxSdkUtils.VersionComparisonResult.Lesser)
            {    // Note that App transparency tracking authorization can be checked via `sdkConfiguration.AppTrackingStatus` for Unity Editor and iOS targets
                 // 1. Set Facebook ATE flag here, THEN
                FB.Mobile.SetAdvertiserTrackingEnabled(sdkConfiguration.AppTrackingStatus == MaxSdkBase.AppTrackingStatus.Authorized);
            }
#endif

            ADProp.Init();
#if !UNITY_EDITOR
#if BANPOFRI_LOG
            //Amazon.EnableLogging(true);
            //Amazon.EnableTesting(true);
#endif
#endif
            //if (GameRoot.Instance.PluginSystem.appOpenAvailable)
            //{
            //    ADProp.InitializeAppOpenAds();
            //}
            var appsFlyerObj = GameRoot.Instance.transform.Find("SDKObject");
            AppsFlyer appsFlyerCompo = null;
            if (appsFlyerObj != null)
            {
                AppsFlyer.OnRequestResponse += AppsFlyerOnRequestResponse;
                appsFlyerCompo = appsFlyerObj.GetComponent<AppsFlyer>();
            }

            AppsFlyer.initSDK(APPSFLYER_DEV_KEY, APPSFLYER_APP_ID, appsFlyerCompo);
            AppsFlyer.OnDeepLinkReceived += OnDeepLink;
            AppsFlyer.startSDK();
            AppsFlyer.setAppInviteOneLinkID("u0xf");
            AppsFlyer.subscribeForDeepLink();
            AppsFlyer.setCustomerUserId(GameRoot.Instance.UserData.UUID.ToString());
            if (appsFlyerObj == null) { IsInitAppsflyer = true; }

            InitAction?.Invoke();

            MaxSdk.SetCreativeDebuggerEnabled(false);
        };
        MaxSdk.SetSdkKey("kEMm9P5L89OR0kvrUxOrTTVN9dqKyRhNPrdPnpPGfIfE4VF8qn99egv-sLgnKP8oEBmF9yAWGW1eTOBAvr-qo_");
         
        MaxSdk.InitializeSdk();
        initMax = true;
    }

    void AppsFlyerOnRequestResponse(object sender, System.EventArgs e)
    {
        var args = e as AppsFlyerRequestEventArgs;
        AppsFlyer.AFLog("AppsFlyerOnRequestResponse", " status code " + args.statusCode);
        if (args.statusCode == 200)
        {
            IsInitAppsflyer = true;
            AnalyticsProp.AppsflyerSendReadyLog();
        }
    }


    public void OnDeepLink(object sender, System.EventArgs e)
    {
        var deepLinkEventArgs = e as DeepLinkEventsArgs;

        switch (deepLinkEventArgs.status)
        {
            case DeepLinkStatus.FOUND:

                if (deepLinkEventArgs.isDeferred())
                {
                    BpLog.Log("OnDeepLink This is a deferred deep link");
                }
                else
                {
                    BpLog.Log("OnDeepLink This is a direct deep link");
                }

                BpLog.Log("Deep link on");
                var deeplinks = GetDeepLinkParamsDictionary(deepLinkEventArgs);
                foreach (var i in deeplinks)
                {
                    BpLog.Log($"key ; {i.Key} value {i.Value.ToString()}");
                }
                var deepLinkValue = deepLinkEventArgs.getDeepLinkValue();
                if (!string.IsNullOrEmpty(deepLinkValue) && deepLinkValue != AppsFlyer.getAppsFlyerId())
                {
                    _ = PostInviteFriend(deepLinkValue);
                }
                break;
            case DeepLinkStatus.NOT_FOUND:
                BpLog.Log("Deep link not found");
                break;
            default:
                BpLog.Log("Deep link error");
                break;
        }
    }

    private Dictionary<string, object> GetDeepLinkParamsDictionary(DeepLinkEventsArgs deepLinkEventArgs)
    {
#if UNITY_IOS && !UNITY_EDITOR
    if (deepLinkEventArgs.deepLink.ContainsKey("click_event") && deepLinkEventArgs.deepLink["click_event"] != null)
    {
        return deepLinkEventArgs.deepLink["click_event"] as Dictionary<string, object>;
    }
#elif UNITY_ANDROID && !UNITY_EDITOR
        return deepLinkEventArgs.deepLink;
#endif

        return null;
    }
    public class InviteFriendData
    {
        public string user_id;
        public string friend_user_id;
    }

    public bool WaitUserData = false;
    async UniTaskVoid PostInviteFriend(string user_id)
    {
        await UniTask.WaitUntil(() => WaitUserData);
        var sendData = new InviteFriendData()
        {
            user_id = user_id,
            friend_user_id = AppsFlyer.getAppsFlyerId()
        };

        BpLog.Log($"sendData  user_id:{sendData.user_id} friend_user_id:{sendData.friend_user_id}");

        using (var request = new UnityWebRequest(InviteFriendServerURL, "POST"))
        {
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(sendData)));
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            await request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.Success)
            {
                BpLog.Log("PostInviteFriend Success");
                //logs
                List<TpParameter> parameters = new List<TpParameter>();
                parameters.Add(new TpParameter("invite_user", user_id));
                GameRoot.Instance.PluginSystem.AnalyticsProp.AllEvent(IngameEventType.None,
                    "invited_user", parameters);
            }
            else
            {
                BpLog.Log("PostInviteFriend Failed");
            }
        }
        WaitUserData = false;
    }


    private void InitTapjoyCallbackMgr()
    {
        // var type = typeof(TpTapjoyProp);
        // TapjoyProp = new GameObject("TapjoyCallbacks", type)
        //     .GetComponent<TpTapjoyProp>();

        // TapjoyProp.InitConnect();
    }

    public void Update()
    {
        LoginProp.Update();
    }

    bool onReview = false;
    IEnumerator coEndReview()
    {
        onReview = true;
        yield return new WaitForSeconds(120.0f);
        onReview = false;
    }

    public void StartReview()
    {
        GameRoot.Instance.StartCoroutine(coEndReview());
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_playReviewInfo == null)
        {
            BanpoFriNative.OpenURL(googleStoreURL);
            return;
        }
        GameRoot.Instance.StartCoroutine(ReviewInProgressAfter());
#elif UNITY_IOS && !UNITY_EDITOR
        if (UnityEngine.iOS.Device.RequestStoreReview())
        {
            BpLog.Log("UnityEngine.iOS.Device.RequestStoreReview");
        }
        else
        {
            IOSCallReview();
        }
#endif
    }
#if UNITY_ANDROID && !UNITY_EDITOR
    private IEnumerator ReviewInProgressBefore()
    {
        yield return null;
        var requestFlowOperation = _reviewManager.RequestReviewFlow();
        yield return requestFlowOperation;
        if (requestFlowOperation.Error != ReviewErrorCode.NoError)
        {
            // Log error. For example, using requestFlowOperation.Error.ToString().
            BpLog.Log(requestFlowOperation.Error.ToString());
            yield break;
        }
        _playReviewInfo = requestFlowOperation.GetResult();
    }

    private IEnumerator ReviewInProgressAfter()
    {
        yield return null;
        var launchFlowOperation = _reviewManager.LaunchReviewFlow(_playReviewInfo);
        yield return launchFlowOperation;
        _playReviewInfo = null; // Reset the object
        if (launchFlowOperation.Error != ReviewErrorCode.NoError)
        {
            BanpoFriNative.OpenURL(googleStoreURL);
            // Log error. For example, using requestFlowOperation.Error.ToString().
            BpLog.Log(launchFlowOperation.Error.ToString());
            yield break;
        }
    }
#endif

    public void ShowBanner(MaxSdk.BannerPosition pos)
    {
        
        if (!initMax)
        {
            InitMax(() => ShowBanner(pos));
            return;
        }
        
        if (!ADProp.isBanner) 
        { 
            ADProp.InitializeBannerAds(pos); 
        }
        
        ADProp.ShowBannerAD(pos);
    }


    public bool CheckBannerInit()
    {
        if (ADProp.isBanner) { return true; }

        ADProp.InitializeBannerAds(MaxSdkBase.BannerPosition.TopCenter);
        return true;
    }

    public void HideBanner(bool isDestroy = false)
    {
        ADProp.HideBannerAD();
    }


    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            // Signal an app activation App Event
            FB.ActivateApp();
            IsInitFacebook = true;
            // Continue with Facebook SDK
            // ...
        }
        else
        {
            BpLog.Log("Failed to Initialize the Facebook SDK");
        }
    }

    private void OnHideUnity(bool isGameShown)
    {
        // if (!isGameShown)
        // {
        //     // Pause the game - we will need to hide
        //     GameRoot.Instance.GameSpeedSystem.CurGameSpeedValue.Value = 0f;
        // }
        // else
        // {
        //     // Resume the game - we're getting focus again
        //     GameRoot.Instance.GameSpeedSystem.CurGameSpeedValue.Value = GameRoot.Instance.GameSpeedSystem.InitGameSpeedValue.Value;
        // }
    }

    public void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
    {
        BpLog.Log("Received Registration Token: " + token.Token);
    }

    public void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
    {
        BpLog.Log("Received a new message from: " + e.Message.From);
    }

    public void OnApplicationPause(bool value)
    {
        if (value)
        {
            if (GameRoot.Instance == null)
            {
                Debug.Log("GameRoot is Null");
                return;
            }
            if (!GameRoot.LoadComplete)
            {
                return;
            }
            if (GameRoot.Instance.UserData == null)
            {
                Debug.Log("UserData is Null");
                return;
            }
            if (GameRoot.Instance.UserData.CurMode == null)
            {
                Debug.Log("CurMode is Null");
                return;
            }

            //sa1001_TODO : 오프라인리워드 팝업이 등장하기 전 앱오픈 광고가 뜨고 들어오면 저장된 시간으로 오프라인을 계산해서 받지 못하는 문제가 있음
            //https://treeplla.slack.com/archives/C03STJ0U99T/p1679990049422999


      
            GameRoot.Instance.UserData.CurMode.LastLoginTime = TimeSystem.GetCurTime();
            GameRoot.Instance.UserData.Save(true);

            /*LocalNotification.CallNotification(
                99,
                0,
                "test",
                "test");*/


            //LocalNotification.AllClearNotification(); // 기존 로컬 푸시 삭제


            //var list = Tables.Instance.GetTable<LocalPush>().DataList;
            //var curTime = TimeSystem.GetCurTime();
            //foreach (var element in list)
            // {
            //     double remainTime = -1;
            //     string title = Tables.Instance.GetTable<Localize>().GetString(element.title);
            //     string desc = Tables.Instance.GetTable<Localize>().GetString(element.desc);


            //     switch (element.idx)
            //     {
            //         case 3:
            //             {
            //                 var nextDay = new DateTime(curTime.Year, curTime.Month, curTime.Day, 10, 0, 0).AddDays(2);
            //                 remainTime = (int)nextDay.Subtract(curTime).TotalSeconds;
            //             }
            //             break;
            //     }

            //     if (remainTime <= 0)
            //         continue;

            //     LocalNotification.CallNotification(
            //         element.idx,
            //         remainTime,
            //         title,
            //         desc);
            // }
        }
        else
        {
            var curTime = TimeSystem.GetCurTime();
            if ((curTime - GameRoot.Instance.UserData.CurMode.LastLoginTime).TotalSeconds > 5)
            {
                ShowAppOpenAdIfAvailable();
            }
        }
    }

    public void Dispose()
    {
#if !UNITY_EDITOR
        Firebase.Messaging.FirebaseMessaging.TokenReceived -= OnTokenReceived;
        Firebase.Messaging.FirebaseMessaging.MessageReceived -= OnMessageReceived;
#endif
        ADProp.Dispose();
    }

    public void ShowAppOpenAdIfAvailable()
    {
        return;
        // #if UNITY_EDITOR
        //         return;
        // #else
        //         if (appOpenAvailable
        //                 && GameRoot.Instance.UserData.BuyInappIds.Count == 0
        //                 && GameRoot.Instance.ShopSystem.state == ShopState.None
        //                 && TpPlatformLoginProp.state == LoginState.None
        //                 && !ADProp.showingAd
        //                 && !onReview
        //                 && GameRoot.Instance.UserData.mainData.StageData.StageIdx >= Tables.Instance.GetTable<Define>().GetData("appopen_ad_first_stage").value)
        //         {
        //             ADProp.ShowAppOpenAd();
        //         }
        // #endif
    }



    bool appOpenAvailable = false;
    void InitAppOpenAvailable()
    {
        var LocaleCode = BanpoFriNative.getLocaleCountry();
        switch (LocaleCode)
        {
            // case "US": //미국
            // case "KR": //한국
            // case "JP": //일본
            // case "TW": //대만
            // case "CA": //캐나다
            // case "UK": //영국
            // case "GB": //영국
            // case "DE": //독일
            // case "AU": //호주
            //     {
            //         appOpenAvailable = false;
            //     }
            //     break;

            default:
                //case "VN": //베트남
                //case "BR": //브라질
                //case "ID": //인도네시아
                //case "PH": //필리핀
                //case "MX": //멕시코
                //case "TH": //태국
                //case "MY": //말레이시아
                //case "FR": //프랑스
                //case "PL": //폴란드
                //case "IT": //이탈리아
                //case "ES": //스페인
                //case "CL": //칠레
                //case "DE": //독일
                //case "NL": //네덜란드
                //case "BE": //벨기에
                //case "UA": //우크라이나
                //case "AR": //아르헨티나
                //case "IN": //인도
                //case "TR": //터키
                //case "PT": //포르투갈
                //case "KZ": //카자흐스탄
                //case "CO": //콜롬비아
                //case "PE": //페루
                //case "RU": //러시아
                //case "SG": //싱가포르
                //case "BY": //벨라루스
                {
                    appOpenAvailable = false;
                    BpLog.Log("AppOpenAvailable");
                }
                break;
        }
    }
}
