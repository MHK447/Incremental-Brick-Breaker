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

    [SerializeField]
    private PlayerUnit PlayerUnit;

    public PlayerUnit Player { get { return PlayerUnit; } }

    private Coroutine currentWaveCoroutine = null;

    [SerializeField]
    private Transform MoveMapRoot;

    [SerializeField]
    private WeaponFallControler WeaponFallControler;

    public WeaponFallControler GetWeaponFallControler { get { return WeaponFallControler; } }

    // MoveMapComponent 통합 관리
    private List<MoveMapComponent> MoveMapComponents = new List<MoveMapComponent>();

    /// <summary> 웨이브 전환 시 플레이어 Run + 맵 이동 연출 시간(초). 웨이브 진행 바 채우기 시간과 동기화됨. </summary>
    public const float WaveMoveDuration = 4f;

    /// <summary> 첫 웨이브 이동 연출을 이미 했으면 true (다음 웨이브부터는 이동 생략) </summary>
    private bool _firstWaveMoveDone = false;

    [HideInInspector]
    public int StageStartTime = 0;

    // 웨이브가 완전히 끝났는지 확인 (스폰이 모두 완료되었는지)
    public bool IsWaveSpawnComplete { get { return currentWaveCoroutine == null; } }



    public void InitStage()
    {
        _firstWaveMoveDone = false;

        GameRoot.Instance.UISystem.OpenUI<PopupInGame>(popup => popup.Init());

        PlayerUnit.Init();

        EquipTutorialCheck();

        InitMoveMapComponents();

        StartCoroutine(EnemySpawnStart());

        WeaponFallControler.Init();
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

        // 다음 웨이브: 승리 연출(PlayWinActionAndStartRest) 후 휴식 처리 끝났으므로 이동 연출 → 스폰
        StartCoroutine(EnemySpawnStart());
    }

    public IEnumerator EnemySpawnStart()
    {
        // 매 웨이브마다 이동 연출: 플레이어 Run + 맵(바퀴) 무한 이동 4초 후 스폰
        PlayerUnit.ChangeState(PlayerUnit.PlayerState.Run);

        foreach (var movemap in MoveMapComponents)
        {
            movemap.StartInfiniteMove();
        }

        yield return new WaitForSeconds(WaveMoveDuration);

        // 이동 연출 완전히 종료
        foreach (var movemap in MoveMapComponents)
        {
            movemap.PauseMove();
        }
        PlayerUnit.ChangeState(PlayerUnit.PlayerState.Idle);
        yield return null; // 한 프레임 대기해 이동 종료 상태 반영 후 진행


        if (!_firstWaveMoveDone)
            _firstWaveMoveDone = true;

        var waveidx = GameRoot.Instance.UserData.Waveidx.Value;
        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;
        var td = Tables.Instance.GetTable<WaveInfo>().GetData(new KeyValuePair<int, int>(stageidx, waveidx));

        if (td != null)
        {
            currentWaveCoroutine = StartCoroutine(SpawnEnemiesSequentially(td)); // 이동 다 끝난 뒤 적 소환
        }
        else
        {
            // 해당 스테이지/웨이브 데이터가 없으면 다음 구간으로 진행 후 재시도
            GameRoot.Instance.UserData.Waveidx.Value += 1;
            int nextStage = GameRoot.Instance.UserData.Stageidx.Value;
            int nextWave = GameRoot.Instance.UserData.Waveidx.Value;
            var nextTd = Tables.Instance.GetTable<WaveInfo>().GetData(new KeyValuePair<int, int>(nextStage, nextWave));
            if (nextTd == null)
            {
                GameRoot.Instance.UserData.Stageidx.Value += 1;
                GameRoot.Instance.UserData.Waveidx.Value = 1;
            }
            StartCoroutine(EnemySpawnStart());
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
            float appearTime = (i < waveData.unit_appear_time.Count) ? waveData.unit_appear_time[i] * 0.01f : 0f;

            if (appearTime > 0f)
            {
                yield return new WaitForSeconds(appearTime);
            }

            for (int j = 0; j < count; j++)
            {
                EnemyUnitGroup.EnemySpawn(enemyIdx, dmg, hp);
            }
        }

        // 스폰 완료 → 웨이브 진행 플래그 해제 (승리 연출 시 IsWaveSpawnComplete true)
        currentWaveCoroutine = null;

        yield return new WaitUntil(() => EnemyUnitGroup.IsAllDeadCheck);
    }


    private void InitMoveMapComponents()
    {
        MoveMapComponents.Clear();

        if (MoveMapRoot != null)
        {
            var components = MoveMapRoot.GetComponentsInChildren<MoveMapComponent>().ToList();
            MoveMapComponents.AddRange(components);
        }

    }

}
