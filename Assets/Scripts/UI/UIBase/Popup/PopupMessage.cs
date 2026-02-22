using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanpoFri;
using TMPro;

[UIPath("UI/Popup/PopupMessage", false)]
public class PopupMessage : CommonUIBase
{
    [SerializeField]
    private TextMeshProUGUI textTitle;
    [SerializeField]
    private TextMeshProUGUI textMessage;
    [SerializeField]
    private TextMeshProUGUI textYesBtn;
    [SerializeField]
    private Button YesBtn;

    private Action OnYes = null;
    private bool ClickHide = false;

    protected override void Awake()
    {
        base.Awake();
        YesBtn.onClick.AddListener(OnClickYes);
    }

    public void Show(
        string title,
        string message,
        Action onYes = null,
        string yes_str_key = "str_confirm",
        bool clickHide = true)
    {
        textTitle.text = title;
        textMessage.text = message;

        textYesBtn.text = Tables.Instance.GetTable<Localize>().GetString(yes_str_key);

        OnYes = onYes;

        ClickHide = clickHide;

    }

    public void Show(
        string title,
        string message,
        Config.RewardType rewardType,
        int rewardIdx,
        double rewardValue,
        Action onYes = null,
        string yes_str_key = "str_confirm",
        bool clickHide = true)
    {
        Show(title, message, onYes, yes_str_key, clickHide);

    }

    private void OnClickYes()
    {
        OnYes?.Invoke();
        if (ClickHide)
            Hide();
    }
}
