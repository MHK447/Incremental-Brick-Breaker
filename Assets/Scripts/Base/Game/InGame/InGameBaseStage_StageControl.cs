using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Collections;
using System.IO.Compression;
using System.Linq;
using UnityEngine.AddressableAssets;

public partial class InGameBaseStage : MonoBehaviour
{
    public GameObject ChapterMapRoot;

    public EnemyUnitGroup EnemyUnitGroup;

    private Coroutine currentWaveCoroutine = null;



    [HideInInspector]
    public int StageStartTime = 0;

    // 웨이브가 완전히 끝났는지 확인 (스폰이 모두 완료되었는지)
    public bool IsWaveSpawnComplete { get { return currentWaveCoroutine == null; } }



    public void InitStage()
    {
        GameRoot.Instance.UISystem.OpenUI<PopupInGame>();

        EquipTutorialCheck();   

        GameRoot.Instance.StartCoroutine(EnemySpawnStart());
    }

    public void StartBattle()
    {


    }

    private float stagedeltatime = 0;
    private float resumeSnapshotDeltaTime = 0f;

    void Update()
    {

    }


    public void SetHp(int hp)
    {
    }

    public void TutorialCheck()
    {

    }

    public void StageClear()
    {
        GameRoot.Instance.UserData.Ingamesilvercoin.Value = 0;

        // PopupInGame의 TileWeaponGroup 초기화
    }

    public bool GameFinishSequenceStarted = false;
    public IEnumerator GameOverSequence()
    {
        if (GameFinishSequenceStarted) yield break;
        GameFinishSequenceStarted = true;


        //slow mo
        yield return new WaitForSecondsRealtime(1f);

        yield return new WaitForSeconds(1f);

        //not really dead
        if (this != null)
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
        StageClear();

        fadeaction += StartMainUI;
    }

    public void StartMainUI()
    {
        ProjectUtility.SetActiveCheck(ChapterMapRoot, false);

        SoundPlayer.Instance.SetBGMVolume(0.125f);
        SoundPlayer.Instance.RestartBGM();


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


        // 웨이브 중지 시 휴식 상태로 전환하여 TileWeaponComponent 드래그 가능하도록 설정
    }


    public void StartWave()
    {
        // 이미 웨이브가 진행 중이면 무시
        if (currentWaveCoroutine != null)
        {
            return;
        }

        if (EnemyUnitGroup == null) return;

        int stageIdx = GameRoot.Instance.UserData.Stageidx.Value;
        int waveIdx = GameRoot.Instance.UserData.Waveidx.Value;

        currentWaveCoroutine = StartCoroutine(RunWave(stageIdx, waveIdx));
    }

    private IEnumerator RunWave(int stageIdx, int waveIdx)
    {
        yield return StartCoroutine(EnemyUnitGroup.SpawnWave(stageIdx, waveIdx));
        currentWaveCoroutine = null;

        // 스폰 완료 후 이미 모든 적이 죽었는지 체크
        EnemyUnitGroup.CheckAndStartRestIfAllDead();
    }

    public void StartRest()
    {

        //logs
        GameRoot.Instance.UserData.Waveidx.Value += 1;


        SoundPlayer.Instance.PlaySound("sfx_wave_win");

        //      EnemyUnitGroup.CheckEnemyBlockSpawner();


        //리롤 튜토리얼 체크
        if (GameRoot.Instance.UserData.Waveidx.Value == 2 && GameRoot.Instance.UserData.Stageidx.Value == 1)
        {
            if (GameRoot.Instance.UserData.Tutorial.Contains(TutorialSystem.Tuto_1))
            {
                GameRoot.Instance.UserData.Tutorial.Remove(TutorialSystem.Tuto_1);
            }

            GameRoot.Instance.TutorialSystem.StartTutorial(TutorialSystem.Tuto_1);
        }

    }

    public IEnumerator EnemySpawnStart()
    {
        // Move Truck

        yield return new WaitForSeconds(2f);

        var waveidx = GameRoot.Instance.UserData.Waveidx.Value;
        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;

        var td = Tables.Instance.GetTable<WaveInfo>().GetData(new KeyValuePair<int, int>(stageidx, waveidx));

        if (td != null)
        {
           GameRoot.Instance.StartCoroutine(SpawnEnemiesSequentially(td));
        }
    }

    private IEnumerator SpawnEnemiesSequentially(BanpoFri.WaveInfoData waveData)
    {
        for (int i = 0; i < waveData.unit_idx.Count; i++)
        {
            int enemyIdx = waveData.unit_idx[i];
            int dmg = (i < waveData.unit_dmg.Count) ? waveData.unit_dmg[i] : 0;
            int hp = (i < waveData.unit_hp.Count) ? waveData.unit_hp[i] : 0;
            int count = (i < waveData.unit_count.Count) ? waveData.unit_count[i] : 1;
            float appearTime = (i < waveData.unit_appear_time.Count) ? waveData.unit_appear_time[i] * 0.001f : 0f;

            if (appearTime > 0f)
            {
                yield return new WaitForSeconds(appearTime);
            }

            for (int j = 0; j < count; j++)
            {
                EnemyUnitGroup.EnemySpawn(enemyIdx, dmg, hp);
            }
        }

        // 스폰 완료 후 모든 적이 죽을 때까지 대기
        yield return new WaitUntil(() => EnemyUnitGroup.IsAllDeadCheck);

        // 다음 웨이브로 진행
        GameRoot.Instance.UserData.Waveidx.Value += 1;  

        int nextWaveIdx = GameRoot.Instance.UserData.Waveidx.Value;
        int curStageIdx = GameRoot.Instance.UserData.Stageidx.Value;

        var nextWaveData = Tables.Instance.GetTable<WaveInfo>().GetData(new KeyValuePair<int, int>(curStageIdx, nextWaveIdx));

        if (nextWaveData == null)
        {
            // 현재 스테이지의 웨이브가 없으면 다음 스테이지로
            GameRoot.Instance.UserData.Stageidx.Value += 1;
            GameRoot.Instance.UserData.Waveidx.Value = 1;
        }

        // 다음 웨이브 시작
        GameRoot.Instance.StartCoroutine(EnemySpawnStart());
    }

}
