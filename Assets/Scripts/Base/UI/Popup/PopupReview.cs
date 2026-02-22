using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanpoFri;
using System;
using System.Linq;

[UIPath("UI/Popup/Review")]
public class PopupReview : CommonUIBase
{
    [SerializeField] Button ReviewBtn;

    protected override void Awake()
    {
        base.Awake();
        ReviewBtn.onClick.AddListener(OnClickReview);
    }

    private void OnClickReview()
    {
        GameRoot.Instance.PluginSystem.StartReview();

        //GameRoot.Instance.PluginSystem.AnalyticsProp.TargetEvent(MafAnalyticsProp.Analytics.Firebase, IngameEventType.None,
        //	"m_review_accept");

        Hide();
    }

    public override void OnHideAfter(){
        base.OnHideAfter();
    }
}

