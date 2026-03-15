using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BanpoFri;
using UnityEngine.AddressableAssets;
using DG.Tweening;
using System.Linq;


public class EnemyUnitGroup : MonoBehaviour
{

    public HashSet<EnemyUnitBase> ActiveUnits = new HashSet<EnemyUnitBase>();
    public HashSet<EnemyUnitBase> DeadUnits = new HashSet<EnemyUnitBase>();
    public HashSet<EnemyUnitBase> PersistentUnits = new HashSet<EnemyUnitBase>();

    [SerializeField]
    private List<Transform> UnitSpawnList = new List<Transform>();

    public EnemyBlockSpawner EnemyBlockSpawner;

    [HideInInspector]
    public bool IsEnemyBlockSpawnerActive = false;

    public bool IsAllDeadCheck
    {
        get
        {
            // persistent 유닛을 제외한 일반 적 유닛이 모두 죽었는지 확인
            if (ActiveUnits.Any(u => !PersistentUnits.Contains(u))) return false;

            if (IsEnemyBlockSpawnerActive && EnemyBlockSpawner != null && !EnemyBlockSpawner.IsDead)
                return false;

            return true;
        }
    }

    private int SpawnOrder = 0;
    private bool isWinSequenceRunning = false;

    private int OneTimeRewardCount = 0;

    private void ReleaseInstance(GameObject target)
    {
        if (target == null)
            return;

        if (!Addressables.ReleaseInstance(target))
        {
            Destroy(target);
        }
    }

    private void ReleaseEnemyUnit(EnemyUnitBase unit)
    {
        if (unit == null)
            return;

        unit.transform.DOKill();
        ProjectUtility.SetActiveCheck(unit.gameObject, false);
        ReleaseInstance(unit.gameObject);
    }

    private void TryStartWinActionAndStartRest()
    {
        if (isWinSequenceRunning || !IsAllDeadCheck)
            return;

        var stage = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage;
        if (stage == null || !stage.IsWaveSpawnComplete)
            return;

        isWinSequenceRunning = true;
        StartCoroutine(PlayWinActionAndStartRest());
    }

    public void Init()
    {
        SpawnOrder = 0;
        OneTimeRewardCount = 0;
        isWinSequenceRunning = false;
        PersistentUnits.Clear();
    }



    public void AddUnit(EnemyUnitData enemydata)
    {
        var td = Tables.Instance.GetTable<EnemyUnitInfo>().GetData(enemydata.EnemyIdx);

        if (td != null)
        {
            // DeadUnits에서 같은 enemyidx를 가진 유닛을 찾아 재활용
            var find = DeadUnits.FirstOrDefault(x => x.EnemyIdx == enemydata.EnemyIdx);

            EnemyUnitBase instance;

            if (find != null)
            {
                // 재활용
                instance = find;
                DeadUnits.Remove(find);
                ActiveUnits.Add(instance);
            }
            else
            {
                // 새로 생성
                var handle = Addressables.InstantiateAsync(td.prefab, transform);
                var result = handle.WaitForCompletion();
                instance = result.GetComponent<EnemyUnitBase>();
                ProjectUtility.SetActiveCheck(instance.gameObject, false);

                ActiveUnits.Add(instance);
            }

            var randcount = Random.Range(0, UnitSpawnList.Count);
            instance.Set(enemydata);
            instance.transform.position = UnitSpawnList[randcount].position;
            ProjectUtility.SetActiveCheck(instance.gameObject, true);
        }
    }


    public void DeleteUnit(EnemyUnitBase unit)
    {
        if (unit == null)
            return;

        if (ActiveUnits.Contains(unit))
        {
            ActiveUnits.Remove(unit);
            PersistentUnits.Remove(unit);
            DeadUnits.Add(unit);
            unit.transform.DOKill();
            ProjectUtility.SetActiveCheck(unit.gameObject, false);
        }

        // 모든 적 유닛을 처치했고, 웨이브 스폰이 완전히 끝났는지 확인
        TryStartWinActionAndStartRest();

        //GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.NextWaveCheck();
    }

    // 웨이브 스폰 완료 후 적이 이미 모두 죽었는지 체크하는 메서드
    public void CheckAndStartRestIfAllDead()
    {
        TryStartWinActionAndStartRest();
    }

    public IEnumerator PlayWinActionAndStartRest()
    {
        try
        {
            var stage = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage;
            if (stage == null)
                yield break;

            // 승리 연출 플래그 설정 (플레이어 유닛 추가 방지)
        



            // 2초 대기
            yield return new WaitForSeconds(2f);

            EndStageReset();
        }
        finally
        {
            isWinSequenceRunning = false;
        }
    }



    public void EndStageReset()
    {

        // 웨이브 휴식 상태로 전환
        GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.StartRest();
    }

    public EnemyUnitBase FindTargetEnemy(Transform closetroottr, float attackrange = -1)
    {
        EnemyUnitBase closestEnemy = null;
        float closestDistance = float.MaxValue;
        float originX = closetroottr.position.x;
        bool useRange = attackrange >= 0f;

        foreach (var unit in ActiveUnits)
        {
            if (unit == null || unit.IsDead)
                continue;

            float distance = Mathf.Abs(originX - unit.transform.position.x);
            if (useRange && distance > attackrange)
                continue;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = unit;
            }
        }

        return closestEnemy;
    }

    public void ClearActiveUnitsForResume()
    {

        foreach (var unit in ActiveUnits)
        {
            if (unit != null)
            {
                unit.transform.DOKill();
                ProjectUtility.SetActiveCheck(unit.gameObject, false);
                DeadUnits.Add(unit);
            }
        }
        ActiveUnits.Clear();
    }

    public void CheckEnemyBlockSpawner()
    {
        ProjectUtility.SetActiveCheck(EnemyBlockSpawner.gameObject, false);
        IsEnemyBlockSpawnerActive = false;
    }



    public void ClearData()
    {
        isWinSequenceRunning = false;

        var unitsToRelease = new HashSet<EnemyUnitBase>();

        foreach (var block in ActiveUnits)
            unitsToRelease.Add(block);

        foreach (var block in DeadUnits)
            unitsToRelease.Add(block);

        // 컬렉션 참조가 끊긴 잔존 유닛까지 정리
        var allUnits = GetComponentsInChildren<EnemyUnitBase>(true);
        foreach (var unit in allUnits)
            unitsToRelease.Add(unit);

        foreach (var unit in unitsToRelease)
            ReleaseEnemyUnit(unit);

        ActiveUnits.Clear();
        DeadUnits.Clear();
        PersistentUnits.Clear();

        EnemyBlockSpawner.ClearData();
    }


    public void EnemySpawn(int enemyidx, int unitdmg, int unithp)
    {
        var td = Tables.Instance.GetTable<EnemyUnitInfo>().GetData(enemyidx);
        if (td != null)
        {
            // DeadUnits에서 같은 enemyidx를 가진 유닛을 찾아 재활용
            var find = DeadUnits.FirstOrDefault(x => x.EnemyIdx == enemyidx);

            EnemyUnitBase instance;

            if (find != null)
            {
                // 재활용
                instance = find;
                DeadUnits.Remove(find);
                ActiveUnits.Add(instance);
            }
            else
            {
                // 새로 생성
                var handle = Addressables.InstantiateAsync(td.prefab, transform);
                var result = handle.WaitForCompletion();
                instance = result.GetComponent<EnemyUnitBase>();
                ProjectUtility.SetActiveCheck(instance.gameObject, false);

                ActiveUnits.Add(instance);
            }

         
            var randcount = Random.Range(0, UnitSpawnList.Count);

            // 스폰 위치 설정
            instance.transform.position = UnitSpawnList[randcount].position;

            // 초기화
            var enemydata = new EnemyUnitData();
            enemydata.EnemyIdx = enemyidx;
            enemydata.StartHp = unithp;
            enemydata.CurHp = unithp;
            enemydata.Dmg = unitdmg;
            enemydata.MoveSpeed = td.move_speed;
            enemydata.AtkSpeed = (float)td.attack_speed * 0.01f;
            enemydata.AtkRange = td.atk_range_factor;

            instance.Set(enemydata);


            GameRoot.Instance.EffectSystem.MultiPlay<EnemySpawnEffect>(instance.transform.position, (x) =>
            {
                x.SetAutoRemove(true, 1.5f);
            });

            // 활성화
            ProjectUtility.SetActiveCheck(instance.gameObject, true);
        }
    }

    public EnemyUnitBase EnemySpawnAtPosition(int enemyidx, int unitdmg, int unithp, Vector3 spawnPosition, bool isPersistent = false)
    {
        var td = Tables.Instance.GetTable<EnemyUnitInfo>().GetData(enemyidx);
        if (td == null) return null;

        var find = DeadUnits.FirstOrDefault(x => x.EnemyIdx == enemyidx);
        EnemyUnitBase instance;

        if (find != null)
        {
            instance = find;
            DeadUnits.Remove(find);
            ActiveUnits.Add(instance);
        }
        else
        {
            var handle = Addressables.InstantiateAsync(td.prefab, transform);
            var result = handle.WaitForCompletion();
            instance = result.GetComponent<EnemyUnitBase>();
            ProjectUtility.SetActiveCheck(instance.gameObject, false);
            ActiveUnits.Add(instance);
        }

        instance.transform.position = spawnPosition;

        var enemydata = new EnemyUnitData();
        enemydata.EnemyIdx = enemyidx;
        enemydata.StartHp = unithp;
        enemydata.CurHp = unithp;
        enemydata.Dmg = unitdmg;
        enemydata.MoveSpeed = td.move_speed;
        enemydata.AtkSpeed = (float)td.attack_speed * 0.01f;
        enemydata.AtkRange = td.atk_range_factor;

        instance.Set(enemydata);

        if (isPersistent)
        {
            PersistentUnits.Add(instance);
        }

        GameRoot.Instance.EffectSystem.MultiPlay<EnemySpawnEffect>(instance.transform.position, (x) =>
        {
            x.SetAutoRemove(true, 1.5f);
        });

        ProjectUtility.SetActiveCheck(instance.gameObject, true);

        return instance;
    }

    public IEnumerator SpawnWave(int stageIdx, int waveIdx)
    {
        var waveData = Tables.Instance.GetTable<WaveInfo>().GetData(
            new KeyValuePair<int, int>(stageIdx, waveIdx));

        if (waveData == null)
            yield break;

        // block_spawn_check가 1이면 EnemyBlockSpawner 활성화
        if (waveData.block_spawn_check == 1 && EnemyBlockSpawner != null)
        {
            IsEnemyBlockSpawnerActive = true;
            ProjectUtility.SetActiveCheck(EnemyBlockSpawner.gameObject, true);
        }

        // unit_idx 리스트 순회하면서 각 유닛 타입별로 스폰
        for (int i = 0; i < waveData.unit_idx.Count; i++)
        {
            int enemyIdx = waveData.unit_idx[i];
            int dmg = (i < waveData.unit_dmg.Count) ? waveData.unit_dmg[i] : 0;
            int hp = (i < waveData.unit_hp.Count) ? waveData.unit_hp[i] : 0;
            int count = (i < waveData.unit_count.Count) ? waveData.unit_count[i] : 1;
            float appearTime = (i < waveData.unit_appear_time.Count) ? waveData.unit_appear_time[i] * 0.001f : 0f;

            // 등장 시간까지 대기
            if (appearTime > 0f)
            {
                yield return new WaitForSeconds(appearTime);
            }

            // count만큼 스폰
            for (int j = 0; j < count; j++)
            {
                EnemySpawn(enemyIdx, dmg, hp);
            }
        }
    }

}

