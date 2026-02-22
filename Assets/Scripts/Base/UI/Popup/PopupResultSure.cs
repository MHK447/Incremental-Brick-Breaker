using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;


[UIPath("UI/Popup/PopupResultSure")]
public class PopupResultSure : CommonUIBase
{
    [SerializeField]
    private Button StartBtn;

    private System.Action Action = null;

    protected override void Awake()
    {
        base.Awake();
        StartBtn.onClick.AddListener(OnClickStart);
    }

    public void Init(System.Action action)
    {
        Action = action;
    }


    public void OnClickStart()
    {
        Action?.Invoke();
        Hide();
    }

}

