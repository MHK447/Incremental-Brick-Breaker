using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

[UIPath("UI/Popup/PopupInsufficientCoin")]
public class PopupInsufficientCoin : CommonUIBase
{
    [SerializeField]
    private AdsButton AdBtn;

    private System.Action OnRewardResult;

    protected override void Awake()
    {
        base.Awake();
        AdBtn.AddListener(TpMaxProp.AdRewardType.InSufficientSilverCoint, OnClickAd);
    }


    public void Init(System.Action rewardaction)
    {
        OnRewardResult = rewardaction;
    }


    private void OnClickAd()
    {
        OnRewardResult?.Invoke();
        Hide();
    }
}

