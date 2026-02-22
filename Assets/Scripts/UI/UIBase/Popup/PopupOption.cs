using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using TextOutline;
using UnityEngine.UI;
using TMPro;

[UIPath("UI/Popup/PopupOption")]
public class PopupOption : CommonUIBase
{
    [Space(1.5f)]
    [Header("Option")]
    [SerializeField]
    private ToggleController VibToggle;
    [SerializeField]
    private ToggleController SoundToggle;
    [SerializeField]
    private ToggleController MusicToggle;

    [Header("Lang")]
    //[SerializeField] private optionbtn[] langBtns;
    //[SerializeField] private Sprite normalLangSprite;
    //[SerializeField] private Sprite selectLangSprite;
    //[SerializeField] private Dropdown langDropdown;

    [Header("Restore")]
    [SerializeField] Button restoreBtn;

    [Header("DataSave")]
    [SerializeField] Button mailBtn;

    [Header("DataSave")]
    [SerializeField] Button saveBtn;
    [SerializeField] GameObject bonusGem;
    [SerializeField] TextMeshProUGUI bonusGemText;

    [Header("Version Info")]
    [SerializeField] TextMeshProUGUI versionInfo;

    [SerializeField] private TMP_Dropdown langDropdown;

    protected override void Awake()
    {
        base.Awake();

        VibToggle.Init(GameRoot.Instance.UserData.Vib);
        SoundToggle.Init(GameRoot.Instance.UserData.Effect);
        MusicToggle.Init(GameRoot.Instance.UserData.Bgm);

        VibToggle.setToggleListener(OnClickVibration);
        SoundToggle.setToggleListener(OnClickEffect);
        MusicToggle.setToggleListener(OnClickBGM);

        if (restoreBtn != null)
            restoreBtn.onClick.AddListener(OnClickRestore);
        saveBtn.onClick.AddListener(OnClickDataSave);
        //mailBtn.onClick.AddListener(OnClcikMail);

        var list = new List<string>();


        list.Add(Tables.Instance.GetTable<Localize>().GetString($"str_popup_option_language_{Language.en.ToString()}"));
        list.Add(Tables.Instance.GetTable<Localize>().GetString($"str_popup_option_language_{Language.ko.ToString()}"));
        list.Add(Tables.Instance.GetTable<Localize>().GetString($"str_popup_option_language_{Language.ja.ToString()}"));
        list.Add(Tables.Instance.GetTable<Localize>().GetString($"str_popup_option_language_{Language.es.ToString()}"));
        list.Add(Tables.Instance.GetTable<Localize>().GetString($"str_popup_option_language_{Language.de.ToString()}"));
        list.Add(Tables.Instance.GetTable<Localize>().GetString($"str_popup_option_language_{Language.tw.ToString()}"));
        list.Add(Tables.Instance.GetTable<Localize>().GetString($"str_popup_option_language_{Language.ru.ToString()}"));
        list.Add(Tables.Instance.GetTable<Localize>().GetString($"str_popup_option_language_{Language.fr.ToString()}"));
        list.Add(Tables.Instance.GetTable<Localize>().GetString($"str_popup_option_language_{Language.vi.ToString()}"));

        langDropdown.AddOptions(list);

        langDropdown.onValueChanged.AddListener(OnClickSelectLang);

        //versionInfo.text = $"version : {Application.version}";
    }

    public override void OnShowBefore()
    {
        base.OnShowBefore();

        SetLang();
        //SetBonusGem();
    }


    public void Set(bool isingame)
    {
        ProjectUtility.SetActiveCheck(saveBtn.gameObject, !isingame);

        if (isingame)
        {
            GameRoot.Instance.GameSpeedSystem.StopGameSpeed(true);
        }
    }


    private void SetLang()
    {
        int idx = 0;
        switch (GameRoot.Instance.UserData.Language)
        {
            case Language.en: idx = 0; break;
            case Language.ko: idx = 1; break;
            case Language.ja: idx = 2; break;
            case Language.es: idx = 3; break;
            case Language.de: idx = 4; break;
            case Language.tw: idx = 5; break;
            case Language.ru: idx = 6; break;
            case Language.fr: idx = 7; break;
            case Language.vi: idx = 8; break;
            default: idx = 0; break;
        }

        langDropdown.value = idx;
    }

    private void OnClickSelectLang(int index)
    {
        Language curLang = Language.en;
        switch (index)
        {
            case 0: curLang = Language.en; break;
            case 1: curLang = Language.ko; break;
            case 2: curLang = Language.ja; break;
            case 3: curLang = Language.es; break;
            case 4: curLang = Language.de; break;
            case 5: curLang = Language.tw; break;
            case 6: curLang = Language.ru; break;
            case 7: curLang = Language.fr; break;
            case 8: curLang = Language.vi; break;
        }

        GameRoot.Instance.UserData.Language = curLang;

        Config.Instance.UpdateFallbackOrder(curLang);

        GameRoot.Instance.WaitEndFrameCallback(() =>
        {
            foreach (var ls in LocalizeString.Localizelist)
            {
                if (ls != null)
                {
                    ls.RefreshText();
                }
            }
            foreach (var ls in LocalizeString.Localizelist)
            {
                if (ls != null)
                {
                    ls.RefreshText();
                }
            }

            var list = GameRoot.Instance.UISystem.RefreshComponentList;
            foreach (var ls in list)
                ls.RefreshText();

        });
    }




    string EscapeURL(string url)
    {
        return UnityEngine.Networking.UnityWebRequest.EscapeURL(url).Replace("+", "%20");
    }

    //public void SetBonusGem()
    //{
    //    TpUtility.SetActiveCheck(bonusGem, GameRoot.Instance.UserData.GetRecordValue(Config.RecordKeys.HasLogined) <= 0);
    //    // bonusGemText.text = $"+{Tables.Instance.GetTable<Define>().GetData("login_bonus_gem").value}";
    //}

    private void OnClickVibration(bool isOn)
    {
        GameRoot.Instance.UserData.Vib = isOn;
        GameRoot.Instance.UserData.Save();
    }

    private void OnClickEffect(bool isOn)
    {
        GameRoot.Instance.UserData.Effect = isOn;
        SoundPlayer.Instance.EffectSwitch(isOn);
        GameRoot.Instance.UserData.Save();
    }

    private void OnClickBGM(bool isOn)
    {
        GameRoot.Instance.UserData.Bgm = isOn;
        SoundPlayer.Instance.BgmSwitch(isOn);
        GameRoot.Instance.UserData.Save();
    }

    public override void Hide()
    {
        base.Hide();

        GameRoot.Instance.GameSpeedSystem.StopGameSpeed(false);
    }

    void OnClickRestore()
    {
        restoreBtn.interactable = false;
        GameRoot.Instance.Loading.Show(false);
        GameRoot.Instance.InAppPurchaseManager.RestorePurchase((result) =>
        {
            var stringKey = result == InAppPurchaseManager.Result.Success ? "str_restore_success" : "str_restore_fail";
            GameRoot.Instance.UISystem.OpenUI<PopupToastmessage>(popup =>
            {
                popup.Show(Tables.Instance.GetTable<Localize>().GetString("str_restore"), Tables.Instance.GetTable<Localize>().GetString(stringKey));
            }, () =>
            {
                // if (GameRoot.Instance.UserData.VipData.IsReceiveDaily)
                // {
                //     GameRoot.Instance.UISystem.OpenUI<PopupVIPDailyReward>();
                // }
            });

            GameRoot.Instance.Loading.Hide(true);
        });
    }


    //#region Lang
    //    private void SetLang()
    //    {
    //        langDropdown.value = (int)GameRoot.Instance.UserData.Language;
    //    }

    //    private void OnClickSelectLang(int index)
    //    {
    //        Config.Language curLang = Config.Language.en;
    //        switch (index)
    //        {
    //            case 0: curLang = Config.Language.en; break;
    //            case 1: curLang = Config.Language.ko; break;
    //            case 2: curLang = Config.Language.ja; break;
    //            case 3: curLang = Config.Language.ru; break;
    //            case 4: curLang = Config.Language.de; break;
    //            default: curLang = Config.Language.en; break;
    //        }

    //        GameRoot.Instance.UserData.Language = curLang;

    //        Config.Instance.UpdateFallbackOrder(curLang);

    //        //SetLang();

    //        foreach (var ls in LocalizeString.Localizelist)
    //        {
    //            if (ls != null)
    //            {
    //                ls.RefreshText();
    //            }
    //        }

    //        var list = GameRoot.Instance.UISystem.RefreshComponentList;
    //        foreach (var ls in list)
    //            ls.RefreshText();
    //    }
    //#endregion

    private void OnClickDataSave()
    {
        GameRoot.Instance.UISystem.OpenUI<PopupSaveLoad>();
    }

    //private void OnClcikMail()
    //{
    //    if (TpPlatformLoginProp.fUser != null)
    //    {
    //        SendMail();
    //    }
    //    else
    //    {
    //        GameRoot.Instance.UISystem.OpenUI<PopupMessage>(popup => popup.Show(
    //            Tables.Instance.GetTable<Localize>().GetString("str_popup_data_title"),
    //            Tables.Instance.GetTable<Localize>().GetString("str_desc_faq_login"),
    //            () =>
    //            {
    //                GameRoot.Instance.UISystem.OpenUI<PopupDataSave>(popup =>
    //                {
    //                    popup.cb_LoginCallback = (isLogin) =>
    //                    {
    //                        if (isLogin)
    //                        {
    //                            GameRoot.Instance.PluginSystem.DataProp.GetSaveDate((date) =>
    //                            {
    //                                if (date <= 0)
    //                                {
    //                                    GameRoot.Instance.PluginSystem.DataProp.UpLoadData();
    //                                }
    //                                popup.Hide();
    //                                SendMail();
    //                            });
    //                        }
    //                    };
    //                }
    //                );
    //            })
    //        );
    //    }

    //}

    //private void SendMail()
    //{
    //    string mailto = "support@treeplla.com";
    //    string subject = EscapeURL("Question");
    //    string body = EscapeURL("Please Insert Message\n\n\n\n" + "________" + "User ID : " + TpPlatformLoginProp.fUser.UserId + "\n\n" + "Device Model : " + SystemInfo.deviceModel + "\n\n" + "Game Name : Lumber Cat\n\n" + "Game Version : " + Application.version + "\n\n" + "Device OS : " + SystemInfo.operatingSystem + "\n\n" + "________");
    //    TreepllaNative.OpenURL("mailto:" + mailto + "?subject=" + subject + "&body=" + body);
    //}
}
