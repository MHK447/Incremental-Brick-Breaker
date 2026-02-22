using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;
using TextOutline;
using UnityEngine.UI;
using Firebase.Firestore;
using Firebase.Extensions;
using TMPro;

[UIPath("UI/Popup/PopupSaveLoad")]
public class PopupSaveLoad : CommonUIBase
{

    [Header("Login")]
    [SerializeField] Button GoogleLoginBtn;
    [SerializeField] Button AppleLoginBtn;
    [SerializeField] GameObject obj_LoginText;
    [Header("Info")]
    [SerializeField] TextMeshProUGUI UserInfoText;
    [SerializeField] GameObject SaveRoot;
    [SerializeField] GameObject LoadRoot;
    [SerializeField] Button SaveBtn;
    [SerializeField] Button LoadBtn;
    [SerializeField] Button LogOutBtn;
    [SerializeField] Button DelBtn;
    [SerializeField] Button TestBtn;
    [SerializeField] GameObject LogoutRoot;
    //[SerializeField] GameObject PostRoot;
    //[SerializeField] Button PostBtn;


    private bool autoSave = false;

    public System.Action<bool> cb_LoginCallback = null;

    protected override void Awake()
    {
        base.Awake();
        GoogleLoginBtn.onClick.AddListener(OnClickGoogleLogin);
        AppleLoginBtn.onClick.AddListener(OnClickAppleLogin);
        SaveBtn.onClick.AddListener(OnClickSave);
        LoadBtn.onClick.AddListener(OnClickLoad);
        LogOutBtn.onClick.AddListener(OnClickLogout);
        DelBtn.onClick.AddListener(OnClickAccountDelete);
        TestBtn.onClick.AddListener(Test);

        #if UNITY_EDITOR
                ProjectUtility.SetActiveCheck(TestBtn.gameObject, true);
                TestBtn.onClick.AddListener(Test);
        #else
                ProjectUtility.SetActiveCheck(TestBtn.gameObject, false);
        #endif

        

        //PostBtn.onClick.AddListener(OnClickPost);
        //GameRoot.Instance.PluginSystem.BackEndProp.CheckInit();

        //ProjectUtility.SetActiveCheck(PostRoot, false);
    }

    public override void OnShowBefore()
    {
        base.OnShowBefore();
        if (TpPlatformLoginProp.fUser != null)
        {
            ShowLogin(false);
            ShowUserInfo(true);
            UserInfoText.text = $"User ID : {TpPlatformLoginProp.fUser.UserId}";

            //GameRoot.Instance.PluginSystem.BackEndProp.CustomLogin(TpPlatformLoginProp.fUser.UserId, CheckPost);


        }
        else
        {
            ShowLogin(true);
            ShowUserInfo(false);
        }

    }

    private void CheckPost()
    {
        //var backend = GameRoot.Instance.PluginSystem.BackEndProp;

        //if (backend.IsLogin() && backend.IsBackendTokenAlive())
        //{
        //    GameRoot.Instance.PluginSystem.BackEndProp.CheckPost(() =>
        //    {
        //        TpUtility.SetActiveCheck(PostRoot, BackendPost.Instance.postList.Count > 0);
        //    });
        //}
        //else
        //{
        //    TpUtility.SetActiveCheck(PostRoot, false);
        //}
    }

    public override void OnHideAfter()
    {
        base.OnHideAfter();
        autoSave = false;
        cb_LoginCallback = null;
    }

    public void SetAutoSave(bool value)
    {
        autoSave = value;
    }

    private void OnClickGoogleLogin()
    {
        GoogleLoginBtn.interactable = false;
        GameRoot.Instance.PluginSystem.LoginProp.Login(TpPlatformLoginProp.LoginPlatformType.Google, user =>
        {
            if (user != null)
            {
                ShowLogin(false);
                ShowUserInfo(true);

                UserInfoText.text = $"User ID : {user.UserId}";
                if (autoSave)
                {
                    GameRoot.Instance.PluginSystem.DataProp.UpLoadData((result) => { });
                    Hide();
                }

                //GameRoot.Instance.GameNotification.UpdateNotification(GameNotificationSystem.NotificationCategory.CheckLogin);


                cb_LoginCallback?.Invoke(true);
            }
            else
            {
                Debug.Log("OnClickGoogleLogin user null");
                GoogleLoginBtn.interactable = true;
            }
        });
    }

    private void OnClickAppleLogin()
    {
        AppleLoginBtn.interactable = false;
        GameRoot.Instance.PluginSystem.LoginProp.Login(TpPlatformLoginProp.LoginPlatformType.Apple, user =>
        {
            if (user != null)
            {
                ShowLogin(false);
                ShowUserInfo(true);

                UserInfoText.text = $"User ID : {user.UserId}";
                if (autoSave)
                {
                    GameRoot.Instance.PluginSystem.DataProp.UpLoadData((result) => { });
                    Hide();
                }

                //GameRoot.Instance.GameNotification.UpdateNotification(GameNotificationSystem.NotificationCategory.CheckLogin);

                cb_LoginCallback?.Invoke(true);
            }
            else
            {
                AppleLoginBtn.interactable = true;
            }
        });
    }

    private void ShowLogin(bool value)
    {
        GoogleLoginBtn.interactable = true;
        AppleLoginBtn.interactable = true;

        ProjectUtility.SetActiveCheck(GoogleLoginBtn.gameObject, value);
        ProjectUtility.SetActiveCheck(AppleLoginBtn.gameObject, value && Application.platform == RuntimePlatform.IPhonePlayer);

        ProjectUtility.SetActiveCheck(obj_LoginText, value);
    }

    private void ShowUserInfo(bool value)
    {
        ProjectUtility.SetActiveCheck(UserInfoText.gameObject, value);
        ProjectUtility.SetActiveCheck(SaveRoot, value);
        ProjectUtility.SetActiveCheck(LoadRoot, value);
        ProjectUtility.SetActiveCheck(LogoutRoot, value);
        ProjectUtility.SetActiveCheck(LogOutBtn.gameObject, value);
        ProjectUtility.SetActiveCheck(DelBtn.gameObject, value);
    }

    private void OnClickSave()
    {
        GameRoot.Instance.Loading.Show(false);
        SaveBtn.interactable = false;
        GameRoot.Instance.UserData.Save(true);
        GameRoot.Instance.PluginSystem.DataProp.GetSaveDate(saveTime =>
        {
            GameRoot.Instance.Loading.Hide(true);
            if (saveTime > 0)
            {
                GameRoot.Instance.UISystem.OpenUI<PopupMessageTwoButton>(popup => popup.Show(
                    Tables.Instance.GetTable<Localize>().GetString("str_popup_data_title"),
                    Tables.Instance.GetTable<Localize>().GetFormat("str_data_save_confirm", new System.DateTime(saveTime))
                    , () =>
                    {
                        GameRoot.Instance.UserData.CurMode.LastLoginTime = TimeSystem.GetCurTime();
                        GameRoot.Instance.UserData.Save(true);

                        GameRoot.Instance.WaitTimeAndCallback(.5f, () =>
                        {
                            GameRoot.Instance.PluginSystem.DataProp.UpLoadData((result) =>
                            {
                                if (result)
                                {
                                    GameRoot.Instance.UISystem.OpenUI<PopupMessage>(popup => popup.Show(
                                        Tables.Instance.GetTable<Localize>().GetString("str_popup_data_title"),
                                        Tables.Instance.GetTable<Localize>().GetString("str_data_save_complete")
                                    ));
                                }
                                else
                                {
                                    GameRoot.Instance.UISystem.OpenUI<PopupMessage>(popup => popup.Show(
                                        Tables.Instance.GetTable<Localize>().GetString("str_popup_data_title"),
                                        Tables.Instance.GetTable<Localize>().GetString("str_desc_cdn_error")
                                    ));
                                }
                                SaveBtn.interactable = true;
                            });
                        });
                    }, () =>
                    {
                        // 취소 버튼 클릭 시 버튼 다시 활성화
                        SaveBtn.interactable = true;
                    }, "str_yes", "str_cancel"
                ));
            }
            else
            {
                GameRoot.Instance.UserData.CurMode.LastLoginTime = TimeSystem.GetCurTime();
                GameRoot.Instance.UserData.Save(true);

                GameRoot.Instance.PluginSystem.DataProp.UpLoadData((result) =>
                {
                    if (result)
                    {
                        GameRoot.Instance.UISystem.OpenUI<PopupMessage>(popup => popup.Show(
                            Tables.Instance.GetTable<Localize>().GetString("str_popup_data_title"),
                            Tables.Instance.GetTable<Localize>().GetString("str_data_save_complete")
                        ));
                    }
                    else
                    {
                        GameRoot.Instance.UISystem.OpenUI<PopupMessage>(popup => popup.Show(
                           Tables.Instance.GetTable<Localize>().GetString("str_popup_data_title"),
                           Tables.Instance.GetTable<Localize>().GetString("str_desc_cdn_error")
                       ));
                    }
                    SaveBtn.interactable = true;
                });
            }
        });
    }

    private void OnClickLoad()
    {
        LoadBtn.interactable = false;
        GameRoot.Instance.Loading.Show(false);
        GameRoot.Instance.UserData.Save(true);
        GameRoot.Instance.PluginSystem.DataProp.GetSaveDate(saveTime =>
        {
            GameRoot.Instance.Loading.Hide(true);
            if (saveTime > 0)
            {
                GameRoot.Instance.UISystem.OpenUI<PopupMessageTwoButton>(popup => popup.Show(
                    Tables.Instance.GetTable<Localize>().GetString("str_popup_data_title"),
                    Tables.Instance.GetTable<Localize>().GetFormat("str_data_load_confirm", new System.DateTime(saveTime).ToString())
                    , () =>
                    {
                        GameRoot.Instance.WaitTimeAndCallback(.5f, () =>
                        {
                            GameRoot.Instance.PluginSystem.DataProp.DownLoadData(() =>
                            {
                                ReloadData();
                            });
                        });
                    }, () =>
                    {
                        // 취소 버튼 클릭 시 버튼 다시 활성화
                        LoadBtn.interactable = true;
                    }, "str_yes", "str_cancel"
                ));
            }
            else
            {
                GameRoot.Instance.UISystem.OpenUI<PopupMessage>(popup => popup.Show(
                    Tables.Instance.GetTable<Localize>().GetString("str_popup_data_title"),
                    Tables.Instance.GetTable<Localize>().GetString("str_data_no_data")
                ));
                LoadBtn.interactable = true;
            }
        });
    }

    private void OnClickLogout()
    {
        GameRoot.Instance.PluginSystem.LoginProp.Logout();

        ShowLogin(true);
        ShowUserInfo(false);
    }

    private void OnClickAccountDelete()
    {
        if (TpPlatformLoginProp.fUser == null)
            return;


        var title = Tables.Instance.GetTable<Localize>().GetString("str_account_delete_title");
        var desc = Tables.Instance.GetTable<Localize>().GetString("str_account_delete_desc");

        var localize = Tables.Instance.GetTable<Localize>();
        GameRoot.Instance.UISystem.OpenUI<PopupMessageTwoButton>(popup => popup.Show(
            title,
            desc,
            () =>
            {

                var firestore = FirebaseFirestore.DefaultInstance;
                firestore.Document($"{TpFirebaseDataProp.dbPath}{TpPlatformLoginProp.fUser.UserId}").DeleteAsync().ContinueWithOnMainThread(task =>
                {
                    if (!task.IsFaulted)
                    {
                        OnClickLogout();
                        GameRoot.Instance.UISystem.OpenUI<PopupMessage>(pop2 => pop2.Show(
                            localize.GetString("str_account_delete_end_title"),
                            localize.GetString("str_account_delete_end_desc")));
                    }
                });
            }, null,
            "str_account_delete_btn_yes",
            "str_account_delete_btn_no"
        ));
    }

    private string EscapeURL(string url)
    {
        return UnityEngine.Networking.UnityWebRequest.EscapeURL(url).Replace("+", "%20");
    }

    void Test()
    {
#if UNITY_EDITOR
        GameRoot.Instance.PluginSystem.DataProp.EditorDownLoadData(() =>
        {
            ReloadData();
        });
#endif

#if UNITY_EDITOR
        //GameRoot.Instance.PluginSystem.BackEndProp.CustomLogin("NEOiHKQrdYbCXn4wPonHOe0c8FS2", CheckPost);
#endif
    }

    void ReloadData()
    {
        GameRoot.Instance.UISystem.OpenUI<PopupMessage>(popup => popup.Show(
            Tables.Instance.GetTable<Localize>().GetString("str_popup_data_title"),
            Tables.Instance.GetTable<Localize>().GetString("str_data_load_complete"),
            () =>
            {
                ProjectUtility.ReLoadGame();
            }
        ));
    }

    void OnClickPost()
    {
        OpenMailBox();
    }

    void OpenMailBox()
    {
        //GameRoot.Instance.UISystem.OpenUI<PopupMailBox>(ui => ui.Init());
    }

}
