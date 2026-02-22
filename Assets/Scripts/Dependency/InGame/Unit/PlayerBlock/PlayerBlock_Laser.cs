using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class PlayerBlock_Laser : PlayerBlock
{
    private struct LaserCallback
    {
        public readonly bool isValid { get => hit && enemy != null; }
        public RaycastHit2D hit;
        public EnemyUnit enemy;
    }
    private bool DoTarget = false;

    [SerializeField]
    private GameObject LaserRoot;
    [SerializeField]
    private Transform LaserTransform;
    [SerializeField]
    private GameObject LaserEnd;
    [SerializeField]
    private SpriteRenderer[] LaserSpriteRenderers;

    [SerializeField]
    private GameObject[] NormalObjs;
    [SerializeField]
    private GameObject[] UniqueObjs;

    [SerializeField]
    private SpriteRenderer WeaponImg;

    private EnemyUnit CurrentTarget;
    private Vector3 CurrentOffset;
    private List<LaserCallback> callbacks = new();

    // 적의 중심점 오프셋 (적의 실제 중심점이 아닌 타겟 위치)
    public Vector3 EnemyCenterOffset = new Vector3(-0.17f, 0.2f, 0f);

    private float cooldownTimer;
    private Collider2D currentCollider;
    private float lastAngle = 0f; // 마지막 회전 각도 저장


    private EnemyBlockSpawner EnemyBlockSpawner;

    // private Stopwatch stopwatch;

    protected override void PreInitialize()
    {
        foreach (var obj in NormalObjs) ProjectUtility.SetActiveCheck(obj, true);
        foreach (var obj in UniqueObjs) ProjectUtility.SetActiveCheck(obj, false);

        EnemyBlockSpawner = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.EnemyUnitGroup.EnemyBlockSpawner;

        //AttackSoundEffectKey = "effect_player_attack_laser";
        CurrentTarget = null;
        CheckTarget();
        CurrentOffset = EnemyCenterOffset;
        DoTarget = false;
        currentCollider = null;
        cooldownTimer = 0;
        lastAngle = 0f;
    }

    public override void Set(int blockidx, int order, int grade, PlayerBlockGroup parentGroup)
    {
        base.Set(blockidx, order, grade, parentGroup);

        WeaponImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_InGame, $"InGameWeapon_{blockidx}_{grade}");
    }

    protected override void Update()
    {
        base.Update();
        // 적이 죽으면 바로 다음 적 찾기
        if (CurrentTarget != null && !CheckTarget())
        {
            // 타겟이 죽었거나 null이면 즉시 다음 적 찾기
            if (DoTarget)
            {
                FindTarget(); // DoTarget 모드에서는 FindTarget 호출
            }
            // ContinuousLaser는 아래에서 호출되므로 여기서는 호출하지 않음
        }
        if (DoTarget) FollowLaser();
        else ContinuousLaser();
    }

    protected override void Attack()
    {
        if (!DoTarget)
        {
            if (!CheckTarget()) return;
            // BlockData.Damage를 사용하여 초당 데미지 조절
            // Cooldown을 고려한 초당 데미지 = Damage / Cooldown
            // 하지만 Attack()은 쿨다운마다 호출되므로 Damage를 그대로 사용
            if (CurrentTarget != null && !CurrentTarget.IsDead)
            {
                CurrentTarget.Damage((float)BlockData.Damage);
            }
            else if (EnemyBlockSpawner != null && !EnemyBlockSpawner.IsDead && EnemyBlockSpawner.IsSpawn)
            {
                int blockDamage = Mathf.Max(1, (int)BlockData.Damage);
                EnemyBlockSpawner.Damage(blockDamage);
            }
            return;
        }
        FindTarget();
        if (!CheckTarget()) return;
        // BlockData.Damage를 사용하여 초당 데미지 조절
        if (CurrentTarget != null && !CurrentTarget.IsDead)
        {
            CurrentTarget.Damage((float)BlockData.Damage);
        }
        else if (EnemyBlockSpawner != null && !EnemyBlockSpawner.IsDead && EnemyBlockSpawner.IsSpawn)
        {
            int blockDamage = Mathf.Max(1, (int)BlockData.Damage);
            EnemyBlockSpawner.Damage(blockDamage);
        }
    }

    private void ContinuousLaser()
    {
        if (DoTarget) return;

        CurrentTarget = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.EnemyUnitGroup.FindTargetEnemy(transform, BlockData.AttackRange);

        if (!CheckTarget() || GameRoot.Instance.UserData.Playerdata.IsWaveRestProperty.Value)
        {
            ProjectUtility.SetActiveCheck(LaserRoot, false);
            currentCollider = null;
            return;
        }

        ProjectUtility.SetActiveCheck(LaserRoot, true);
        Vector3 targetPosition;
        if (CurrentTarget != null && !CurrentTarget.IsDead)
        {
            targetPosition = CurrentTarget.transform.position + CurrentOffset;
        }
        else if (EnemyBlockSpawner != null && !EnemyBlockSpawner.IsDead && EnemyBlockSpawner.IsSpawn)
        {
            targetPosition = EnemyBlockSpawner.transform.position;
        }
        else
        {
            ProjectUtility.SetActiveCheck(LaserRoot, false);
            return;
        }
        SetLaser(barrelTransform.position, targetPosition);
    }

    override protected void UpdateCooldownVisualizer()
    {
    }

    private void FindTarget()
    {
        if (!DoTarget) return;

        // y축 범위를 크게 설정하여 모든 적을 감지
        CurrentTarget = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.EnemyUnitGroup.FindTargetEnemy(transform, BlockData.AttackRange);
        CheckTarget();

        callbacks.Clear();
    }


    private bool CheckTarget()
    {
        if (CurrentTarget == null || CurrentTarget.IsDead)
        {
            // 적이 없으면 EnemyBlockSpawner를 타겟으로 사용
            if (EnemyBlockSpawner != null && !EnemyBlockSpawner.IsDead && EnemyBlockSpawner.IsSpawn)
            {
                CurrentOffset = Vector3.zero; // EnemyBlockSpawner는 오프셋 불필요
                return true;
            }
            CurrentOffset = EnemyCenterOffset;
            CurrentTarget = null;
            currentCollider = null;
            return false;
        }
        // 타겟이 유효할 때 오프셋 적용
        CurrentOffset = EnemyCenterOffset;
        return true;
    }

    private void FollowLaser()
    {
        bool checkTarget = CheckTarget();
        ProjectUtility.SetActiveCheck(LaserRoot, checkTarget);
        if (!checkTarget) return;
        
        Vector3 targetPosition;
        if (CurrentTarget != null && !CurrentTarget.IsDead)
        {
            targetPosition = CurrentTarget.transform.position + CurrentOffset;
        }
        else if (EnemyBlockSpawner != null && !EnemyBlockSpawner.IsDead && EnemyBlockSpawner.IsSpawn)
        {
            targetPosition = EnemyBlockSpawner.transform.position;
        }
        else
        {
            ProjectUtility.SetActiveCheck(LaserRoot, false);
            return;
        }
        SetLaser(barrelTransform.position, targetPosition);
    }

    private void SetLaser(Vector3 p1, Vector3 p2)
    {
        Vector3 dir = (p2 - p1);
        float length = dir.magnitude;

        // 최소 거리 체크 - 너무 가까우면 레이저 비활성화
        const float minDistance = 1.25f;
        if (length < minDistance)
        {
            ProjectUtility.SetActiveCheck(LaserTransform.gameObject, false);
            ProjectUtility.SetActiveCheck(LaserEnd, false);
            return;
        }
        else if (!LaserTransform.gameObject.activeSelf)
        {
            ProjectUtility.SetActiveCheck(LaserTransform.gameObject, true);
            ProjectUtility.SetActiveCheck(LaserEnd, true);
        }

        // 가까운 거리에서도 안정적인 계산을 위해 최소 거리 보장
        Vector3 dirNormalized = dir.normalized;
        Vector3 actualP2 = p2;
        if (length < 1.0f)
        {
            // 최소 거리만큼 떨어진 위치로 조정
            actualP2 = p1 + dirNormalized * Mathf.Max(length, minDistance);
            dir = actualP2 - p1;
            length = dir.magnitude;
            // dir이 변경되었으므로 dirNormalized도 다시 계산
            dirNormalized = dir.normalized;
        }

        // 가까운 거리에서는 마지막 회전으로 유지
        const float noRotationDistance = 3.0f;
        float angle;
        Quaternion rotation;

        if (length < noRotationDistance)
        {
            // 거리가 가까우면 마지막 회전 각도 유지
            angle = lastAngle;
            rotation = Quaternion.Euler(0, 0, angle);
        }
        else
        {
            // 정규화된 벡터를 사용하여 각도 계산 (더 안정적)
            angle = Mathf.Atan2(dirNormalized.y, dirNormalized.x) * Mathf.Rad2Deg;
            rotation = Quaternion.Euler(0, 0, angle);
            // 새로운 각도를 저장
            lastAngle = angle;
        }

        // 오프셋을 레이저 회전 각도에 맞춰 회전시킴
        // 가까운 거리에서는 오프셋을 줄여서 안정성 확보
        float offsetScale = Mathf.Clamp01(length / 2.0f); // 거리가 가까우면 오프셋 축소
        Vector3 offset = new Vector3(1.2f * offsetScale, -0.1f, 0);
        Vector3 rotatedOffset = rotation * offset;

        LaserTransform.position = ((p1 + actualP2) * 0.5f) - rotatedOffset;
        LaserTransform.rotation = rotation;

        // LaserImg를 적 타겟 방향으로 회전s
        if (WeaponImg != null)
        {
            WeaponImg.transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        foreach (var sprite in LaserSpriteRenderers)
        {
            sprite.size = new(length, DoTarget ? 0.3f : 0.2f);
        }

        // LaserEnd를 LaserSpriteRenderers의 끝 위치로 설정
        LaserEnd.transform.position = LaserTransform.position + dirNormalized * (length * 0.5f);
        LaserEnd.transform.rotation = Quaternion.Euler(0, 0, angle);
    }


    override public void SetBlockInfo()
    {
        base.SetBlockInfo();
        BlockData.Damage = BlockData.Damage * 0.1f;
    }


}

