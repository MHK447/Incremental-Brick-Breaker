using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanpoFri;
using TMPro;

[UIPath("UI/Popup/PopupMessageTwoButton", false)]
public class PopupMessageTwoButton : CommonUIBase
{
    [SerializeField]
    private TextMeshProUGUI textTitle;
    [SerializeField]
    private TextMeshProUGUI textMessage;
    [SerializeField]
    private TextMeshProUGUI textYesBtn;
    [SerializeField]
    private TextMeshProUGUI textNoBtn;
    [SerializeField]
    private Button YesBtn;
    [SerializeField]
    private Button NoBtn;

    private Action OnYes = null;
    private Action OnNo = null;

    private bool ClickHide = false;

    protected override void Awake()
    {
        base.Awake();
        YesBtn.onClick.AddListener(OnClickYes);
        NoBtn.onClick.AddListener(OnClickNo);
    }

    public void Show(
        string title,
        string message,
        Action onYes = null,
        Action onNo = null,
        string yes_str_key = "str_confirm",
        string no_str_key = "str_no",
        bool clickHide = true)
    {
        textTitle.text = title;
        textMessage.text = message;

        textYesBtn.text = Tables.Instance.GetTable<Localize>().GetString(yes_str_key);
        textNoBtn.text = Tables.Instance.GetTable<Localize>().GetString(no_str_key);

        OnYes = onYes;
        OnNo = onNo;

        ClickHide = clickHide;
    }

    private void OnClickYes()
    {
        OnYes?.Invoke();
        if (ClickHide)
            Hide();
    }

    private void OnClickNo()
    {
        OnNo?.Invoke();
        Hide();
    }
}

