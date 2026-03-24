using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Collections;
using System.IO.Compression;
using System.Linq;
using UnityEngine.AddressableAssets;
using DG.Tweening;

public partial class InGameBaseStage : MonoBehaviour
{
    public GameObject ChapterMapRoot;

    public EnemyUnitGroup EnemyUnitGroup;

    [SerializeField]
    private PlayerUnit PlayerUnit;

    public PlayerUnit Player { get { return PlayerUnit; } }

    private Coroutine currentWaveCoroutine = null;

    /// <summary> 유닛 스폰 for 루프까지 끝났는지. WaitUntil(전멸) 구간에서도 true라 승리 연출 시작 가능. </summary>
    private bool waveEnemySpawnDeliveryComplete = false;

    [SerializeField]
    private Transform MoveMapRoot;

    [SerializeField]
    private WeaponFallControler WeaponFallControler;

    [SerializeField]
    private EnemyBlockSpawner EnemyBlockSpawner;

    [SerializeField]
    private List<Transform> EnemySpawnTrList = new List<Transform>();

    public WeaponFallControler GetWeaponFallControler { get { return WeaponFallControler; } }

    // MoveMapComponent 통합 관리
    private List<MoveMapComponent> MoveMapComponents = new List<MoveMapComponent>();

    

    /// <summary> 웨이브 전환 시 플레이어 Run + 맵 이동 연출 시간(초). 웨이브 진행 바 채우기 시간과 동기화됨. </summary>
    public const float WaveMoveDuration = 5f;
    private const float BlockSpawnerShowLeadTime = 1f;

    /// <summary> 트럭 미해금(고정 스폰) 구간에서 슬롯당 적 사망 후 재스폰까지 대기 시간(초). </summary>
    private const float PersistentEnemyRespawnDelay = 10f;
    [SerializeField] private float enemySpawnMoveLeftSpeed = 2f;

    /// <summary> 첫 웨이브 이동 연출을 이미 했으면 true (다음 웨이브부터는 이동 생략) </summary>
    private bool _firstWaveMoveDone = false;

    [HideInInspector]
    public int StageStartTime = 0;

    // 웨이브 유닛 스폰이 모두 끝났는지 (전멸 대기 중이어도 true — 승리 연출 조건용)
    public bool IsWaveSpawnComplete { get { return waveEnemySpawnDeliveryComplete; } }



    public void InitStage()
    {
        _firstWaveMoveDone = false;

        GameRoot.Instance.UISystem.OpenUI<PopupInGame>(popup => popup.Init());

        PlayerUnit.Init();
        GameRoot.Instance.IncreaMentalSystem.ApplyWeaponUnlocksOnStageStart();

        EquipTutorialCheck();

        InitMoveMapComponents();

        FirstStartEnemySpawn();

        StartCoroutine(EnemySpawnStart());

        WeaponFallControler.Init();

        persistentBlockSpawnerWaveKey = -1;
        ProjectUtility.SetActiveCheck(EnemyBlockSpawner.gameObject, false);
        if (EnemyUnitGroup != null)
            EnemyUnitGroup.IsEnemyBlockSpawnerActive = false;
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

        // 스폰 루프 중단 여부와 무관하게 현재 웨이브는 더 이상 스폰 진행으로 간주하지 않음
        waveEnemySpawnDeliveryComplete = true;
    }


    // public void StartWave()
    // {
    //     // 이미 웨이브가 진행 중이면 무시
    //     if (currentWaveCoroutine != null)
    //     {
    //         return;
    //     }

    //     if (EnemyUnitGroup == null) return;

    //     int stageIdx = GameRoot.Instance.UserData.Stageidx.Value;
    //     int waveIdx = GameRoot.Instance.UserData.Waveidx.Value;

    //     currentWaveCoroutine = StartCoroutine(RunWave(stageIdx, waveIdx));
    // }

    // private IEnumerator RunWave(int stageIdx, int waveIdx)
    // {
    //     yield return StartCoroutine(EnemyUnitGroup.SpawnWave(stageIdx, waveIdx));
    //     currentWaveCoroutine = null;

    //     // 스폰 완료 후 이미 모든 적이 죽었는지 체크
    //     EnemyUnitGroup.CheckAndStartRestIfAllDead();
    // }

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
        bool truckUnlocked = GameRoot.Instance.IncreaMentalSystem.IsUnlocked(IncreaMentalType.TruckUnlock);

        if (truckUnlocked)
        {
            StartWaveSpawnForCurrentWave();

            PlayerUnit.ChangeState(PlayerUnit.PlayerState.Run);

            foreach (var movemap in MoveMapComponents)
            {
                movemap.StartInfiniteMove();
            }

            float showDelay = Mathf.Max(0f, WaveMoveDuration - BlockSpawnerShowLeadTime);
            if (showDelay > 0f)
                yield return new WaitForSeconds(showDelay);

            // 블록 스포너는 이동 종료 직전(1초 전)에 미리 노출
            if (EnemyUnitGroup != null
                && EnemyUnitGroup.IsEnemyBlockSpawnerActive
                && EnemyBlockSpawner != null
                && !EnemyBlockSpawner.IsDead)
            {
                ProjectUtility.SetActiveCheck(EnemyBlockSpawner.gameObject, true);
                EnemyBlockSpawner.StartSpawning();
            }

            float remainDelay = WaveMoveDuration - showDelay;
            if (remainDelay > 0f)
                yield return new WaitForSeconds(remainDelay);

            foreach (var movemap in MoveMapComponents)
            {
                movemap.PauseMove();
            }
            PlayerUnit.ChangeState(PlayerUnit.PlayerState.Idle);
            yield return null;
        }
        else
        {
            foreach (var movemap in MoveMapComponents)
            {
                movemap.PauseMove();
            }
        }

        if (!_firstWaveMoveDone)
            _firstWaveMoveDone = true;

        if (!truckUnlocked)
            yield break;
    }

    private void StartWaveSpawnForCurrentWave()
    {
        if (currentWaveCoroutine != null)
            return;

        var waveidx = GameRoot.Instance.UserData.Waveidx.Value;
        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;
        var td = Tables.Instance.GetTable<WaveInfo>().GetData(new KeyValuePair<int, int>(stageidx, waveidx));

        if (td != null)
        {
            if (td.block_spawn_check > 0)
            {

                if (td.block_spawn_check > 0 && EnemyBlockSpawner != null)
                {
                    EnemyUnitGroup.EnemyBlockSpawner = EnemyBlockSpawner;
                    EnemyUnitGroup.IsEnemyBlockSpawnerActive = true;
                    ProjectUtility.SetActiveCheck(EnemyBlockSpawner.gameObject, false);
                    EnemyBlockSpawner.Set(td.block_spawn_check, td.block_spawn_hp);
                    EnemyBlockSpawner.SetSpawnData(td.unit_idx, td.unit_dmg, td.unit_hp, td.unit_count);
                }


                EnsurePersistentBlockSpawnerForWave(stageidx, waveidx, td);
            }

            currentWaveCoroutine = StartCoroutine(SpawnEnemiesSequentially(td));
        }
        else
        {
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
        waveEnemySpawnDeliveryComplete = false;

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

        // 스폰 루프 종료 → 승리 판정(전멸) 가능. 코루틴 핸들은 WaitUntil 끝날 때까지 유지해 StopWave로 중단 가능.
        waveEnemySpawnDeliveryComplete = true;

        yield return new WaitUntil(() => EnemyUnitGroup.IsAllDeadCheck);

        currentWaveCoroutine = null;
    }

    private Coroutine persistentSpawnCoroutine;
    private List<EnemyUnitBase> persistentEnemyList = new List<EnemyUnitBase>();
    private float[] persistentRespawnTimers;

    private int persistentBlockSpawnerWaveKey = -1;

    private static int MakePersistentWaveKey(int stageIdx, int waveIdx) => stageIdx * 10000 + waveIdx;

    public void FirstStartEnemySpawn()
    {
        if (GameRoot.Instance.IncreaMentalSystem.IsUnlocked(IncreaMentalType.TruckUnlock))
            return;

        if (persistentSpawnCoroutine != null)
        {
            StopCoroutine(persistentSpawnCoroutine);
        }
        persistentSpawnCoroutine = StartCoroutine(PersistentEnemySpawnRoutine());
    }

    private EnemyUnitBase SpawnStationaryEnemy(int enemyIdx, int dmg, int hp, Vector3 position, bool playSpawnPop = false)
    {
        var unit = EnemyUnitGroup.EnemySpawnAtPosition(enemyIdx, dmg, hp, position, true);
        if (unit != null)
        {
            unit.IsStationary = true;
            unit.ChangeState(EnemyUnitBase.EnemyUnitState.Idle);

            if (playSpawnPop)
            {
                var t = unit.transform;
                t.DOKill();
                Vector3 endScale = t.localScale;
                if (endScale.sqrMagnitude < 0.0001f)
                    endScale = Vector3.one;
                t.localScale = Vector3.zero;
                t.DOScale(endScale, 0.28f).SetEase(Ease.OutBack);
            }
        }
        return unit;
    }

    private void EnsurePersistentBlockSpawnerForWave(int stageIdx, int waveIdx, BanpoFri.WaveInfoData td)
    {
        int key = MakePersistentWaveKey(stageIdx, waveIdx);
        if (persistentBlockSpawnerWaveKey == key)
            return;

        persistentBlockSpawnerWaveKey = key;
        StartEnemyBlockSpawner(td.block_spawn_check, td.block_spawn_hp, td.unit_idx, td.unit_dmg, td.unit_hp, td.unit_count);
    }

    private void ClearPersistentBlockSpawnerState()
    {
        persistentBlockSpawnerWaveKey = -1;
        if (EnemyBlockSpawner != null)
            ProjectUtility.SetActiveCheck(EnemyBlockSpawner.gameObject, false);
        if (EnemyUnitGroup != null)
            EnemyUnitGroup.IsEnemyBlockSpawnerActive = false;
    }

    private IEnumerator PersistentEnemySpawnRoutine()
    {
        persistentEnemyList.Clear();
        persistentRespawnTimers = new float[EnemySpawnTrList.Count];
        for (int i = 0; i < EnemySpawnTrList.Count; i++)
        {
            persistentEnemyList.Add(null);
        }

        while (!GameRoot.Instance.IncreaMentalSystem.IsUnlocked(IncreaMentalType.TruckUnlock))
        {
            var stageidx = GameRoot.Instance.UserData.Stageidx.Value;
            var waveidx = GameRoot.Instance.UserData.Waveidx.Value;
            var td = Tables.Instance.GetTable<WaveInfo>().GetData(new KeyValuePair<int, int>(stageidx, waveidx));

            if (td == null || td.unit_idx.Count == 0)
            {
                ClearPersistentBlockSpawnerState();
                yield return null;
                continue;
            }

            int enemyIdx = td.unit_idx[0];
            int dmg = td.unit_dmg.Count > 0 ? td.unit_dmg[0] : 0;
            int hp = td.unit_hp.Count > 0 ? td.unit_hp[0] : 0;
            bool blockSpawnWave = td.block_spawn_check > 0;

            if (blockSpawnWave)
            {
                EnsurePersistentBlockSpawnerForWave(stageidx, waveidx, td);

                for (int i = 0; i < EnemySpawnTrList.Count; i++)
                {
                    var enemy = persistentEnemyList[i];
                    bool isAlive = enemy != null && !enemy.IsDead && enemy.gameObject.activeSelf;

                    if (isAlive) continue;

                    if (persistentRespawnTimers[i] > 0f)
                    {
                        persistentRespawnTimers[i] -= Time.deltaTime;
                        continue;
                    }

                    if (enemy != null)
                    {
                        persistentRespawnTimers[i] = 5f;
                        persistentEnemyList[i] = null;
                        continue;
                    }

                    persistentEnemyList[i] = SpawnStationaryEnemy(
                        enemyIdx, dmg, hp, EnemySpawnTrList[i].position, playSpawnPop: true);
                }
            }
            else
            {
                if (persistentBlockSpawnerWaveKey != -1)
                    ClearPersistentBlockSpawnerState();

                for (int i = 0; i < EnemySpawnTrList.Count; i++)
                {
                    var enemy = persistentEnemyList[i];
                    bool isAlive = enemy != null && !enemy.IsDead && enemy.gameObject.activeSelf;

                    if (isAlive) continue;

                    if (persistentRespawnTimers[i] > 0f)
                    {
                        persistentRespawnTimers[i] -= Time.deltaTime;
                        continue;
                    }

                    if (enemy != null)
                    {
                        persistentRespawnTimers[i] = PersistentEnemyRespawnDelay;
                        persistentEnemyList[i] = null;
                        continue;
                    }

                    persistentEnemyList[i] = SpawnStationaryEnemy(
                        enemyIdx, dmg, hp, EnemySpawnTrList[i].position);
                }
            }

            yield return null;
        }

        ClearPersistentBlockSpawnerState();
        EnemyUnitGroup.PersistentUnits.Clear();
        persistentEnemyList.Clear();
        persistentSpawnCoroutine = null;
    }

    public void StopPersistentSpawn()
    {
        if (persistentSpawnCoroutine != null)
        {
            StopCoroutine(persistentSpawnCoroutine);
            persistentSpawnCoroutine = null;
        }
        ClearPersistentBlockSpawnerState();
        EnemyUnitGroup.PersistentUnits.Clear();
        persistentEnemyList.Clear();
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

    /// <summary>MoveMapComponent와 동일한 이번 프레임 스크롤 분량(월드). 이펙트 등을 배경과 같이 움직일 때 사용.</summary>
    public Vector3 GetMapScrollWorldDelta()
    {
        if (MoveMapComponents == null || MoveMapComponents.Count == 0)
            return Vector3.zero;

        foreach (var mm in MoveMapComponents)
        {
            if (mm == null)
                continue;
            Vector3 d = mm.GetWorldScrollDelta();
            if (d.sqrMagnitude > 0f)
                return d;
        }

        return Vector3.zero;
    }

    public void StartEnemyBlockSpawner(int spawnidx, int hp, System.Collections.Generic.List<int> enemyIdxList = null, System.Collections.Generic.List<int> enemyDmgList = null, System.Collections.Generic.List<int> enemyHpList = null, System.Collections.Generic.List<int> enemyCountList = null)
    {
        ProjectUtility.SetActiveCheck(EnemyBlockSpawner.gameObject, false);

        EnemyBlockSpawner.Set(spawnidx, hp);

        if (enemyIdxList != null)
        {
            EnemyBlockSpawner.SetSpawnData(enemyIdxList, enemyDmgList, enemyHpList, enemyCountList);
        }

        if (EnemyUnitGroup != null)
        {
            EnemyUnitGroup.EnemyBlockSpawner = EnemyBlockSpawner;
            EnemyUnitGroup.IsEnemyBlockSpawnerActive = true;
        }
    }



}
