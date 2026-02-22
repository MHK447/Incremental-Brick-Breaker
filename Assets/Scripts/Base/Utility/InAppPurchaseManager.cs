using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine.Purchasing.Extension;

namespace BanpoFri
{
    public class InAppPurchaseManager : MonoBehaviour, IDetailedStoreListener
    {
        private const string googleTreepllaPurchaseServerURL = "https://us-central1-inapppurchasevalidation-c062c.cloudfunctions.net/verifyInAppPurchaseGoogleMH";
        private const string appleTreepllaPurchaseServerURL = "https://us-central1-inapppurchasevalidation-c062c.cloudfunctions.net/verifyInAppPurchaseAppleMH";

        ///<summary> softlaunching only </summary>
        //public static readonly string NoAds_productID = "luckyguy_100_noisads";
        //public static readonly string NoAdsPackage_productID = "luckyguy_100_noisads";

        public static readonly string luckyguy_1001_Currency_Package = "luckyguy_1001_lowestprice_1";
        public static readonly string luckyguy_1002_Card_Package = "luckyguy_1002_lowestprice_2";
        public static readonly string Frost_Character_Package = "luckyguy_1101_character_1";
        public static readonly string Poison_Character_Package = "luckyguy_1102_character_2";
        public static readonly string Spark_Character_Package = "luckyguy_1103_character_3";
        public static readonly string Brady_Character_Package = "luckyguy_1104_character_4";
        public static readonly string luckyguy_Rare_Artifact_Random = "luckyguy_1201_rnd_arti";
        public static readonly string luckyguy_Epic_Artifact_Random = "luckyguy_1202_rnd_arti";
        public static readonly string luckyguy_Legend_Artifact_Random = "luckyguy_1203_rnd_arti";
        public static readonly string luckyguy_item_key_package = "luckyguy_1301_key_bundle";
        public static readonly string Noads = "noads_1001";
        public static readonly string NoadsCurrency = "luckyguy_1003_starter";
        public static readonly string BlessNoAds = "luckyguy_1402_bless";
        public static readonly string SpeedUp = "luckyguy_1403_speedup";
        public static readonly string VIPForever = "luckyguy_112_vip";
        public static readonly string VIPForeverSale = "luckyguy_114_vip";

        public static readonly List<string> nonConsumableItemList = new List<string>()
        {
            Frost_Character_Package,
            Poison_Character_Package,
            Noads,
            NoadsCurrency,
            VIPForever,
            VIPForeverSale,
            BlessNoAds,
        };


        public enum PackageIdx
        {
            PackageSpecialOffer = 100004,
            PackageUltimateOffer = 100005,
            PackageWorkSpaceOffer = 100006,
            PackagePremiumPass = 100003,
            PackageNoadsOrigin = 100002,
            PackageNoads52 = 100007,
            PackageNoads34 = 100008,
            PackageManagementExpert = 100011,
            PackageStarterPack = 100012,
        }

        public enum EventPackageIdx
        {
            PackageEventWoodSpecialOffer = 100013,
            PackageEventSpecialOffer = 100014,
            PackageEventStarterPack = 100015,
            PackageEventBoostPack = 100016,
            PackageEventProPack = 100017,
            PackageEventPowerPack = 100018,
            PackageEventUltimatePack = 100019,
            PackageEventSuperPackage = 100020,
        }

        public bool recoverInterNoads { get; private set; } = false;
        public bool recoveryFrostChar { get; private set; } = false;
        public bool recoveryPoisonChar { get; private set; } = false;
        public bool recoveryRevenue { get; private set; } = false;
        public bool recoveryChapter { get; private set; } = false;
        public bool recoveryAutoTreat { get; private set; } = false;
        public bool recoveryBlessNoads { get; private set; } = false;
        public bool recoveryVIP { get; private set; } = false;

        private bool checkNonConsumable = false;
        private bool serverWait = false;
        enum ProcessPurchaseType
        {
            Initialzing, // IAP 매니저 초기화 중
            BuyProduct, // 유저가 상품 구매 버튼을 눌렀을 때
            RestorePurchase, // 유저가 설정의 구매복원 버튼을 눌렀을 때
        }

        private ProcessPurchaseType processPurchaseType = ProcessPurchaseType.Initialzing;
        public enum Result
        {
            Failed,
            Success,
        }

        enum ReceiptValidationError
        {
            Unknown,
            ConnectionError,
            InvalidReceipt,
            DuplicateReceipt,
            JsonParsingFailed,
        }

        //https://docs.unity3d.com/Manual/UnityIAPPurchaseReceipts.html?_ga=2.253454090.264938391.1603071359-367754767.1592457765
        [System.Serializable]
        public class Receipts
        {
            public string Store;
            public string TransactionID;
            public string Payload;
            public Payload PayloadData;

            public void CovertPayloadData()
            {
                PayloadData = JsonUtility.FromJson<Payload>(Payload);
            }
        }
        [System.Serializable]
        public class Payload
        {
            public jsonData jsonData;
            public string json;
            public string signature;

            public void CovertJsonData()
            {
                jsonData = JsonUtility.FromJson<jsonData>(json);
            }
        }
        [System.Serializable]
        public class jsonData
        {
            public string orderId;
            public string packageName;
            public string productId;
            public string purchaseTime;
            public string purchaseState;
            public bool acknowledged;
            public string purchaseToken;
        }

        [System.Serializable]
        public abstract class ISendData
        {
            public string orderId;
            public string packageName;
            public string productId;
            public int itemidx;
            public string priceCode;
            public string price;
        }

        [System.Serializable]
        public class GoogleSendData : ISendData
        {
            public long purchaseTime;
            public long purchaseState;
            public bool acknowledged;
            public string purchaseToken;
            public string signature;
        }

        [System.Serializable]
        public class AppleSendData : ISendData
        {
            public string receipt;
        }

        class ReturnData
        {
            public int result;
        }

        private static IStoreController storeController;
        private static IExtensionProvider extensionProvider;

        struct ProcessPurchaseStreamData
        {
            public Result result;
            public string productId;
        }

        private Subject<ProcessPurchaseStreamData> buyProductStream = new Subject<ProcessPurchaseStreamData>();
        private Subject<ProcessPurchaseStreamData> restoreTransactionStream = new Subject<ProcessPurchaseStreamData>();
        private Subject<PurchaseEventArgs> processPurchaseStream = new Subject<PurchaseEventArgs>();
        private ReactiveProperty<string> prevTransactionID = new ReactiveProperty<string>();
        private System.IDisposable prevTransactionSubscription;

        public void Init()
        {
            BpLog.Log("[IAP] UNITY_IAP init");
            var module = StandardPurchasingModule.Instance();
#if UNITY_EDITOR
            module.useFakeStoreAlways = true;
#endif
            ConfigurationBuilder builder = ConfigurationBuilder.Instance(module);
            ProductCatalog catalog = ProductCatalog.LoadDefaultCatalog();
            PopulateConfigurationBuilder(ref builder, catalog);
            UnityPurchasing.Initialize(this, builder);

            prevTransactionID = new ReactiveProperty<string>();
            prevTransactionSubscription?.Dispose();
            prevTransactionSubscription = prevTransactionID.Subscribe(transactionId =>
            {
                BpLog.Log("[IAP] prevTransactionID : " + transactionId);
            });
        }


        // public bool IsNoAds()
        // {
        //     return GameRoot.Instance.UserData.BuyInappIds.Contains(InAppPurchaseManager.NoAds_productID);
        // }
        private bool IsNonConsumableProduct(string productId)
        {
            return nonConsumableItemList.Contains(productId);
        }

        private bool IsInitialized()
        {
            return (storeController != null && extensionProvider != null);
        }

        public void InitializePurchasing()
        {
            if (IsInitialized())
                return;

            BpLog.Log("UNITY_IAP init");

            var module = StandardPurchasingModule.Instance();
#if UNITY_EDITOR
            module.useFakeStoreAlways = true;
#endif

            ConfigurationBuilder builder = ConfigurationBuilder.Instance(module);
            ProductCatalog catalog = ProductCatalog.LoadDefaultCatalog();
            PopulateConfigurationBuilder(ref builder, catalog);

            BpLog.Log("UNITY_IAP init");

            UnityPurchasing.Initialize(this, builder);
        }

        public bool checkItemBougthAnything()
        {
            if (storeController != null && storeController.products != null)
            {
                foreach (var pd in storeController.products.all)
                {
                    if (pd.hasReceipt) return true;
                }
            }

            return false;
        }

        public IEnumerator WaitTime(float time, Action End)
        {
            yield return new WaitForSeconds(time);
            End?.Invoke();
        }

        public bool checkItemBought(string productId)
        {
            var product = storeController.products.WithID(productId);
            if (product != null && product.hasReceipt)
            {
                return true;
            }
            return false;
        }

        private int tableIdx = 0;

        public void BuyProductID(string productId, int _tableIdx, System.Action<Result> onCompeleteAction)
        {
            try
            {
                if (IsInitialized())
                {
                    var p = storeController.products.WithID(productId);
                    if (p != null && p.availableToPurchase)
                    {
                        BpLog.Log(string.Format("[IAP]Purchasing product asychronously: '{0}'", p.definition.id));
                        processPurchaseType = ProcessPurchaseType.BuyProduct;
                        buyProductStream = new Subject<ProcessPurchaseStreamData>();
                        buyProductStream.AsObservable().Take(1).Subscribe(x =>
                        {
                            BpLog.Log("[IAP] process purchase ended: " + x.result + " - " + x.productId);
                            processPurchaseType = ProcessPurchaseType.Initialzing;
                            onCompeleteAction?.Invoke(x.result);
                        });
                        prevTransactionID.SetValueAndForceNotify(p.transactionID);
                        storeController.InitiatePurchase(p);
                        this.tableIdx = _tableIdx;
                    }
                    else
                    {
                        BpLog.Log("[IAP] BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                    }
                }
                else
                {
                    BpLog.Log("[IAP] BuyProductID FAIL. Not initialized.");
                }
            }
            catch (Exception e)
            {
                BpLog.Log("[IAP] BuyProductID: FAIL. Exception during purchase. " + e);
            }
        }

        public void RestorePurchase(System.Action<Result> onCompeleteAction)
        {
            if (!IsInitialized())
            {
                BpLog.Log("[IAP] RestorePurchases FAIL. Not initialized.");
                return;
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
            {
                BpLog.Log("[IAP] RestorePurchases started ...");

                buyProductStream = new Subject<ProcessPurchaseStreamData>();
                restoreTransactionStream = new Subject<ProcessPurchaseStreamData>();
                processPurchaseType = ProcessPurchaseType.RestorePurchase;
                // 복구 트랜잭션 진행 결과 스트림
                var restoreSignal = Observable.Merge(buyProductStream.AsObservable(), restoreTransactionStream.AsObservable()).Throttle(TimeSpan.FromSeconds(2)).Take(1).Select(x => (x.productId, x.result, false));
                // 비소모성상품 구매한 적이 없다면 restoreSignal이 발생하지 않음에, 시간이 지나면 취소함
                var timeoutSignal = Observable.Timer(TimeSpan.FromSeconds(5)).Select(x => ("", Result.Failed, true));
                var disposeSignal = Observable.Merge(timeoutSignal, restoreSignal).Take(1).Subscribe(x =>
                {
                    string productId = x.Item1;
                    Result result = x.Item2;
                    bool isTimeout = x.Item3;
                    BpLog.Log($"[IAP] RestorePurchases: productId = {productId}, result = {result}, isTimeout = {isTimeout}");
                    processPurchaseType = ProcessPurchaseType.Initialzing;
                    onCompeleteAction?.Invoke(result);
                });
                extensionProvider.GetExtension<IAppleExtensions>().RestoreTransactions((result, msg) =>
                    {
                        BpLog.Log($"[IAP] RestoreTransactions Result: {result}, msg: {msg}");
                        if (false == result)
                        {
                            disposeSignal.Dispose();
                            processPurchaseType = ProcessPurchaseType.Initialzing;
                            onCompeleteAction?.Invoke(Result.Failed);
                        }
                    }
                );
            }
            else if (Application.platform == RuntimePlatform.Android)
            {
                bool recovery = false;
                if (storeController is { products: not null })
                {
                    foreach (var item in nonConsumableItemList)
                    {
                        Product product = storeController.products.WithID(item);
                        if (product is { hasReceipt: true })
                        {
                            recovery |= TryRestoreNonConsumableItem(item);
                        }
                    }
                }

                BpLog.Log("[IAP] RestorePurchases: " + recovery);
                onCompeleteAction?.Invoke(recovery ? Result.Success : Result.Failed);
            }
            else
            {
                BpLog.Log("[IAP] RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
                onCompeleteAction?.Invoke(Result.Failed);
            }
        }

        public void OnInitialized(IStoreController sc, IExtensionProvider ep)
        {
            BpLog.Log("[IAP] OnInitialized : PASS");

            storeController = sc;
            extensionProvider = ep;

            if (Application.platform == RuntimePlatform.Android && storeController != null)
            {
                FetchProducts();
            }
        }

        public void OnInitializeFailed(InitializationFailureReason reason)
        {
            BpLog.Log("[IAP] OnInitializeFailed InitializationFailureReason:" + reason);
        }


        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            BpLog.Log($"OnInitializeFailed InitializationFailureReason:{error} " + message);
        }

        public void OnPurchaseFailed(UnityEngine.Purchasing.Product product, PurchaseFailureReason failureReason)
        {
        }

        private Subject<bool> FetchProducts()
        {
            BpLog.Log("[IAP] FetchProducts");

            HashSet<ProductDefinition> hashSet = new HashSet<ProductDefinition>();
            Subject<bool> fetchStream = new Subject<bool>();
            if (null != storeController)
            {
                storeController.FetchAdditionalProducts(hashSet,
                    () =>
                    {
                        BpLog.Log("[IAP] fetchSuccess");
                        fetchStream.OnNext(true);
                    },
                    (e, str) =>
                    {
                        BpLog.Log($"fetchFailed, {e} / {str}");
                        fetchStream.OnNext(false);
                    });
            }

            return fetchStream;
        }

        private void SendInAppPurchaseEvent(ISendData sendData, string productTransactionID)
        {
            string id = sendData.productId;
            string priceCode = sendData.priceCode;
            string price = sendData.price;
            string orderId = sendData.orderId;
            int idx = sendData.itemidx;

            bool isInitUserData = GameRoot.Instance.UserData != null && GameRoot.Instance.UserData.CurMode != null;
            if (!isInitUserData)
            {
                BpLog.LogError($"User data is not initialized. id : {id}, priceCode : {priceCode}, price : {price}, orderId : {orderId}");
            }

            if (isInitUserData)
            {
                if (GameRoot.Instance.UserData != null && GameRoot.Instance.UserData.CurMode != null)
                {
                  
                    List<TpParameter> parameters = new List<TpParameter>();
                    parameters.Add(new TpParameter("stage", GameRoot.Instance.UserData.Stageidx.Value));
                    parameters.Add(new TpParameter("idx", idx));
                    parameters.Add(new TpParameter("af_content_id", id));
                    int recordCount = 0;
                    var recordkey = ProjectUtility.GetRecordCountText(Config.RecordCountKeys.BuyInAppCountTotal);
                    GameRoot.Instance.UserData.AddRecordCount(Config.RecordCountKeys.BuyInAppCountTotal, 1);
                    if (GameRoot.Instance.UserData.RecordCount.ContainsKey(recordkey))
                    {
                        recordCount = GameRoot.Instance.UserData.RecordCount[recordkey];
                    }
                    parameters.Add(new TpParameter("count", recordCount));
                    parameters.Add(new TpParameter("place", GameRoot.Instance.ShopSystem.curLocation.ToString()));
                    GameRoot.Instance.PluginSystem.AnalyticsProp.AllEvent(IngameEventType.None,
                        "m_purchase_inapp", parameters);


                    if (recordCount == 1)
                    {
                        parameters = new List<TpParameter>();
                        parameters.Add(new TpParameter("idx",idx));
                        parameters.Add(new TpParameter("af_content_id", id));
                        parameters.Add(new TpParameter("stage", GameRoot.Instance.UserData.Stageidx.Value));
                        parameters.Add(new TpParameter("place", GameRoot.Instance.ShopSystem.curLocation.ToString()));
                        GameRoot.Instance.PluginSystem.AnalyticsProp.AllEvent(IngameEventType.None,
                            "m_purchase_first", parameters);
                    }

                    bool isPendingProduct = prevTransactionID.Value == productTransactionID;
                    BpLog.Log("[IAP] Pending Product : " + isPendingProduct);
                    if (isPendingProduct)
                    {
                        string userId = string.Empty;
                        if (null != TpPlatformLoginProp.fUser && null != TpPlatformLoginProp.fUser.UserId)
                        {
                            userId = TpPlatformLoginProp.fUser.UserId;
                        }
                        parameters = new List<TpParameter>();
                        parameters.Add(new TpParameter("stage", GameRoot.Instance.UserData.Stageidx.Value));
                        parameters.Add(new TpParameter("idx", idx));
                        parameters.Add(new TpParameter("af_content_id", id));
                        parameters.Add(new TpParameter("count", recordCount));
                        parameters.Add(new TpParameter("place", GameRoot.Instance.ShopSystem.curLocation.ToString()));
                        parameters.Add(new TpParameter("order_id", orderId));
                        parameters.Add(new TpParameter("user_id", userId));
                        GameRoot.Instance.PluginSystem.AnalyticsProp.AllEvent(IngameEventType.None, "m_purchase_restore", parameters);
                    }
                }
            }

            GameRoot.Instance.PluginSystem.AnalyticsProp.InAppPurchaseEvent(priceCode, id, price, orderId);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            BpLog.Log(string.Format("[IAP] OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}, PurchaseFailureDescription: {2}", product.definition.storeSpecificId,
                failureDescription.reason, failureDescription.message));

            if (Application.platform == RuntimePlatform.Android) // Android는 Pending 상품을 다시 재구매할 경우 DuplicateTransaction 발생하고 ProcessPurchase 자동호출
            {
                if (failureDescription.reason is PurchaseFailureReason.DuplicateTransaction or PurchaseFailureReason.Unknown)
                {
                    string transactionID = product.transactionID;
                    // Pending 상태에서 이미 결제한 상품을 다시 구매 시도하면, DuplicateTransaction(또는 간혈적으로 Unknown) 에러가 발생합니다. 
                    // 이 경우 FetchProducts를 호출하면 상품 정보가 갱신되고 ProcessPurchase 메서드가 자동으로 호출되어 결제를 정상적으로 처리할 수 있습니다.
                    FetchProducts().Take(1).Subscribe(success =>
                    {
                        if (success)
                        {
                            prevTransactionID.SetValueAndForceNotify(transactionID);
                            processPurchaseType = ProcessPurchaseType.BuyProduct; // 자동 호출될 ProcessPurchase에서 구매 플로우 진행되도록 설정 
                        }

                    });

                    // 과거에 구매했던 비소모성 상품을 (Pending 상태가 아닐 때) 구매 시도를 하면 결제창에러(Got it)가 DP되고 지금 이곳의 OnPurchaseFailed가 호출됩니다.
                    // 이 경우 상품은 비소모성 이기에 응당 구매 불가능한 상태이고 따라서, ProcessPurchase 메서드는 호출되지 않습니다.
                    // 이러한 케이스를 처리하기 위해 Fetch 이후 5초간 대기하여 ProcessPurchase 메서드가 호출되지 않는다면, 구매실패 이벤트를 발생시켜 결제를 종료합니다.
                    // 참고로, iOS는 이 경우 ProcessPurchase 메서드가 호출되어 (이미 구매했던 비소모성 상품이지만) 결제 처리를 정상적으로 진행합니다.
                    Observable.Amb( // 5초 타이머와 구매 처리 스트림 중 먼저 오는 것 감지
                        processPurchaseStream.Take(1).Select(_ => false),  // 구매 처리 완료
                        Observable.Timer(TimeSpan.FromSeconds(5)).Select(_ => true)  // 타임아웃
                    ).Take(1).Subscribe(isTimeout =>
                    {
                        BpLog.Log($"[IAP] iap process wait timeout => {isTimeout}");
                        if (false == isTimeout)
                        {
                            return;
                        }

                        buyProductStream.OnNext(new ProcessPurchaseStreamData() { result = Result.Failed, productId = product.definition.id });

                        // 구매 복구 안내 토스트 메시지
                        if (IsNonConsumableProduct(product.definition.id))
                        {
                            GameRoot.Instance.UISystem.OpenUI<PopupToastmessage>(popup =>
                            {
                                popup.Show(
                                    Tables.Instance.GetTable<Localize>().GetString("str_iap_recommend_restore_title"),
                                    Tables.Instance.GetTable<Localize>().GetString("str_iap_recommend_restore_desc")
                                );
                            });
                        }
                    });
                    return;
                }
            }

            buyProductStream.OnNext(new ProcessPurchaseStreamData() { result = Result.Failed, productId = product.definition.id });
        }

        public UnityEngine.Purchasing.Product GetProduct(string productID)
        {
            if (storeController != null && storeController.products != null && !string.IsNullOrEmpty(productID))
            {
                return storeController.products.WithID(productID);
            }
            BpLog.LogError("product attempted to get unknown product " + productID);
            return null;
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
        {
            BpLog.Log("[IAP] ProcessPurchase Start");

            ProcessPurchaseType purchaseType = processPurchaseType;
            try
            {
                processPurchaseStream.OnNext(e);

                if (ProcessPurchaseType.Initialzing == purchaseType) // IAP 초기화 중에 ProcessPurchase가 호출될 수 있다.
                {
                    string productId = e.purchasedProduct.definition.id;
                    if (TryRestoreNonConsumableItem(productId)) // 비소모성 상품을 이전에 구매한 경우, 복원 처리
                    {
                        BpLog.Log("[IAP] ProcessPurchase restore non-consumable item : " + productId);
                        return PurchaseProcessingResult.Complete;
                    }
                    BpLog.Log("[IAP] ProcessPurchase skip restore on initializing: " + productId);
                    return PurchaseProcessingResult.Pending;
                }

                switch (Application.platform)
                {
                    case RuntimePlatform.Android:
                        if (HandleGoogleReceipt(e.purchasedProduct))
                        {
                            return PurchaseProcessingResult.Pending; // 영수증 유효검사 전까지 결제지연 처리
                        }
                        break;
                    case RuntimePlatform.IPhonePlayer:
                    case RuntimePlatform.OSXPlayer:
                        {
                            if (HandleAppleReceipt(e.purchasedProduct))
                            {
                                return PurchaseProcessingResult.Pending; // 영수증 유효검사 전까지 결제지연 처리
                            }
                            break;
                        }
                }

                if (ProcessPurchaseType.BuyProduct == purchaseType)
                {
                    BpLog.Log("[IAP] Not expected receipt : " + e.purchasedProduct.definition.id);
                    buyProductStream.OnNext(new ProcessPurchaseStreamData() { result = Result.Failed, productId = e.purchasedProduct.definition.id });
                }
            }
            catch (Exception exception)
            {
                BpLog.Log("[IAP] ProcessPurchase exception : " + exception);
            }

            return PurchaseProcessingResult.Complete;
        }

        private bool HandleAppleReceipt(Product product)
        {
            CrossPlatformValidator validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);
            List<AppleInAppPurchaseReceipt> appleReceipts = validator.Validate(product.receipt).Where(x => null != x).OfType<AppleInAppPurchaseReceipt>().ToList();

            bool tryValidate = false;
            if (ProcessPurchaseType.RestorePurchase == processPurchaseType)
            {
                foreach (var apple in appleReceipts)
                {
                    if (TryRestoreNonConsumableItem(apple.productID))
                    {
                        BpLog.Log("[IAP] ProcessPurchase restore non-consumable item : " + apple.productID);
                        restoreTransactionStream.OnNext(new ProcessPurchaseStreamData() { result = Result.Success, productId = apple.productID });
                    }
                }
            }
            else if (ProcessPurchaseType.BuyProduct == processPurchaseType)
            {
                var receipt = appleReceipts.Where(x => x.productID == product.definition.id).FirstOrDefault();
                string transactionReceipt = extensionProvider.GetExtension<IAppleExtensions>().GetTransactionReceiptForProduct(product);
                if (false == string.IsNullOrEmpty(transactionReceipt) && null != receipt)
                {
                    BpLog.Log("[IAP] ProcessPurchase validate receipt : " + receipt.productID);
                    ISendData sendData = ToAppleSendData(product, receipt, transactionReceipt);
                    ValidateReceipt(product, sendData);
                    tryValidate = true;
                }
            }

            return tryValidate;
        }

        private bool HandleGoogleReceipt(Product purchasedProduct)
        {
            CrossPlatformValidator validator = new CrossPlatformValidator(GooglePlayTangle.Data(), AppleTangle.Data(), Application.identifier);
            List<GooglePlayReceipt> googleReceipts = validator.Validate(purchasedProduct.receipt).Where(x => null != x).OfType<GooglePlayReceipt>().ToList();

            bool validateReceipt = false;
            Receipts receipts = JsonUtility.FromJson<Receipts>(purchasedProduct.receipt);
            receipts.CovertPayloadData();
            receipts.PayloadData.CovertJsonData();
            if (processPurchaseType == ProcessPurchaseType.BuyProduct)
            {
                foreach (var google in googleReceipts)
                {
                    BpLog.Log("[IAP] ProcessPurchase google.productID : " + google.productID);
                    if (receipts.PayloadData.jsonData.orderId == google.orderID)
                    {
                        if (google.productID == purchasedProduct.definition.id)
                        {
                            if (google.purchaseState == GooglePurchaseState.Purchased)
                            {
                                BpLog.Log("[IAP] ProcessPurchase validate receipt : " + google.productID);
                                ISendData sendData = ToGoogleSendData(purchasedProduct, google, receipts);
                                ValidateReceipt(purchasedProduct, sendData);
                                validateReceipt = true;
                            }
                        }
                    }
                }
            }
            return validateReceipt;
        }

        private void ValidateReceipt(Product product, ISendData sendData)
        {
            string serverUrl = googleTreepllaPurchaseServerURL;
            if (Application.platform == RuntimePlatform.IPhonePlayer || Application.platform == RuntimePlatform.OSXPlayer)
            {
                serverUrl = appleTreepllaPurchaseServerURL;
            }

            StartCoroutine(PostJson(JsonUtility.ToJson(sendData), sendData, serverUrl, () =>
            {
                BpLog.Log("[IAP] ValidateReceipt Confirm Pending Purchase : " + product.definition.id);
                storeController.ConfirmPendingPurchase(product); // 결제지연 완료 (finishTransaction)
                SendInAppPurchaseEvent(sendData, product.transactionID);
                buyProductStream.OnNext(new ProcessPurchaseStreamData() { result = Result.Success, productId = product.definition.id });
            }, (error) =>
            {
                BpLog.Log("[IAP] ValidateReceipt Failed : " + error);
                buyProductStream.OnNext(new ProcessPurchaseStreamData() { result = Result.Failed, productId = product.definition.id });
                if (error == ReceiptValidationError.ConnectionError) // 인터넷 연결 끊김으로, 보상을 미지급한 케이스가 발생할 수 있음으로 결제버튼을 다시 누를 수 있도록 유도 
                {
                    BpLog.Log("[IAP] ValidateReceipt ConnectionError");
                    GameRoot.Instance.UISystem.OpenUI<PopupToastmessage>(popup =>
                    {
                        BpLog.Log("[IAP] ValidateReceipt ConnectionError Toast");
                        popup.Show(Tables.Instance.GetTable<Localize>().GetString("str_iap_not_received_toast_title"), Tables.Instance.GetTable<Localize>().GetString("str_iap_not_received_toast_desc"));
                    });
                }
            }));
        }

        private bool TryRestoreNonConsumableItem(string productID)
        {
            // if (NoadsCurrency == productID)
            // {
            //     if (!GameRoot.Instance.UserData.BuyInappIds.Contains(NoadsCurrency))
            //     {
            //         GameRoot.Instance.UserData.BuyInappIds.Add(NoadsCurrency);
            //         GameRoot.Instance.ShopSystem.NoInterstitialAds.Value = true;
            //         TpLog.Log("restored managerExpertPack");
            //     }
            //     return true;
            // }

            // if (Noads == productID)
            // {
            //     if (!GameRoot.Instance.UserData.BuyInappIds.Contains(Noads))
            //     {
            //         GameRoot.Instance.UserData.BuyInappIds.Add(Noads);
            //         GameRoot.Instance.ShopSystem.NoInterstitialAds.Value = true;
            //         TpLog.Log("restored NoAds");
            //     }

            //     return true;
            // }

            if (VIPForever == productID)
            {
                if (!GameRoot.Instance.UserData.BuyInappIds.Contains(VIPForever))
                {
                    GameRoot.Instance.UserData.BuyInappIds.Add(VIPForever);
                    BpLog.Log("restored VIPForever");
                }

                return true;
            }

            if (VIPForeverSale == productID)
            {
                if (!GameRoot.Instance.UserData.BuyInappIds.Contains(VIPForeverSale))
                {
                    GameRoot.Instance.UserData.BuyInappIds.Add(VIPForeverSale);
                    BpLog.Log("restored VIPForeverSale");
                }

                return true;
            }

            if (BlessNoAds == productID)
            {
                if (!GameRoot.Instance.UserData.BuyInappIds.Contains(BlessNoAds))
                {
                    GameRoot.Instance.UserData.BuyInappIds.Add(BlessNoAds);
                    BpLog.Log("restored BlessNoAds");
                }
                return true;
            }



            return false;
        }

        private ISendData ToAppleSendData(Product product, AppleInAppPurchaseReceipt apple, string transactionReceipt)
        {
            string CurrencyCode = product.metadata.isoCurrencyCode;
            string PurchasePrice = product.metadata.localizedPrice.ToString();

            var newPrice = PurchasePrice;
            if (PurchasePrice.Contains(","))
            {
                newPrice = PurchasePrice.Replace(",", ".");
            }
            return new AppleSendData()
            {
                orderId = apple.transactionID,
                packageName = Application.identifier,
                productId = apple.productID,
                itemidx = tableIdx,
                priceCode = CurrencyCode,
                price = newPrice,
                receipt = transactionReceipt
            };
        }

        private ISendData ToGoogleSendData(Product product, GooglePlayReceipt google, Receipts receipts)
        {
            string CurrencyCode = product.metadata.isoCurrencyCode;
            string PurchasePrice = product.metadata.localizedPrice.ToString();
            var newPrice = PurchasePrice;
            if (PurchasePrice.Contains(","))
            {
                newPrice = PurchasePrice.Replace(",", ".");
            }

            return new GoogleSendData()
            {
                orderId = google.orderID,
                packageName = google.packageName,
                productId = google.productID,
                itemidx = tableIdx,
                priceCode = CurrencyCode,
                price = newPrice,
                purchaseTime = long.Parse(receipts.PayloadData.jsonData.purchaseTime),
                purchaseState = long.Parse(receipts.PayloadData.jsonData.purchaseState),
                acknowledged = receipts.PayloadData.jsonData.acknowledged,
                purchaseToken = google.purchaseToken,
                signature = receipts.PayloadData.signature,
            };
        }

        IEnumerator PostJson(string jsonSendData, ISendData data, string serverURL, System.Action OnSuccess, System.Action<ReceiptValidationError> OnFail = null, bool log = true)
        {
            BpLog.Log("[IAP] purchase information Test: PostJson : " + jsonSendData);

            using (UnityWebRequest www = new UnityWebRequest(serverURL, "POST"))
            {
                www.uploadHandler = (UploadHandler)new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonSendData));
                www.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
                www.SetRequestHeader("Content-Type", "application/json");

                yield return www.SendWebRequest();

                if (www.result == UnityWebRequest.Result.ConnectionError)
                {
                    BpLog.LogError(www.error);
                    OnFail?.Invoke(ReceiptValidationError.ConnectionError);
                }
                else
                {
                    BpLog.Log("[IAP] result :" + www.downloadHandler.text);
                    ReturnData resultData = JsonUtility.FromJson<ReturnData>(www.downloadHandler.text);
                    if (resultData != null)
                    {
                        switch (resultData.result)
                        {
                            case 0:
                                {
                                    BpLog.LogError("[IAP] purchase information Test: Purchase Fail");
                                    // fail
                                    OnFail?.Invoke(ReceiptValidationError.InvalidReceipt);
                                }
                                break;
                            case 1:
                                {
                                    BpLog.LogError("[IAP] purchase information Test: Purchase Success");
                                    OnSuccess?.Invoke();
                                }
                                break;
                            case 3:
                                {
                                    BpLog.LogError("[IAP] purchase information Test: Purchase Duplicate");
                                    OnFail?.Invoke(ReceiptValidationError.DuplicateReceipt);
                                }
                                break;
                            default:
                                {
                                    BpLog.LogError($"Don't exist result type, text: {www.downloadHandler.text}");
                                    OnFail?.Invoke(ReceiptValidationError.Unknown);
                                }
                                break;
                        }
                    }
                    else
                    {
                        BpLog.LogError($"File is not json, text: {www.downloadHandler.text}");
                        OnFail?.Invoke(ReceiptValidationError.JsonParsingFailed);
                    }
                }
            }
        }

        public void PopulateConfigurationBuilder(ref ConfigurationBuilder builder, ProductCatalog catalog)
        {
            foreach (var product in catalog.allValidProducts)
            {
                IDs ids = null;

                if (product.allStoreIDs.Count > 0)
                {
                    ids = new IDs();
                    foreach (var storeID in product.allStoreIDs)
                    {
                        ids.Add(storeID.id, storeID.store);
                    }
                }

#if UNITY_2017_2_OR_NEWER

                var payoutDefinitions = new List<PayoutDefinition>();
                foreach (var payout in product.Payouts)
                {
                    payoutDefinitions.Add(new PayoutDefinition(payout.typeString, payout.subtype, payout.quantity, payout.data));
                }
                builder.AddProduct(product.id, product.type, ids, payoutDefinitions.ToArray());

#else

                builder.AddProduct(product.id, product.type, ids);

#endif
            }
        }

        public static bool IsNoAdsPackageID(int pid)
        {
            return pid == (int)PackageIdx.PackageNoadsOrigin || pid == (int)PackageIdx.PackageNoads34 || pid == (int)PackageIdx.PackageNoads52;
        }
    }

}
