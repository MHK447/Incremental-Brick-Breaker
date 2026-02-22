using System.Collections;
using System.Collections.Generic;
using TMPro;
using BanpoFri;
using UnityEngine;
using UnityEngine.UI;

public enum HudBottomBtnType
{
    TRAINING = 0,
    CARD = 1,
    BATTLE = 2,
    HERO = 3,
    SHOP = 4,
    Done,
}



[UIPath("UI/Page/HUDTotal", true)]
public class HUDTotal : UIBase
{
    [SerializeField]
    private Button ShopBtn;

    [SerializeField]
    private GameObject ShopReddotObj;

    [SerializeField]
    private Image ReddotImg;

    [SerializeField]
    private TextMeshProUGUI FreeTextObj;

    [SerializeField]
    private List<HudBottomBtnComponent> HudBottomBtnList = new List<HudBottomBtnComponent>();

    public Queue<System.Action> ActionQueue = new Queue<System.Action>();
    private Coroutine waitQueue = null;

    private List<UIBase> UIList = new List<UIBase>();

    public HudBottomBtnType CurrentlyOpenPage;

    protected override void Awake()
    {
        base.Awake();

        if (UIList.Count == 0)
        {
            for (int i = 0; i < (int)HudBottomBtnType.Done; ++i)
            {
                switch (i)
                {
                    case (int)HudBottomBtnType.TRAINING:
                        {
                            GameRoot.Instance.UISystem.PreLoadUI(typeof(PageLobbyTraining), ui => { UIList.Add(ui); });

                        }
                        break;
                    case (int)HudBottomBtnType.CARD:
                        {
                            GameRoot.Instance.UISystem.PreLoadUI(typeof(PageLobbyCard), ui => { UIList.Add(ui); });
                        }
                        break;
                    case (int)HudBottomBtnType.BATTLE:
                        {
                            GameRoot.Instance.UISystem.PreLoadUI(typeof(PageLobbyBattle), ui =>
                            {
                                UIList.Add(ui);
                                OpenPage(HudBottomBtnType.BATTLE);
                                UpdateButtonLock();
                            });
                        }
                        break;
                    case (int)HudBottomBtnType.HERO:
                        {
                            GameRoot.Instance.UISystem.PreLoadUI(typeof(PageLobbyHero), ui =>
                            {
                                UIList.Add(ui);
                                UpdateButtonLock();
                            });
                        }
                        break;
                    case (int)HudBottomBtnType.SHOP:
                        {
                            GameRoot.Instance.UISystem.PreLoadUI(typeof(PageLobbyShop), ui =>
                            {
                                UIList.Add(ui);
                            });
                        }
                        break;

                }
            }
        }


        GameRoot.Instance.ActionQueueSystem.ExecuteAfterQueueFinished(RegisterContentsOpen);
    }

    public override void OnShowAfter()
    {
        base.OnShowAfter();

        // var getui = GameRoot.Instance.UISystem.GetUI<PageLobbyTraining>();

        // if (getui != null)
        // {
        //     // getui.SetScroll();
        // }


    }


    public override void OnShowBefore()
    {
        base.OnShowBefore();

        foreach (var item in HudBottomBtnList)
        {
            item.Set(OnClickHudBottomBtn);
        }

        GameRoot.Instance.ActionQueueSystem.NextAction();
    }


    public void RegisterContentsOpen()
    {
        GameRoot.Instance.ContentsOpenSystem.RegisterOpenWaitContentByStage(ContentsOpenSystem.ContentsOpenType.TRAININGOPEN, CheckContentsOpen_Training);
        //GameRoot.Instance.ContentsOpenSystem.RegisterOpenWaitContentByStage(ContentsOpenSystem.ContentsOpenType.CARDOPEN, CheckContentsOpen_Card);
    }

    public void EnqueueTutorialContentsOpen()
    {
        CheckContentsOpen_Training(true);
        //CheckContentsOpen_Card(true);
    }

    public void UpdateButtonLock()
    {
        HudBottomBtnList[(int)HudBottomBtnType.TRAINING].SetLocked(
            !GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.TRAININGOPEN) ||
            !GameRoot.Instance.TutorialSystem.IsClearTuto(TutorialSystem.Tuto_5));

        HudBottomBtnList[(int)HudBottomBtnType.CARD].SetLocked(
            !GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.CARDOPEN) ||
            !GameRoot.Instance.TutorialSystem.IsClearTuto(TutorialSystem.Tuto_6));

        HudBottomBtnList[(int)HudBottomBtnType.SHOP].SetLocked(
            !GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.ShopOpen));


        HudBottomBtnList[(int)HudBottomBtnType.HERO].SetLocked(
            GameRoot.Instance.UserData.Herogroudata.Equipplayeridx == 0);
    }


    public RectTransform GetWorldPosByContentsOpen(ContentsOpenSystem.ContentsOpenType opentype)
    {
        RectTransform targettr = null;


        switch (opentype)
        {
            case ContentsOpenSystem.ContentsOpenType.TRAININGOPEN:
                targettr = HudBottomBtnList[(int)HudBottomBtnType.BATTLE].GetLockObj.transform as RectTransform;
                break;
            case ContentsOpenSystem.ContentsOpenType.CARDOPEN:
                targettr = HudBottomBtnList[(int)HudBottomBtnType.CARD].GetLockObj.transform as RectTransform;
                break;
        }

        return targettr;
    }

    public HudBottomBtnComponent GetHudBottomBtn(HudBottomBtnType type)
    {
        return HudBottomBtnList[(int)type];
    }


    // private void CheckContentsOpen_Shop(bool action)
    // {
    //     if (!action) return;

    //     if (GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.ShopOpen) &&
    //     GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.FirstShopOpen) == 0)
    //     {
    //         GameRoot.Instance.ActionQueueSystem.Append(() =>
    //         {
    //             GameObject target = HudBottomBtnList[(int)HudBottomBtnType.Shop].GetLockObj;
    //             GameRoot.Instance.UISystem.OpenUI<PopupContentsOpen>(popup =>
    //             {
    //                 popup.Set(ContentsOpenSystem.ContentsOpenType.ShopOpen, target.transform as RectTransform, () =>
    //                 {
    //                     HudBottomBtnList[(int)HudBottomBtnType.Shop].SetLocked(false);

    //                     GameRoot.Instance.UserData.AddRecordCount(Config.RecordCountKeys.FirstShopOpen, 1);

    //                     NextAction();
    //                 });
    //             });
    //         });
    //     }
    // }


    public void CheckContentsOpen_Hero(bool action)
    {
        if (!action) return;

        if (GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.HeroUpgradeOpen))
        {
            GameObject target = HudBottomBtnList[(int)HudBottomBtnType.HERO].GetLockObj;
            GameRoot.Instance.UISystem.OpenUI<PopupContentsOpen>(popup =>
            {
                popup.Set(ContentsOpenSystem.ContentsOpenType.HeroUpgradeOpen, target.transform as RectTransform, () =>
                {
                    HudBottomBtnList[(int)HudBottomBtnType.HERO].SetLocked(false);
                    GameRoot.Instance.TutorialSystem.StartTutorial(TutorialSystem.Tuto_7);

                    GameRoot.Instance.TutorialSystem.OnActiveTutoEnd = () =>
                    {
                        NextAction();
                    };
                });
            });

        }
    }


    private void CheckContentsOpen_Training(bool action)
    {
        if (!action) return;

        if (!GameRoot.Instance.TutorialSystem.IsClearTuto(TutorialSystem.Tuto_5)
            && GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.TRAININGOPEN))
        {
            GameRoot.Instance.ActionQueueSystem.Append(() =>
            {
                GameObject target = HudBottomBtnList[(int)HudBottomBtnType.TRAINING].GetLockObj;
                GameRoot.Instance.UISystem.OpenUI<PopupContentsOpen>(popup =>
                {
                    popup.Set(ContentsOpenSystem.ContentsOpenType.TRAININGOPEN, target.transform as RectTransform, () =>
                    {
                        HudBottomBtnList[(int)HudBottomBtnType.TRAINING].SetLocked(false);
                        GameRoot.Instance.TutorialSystem.StartTutorial(TutorialSystem.Tuto_5);

                        GameRoot.Instance.TutorialSystem.OnActiveTutoEnd = () =>
                        {
                            GameRoot.Instance.ActionQueueSystem.IsTutorialonCheck = false;
                            NextAction();
                        };
                    });
                });
            });
        }
    }

    public void CheckContentsOpen_Card(bool action)
    {
        if (!action) return;

        GameObject target = HudBottomBtnList[(int)HudBottomBtnType.CARD].GetLockObj;
        //card
        if (!GameRoot.Instance.TutorialSystem.IsClearTuto(TutorialSystem.Tuto_6)
            && GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.CARDOPEN))
        {
            HudBottomBtnList[(int)HudBottomBtnType.CARD].SetLocked(true);
            GameRoot.Instance.UISystem.OpenUI<PopupContentsOpen>(popup =>
            {
                popup.Set(ContentsOpenSystem.ContentsOpenType.CARDOPEN, target.transform as RectTransform, () =>
                {
                    HudBottomBtnList[(int)HudBottomBtnType.CARD].SetLocked(false);
                    GameRoot.Instance.TutorialSystem.StartTutorial(TutorialSystem.Tuto_6);
                    GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency, (int)Config.CurrencyID.Material, 20, false);

                    GameRoot.Instance.TutorialSystem.OnActiveTutoEnd = () =>
                    {
                        GameRoot.Instance.ActionQueueSystem.IsTutorialonCheck = false;
                        NextAction();
                    };
                });
            });
        }
    }

    // private void CheckContentsOpen_Pass(bool action)
    // {
    //     if (!action) return;
    //     GameRoot.Instance.ActionQueueSystem.Append(() =>
    //     {

    //         GameRoot.Instance.PassSystem.FirstStartCheck();
    //         GameObject target = null;
    //         OpenPage(HudBottomBtnType.BATTLE);
    //         var getui = GameRoot.Instance.UISystem.GetUI<PageLobbyBattle>();
    //         if (getui != null)
    //         {
    //             var getpassicon = getui.GetIconComponent(PageIconType.Pass) as PassIconComponent;
    //             target = getpassicon.gameObject;
    //             getpassicon.IsWaitShow = true;
    //             TpUtility.SetActiveCheck(target, false);
    //         }
    //         //pass
    //         GameRoot.Instance.UISystem.OpenUI<PopupContentsOpen>(popup =>
    //         {
    //             popup.Set(ContentsOpenSystem.ContentsOpenType.Pass, target.transform as RectTransform, () =>
    //             {
    //                 var getui = GameRoot.Instance.UISystem.GetUI<PageLobbyBattle>();
    //                 var getpassicon = getui.GetIconComponent(PageIconType.Pass) as PassIconComponent;
    //                 getpassicon.IsWaitShow = false;
    //                 TpUtility.SetActiveCheck(getpassicon.gameObject, true);

    //                 GameRoot.Instance.UISystem.OpenUI<PopupPackagePassStart>(null, NextAction);
    //             });
    //         });
    //     });
    // }


    // private void CheckContentsOpen_Attendance(bool action)
    // {
    //     if (!action) return;
    //     GameRoot.Instance.ActionQueueSystem.Append(() =>
    //     {

    //         GameRoot.Instance.AttendanceSystem.FirstStartCheck();
    //         GameObject target = null;
    //         OpenPage(HudBottomBtnType.BATTLE);
    //         var getui = GameRoot.Instance.UISystem.GetUI<PageLobbyBattle>();
    //         if (getui != null)
    //         {
    //             var getpassicon = getui.GetIconComponent(PageIconType.Attendance) as AttendanceIconComponent;
    //             target = getpassicon.gameObject;
    //             getpassicon.IsWaitShow = true;
    //             TpUtility.SetActiveCheck(target, false);
    //         }

    //         GameRoot.Instance.UISystem.OpenUI<PopupContentsOpen>(popup =>
    //         {
    //             popup.Set(ContentsOpenSystem.ContentsOpenType.AttendanceReward, target.transform as RectTransform, () =>
    //             {
    //                 var getui = GameRoot.Instance.UISystem.GetUI<PageLobbyBattle>();
    //                 var getpassicon = getui.GetIconComponent(PageIconType.Attendance) as AttendanceIconComponent;
    //                 getpassicon.IsWaitShow = false;
    //                 TpUtility.SetActiveCheck(getpassicon.gameObject, true);
    //             });
    //         });
    //     });
    // }

    // private void CheckContentsOpen_Block(bool action)
    // {
    //     if (!action) return;

    //     GameRoot.Instance.ActionQueueSystem.Append(() =>
    //     {
    //         HudBottomBtnList[(int)HudBottomBtnType.Blocks].SetLocked(true);
    //         GameObject target = HudBottomBtnList[(int)HudBottomBtnType.Blocks].GetLockObj;
    //         GameRoot.Instance.UISystem.OpenUI<PopupContentsOpen>(popup =>
    //         {
    //             popup.Set(ContentsOpenSystem.ContentsOpenType.BlockOpen, target.transform as RectTransform, null);
    //         }, () =>
    //         {
    //             HudBottomBtnList[(int)HudBottomBtnType.Blocks].SetLocked(false);
    //             GameRoot.Instance.WaitRealTimeAndCallback(0.05f, NextAction);
    //         });
    //     });
    // }



    public void NextAction()
    {
        GameRoot.Instance.ActionQueueSystem.NextAction();
    }



    public void OnClickHudBottomBtn(HudBottomBtnType type, bool isopen)
    {
        OpenPage(type);
    }

    public void OpenPage(HudBottomBtnType type, bool forceOpen = false)
    {
        if (type == CurrentlyOpenPage && !forceOpen) return;

        // 파괴된 참조를 정리
        UIList.RemoveAll(x => x == null);

        // PreLoadUI로 로드된 UI들을 직접 비활성화
        foreach (var ui in UIList)
        {
            if (ui != null && ui.gameObject.activeSelf)
            {
                ui.gameObject.SetActive(false);
            }
        }

        for (int i = 0; i < HudBottomBtnList.Count; ++i)
        {
            HudBottomBtnList[i].SetActive(HudBottomBtnList[i].CurBtnType != type);
        }

        // PreLoadUI로 로드된 UI를 활성화 (OnEnable에서 Show 애니메이션 자동 재생)
        switch (type)
        {
            case HudBottomBtnType.CARD:
                {
                    GameRoot.Instance.UISystem.OpenUI<PageLobbyCard>(ui =>
                    {
                        if (!UIList.Contains(ui)) UIList.Add(ui);
                        ui.Init();
                    });
                }
                break;
            case HudBottomBtnType.BATTLE:
                {
                    GameRoot.Instance.UISystem.OpenUI<PageLobbyBattle>(ui =>
                    {
                        if (!UIList.Contains(ui)) UIList.Add(ui);
                        ui.Init();
                        UpdateButtonLock();
                    });
                }
                break;
            case HudBottomBtnType.TRAINING:
                {
                    GameRoot.Instance.UISystem.OpenUI<PageLobbyTraining>(ui =>
                    {
                        if (!UIList.Contains(ui)) UIList.Add(ui);
                        ui.Init();
                    });
                }
                break;
            case HudBottomBtnType.HERO:
                {
                    GameRoot.Instance.UISystem.OpenUI<PageLobbyHero>(ui =>
                    {
                        if (!UIList.Contains(ui)) UIList.Add(ui);
                        ui.Init();
                    });
                }
                break;
            case HudBottomBtnType.SHOP:
                {
                    GameRoot.Instance.UISystem.OpenUI<PageLobbyShop>(ui =>
                    {
                        if (!UIList.Contains(ui)) UIList.Add(ui);
                        ui.Init();
                    });
                }
                break;
        }

        CurrentlyOpenPage = type;
    }

}
