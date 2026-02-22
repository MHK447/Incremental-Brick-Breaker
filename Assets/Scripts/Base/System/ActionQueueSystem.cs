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
      
    }

    public void NewAddKing()
    {
       
    }

    public void OpenRewardPackage()
    {
       
    }

    public void WelComeBackPopupCheck()
    {

    }

    public void StarterPackageRewardOn()
    {
      
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
       
    }


}