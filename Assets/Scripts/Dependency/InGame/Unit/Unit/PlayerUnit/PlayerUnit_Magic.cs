using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class PlayerUnit_Magic : PlayerUnit   
{
    [SerializeField]
    private GameObject BulletPrefab;

    [SerializeField]
    private Transform BulletSpawnTr;

    public PrefabPool<Bullet> BulletPool = new PrefabPool<Bullet>();

    private BulletInfo BulletInfo = new BulletInfo();

    // 현재 활성화된 bullet들을 추적
    private List<Bullet> activeBullets = new List<Bullet>();

    // 쿨타임 추적 (공격 후 Idle 대기 시간)
    private float lastAttackTime = -999f;



    public override void Set(int idx, int grade, int order = 0)
    {
        base.Set(idx, grade, order);

        BulletPool.Init(BulletPrefab, transform, 4);

        BulletInfo.AttackDamage = InfoData.Damage;
        BulletInfo.AttackSpeed = InfoData.AttackSpeed;
        BulletInfo.AttackForce = 10;

        


    }




    /// <summary> 쿨타임이 끝났을 때만 Attack 상태로 전환되도록 (애니 겹침 방지)
    /// </summary>
    protected override bool CanTransitionToAttack()
    {
        return IsCooldownReady();
    }

    public override void Update()
    {
        // Attack 상태에서 타겟이 바뀌거나 넉백으로 사거리 밖으로 밀린 경우
        // Move 상태로 복귀시켜 실제 이동 로직이 재개되도록 보정한다.
        if (CurState == StateType.Attack)
        {
            bool shouldReturnToMove = false;

            if (UnitTarget != null && UnitTarget.gameObject.activeInHierarchy && UnitTarget.CurState != StateType.Dead)
            {
                float distanceToTarget = Vector3.Distance(transform.position, UnitTarget.transform.position);
                if (distanceToTarget > InfoData.AttackRange)
                {
                    shouldReturnToMove = true;
                }
            }
            else if (EnemyBlockSpawner != null && EnemyBlockSpawner.CurHp > 0 && EnemyBlockSpawner.IsSpawn)
            {
                float distanceToSpawner = Vector3.Distance(transform.position, EnemyBlockSpawner.transform.position);
                if (distanceToSpawner > InfoData.AttackRange)
                {
                    shouldReturnToMove = true;
                }
            }
            else
            {
                shouldReturnToMove = true;
            }

            if (shouldReturnToMove)
            {
                SetState(StateType.Move);
            }
        }
        // Move 상태인데 사거리 안에 있고 쿨타임 대기 중이면 Idle로 전환
        else if (CurState == StateType.Move && !IsCooldownReady())
        {
            bool inRange = false;

            if (UnitTarget != null && UnitTarget.gameObject.activeInHierarchy && UnitTarget.CurState != StateType.Dead)
            {
                float dist = Vector3.Distance(transform.position, UnitTarget.transform.position);
                inRange = dist <= InfoData.AttackRange;
            }
            else if (EnemyBlockSpawner != null && EnemyBlockSpawner.CurHp > 0 && EnemyBlockSpawner.IsSpawn)
            {
                float dist = Vector3.Distance(transform.position, EnemyBlockSpawner.transform.position);
                inRange = dist <= InfoData.AttackRange;
            }

            if (inRange)
            {
                SetState(StateType.Idle);
            }
        }

        base.Update();
    }

    public override void Attack()
    {
        // 쿨타임 전에 애니 이벤트 등으로 호출된 경우 재발사 방지
        if (!IsCooldownReady())
        {
            SetState(StateType.Idle);
            return;
        }

        var bullet = BulletPool.Get();
        if (bullet == null)
        {
            Debug.LogError("Bullet is null!");
            return;
        }

        var findtr = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.EnemyUnitGroup.FindTargetEnemy(transform);
        
        Transform targetTransform = null;
        
        if (findtr != null)
        {
            targetTransform = findtr.transform;
        }
        else
        {
            // 적이 없으면 EnemyBlockSpawner를 타겟으로 사용
            var enemyUnitGroup = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.EnemyUnitGroup;
            var enemyBlockSpawner = enemyUnitGroup.EnemyBlockSpawner;
            
            if (enemyBlockSpawner != null && enemyBlockSpawner.IsSpawn && !enemyBlockSpawner.IsDead)
            {
                targetTransform = enemyBlockSpawner.transform;
            }
        }
        
        if (targetTransform == null)
        {
            Debug.LogWarning("No enemy target found!");
            BulletPool.Return(bullet); // 사용하지 않은 bullet 반환
            SetState(StateType.Idle); // 공격 불가 시 Idle로 전환 (쿨타임 미적용 - 실제 공격 안 함)
            return;
        }

        bullet.Set(BulletInfo, BulletSpawnTr, targetTransform, ReturnBullet);
        
        // 활성화된 bullet 리스트에 추가
        activeBullets.Add(bullet);

        // 파이어볼 발사 후 Idle로 전환하여 쿨타임 대기
        SetState(StateType.Idle);
        lastAttackTime = Time.time;
        
    }

    private bool IsCooldownReady()
    {
        return Time.time >= lastAttackTime + InfoData.AttackSpeed;
    }


    public void ReturnBullet(Bullet bullet)
    {
        // 활성화된 bullet 리스트에서 제거
        activeBullets.Remove(bullet);
        BulletPool.Return(bullet);
    }

    // 게임이 reset될 때 모든 활성화된 bullet들을 초기화
    protected override void ResetUnit()
    {
        base.ResetUnit();

        lastAttackTime = -999f;

        // 현재 활성화된 모든 bullet들을 풀에 반환
        for (int i = activeBullets.Count - 1; i >= 0; i--)
        {
            if (activeBullets[i] != null)
            {
                BulletPool.Return(activeBullets[i]);
            }
        }
        
        // 리스트 초기화
        activeBullets.Clear();
    }
}

