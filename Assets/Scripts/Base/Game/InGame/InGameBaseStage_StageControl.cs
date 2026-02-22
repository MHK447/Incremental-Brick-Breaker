using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Collections;
using System.IO.Compression;
using System.Linq;

public partial class InGameBaseStage : MonoBehaviour
{
    public GameObject ChapterMapRoot;

    public PlayerBlockGroup PlayerBlockGroup;

    public EnemyUnitGroup EnemyUnitGroup;

    public PlayerUnitGroup PlayerUnitGroup;

    private Coroutine currentWaveCoroutine = null;

    

    [HideInInspector]
    public int StageStartTime = 0;

    // 웨이브가 완전히 끝났는지 확인 (스폰이 모두 완료되었는지)
    public bool IsWaveSpawnComplete { get { return currentWaveCoroutine == null; } }



    public void InitStage()
    {
        PlayerBlockGroup.Init();
        EnemyUnitGroup.Init();
        PlayerUnitGroup.Init();

        EquipTutorialCheck();

        EnemyUnitGroup.CheckEnemyBlockSpawner();
    }

    public void StartBattle()
    {
        this.transform.position = GameRoot.Instance.ShopSystem.NoInterstitialAds.Value ? new Vector3( this.transform.position.x, 0f, this.transform.position.z) 
        : new Vector3(this.transform.position.x, 1.5f, this.transform.position.z);

        GameRoot.Instance.UserData.AddRecordCount(Config.RecordCountKeys.TryStageClear, 1, GameRoot.Instance.UserData.Stageidx.Value);

        ProjectUtility.SetActiveCheck(ChapterMapRoot, true);
        GameRoot.Instance.UserData.Waveidx.Value = 1;
        GameRoot.Instance.UserData.Ingamesilvercoin.Value = 0;
        // 세트 버프: StartSilvercoinIncrease - 전투 시작 시 추가 은화
        var startSilverCoin = GameRoot.Instance.HeroSystem.GetSetBuffValue(HeroItemSetType.StartSilvercoinIncrease);
        if (startSilverCoin > 0)
            GameRoot.Instance.UserData.Ingamesilvercoin.Value += startSilverCoin;
        GameRoot.Instance.UserData.Playerdata.InGameExpProperty.Value = 0;
        GameRoot.Instance.UserData.Playerdata.KillCountProperty.Value = 0;
        SoundPlayer.Instance.SetBGMVolume(0f);
        GameRoot.Instance.UISystem.GetUI<HUDTotal>()?.Hide();
        GameRoot.Instance.UserData.Playerdata.IsGameStartProperty.Value = true;
        GameRoot.Instance.UserData.Playerdata.IsWaveRestProperty.Value = true;
        StageStartTime = 0;
        resumeSnapshotDeltaTime = 0f;

        var trainingvalue = GameRoot.Instance.TrainingSystem.GetBuffValue(TrainingSystem.TrainingType.CastleHpIncrease);

        var unithp = GameRoot.Instance.HeroSystem.GetHeroStatusValue(HeroStatus.Hp);

        var hpvalue = Tables.Instance.GetTable<Define>().GetData("base_hp").value + trainingvalue + unithp;

        SetHp((int)hpvalue);

        GameRoot.Instance.UISystem.OpenUI<PopupInGame>(x =>
        {
            x.Init();
        });

        GameRoot.Instance.UserData.Waveidx.Value = 1;

        InitStage();
        GameRoot.Instance.UserData.SaveInGameResumeSnapshot(true, true);

    }

    private float stagedeltatime = 0;
    private float resumeSnapshotDeltaTime = 0f;

    void Update()
    {
        if(GameRoot.Instance.UserData.Playerdata.IsGameStartProperty.Value)
        {
            stagedeltatime += Time.deltaTime;
            resumeSnapshotDeltaTime += Time.deltaTime;

            if(stagedeltatime >= 1)
            {
                stagedeltatime = 0;
                StageStartTime += 1;
            }

            if (!GameRoot.Instance.UserData.Playerdata.IsWaveRestProperty.Value && resumeSnapshotDeltaTime >= 10f)
            {
                resumeSnapshotDeltaTime = 0f;
                GameRoot.Instance.UserData.SaveInGameResumeSnapshot(false, true);
            }

        }
    }


    public void SetHp(int hp)
    {
        GameRoot.Instance.UserData.Playerdata.StartHpProperty.Value = hp;
        GameRoot.Instance.UserData.Playerdata.CurHpProperty.Value = hp;
    }

    public void TutorialCheck()
    {

    }

    public void StageClear()
    {
        GameRoot.Instance.UserData.Playerdata.StageClear();
        PlayerUnitGroup.ClearData();
        EnemyUnitGroup.ClearData();
        PlayerBlockGroup.ClearData();
        GameRoot.Instance.UserData.Playerdata.IsGameStartProperty.Value = false;
        GameRoot.Instance.UserData.Ingamesilvercoin.Value = 0;
        GameRoot.Instance.AlimentSystem.Clear();
        GameRoot.Instance.InGameUpgradeSystem.ClearData();

        GameRoot.Instance.GameSpeedSystem.ResetGameSpeed();
        GameRoot.Instance.InGameUpgradeSystem.Reset();

        // PopupInGame의 TileWeaponGroup 초기화
        var popupInGame = GameRoot.Instance.UISystem.GetUI<PopupInGame>();
        if (popupInGame != null && popupInGame.TileWeaponGroup != null)
        {
            popupInGame.TileWeaponGroup.ClearData();
        }
    }

    public bool GameFinishSequenceStarted = false;
    public IEnumerator GameOverSequence()
    {
        if (GameFinishSequenceStarted) yield break;
        GameFinishSequenceStarted = true;


        //slow mo
        GameRoot.Instance.GameSpeedSystem.CurGameSpeedValue.Value = 0.3f;
        yield return new WaitForSecondsRealtime(1f);
        GameRoot.Instance.GameSpeedSystem.CurGameSpeedValue.Value = 1;

        yield return new WaitForSeconds(1f);

        //not really dead
        if (this != null && PlayerBlockGroup != null)
        {
            GameFinishSequenceStarted = false;
            yield break;
        }

        // //show ui
        // if (GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.CARDOPEN))
        // {
        //     GameRoot.Instance.UISystem.OpenUI<PopupRevival>(popup => popup.Init());
        // }
        // else
        // {
        //     GameRoot.Instance.WaitRealTimeAndCallback(1f, () =>
        //     {
        //         GameRoot.Instance.UISystem.OpenUI<PopupStageResult>(popup => popup.Init(false));
        //     });
        // }
        GameFinishSequenceStarted = false;
    }




    public void ReturnMainScreen(System.Action fadeaction = null)
    {
        GameRoot.Instance.GameSpeedSystem.StopGameSpeed(false, false);
        GameRoot.Instance.PluginSystem.HideBanner();

        StageClear();

        fadeaction += StartMainUI;

        GameRoot.Instance.UISystem.OpenUI<PageFade>(page =>
        {
            page.Set(fadeaction);
        }, null, false);
    }

    public void StartMainUI()
    {
        ProjectUtility.SetActiveCheck(ChapterMapRoot, false);

        SoundPlayer.Instance.SetBGMVolume(0.125f);
        SoundPlayer.Instance.RestartBGM();

        GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.Hide();
        GameRoot.Instance.UISystem.OpenUI<PageLobbyBattle>(popup => popup.Init());

        var hud = GameRoot.Instance.UISystem.GetUI<HUDTotal>();

        GameRoot.Instance.UISystem.OpenUI<HUDTotal>();

        hud.RegisterContentsOpen();
        hud.EnqueueTutorialContentsOpen();
        hud.OpenPage(HudBottomBtnType.BATTLE, forceOpen: true);
        GameRoot.Instance.GameNotification.UpdateNotification(GameNotificationSystem.NotificationCategory.HeroUpgradeCheck);


        GameRoot.Instance.ActionQueueSystem.OnGameFinishCall();
    }

    public void EquipTutorialCheck()
    {
        //리롤 튜토리얼 체크
        if (GameRoot.Instance.UserData.Waveidx.Value == 1 && GameRoot.Instance.UserData.Stageidx.Value == 1)
        {
            if (GameRoot.Instance.UserData.Tutorial.Contains(TutorialSystem.Tuto_2))
            {
                GameRoot.Instance.UserData.Tutorial.Remove(TutorialSystem.Tuto_2);
            }

            GameRoot.Instance.TutorialSystem.StartTutorial(TutorialSystem.Tuto_2);
        }
        else if (GameRoot.Instance.UserData.Waveidx.Value == 1 && GameRoot.Instance.UserData.Stageidx.Value == 2)
        {
            if (GameRoot.Instance.UserData.Tutorial.Contains(TutorialSystem.Tuto_4))
            {
                GameRoot.Instance.UserData.Tutorial.Remove(TutorialSystem.Tuto_4);
            }

            GameRoot.Instance.UserData.Ingamesilvercoin.Value += 10;

            GameRoot.Instance.TutorialSystem.StartTutorial(TutorialSystem.Tuto_4);
        }
    }

    public void StopWave()
    {
        if (currentWaveCoroutine != null)
        {
            StopCoroutine(currentWaveCoroutine);
            currentWaveCoroutine = null;
        }

        EnemyUnitGroup.ClearData();

        // 웨이브 중지 시 휴식 상태로 전환하여 TileWeaponComponent 드래그 가능하도록 설정
    }


    public void StartWave()
    {
        // 이미 웨이브가 진행 중이면 무시
        if (currentWaveCoroutine != null)
        {
            return;
        }

        currentWaveCoroutine = StartCoroutine(StartWaveCoroutine());
    }

    public void StartRest()
    {
        GameRoot.Instance.UserData.Playerdata.IsWaveRestProperty.Value = true;
        PlayerUnitGroup.IsWinAnimationPlaying = false;

        //logs
        List<TpParameter> parameters = new List<TpParameter>();
        parameters.Add(new TpParameter("stage", GameRoot.Instance.UserData.Stageidx.Value));
        parameters.Add(new TpParameter("waveidx", GameRoot.Instance.UserData.Waveidx.Value));
        GameRoot.Instance.PluginSystem.AnalyticsProp.AllEvent(IngameEventType.None, "m_wave_clear", parameters);



        GameRoot.Instance.UserData.Waveidx.Value += 1;
        
        GameRoot.Instance.ShopSystem.ShowInterAd();

        

        SoundPlayer.Instance.PlaySound("sfx_wave_win");

        EnemyUnitGroup.CheckEnemyBlockSpawner();


        //리롤 튜토리얼 체크
        if (GameRoot.Instance.UserData.Waveidx.Value == 2 && GameRoot.Instance.UserData.Stageidx.Value == 1)
        {
            if (GameRoot.Instance.UserData.Tutorial.Contains(TutorialSystem.Tuto_1))
            {
                GameRoot.Instance.UserData.Tutorial.Remove(TutorialSystem.Tuto_1);
            }

            GameRoot.Instance.TutorialSystem.StartTutorial(TutorialSystem.Tuto_1);
        }

        // 웨이브 종료 시 쉴드 초기화
        GameRoot.Instance.UserData.Playerdata.CurShiledProperty.Value = 0;

        var wavecount = Tables.Instance.GetTable<WaveInfo>().DataList.FindAll(x => x.stage == GameRoot.Instance.UserData.Stageidx.Value).Count;

        bool isend = GameRoot.Instance.UserData.Waveidx.Value > wavecount;


        if (isend)
        {
            GameRoot.Instance.UISystem.OpenUI<PopupStageResult>(popup => popup.Set(true));
        }
        else
        {
            var popupInGame = GameRoot.Instance.UISystem.GetUI<PopupInGame>();
            if (popupInGame != null && popupInGame.TileWeaponGroup != null)
            {
                popupInGame.TileWeaponGroup.StartRandSelectBagAd();
                popupInGame.TileWeaponGroup.SetStartBattleWeapon();

                GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.TileWeaponGroup.StartRandSelectBag();
            }
        }

        GameRoot.Instance.UserData.SaveInGameResumeSnapshot(true, true);
    }

    private IEnumerator StartWaveCoroutine()
    {
        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;

        var Waveidx = GameRoot.Instance.UserData.Waveidx.Value;

        var td = Tables.Instance.GetTable<WaveInfo>().GetData(new KeyValuePair<int, int>(stageidx, Waveidx));

        if (td != null)
        {
            int resultwave_silvercoin = td.add_silver_coin;
            int resultwave_exp = td.add_exp_value;

            // 전체 적의 수 계산
            int totalEnemyCount = 0;
            for (int i = 0; i < td.unit_idx.Count; i++)
            {
                int count = (td.unit_count != null && i < td.unit_count.Count) ? td.unit_count[i] : 1;
                totalEnemyCount += count;
            }


            int baseExpPerEnemy = totalEnemyCount > 0 ? resultwave_exp / totalEnemyCount : 0;
            int remainingExp = totalEnemyCount > 0 ? resultwave_exp % totalEnemyCount : 0;

            int enemyIndex = 0; // 적 생성 순서 추적

            for (int i = 0; i < td.unit_idx.Count; i++)
            {
                // 첫 번째 유닛 타입 스폰 전에도 unit_appear_time 적용
                if (i == 0 && td.unit_appear_time != null && i < td.unit_appear_time.Count)
                {
                    float waitTime = td.unit_appear_time[i] / 100f;
                    yield return new WaitForSeconds(waitTime);

                    // 대기 후 게임 포기 체크
                    if (!GameRoot.Instance.UserData.Playerdata.IsGameStartProperty.Value)
                    {
                        currentWaveCoroutine = null;
                        yield break;
                    }
                }

                int spawnCount = (td.unit_count != null && i < td.unit_count.Count) ? td.unit_count[i] : 1;
                for (int j = 0; j < spawnCount; j++)
                {
                    // 게임 포기 체크
                    if (!GameRoot.Instance.UserData.Playerdata.IsGameStartProperty.Value)
                    {
                        currentWaveCoroutine = null;
                        yield break;
                    }



                    int expForThisEnemy = baseExpPerEnemy;
                    if (enemyIndex < remainingExp)
                    {
                        expForThisEnemy += 1;
                    }

                    // WaveInfo에서 unit_dmg, unit_hp 가져오기
                    int unitDmg = (td.unit_dmg != null && td.unit_dmg.Count > 0) ? (i < td.unit_dmg.Count ? td.unit_dmg[i] : td.unit_dmg.Last()) : 1;
                    int unitHp = (td.unit_hp != null && td.unit_hp.Count > 0) ? (i < td.unit_hp.Count ? td.unit_hp[i] : td.unit_hp.Last()) : 1;


                    EnemyUnitGroup.AddUnit(td.unit_idx[i], expForThisEnemy, unitDmg, unitHp);

                    enemyIndex++;
                }

                // unit_appear_time을 백분율로 적용 (100으로 나누어 초 단위로 변환)
                // 마지막 유닛 타입이 아닐 때만 대기 (마지막 유닛 타입 생성 후에는 바로 스폰 완료 처리)
                bool isLastUnitType = (i == td.unit_idx.Count - 1);
                if (!isLastUnitType && td.unit_appear_time != null && i < td.unit_appear_time.Count)
                {
                    float waitTime = td.unit_appear_time[i] / 100f;
                    yield return new WaitForSeconds(waitTime);

                    // 대기 후 다시 게임 포기 체크
                    if (!GameRoot.Instance.UserData.Playerdata.IsGameStartProperty.Value)
                    {
                        currentWaveCoroutine = null;
                        yield break;
                    }
                }
            }
        }

        currentWaveCoroutine = null;

        // 웨이브 스폰이 완료되었을 때, 적이 이미 모두 죽었는지 체크
        EnemyUnitGroup.CheckAndStartRestIfAllDead();
    }



}
