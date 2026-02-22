using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BanpoFri;
using UnityEngine.AddressableAssets;
using DG.Tweening;
using System.Linq;

public class EnemyUnitGroup : MonoBehaviour
{
    public HashSet<EnemyUnit> ActiveUnits = new HashSet<EnemyUnit>();
    public HashSet<EnemyUnit> DeadUnits = new HashSet<EnemyUnit>();

    [SerializeField]
    private List<Transform> UnitSpawnList = new List<Transform>();

    public EnemyBlockSpawner EnemyBlockSpawner;

    [HideInInspector]
    public bool IsEnemyBlockSpawnerActive = false;

    public bool IsAllDeadCheck
    {
        get
        {
            // 일반 적 유닛이 모두 죽었는지 확인
            if (ActiveUnits.Count > 0) return false;

            // EnemyBlockSpawner가 활성화되어 있고 아직 살아있으면 false
            if (IsEnemyBlockSpawnerActive && EnemyBlockSpawner != null && !EnemyBlockSpawner.IsDead)
                return false;

            return true;
        }
    }

    private int SpawnOrder = 0;
    private bool isWinSequenceRunning = false;

    private void ReleaseInstance(GameObject target)
    {
        if (target == null)
            return;

        if (!Addressables.ReleaseInstance(target))
        {
            Destroy(target);
        }
    }

    private void ReleaseEnemyUnit(EnemyUnit unit)
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
        isWinSequenceRunning = false;


    }



    public void AddUnit(int enemyidx, int deadexpvalue, int unitdmg, int unithp)
    {
        var td = Tables.Instance.GetTable<EnemyInfo>().GetData(enemyidx);

        if (td != null)
        {
            // DeadUnits에서 같은 enemyidx를 가진 유닛을 찾아 재활용
            var find = DeadUnits.FirstOrDefault(x => x.EnemyIdx == enemyidx);

            EnemyUnit instance;

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
                instance = result.GetComponent<EnemyUnit>();

                ActiveUnits.Add(instance);
            }

            if (td.boss_unit == 1)
            {
                ProjectUtility.SetActiveCheck(GameRoot.Instance.UISystem.GetUI<PopupInGame>().DifficultyRoot, true);
            }

            Vector3 landingPos;
            Vector3 spawnPos;
            float landingY = 0f;

            if (IsEnemyBlockSpawnerActive && EnemyBlockSpawner != null && EnemyBlockSpawner.UnitSpawner != null)
            {
                // EnemyBlockSpawner가 활성화된 경우 GetUnitSpawnerPoint로 순차적으로 스폰 포인트 가져오기
                Transform spawnPoint = EnemyBlockSpawner.GetUnitSpawnerPoint();
                if (spawnPoint == null)
                {
                    spawnPoint = transform;
                }
                var randx = Random.Range(-0.3f, 0.3f);
                var randy = Random.Range(-0.3f, 0.3f);

                landingPos = new Vector3(spawnPoint.position.x + randx, spawnPoint.position.y + randy, spawnPoint.position.z);
                spawnPos = landingPos;
                landingY = landingPos.y;

            }
            else
            {
                if (UnitSpawnList != null && UnitSpawnList.Count > 0)
                {
                    // 착지 위치는 UnitSpawnList의 원래 위치에 SpawnOrder에 따른 간격 추가
                    landingPos = UnitSpawnList[SpawnOrder].position;
                    landingPos.x += SpawnOrder * 1.0f; // x축으로 1씩 간격 추가
                }
                else
                {
                    // 비정상 데이터 방어: 스폰 포인트가 없으면 그룹 위치를 기본값으로 사용
                    landingPos = transform.position;
                }
                landingY = landingPos.y;

                // 스폰 위치는 착지 위치에서 공중으로 올림 (SpawnOrder에 따라 0.5~1f씩 차이)
                spawnPos = landingPos;
                spawnPos.y += 2f + (SpawnOrder * 0.5f); // 공중에서 시작 (0.5f씩 차이)
            }


            instance.transform.position = spawnPos;

            // Set 호출 시 SpawnOrder와 착지 y값, 그리고 WaveInfo의 dmg, hp 전달 (활성화 전에 초기화)
            instance.Set(enemyidx, unitdmg, unithp, deadexpvalue, SpawnOrder, landingY);


            // 초기화 완료 후 활성화
            ProjectUtility.SetActiveCheck(instance.gameObject, true);

            SpawnOrder++;

            if (UnitSpawnList != null && UnitSpawnList.Count > 0 && SpawnOrder >= UnitSpawnList.Count)
            {
                SpawnOrder = 0;
            }
        }
    }

    public void DeleteUnit(EnemyUnit unit)
    {
        if (unit == null)
            return;

        if (ActiveUnits.Contains(unit))
        {
            ActiveUnits.Remove(unit);
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
            var playerUnitGroup = stage.PlayerUnitGroup;
            if (playerUnitGroup == null)
                yield break;

            playerUnitGroup.IsWinAnimationPlaying = true;

            // 플레이어 유닛 리스트 가져오기
            var untlist = playerUnitGroup.ActiveUnits;

            // 승리 연출 시작 시 모든 플레이어 유닛의 HP Progress 비활성화
            foreach (var unit in untlist)
            {
                if (unit != null)
                {
                    unit.HideHpProgress();
                }
            }


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
        var playerUnitGroup = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.PlayerUnitGroup;
        var untlist = playerUnitGroup.ActiveUnits;
        // 모든 플레이어 유닛 삭제 (역순으로 안전하게 삭제)
        for (int i = playerUnitGroup.ActiveUnits.Count - 1; i >= 0; i--)
        {
            GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.PlayerUnitGroup.DeleteUnit(untlist[i]);
        }

        // 웨이브 휴식 상태로 전환
        GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.StartRest();
    }

    public EnemyUnit FindTargetEnemy(Transform closetroottr, float attackrange = -1)
    {
        EnemyUnit closestEnemy = null;
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
        EnemyBlockSpawner.StopEnemyAllDieSpawn();

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

        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;

        var waveidx = GameRoot.Instance.UserData.Waveidx.Value;

        var wavetd = Tables.Instance.GetTable<WaveInfo>().GetData(new KeyValuePair<int, int>(stageidx, waveidx));

        if (wavetd != null)
        {
            var stagetd = Tables.Instance.GetTable<StageInfo>().GetData(stageidx);

            if (stagetd != null)
            {
                if (stagetd.ingame_map_idx > 0)
                {
                    EnemyBlockSpawner.Set(stagetd.ingame_map_idx, wavetd.devil_check > 0);
                    GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.PlayerBlockGroup.SetMapImg(wavetd.devil_check > 0);
                    ProjectUtility.SetActiveCheck(EnemyBlockSpawner.gameObject, true);
                    IsEnemyBlockSpawnerActive = true;
                }
            }
        }
    }



    public void ClearData()
    {
        isWinSequenceRunning = false;

        var unitsToRelease = new HashSet<EnemyUnit>();

        foreach (var block in ActiveUnits)
            unitsToRelease.Add(block);

        foreach (var block in DeadUnits)
            unitsToRelease.Add(block);

        // 컬렉션 참조가 끊긴 잔존 유닛까지 정리
        var allUnits = GetComponentsInChildren<EnemyUnit>(true);
        foreach (var unit in allUnits)
            unitsToRelease.Add(unit);

        foreach (var unit in unitsToRelease)
            ReleaseEnemyUnit(unit);

        ActiveUnits.Clear();
        DeadUnits.Clear();

        EnemyBlockSpawner.ClearData();
    }


    public void EnemySpawn(int enemyidx, int unitdmg, int unithp)
    {
        var td = Tables.Instance.GetTable<EnemyInfo>().GetData(enemyidx);
        if (td != null)
        {
            // DeadUnits에서 같은 enemyidx를 가진 유닛을 찾아 재활용
            var find = DeadUnits.FirstOrDefault(x => x.EnemyIdx == enemyidx);

            EnemyUnit instance;

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
                instance = result.GetComponent<EnemyUnit>();

                ActiveUnits.Add(instance);
            }

            Transform randspawntr = null;
            if (EnemyBlockSpawner != null && EnemyBlockSpawner.GetUnitSpawnerPointList != null && EnemyBlockSpawner.GetUnitSpawnerPointList.Count > 0)
            {
                randspawntr = EnemyBlockSpawner.GetUnitSpawnerPointList[Random.Range(0, EnemyBlockSpawner.GetUnitSpawnerPointList.Count)];
            }
            else if (UnitSpawnList != null && UnitSpawnList.Count > 0)
            {
                randspawntr = UnitSpawnList[Random.Range(0, UnitSpawnList.Count)];
            }
            else
            {
                randspawntr = transform;
            }

            // 스폰 위치 설정
            instance.transform.position = randspawntr.position;

            // 초기화 (deadexpvalue는 0으로 설정)
            instance.Set(enemyidx, unitdmg, unithp, 0, 0, randspawntr.position.y);


            GameRoot.Instance.EffectSystem.MultiPlay<EnemySpawnEffect>(instance.transform.position, (x) =>
            {   
                x.SetAutoRemove(true, 1.5f);
            });

            // 활성화
            ProjectUtility.SetActiveCheck(instance.gameObject, true);
        }
    }

    // 사용되지 않는 메서드 - EnemySystem이 존재하지 않음
    // public EnemyUnit GetEnemy(int enemyidx)
    // {
    //     return GameRoot.Instance.EnemySystem.GetEnemy(enemyidx);
    // }
}
