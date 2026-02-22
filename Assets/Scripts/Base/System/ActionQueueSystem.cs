using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;
using BanpoFri;


public class ActionQueueSystem
{


    #region System
    private readonly LinkedList<Action> actions = new();

    public bool IsTutorialonCheck = false;

    public Action MainLoobyAction = null;

    public void Append(Action action)
    {
        actions.AddLast(action);
    }

    public void InsertNext(Action action)
    {
        actions.AddFirst(action);
    }

    public void NextAction()
    {
        if (actions.Count == 0)
        {
            BpLog.Log($"ActionQueue finished");
            return;
        }

        var action = actions.First.Value;
        actions.RemoveFirst();
        action?.Invoke();

        BpLog.Log($"ActionQueue left : {actions.Count}");
    }

    public void ExecuteAfterQueueFinished(Action action)
    {
        if (action == null)
            return;

        if (actions.Count == 0)
        {
            action.Invoke();
            return;
        }

        // Ensure this work runs only after all currently queued actions are done.
        Append(() =>
        {
            action.Invoke();
            NextAction();
        });
    }

    public void ClearActions()
    {
        actions.Clear();
    }
    #endregion

    #region Actions

    public struct GameFinishContext
    {
        public bool isStageClear;
        public bool shouldRestoreSkill;

        public int passKeyCount;
    }

    public void OnFirstInitCall()
    {
        ClearActions();

        //AttendancePopupCheck();

        // AttendancePopupCheck();
        // TowerRacePopupCheck();
        // StarterPackageRewardPopupCheck();
        // NoAdsPopupCheck();
        // GoldGrabPopupCheck();
        // FirstTreasureHunterPopupCheck();
        OpenRewardPackage();
        StarterPackageRewardOn();
        WelComeBackPopupCheck();

        NextAction();
    }

    public void OnGameFinishCall()
    {
        ClearActions();
        NewChapterCheck();
        MainRewardOpenTutorialCheck();
        ReviewPopupCheck();
        BoxOpenTutorialCheck();
        UnLocCardTutorialCheck();
        UnLocTrainingTutorialCheck();
        OpenRewardPackage();
        NewAddKing();
        NextAction();
    }


    public void SpearManTutorialCheck()
    {

        // var finddata = GameRoot.Instance.UserData.Unitgroupdata.FindUnit((int)Config.WeaponUnit.Spear);
        // if (GameRoot.Instance.UserData.Stageidx.Value >= 2 && finddata == null)
        // {
        //     Append(() =>
        //     {
        //         GameRoot.Instance.UserData.Unitgroupdata.AddUnit((int)Config.WeaponUnit.Spear);
        //         //GameRoot.Instance.UISystem.OpenUI<PageGetCharacterInfo>(page => page.Set((int)Config.WeaponUnit.Spear), NextAction);
        //         SoundPlayer.Instance.PlaySound("unit_unlock");
        //     });
        // }
    }

    public void UnLocTrainingTutorialCheck()
    {
        if (!GameRoot.Instance.TutorialSystem.IsClearTuto(TutorialSystem.Tuto_5) &&
         GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.TRAININGOPEN))
        {
            IsTutorialonCheck = true;
        }
    }


    public void UnLocCardTutorialCheck()
    {
        if (!GameRoot.Instance.TutorialSystem.IsClearTuto(TutorialSystem.Tuto_6) &&
         GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.CARDOPEN))
        {
            IsTutorialonCheck = true;
        }
    }



    public void UnLockStageUnitCheck()
    {
        // if (GameRoot.Instance.UserData.Stageidx.Value >= 3 &&
        //  GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.UNITUPGRADEOPEN))
        // {
        //     Append(() =>
        //     {
        //         var tdlist = Tables.Instance.GetTable<UnitInfo>().DataList.ToList();

        //         var selectstageidx = 0;

        //         var finddata = tdlist.FirstOrDefault(x => GameRoot.Instance.UserData.Unitgroupdata.FindUnit(x.idx) == null);

        //         if (finddata != null)
        //         {
        //             selectstageidx = finddata.idx;

        //             GameRoot.Instance.UISystem.OpenUI<PageGetCharacterReward>(page => page.Set(selectstageidx), NextAction);
        //         }
        //         else
        //         {
        //             NextAction();
        //         }
        //     });
        // }
    }


    #endregion

    // private void ShowTouchLock()
    // {
    //     Append(() =>
    //     {
    //         GameRoot.Instance.UISystem.OpenUI<PopupTouchLock>(x => NextAction(), NextAction);
    //     });
    // }

    // private void HideTouchLock()
    // {
    //     Append(() =>
    //     {
    //         var touchlock = GameRoot.Instance.UISystem.GetUI<PopupTouchLock>();
    //         if (touchlock == null) { NextAction(); return; }
    //         touchlock.Hide();
    //     });
    // }

    private void MainRewardOpenTutorialCheck()
    {
        //메인 리워드 버튼 체크 
        if (!GameRoot.Instance.TutorialSystem.IsClearTuto(TutorialSystem.Tuto_3) && GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.LobbyReward))
        {
            Append(() =>
             {
                 GameRoot.Instance.TutorialSystem.StartTutorial(TutorialSystem.Tuto_3);
                 GameRoot.Instance.TutorialSystem.OnActiveTutoEnd = NextAction;
             });
        }
    }
    private void BoxOpenTutorialCheck()
    {
        // //스테이지 보상 바운스볼 획득 튜토리얼
        // if (!GameRoot.Instance.TutorialSystem.IsClearTuto(TutorialSystem.Tuto_3)
        //     && GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.BOXOPEN))
        // {
        //     Append(() =>
        //      {
        //          GameRoot.Instance.TutorialSystem.StartTutorial(TutorialSystem.Tuto_3);
        //          GameRoot.Instance.TutorialSystem.OnActiveTutoEnd = NextAction;
        //      });
        // }
    }

    public void NewChapterCheck()
    {
        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;

        var recordcount = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.NewChapter, stageidx);

        if (stageidx % 7 == 0 && recordcount == 0)
        {
            var stagetd = Tables.Instance.GetTable<StageInfo>().GetData(stageidx);
            GameRoot.Instance.UserData.AddRecordCount(Config.RecordCountKeys.NewChapter, 1, stageidx);
            Append(() =>
             {
                 GameRoot.Instance.UISystem.OpenUI<PopupNewChapter>(x => x.Set(stagetd.ingame_map_idx), NextAction);
             });

        }
    }

    public void NewAddKing()
    {
        if (GameRoot.Instance.UserData.Herogroudata.Equipplayeridx == 0 && GameRoot.Instance.UserData.Stageidx.Value > 4)
        {

            if (IsTutorialonCheck)
            {
                MainLoobyAction = () =>
                {
                    GameRoot.Instance.UISystem.OpenUI<PageGetCharacterReward>(null, NextAction);
                };
            }
            else
            {
                Append(() =>
                {
                    GameRoot.Instance.UISystem.OpenUI<PageGetCharacterReward>(null, NextAction);
                });
            }
        }
    }

    public void OpenRewardPackage()
    {
        if (HasPendingLobbyUnlockTutorial())
            return;

        // 2개 조건: ShopOpen + 스타터 패키지 노출 횟수 3회 미만
        bool condition1 = GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.ShopOpen);
        bool condition2 = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.StarterPackage) < 3;
        if (!condition1 || !condition2)
            return;

        // 50% 확률
        // if (UnityEngine.Random.Range(0, 2) != 0)
        //     return;

        // 스타터 패키지 미구매 시 큐에 추가 (순서대로 실행)
        if (!GameRoot.Instance.UserData.Starterpackdata.Isbuy.Value)
            Append(() => GameRoot.Instance.UISystem.OpenUI<PopupPackageStarter>(popup => popup.Init(), NextAction));

        // 노광고 패키지 미구매이고 InterAd 오픈 상태일 때 큐에 추가
        if (!GameRoot.Instance.ShopSystem.NoInterstitialAds.Value &&
            GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.InterAdOpen))
            Append(() => GameRoot.Instance.UISystem.OpenUI<PopupNoAdsPackages>(null, NextAction));
    }

    public void WelComeBackPopupCheck()
    {
        if (GameRoot.Instance.UserData.HasInGameResumeData() && GameRoot.Instance.UserData.Stageidx.Value > 2)
        {
            Append(() =>
            {
                GameRoot.Instance.UISystem.OpenUI<PopupWelComeBack>(x => x.Init(), NextAction);
            });
        }
    }

    public void StarterPackageRewardOn()
    {
        if (HasPendingLobbyUnlockTutorial())
            return;

        if (!GameRoot.Instance.DailyResetSystem.StarterPackageRewardCheck())
            return;

        Append(() => GameRoot.Instance.UISystem.OpenUI<PopupPackageStarter>(popup => popup.Init(), NextAction));
    }

    private bool HasPendingLobbyUnlockTutorial()
    {
        bool isTrainingTutorialPending =
            !GameRoot.Instance.TutorialSystem.IsClearTuto(TutorialSystem.Tuto_5) &&
            GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.TRAININGOPEN);

        bool isCardTutorialPending =
            !GameRoot.Instance.TutorialSystem.IsClearTuto(TutorialSystem.Tuto_6) &&
            GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.CARDOPEN);

        return isTrainingTutorialPending || isCardTutorialPending;
    }


    private void AttendancePopupCheck()
    {
        // if (!GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.AttendanceReward)) return;
        // if (!GameRoot.Instance.AttendanceSystem.IsAttendanceRewardCheck()) return;

        // Append(() =>
        // {
        //     GameRoot.Instance.UISystem.OpenUI<PopupAttendance>(popup => popup.Init(), NextAction);
        // });
    }


    private void ReviewPopupCheck()
    {
        if (GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.ReviewPopup)
        && GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.ReviewPopup) == 0)
        {
            Append(() =>
             {
                 GameRoot.Instance.UserData.AddRecordCount(Config.RecordCountKeys.ReviewPopup, 1);
                 GameRoot.Instance.UISystem.OpenUI<PopupReview>(null, NextAction);
             });
        }
    }


}