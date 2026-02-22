using System.Collections;
using System.Collections.Generic;
using TMPro;
using BanpoFri;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class AdsButton : MonoBehaviour
{
    [SerializeField]
    private GameObject AdButtonObj;
    [SerializeField]
    private GameObject AdTicketButtonObj;
    [SerializeField]
    private GameObject AdLoading;
    public bool AdLoaded;
    [SerializeField]
    private TextMeshProUGUI textTicketCnt;
    [SerializeField]
    public Button AdButton;
    [SerializeField]
    private List<TextMeshProUGUI> listTextBtn = new List<TextMeshProUGUI>();
    [SerializeField]
    private Button AdTicketButton;
    [SerializeField]
    private List<GameObject> AdIcon = new List<GameObject>();
    private TpMaxProp.AdRewardType logType;
    private System.Action OnAdSuccess;
    private System.Func<bool> CanClickButton;
    private bool FreeAD = false;
    public bool IsFree { get => FreeAD; }

    public void Awake()
    {
        AdButton.onClick.AddListener(OnClickAd);
        AdTicketButton.onClick.AddListener(OnClickAdTicket);
        GameRoot.Instance.PluginSystem.ADProp.AddRewardedAdEvent(ReadyAdCheck);
        if(GameRoot.Instance.UserData.Vipticket != null)
        {
            GameRoot.Instance.UserData.Vipticket.Subscribe(x => {
                ProjectUtility.SetActiveCheck(AdTicketButtonObj, x > 0);
                ProjectUtility.SetActiveCheck(AdButtonObj, x < 1);
                textTicketCnt.text = x.ToString();
            }).AddTo(this);
        }
        else
        {
            ProjectUtility.SetActiveCheck(AdTicketButtonObj, false);
            ProjectUtility.SetActiveCheck(AdButtonObj, true);
        }
    }

    private void OnDestroy()
    {
        // Avoid re-spawning GameRoot during teardown
        var root = GameRoot.GetInstance();
        if (root != null)
            root.PluginSystem.ADProp.RemoveRewardedAdEvent(ReadyAdCheck);
    }

    public void SetText(string _text)
    {
        foreach(var text in listTextBtn)
        {
            text.text = _text;
        }
    }


    public bool interactable { get
        {
            return AdButton.interactable;
        }
        set {
            AdButton.interactable = value;
            AdTicketButton.interactable = value;
        } 
    }

    private void ReadyAdCheck(bool ready)
    {
        AdLoaded = ready;
        if(FreeAD) {
            ProjectUtility.SetActiveCheck(AdLoading, false);
            return;
        }
#if UNITY_EDITOR || TREEPLLA_LOG
        ProjectUtility.SetActiveCheck(AdLoading, false); return;
#endif
        ProjectUtility.SetActiveCheck(AdLoading, !ready);
    }

    private void OnEnable()
    {
        ReadyAdCheck(GameRoot.Instance.PluginSystem.ADProp.IsRewardedAd());        
    }

    public void SetFree(bool bFree){
        FreeAD = bFree;
        foreach(var icon in AdIcon)
            icon.SetActive(!FreeAD);
        ReadyAdCheck(AdLoaded);
    }

    public void AddListener(TpMaxProp.AdRewardType _type, System.Action onAdSuccess, bool bFree = false)
    {
        logType = _type;
        OnAdSuccess = onAdSuccess;
        FreeAD = bFree;
        foreach(var icon in AdIcon)
            ProjectUtility.SetActiveCheck(icon, !FreeAD);
    }

    public void AddListener(TpMaxProp.AdRewardType _type, System.Action onAdSuccess, System.Func<bool> canWatchAd, bool bFree = false)
    {
        AddListener(_type, onAdSuccess, bFree);
        CanClickButton = canWatchAd;
    }

    public void SetAdIcon(bool value)
    {
        foreach (var icon in AdIcon)
            ProjectUtility.SetActiveCheck(icon, !FreeAD);
    }

    private void OnClickAd()
    {
        bool canWatch = true;
        if (CanClickButton != null) canWatch = CanClickButton.Invoke();

        if (!canWatch) return;

        if (FreeAD)
            OnAdSuccess?.Invoke();
        else
        {
            GameRoot.Instance.PluginSystem.ADProp.ShowRewardAD(logType, (result) =>
            {
                OnAdSuccess?.Invoke();
            });
        }
    }

    private void OnClickAdTicket()
    {
        if(GameRoot.Instance.UserData.Vipticket != null && GameRoot.Instance.UserData.Vipticket.Value > 0)
        {
            //GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Item, (int)Config.ItemType.VIPTicket, -1);
            OnAdSuccess?.Invoke();

            var ticketCnt = GameRoot.Instance.UserData.GetRecordValue(Config.RecordKeys.UseADTicketCnt);
            if(ticketCnt < 0)
                ticketCnt = 0;

            ++ticketCnt;
            List<TpParameter> parameters = new List<TpParameter>();
            parameters.Add(new TpParameter("stage", GameRoot.Instance.UserData.Stageidx.Value));
            parameters.Add(new TpParameter("idx", (int)logType));
            parameters.Add(new TpParameter("count",ticketCnt));
            GameRoot.Instance.PluginSystem.AnalyticsProp.AllEvent(IngameEventType.None, "m_ads_ticket", parameters);

            GameRoot.Instance.UserData.SetRecordValue(Config.RecordKeys.UseADTicketCnt, ticketCnt);
        }
    }
}
