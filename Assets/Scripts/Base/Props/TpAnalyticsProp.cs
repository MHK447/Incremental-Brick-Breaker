using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AppsFlyerSDK;
using Firebase.Analytics;


public enum IngameEventType
{
    None,
}

public sealed class TpParameter
{
    private string _key;
    private long _value;
    private double _dvalue;
    private string _strvalue;

    public TpParameter(string key, long value)
    {
        _key = key;
        _value = value;
        _dvalue = -1d;
        _strvalue = string.Empty;
    }

    public TpParameter(string key, string strvalue)
    {
        _key = key;
        _value = -1;
        _dvalue = -1d;
        _strvalue = strvalue;
    }

    public TpParameter(string key, double dvalue)
    {
        _key = key;
        _value = -1;
        _dvalue = dvalue;
        _strvalue = string.Empty;
    }

    public Parameter ConvertToFirebaseParameter()
    {
        if (_dvalue > -1d)
            return new Parameter(_key, _dvalue);
        else if (_value > -1)
            return new Parameter(_key, _value);
        else
            return new Parameter(_key, _strvalue);
    }

    public KeyValuePair<string, string> ConvertToKeyValuePair()
    {
        if (_dvalue > -1d)
            return new KeyValuePair<string, string>(_key, _dvalue.ToString());
        else if (_value > -1)
            return new KeyValuePair<string, string>(_key, _value.ToString());
        else
            return new KeyValuePair<string, string>(_key, _strvalue);
    }
}

public class TpAnalyticsProp
{
    public enum Analytics
    {
        Firebase,
        Appsflyer
    }


    public enum LogCostCashType
    {
        PurchaseChar = 0,
        CharRatioUpgrade = 1,
        StoneBuy = 2,
        CardBuy = 3,

        BuyTowerOrder = 4,

        ArtifactPopupPurchaseChar = 5,

        BuyDailyShop = 6,
        PassNextGem = 7,

        None = 9999,
    }


    private List<IAnalytics> analyticsList = new List<IAnalytics>() { new TpFirebaseProp(), new TpAppsflyerProp() };
    public void AllEvent(IngameEventType eventType, string eventName, params object[] args)
    {
        foreach (var analytics in analyticsList)
        {
            analytics.Event(eventType, eventName, args);
        }
    }

    public void TargetEvent(Analytics analytics, IngameEventType eventType, string eventName, params object[] args)
    {
        analyticsList[(int)analytics].Event(eventType, eventName, args);
    }

    public void InAppPurchaseEvent(params object[] args)
    {
        foreach (var analytics in analyticsList)
        {
            analytics.InAppPurchaseEvent(args);
        }
    }

    public void AppsflyerSendReadyLog()
    {
        ((TpAppsflyerProp)analyticsList[1]).AppsflyerReadyLogComplete();
    }
}

public class TpFirebaseProp : IAnalytics
{
    public void Event(IngameEventType eventType, string eventName, params object[] args)
    {
#if TREEPLLA_LOG && !TREEPLLA_CHEAT_LOG
        return;
#endif

        if (args.Length > 0)
        {
            List<TpParameter> tpParams = args[0] as List<TpParameter>;
            List<Parameter> parameters = new List<Parameter>();

            if (tpParams != null)
            {
                foreach (var pm in tpParams)
                {
                    parameters.Add(pm.ConvertToFirebaseParameter());
                }
            }

#if UNITY_EDITOR
            Debug.Log("fb log ===" + eventName);
#endif
            FirebaseAnalytics.LogEvent(eventName, parameters.ToArray());
        }
        else
        {
            FirebaseAnalytics.LogEvent(eventName);
        }
    }

    public void InAppPurchaseEvent(params object[] args)
    {

    }
}

public class TpAppsflyerProp : IAnalytics
{
    struct AppsFlyerLogReady
    {
        public string eventName;
        public Dictionary<string, string> parameters;
    }

    List<AppsFlyerLogReady> list_LogReady = new List<AppsFlyerLogReady>();

    public void AppsflyerReadyLogComplete()
    {
        list_LogReady.ForEach(x =>
        {
            Debug.Log("sendEvnet(list_LogReady) = " + x.eventName);
            AppsFlyer.sendEvent(x.eventName, x.parameters);
        });
    }

    public void Event(IngameEventType eventType, string eventName, params object[] args)
    {
#if TREEPLLA_LOG && !TREEPLLA_CHEAT_LOG
        return;
#endif
        Dictionary<string, string> parameters = null;
        if (args.Length > 0)
        {
            List<TpParameter> tpParams = args[0] as List<TpParameter>;
            parameters = new Dictionary<string, string>();
            if (tpParams != null)
            {
                foreach (var pm in tpParams)
                {
                    var keyvalue = pm.ConvertToKeyValuePair();
                    parameters.Add(keyvalue.Key, keyvalue.Value);
                }
            }
        }

        if (!PluginSystem.IsInitAppsflyer)
        {
            var readyEvent = new AppsFlyerLogReady();
            readyEvent.eventName = eventName; readyEvent.parameters = parameters;
            list_LogReady.Add(readyEvent);
            return;
        }

        Debug.Log($"sendEvent = " + eventName);
        AppsFlyer.sendEvent(eventName, parameters);
    }

    public void EventWithParameter(string eventName, Dictionary<string, string> args = null)
    {
#if UNITY_EDITOR
        Debug.Log("af log ===" + eventName);
#endif
        AppsFlyer.sendEvent(eventName, args);
    }

    public void InAppPurchaseEvent(params object[] args)
    {
        string currency = args[0] as string;
        string id = args[1] as string;
        string price = args[2] as string;
        string order_id = args[3] as string;


        AppsFlyer.setCurrencyCode(currency);
        Dictionary<string, string> purchaseEvent = new Dictionary<string, string>();
        purchaseEvent.Add(AFInAppEvents.CONTENT_ID, id);
        purchaseEvent.Add(AFInAppEvents.CONTENT_TYPE, "InApp");
        purchaseEvent.Add(AFInAppEvents.CURRENCY, currency);
        purchaseEvent.Add(AFInAppEvents.QUANTITY, "1");
        purchaseEvent.Add(AFInAppEvents.ORDER_ID, order_id);
        purchaseEvent.Add("stage", GameRoot.Instance.UserData.Stageidx.Value.ToString());
        purchaseEvent.Add("user_id", TpPlatformLoginProp.fUser?.UserId);
        purchaseEvent.Add(AFInAppEvents.REVENUE, price);
        var userid = string.Empty;
        
        AppsFlyer.sendEvent(AFInAppEvents.PURCHASE, purchaseEvent);
    }
}

interface IAnalytics
{
    void Event(IngameEventType eventType, string eventName, params object[] args);
    void InAppPurchaseEvent(params object[] args);
}