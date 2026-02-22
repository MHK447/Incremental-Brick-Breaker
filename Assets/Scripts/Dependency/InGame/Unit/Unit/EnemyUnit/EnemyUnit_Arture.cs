using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class EnemyUnit_Arture : EnemyUnit
{
    [SerializeField]
    private GameObject BulletPrefab;

    [SerializeField]
    private Transform BulletSpawnTr;

    public PrefabPool<Bullet> BulletPool = new PrefabPool<Bullet>();

    private BulletInfo BulletInfo = new BulletInfo();

    // 현재 활성화된 bullet들을 추적
    private List<Bullet> activeBullets = new List<Bullet>();

    // 타겟 우선순위 (true: 성 공격 모드, false: 플레이어 유닛 우선)
    private bool isTargetingBlock = false;



    public override void Set(int idx, int unitdmg, int unithp, int deadexpvalue, int order = 0, float landingY = 0f)
    {
        base.Set(idx, unitdmg, unithp, deadexpvalue, order, landingY);

        BulletPool.Init(BulletPrefab, transform, 4);

        BulletInfo.AttackDamage = InfoData.Damage;
        BulletInfo.AttackSpeed = 0.5; // fixed 

        BulletInfo.AttackForce = 10;
    }




    // 성 공격 모드일 때는 플레이어 유닛을 찾지 않도록 오버라이드
    public new void FindTarget()
    {
        // 성 공격 모드일 때는 타겟을 찾지 않음
        if (isTargetingBlock)
        {
            return;
        }

        // 성 공격 모드가 아닐 때만 베이스 클래스의 FindTarget 호출
        base.FindTarget();
    }

    public override void Attack()
    {
        var bullet = BulletPool.Get();
        if (bullet == null)
        {
            Debug.LogError("Bullet is null!");
            return;
        }

        Transform targetTransform = null;

        // 성 공격 모드가 아닐 때만 플레이어 유닛을 찾음
        if (!isTargetingBlock)
        {
            // 타겟 찾기
            FindTarget();

            // PlayerUnit이 있으면 우선 공격
            if (UnitTarget != null && UnitTarget.gameObject.activeInHierarchy && UnitTarget.CurState != StateType.Dead)
            {
                // 공격 범위 안에 있는지 확인
                Vector3 currentPosXZ = new Vector3(transform.position.x, 0, transform.position.z);
                Vector3 targetPosXZ = new Vector3(UnitTarget.transform.position.x, 0, UnitTarget.transform.position.z);
                float distanceToTarget = Vector3.Distance(currentPosXZ, targetPosXZ);

                if (distanceToTarget <= InfoData.AttackRange)
                {
                    targetTransform = UnitTarget.transform;
                }
                else
                {
                    // 범위 밖이면 Move 상태로 전환
                    UnitTarget = null;
                    BulletPool.Return(bullet);
                    SetState(StateType.Move);
                    return;
                }
            }
            else
            {
                // PlayerUnit이 없거나 죽었으면 성을 타겟으로 설정하고 성 공격 모드 활성화
                UnitTarget = null;

                if (BlockTarget != null)
                {
                    // BlockTarget까지의 거리 계산 (y축 제외)
                    Vector3 currentPosXZ = new Vector3(transform.position.x, 0, transform.position.z);
                    Vector3 blockPosXZ = new Vector3(BlockTarget.transform.position.x, 0, BlockTarget.transform.position.z);
                    float distanceToBlock = Vector3.Distance(currentPosXZ, blockPosXZ);

                    // 공격 범위 안에 있을 때만 타겟으로 설정
                    if (distanceToBlock <= InfoData.AttackRange)
                    {
                        targetTransform = BlockTarget.transform;
                        isTargetingBlock = true; // 성 공격 모드 활성화
                        Debug.Log("[EnemyUnit_Arture] Now targeting block!");
                    }
                    else
                    {
                        // 범위 밖이면 Move 상태로 전환
                        BulletPool.Return(bullet);
                        SetState(StateType.Move);
                        return;
                    }
                }
                else
                {
                    // BlockTarget도 없으면 Move 상태로 전환
                    BulletPool.Return(bullet);
                    SetState(StateType.Move);
                    return;
                }
            }
        }
        else
        {
            // 성 공격 모드일 때는 성만 공격
            if (BlockTarget != null)
            {
                // BlockTarget까지의 거리 계산 (y축 제외)
                Vector3 currentPosXZ = new Vector3(transform.position.x, 0, transform.position.z);
                Vector3 blockPosXZ = new Vector3(BlockTarget.transform.position.x, 0, BlockTarget.transform.position.z);
                float distanceToBlock = Vector3.Distance(currentPosXZ, blockPosXZ);

                // 공격 범위 안에 있을 때만 타겟으로 설정
                if (distanceToBlock <= InfoData.AttackRange)
                {
                    targetTransform = BlockTarget.transform;
                }
                else
                {
                    // 범위 밖이면 Move 상태로 전환
                    BulletPool.Return(bullet);
                    SetState(StateType.Move);
                    return;
                }
            }
            else
            {
                // BlockTarget이 없으면 Move 상태로 전환
                BulletPool.Return(bullet);
                SetState(StateType.Move);
                return;
            }
        }

        if (targetTransform == null)
        {
            Debug.LogWarning("No target found or target out of range!");
            BulletPool.Return(bullet); // 사용하지 않은 bullet 반환
            SetState(StateType.Move); // Move 상태로 전환
            return;
        }
        
        bullet.Set(BulletInfo, this.transform, targetTransform, ReturnBullet);

        // 활성화된 bullet 리스트에 추가
        activeBullets.Add(bullet);
    }




    public void ReturnBullet(Bullet bullet)
    {
        // 활성화된 bullet 리스트에서 제거
        activeBullets.Remove(bullet);
        BulletPool.Return(bullet);
    }

    // 데미지를 받으면 공격자를 타겟으로 변경
    public override void Damage(double damage, PlayerUnit attacker = null)
    {
        base.Damage(damage, attacker);

        // 공격자가 있고 살아있으면 타겟으로 변경하고 성 공격 모드 해제
        if (attacker != null && attacker.gameObject.activeInHierarchy && attacker.CurState != StateType.Dead)
        {
            UnitTarget = attacker;
            isTargetingBlock = false; // 성 공격 모드 해제
            Debug.Log($"[EnemyUnit_Arture] Damaged by {attacker.name}, switching target from block to attacker!");
        }
    }

    // 게임이 reset될 때 모든 활성화된 bullet들을 초기화
    protected override void ResetUnit()
    {
        base.ResetUnit();

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

        // 타겟 모드 초기화
        isTargetingBlock = false;
    }

    // Update 메서드: 베이스 클래스의 Update를 대체하고 필요한 로직 추가
    void Update()
    {
        // 리플렉션을 사용하여 베이스 클래스의 private 필드에 접근
        var baseType = typeof(EnemyUnit);
        var isSpawningField = baseType.GetField("isSpawning", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var isGroundedField = baseType.GetField("isGrounded", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var directionField = baseType.GetField("Direction", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var groundYField = baseType.GetField("groundY", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        bool isSpawning = (bool)(isSpawningField?.GetValue(this) ?? false);
        bool isGrounded = (bool)(isGroundedField?.GetValue(this) ?? false);
        Vector3 direction = (Vector3)(directionField?.GetValue(this) ?? Vector3.zero);
        float groundY = (float)(groundYField?.GetValue(this) ?? 0f);

        // 스폰 중일 때는 중력 적용 및 착지 체크
        if (isSpawning && !isGrounded)
        {
            // 중력 적용
            direction.y += -9.8f * Time.deltaTime;
            directionField?.SetValue(this, direction);

            transform.position += direction * Time.deltaTime;

            // 착지 체크
            float currentY = transform.position.y;
            if (currentY <= groundY && direction.y <= 0)
            {
                isGrounded = true;
                isSpawning = false;
                isGroundedField?.SetValue(this, true);
                isSpawningField?.SetValue(this, false);

                // 착지 후 수평 이동으로 전환
                direction = new Vector3(-1f, 0f, 0f);
                directionField?.SetValue(this, direction);

                // y 위치 보정
                transform.position = new Vector3(transform.position.x, groundY, transform.position.z);

                // 착지 후 가까운 플레이어 유닛을 타겟으로 설정
                FindTarget();

                // StartAnimName 초기화 후 Move 상태로 전환
                StartAnimName = "";
                SetState(StateType.Move);
            }
        }
        else
        {
            // 공격 중일 때 타겟이 죽었는지 또는 범위 밖인지 확인
            if (CurState == StateType.Attack)
            {
                // 성 공격 모드가 아닐 때만 플레이어 유닛 확인
                if (!isTargetingBlock)
                {
                    bool shouldSwitchToMove = false;

                    if (UnitTarget != null)
                    {
                        // 타겟이 죽었거나 비활성화되었는지 확인
                        if (!UnitTarget.gameObject.activeInHierarchy || UnitTarget.CurState == StateType.Dead)
                        {
                            shouldSwitchToMove = true;
                        }
                        else
                        {
                            // 타겟이 살아있으면 범위 안에 있는지 확인
                            Vector3 currentPosXZ = new Vector3(transform.position.x, 0, transform.position.z);
                            Vector3 targetPosXZ = new Vector3(UnitTarget.transform.position.x, 0, UnitTarget.transform.position.z);
                            float distanceToTarget = Vector3.Distance(currentPosXZ, targetPosXZ);

                            if (distanceToTarget > InfoData.AttackRange)
                            {
                                // 범위 밖이면 Move 상태로 전환
                                shouldSwitchToMove = true;
                            }
                        }

                        if (shouldSwitchToMove)
                        {
                            // 범위 내에 다른 플레이어 유닛이 있는지 확인
                            PlayerUnit newTarget = FindPlayerUnitInRange();

                            if (newTarget != null)
                            {
                                // 범위 내에 플레이어 유닛이 있으면 타겟만 교체하고 공격 상태 유지
                                UnitTarget = newTarget;
                            }
                            else
                            {
                                // 범위 내에 플레이어 유닛이 없으면 Move 상태로 전환
                                UnitTarget = null;
                                if (!BlockRangeTarget())
                                {
                                    SetState(StateType.Move);
                                }
                            }
                        }
                    }
                    else
                    {
                        // 타겟이 없으면 범위 내에 플레이어 유닛이 있는지 확인
                        PlayerUnit newTarget = FindPlayerUnitInRange();

                        if (newTarget == null)
                        {
                            if (!BlockRangeTarget())
                            {
                                SetState(StateType.Move);
                            }
                        }
                        else
                        {
                            // 범위 내에 플레이어 유닛이 있으면 타겟으로 설정
                            UnitTarget = newTarget;
                        }
                    }
                }
            }

            // 착지 후에는 정상 이동
            Move();
        }
    }

    // 공격 범위 내에서 플레이어 유닛을 찾는 메서드
    private PlayerUnit FindPlayerUnitInRange()
    {
        var activePlayerUnits = BaseStage.PlayerUnitGroup.ActiveUnits.ToArray();

        if (activePlayerUnits.Length > 0)
        {
            // 공격 범위 내에서 가장 가까운 플레이어 유닛 찾기
            PlayerUnit closestUnit = null;
            float closestDistance = float.MaxValue;

            foreach (var unit in activePlayerUnits)
            {
                if (unit.gameObject.activeInHierarchy && unit.CurState != StateType.Dead)
                {
                    // y축 제외한 거리 계산
                    Vector3 currentPosXZ = new Vector3(transform.position.x, 0, transform.position.z);
                    Vector3 targetPosXZ = new Vector3(unit.transform.position.x, 0, unit.transform.position.z);
                    float distance = Vector3.Distance(currentPosXZ, targetPosXZ);

                    if (distance <= InfoData.AttackRange && distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestUnit = unit;
                    }
                }
            }

            return closestUnit;
        }

        return null;
    }
}

