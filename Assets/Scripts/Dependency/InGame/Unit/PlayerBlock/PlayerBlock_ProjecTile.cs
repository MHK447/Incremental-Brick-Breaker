using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerBlock_Projectile : PlayerBlock
{
    protected int ShootCount = 1;
    [SerializeField] private GameObject BulletPrefab;
    public PrefabPool<Bullet> BulletPool = new();

    [SerializeField]
    protected SpriteRenderer WeaponImg;

    private Transform weaponTransform;


    public BulletInfo BulletInfo = new BulletInfo();


    protected EnemyBlockSpawner EnemyBlockSpawner;


    private EnemyUnit Target = null;
    protected List<Bullet> ActiveBullets = new List<Bullet>();



    public override void Set(int blockidx, int order, int grade, PlayerBlockGroup parentGroup)
    {
        base.Set(blockidx, order, grade, parentGroup);

        weaponTransform = WeaponImg.transform;
        WeaponImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_InGame, $"InGameWeapon_{blockidx}_{grade}");

        // 초기 scale 설정
        weaponTransform.localScale = Vector3.zero;

        BulletInfoInit();

        // 중복 구독 방지: 기존 구독 해제 후 재구독
        GameRoot.Instance.InGameUpgradeSystem.StateRefreshEvent -= BulletInfoInit;
        GameRoot.Instance.InGameUpgradeSystem.StateRefreshEvent += BulletInfoInit;

        EnemyBlockSpawner = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.EnemyUnitGroup.EnemyBlockSpawner;
    }

    private bool IsColorEffect = false;

    override public void DamageColorEffect()
    {
        base.DamageColorEffect();

        if (!IsColorEffect)
        {
            WeaponImg.EnableHitEffect();

            GameRoot.Instance.WaitTimeAndCallback(0.15f, () =>
            {
                if (this != null)
                {
                    // 효과 종료 후 원래 머티리얼로 복귀

                    WeaponImg.DisableHitEffect();

                    IsColorEffect = false;

                }
            });
        }
    }



    protected override void Update()
    {
        var root = GameRoot.GetInstance();
        if (root == null || root.UserData == null)
        {
            return; // 게임 종료 중이거나 루트가 파괴된 경우
        }

        base.Update();

    // Target이 null이거나 타겟이 유효하지 않으면 새로운 타겟 찾기
        // EnemyBlockSpawner만 있을 때도 계속 EnemyUnit을 찾도록 함
        if (Target == null || !IsValidTarget())
        {
            FindTarget();
        }

        UpdateWeaponScale();
        UpdateWeaponRotation();
    }



    protected virtual void UpdateWeaponScale()
    {
        if (weaponTransform == null || BlockData.Cooldown <= 0) return;

        // 타겟이 없으면 scale을 1로 유지
        if (!IsValidTarget() || GameRoot.Instance.UserData.Playerdata.IsWaveRestProperty.Value)
        {
            weaponTransform.localScale = Vector3.one;
            return;
        }

        // 쿨타임 진행도 계산 (0 = 쿨타임 시작, 1 = 쿨타임 완료)
        float cooldownProgress = 1f - (BlockData.RemainingCooldown / BlockData.Cooldown);
        cooldownProgress = Mathf.Clamp01(cooldownProgress);

        // scale을 0에서 1까지 변화
        weaponTransform.localScale = Vector3.one * cooldownProgress;
    }

    private void UpdateWeaponRotation()
    {
        if (weaponTransform == null) return;

        // 타겟 transform 가져오기
        Transform targetTransform = GetTargetTransform();
        
        // 타겟이 없으면 정면(오른쪽)을 바라보도록 설정
        if (targetTransform == null)
        {
            weaponTransform.rotation = Quaternion.Euler(0, 0, 0);
            return;
        }

        // 타겟 방향 계산
        Vector3 direction = targetTransform.position - weaponTransform.position;

        // 2D에서 회전 각도 계산 (z축 기준)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // WeaponImg가 타겟을 바라보도록 회전
        weaponTransform.rotation = Quaternion.Euler(0, 0, angle);
    }

    protected override void OnSpawn()
    {
        BulletPool.Init(BulletPrefab, null, 4);
        base.OnSpawn();

        ShootCount = 1;
        ActiveBullets.Clear();
    }

    public override void Clear()
    {
        ClearAllActiveBullets();
        BulletPool.Clear();

        // 이벤트 구독 해제
        GameRoot.Instance.InGameUpgradeSystem.StateRefreshEvent -= BulletInfoInit;

        // 게임이 끝날 때 무기 scale을 1로 설정
        if (weaponTransform != null)
        {
            weaponTransform.localScale = Vector3.one;
        }
    }

    public override void Dead()
    {
        // 이벤트 구독 해제
        GameRoot.Instance.InGameUpgradeSystem.StateRefreshEvent -= BulletInfoInit;
        
        // 부모 클래스의 Dead() 호출
        base.Dead();
    }

    protected override void OnAttackStarted()
    {
        base.OnAttackStarted();
        // 공격 시작 시 scale을 0으로 리셋
        if (weaponTransform != null)
        {
            weaponTransform.localScale = Vector3.zero;
        }
    }

    protected override void Attack()
    {
        base.Attack();
        StartCoroutine(FireRoutine(Vector3.right));
    }

    protected IEnumerator FireRoutine(Vector3 direction)
    {
        var root = GameRoot.GetInstance();
        if (root == null || root.InGameUpgradeSystem == null || root.EffectSystem == null)
        {
            yield break; // 게임 종료 중이면 더 이상 처리하지 않음
        }

        // 오브젝트/배럴 파괴 시 코루틴 종료
        if (this == null || barrelTransform == null)
            yield break;

        int doubleshotcheck = 0;


        var findmodifier = root.InGameUpgradeSystem.GetModifier(Config.InGameUpgradeChoice.DoubleShot);

        if (findmodifier != null)
        {
            if (ProjectUtility.IsPercentSuccess(findmodifier.Value_2))
            {
                doubleshotcheck = 1;
                var spawnPos = barrelTransform != null ? barrelTransform.position : transform.position;
                root.EffectSystem.Play<DoubleShotEffect>(spawnPos, x =>
                {
                    if (x != null)
                        x.SetAutoRemove(true, 2f);
                });
            }
        }


        for (int i = 0; i < ShootCount + doubleshotcheck; i++)
        {
            if (this == null || barrelTransform == null || IsDead)
                yield break;
            FireBullet(barrelTransform.position, direction);
            yield return new WaitForSeconds(0.1f);
        }

        if (this != null)
            FindTarget();
    }

    protected virtual void FireBullet(Vector3 position, Vector3 direction)
    {
        var root = GameRoot.GetInstance();
        if (root == null || root.UserData == null)
        {
            return; // 게임 종료 중
        }

        if (IsDead || this == null || barrelTransform == null) return;

        // IsWaveRestProperty가 true일 때 활성화된 bullet 모두 삭제
        if (root.UserData.Playerdata.IsWaveRestProperty.Value)
        {
            ClearAllActiveBullets();
            return;
        }

        // 발사 시점에 가장 가까운 적 찾기
        Transform targetTransform = GetTargetTransformForFire();
        if (targetTransform == null)
        {
            // 적이 없으면 현재 활성화된 모든 bullet 처리
            ClearAllActiveBullets();
            return;
        }

        Bullet instance = BulletPool.Get();
        if (instance == null) return;

        instance.Set(BulletInfo, barrelTransform, targetTransform, OnBulletHit);
        if (WeaponImg != null)
            instance.SetBulletImg(WeaponImg.sprite);

        SoundPlayer.Instance.PlaySound("weapon_shoot");

        // 활성화된 bullet 리스트에 추가
        ActiveBullets.Add(instance);
    }

    protected virtual void OnBulletHit(Bullet bullet)
    {
        ActiveBullets.Remove(bullet);
        BulletPool.Return(bullet);
    }

    protected void ClearAllActiveBullets()
    {
        // 현재 활성화된 모든 bullet에 대해 OnBulletHit 호출
        for (int i = ActiveBullets.Count - 1; i >= 0; i--)
        {
            if (ActiveBullets[i] != null)
            {
                OnBulletHit(ActiveBullets[i]);
            }
        }
        ActiveBullets.Clear();

        // Target을 null로 리셋하여 다음 웨이브에서 새로운 타겟을 찾도록 함
        Target = null;

        // 휴식 중일 때 무기 scale과 rotation을 초기화
        if (weaponTransform != null)
        {
            weaponTransform.localScale = Vector3.one;
            weaponTransform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }



    public void FindTarget()
    {
        if (!TryGetEnemyGroup(out var enemyGroup))
        {
            Target = null;
            return;
        }

        Target = enemyGroup.FindTargetEnemy(transform, BlockData.AttackRange);
        
        // 적이 없거나 다 죽었고, EnemyBlockSpawner가 활성화되어 있고 죽지 않았으면 EnemyBlockSpawner를 타겟으로 설정
        // Target이 null이고 EnemyBlockSpawner만 있을 때도 Update()에서 계속 FindTarget()을 호출하여 새로운 EnemyUnit을 찾도록 함
        if (Target == null && EnemyBlockSpawner != null && EnemyBlockSpawner.IsSpawn && !EnemyBlockSpawner.IsDead)
        {
            // Target은 null로 유지하되, EnemyBlockSpawner를 타겟으로 사용
            // Update()에서 Target == null 조건으로 인해 계속 FindTarget()이 호출됨
        }
    }

    // 타겟이 유효한지 확인하는 메서드
    private bool IsValidTarget()
    {
        // EnemyUnit 타겟이 유효한 경우
        if (Target != null && !Target.IsDead && Target.gameObject.activeInHierarchy)
        {
            return true;
        }
        
        // EnemyBlockSpawner가 활성화되어 있고 죽지 않은 경우
        if (EnemyBlockSpawner != null && EnemyBlockSpawner.IsSpawn && !EnemyBlockSpawner.IsDead)
        {
            return true;
        }
        
        return false;
    }

    // 현재 타겟의 Transform을 반환하는 메서드
    private Transform GetTargetTransform()
    {
        if (!TryGetEnemyGroup(out _))
        {
            return null;
        }

        // EnemyUnit 타겟이 유효한 경우
        if (Target != null && !Target.IsDead && Target.gameObject.activeInHierarchy)
        {
            return Target.transform;
        }
        
        // EnemyBlockSpawner가 활성화되어 있고 죽지 않은 경우
        if (EnemyBlockSpawner != null && EnemyBlockSpawner.IsSpawn && !EnemyBlockSpawner.IsDead)
        {
            return EnemyBlockSpawner.transform;
        }
        
        return null;
    }

    // 발사 시점에 가장 가까운 적의 Transform을 반환하는 메서드
    protected Transform GetTargetTransformForFire()
    {
        if (!TryGetEnemyGroup(out var enemyGroup))
        {
            return null;
        }

        // 발사 시점에 가장 가까운 적 찾기
        var nearestEnemy = enemyGroup.FindTargetEnemy(transform, BlockData.AttackRange);
        
        // 가장 가까운 적이 있으면 그것을 반환
        if (nearestEnemy != null && !nearestEnemy.IsDead && nearestEnemy.gameObject.activeInHierarchy)
        {
            return nearestEnemy.transform;
        }
        
        // 적이 없을 때만 EnemyBlockSpawner 반환
        if (EnemyBlockSpawner != null && EnemyBlockSpawner.IsSpawn && !EnemyBlockSpawner.IsDead)
        {
            return EnemyBlockSpawner.transform;
        }
        
        return null;
    }

    private bool TryGetEnemyGroup(out EnemyUnitGroup enemyGroup)
    {
        enemyGroup = null;

        var root = GameRoot.GetInstance();
        if (root == null || root.InGameSystem == null) return false;

        var stage = root.InGameSystem.GetInGame<InGameBase>()?.Stage;
        if (stage == null || stage.EnemyUnitGroup == null) return false;

        enemyGroup = stage.EnemyUnitGroup;
        return true;
    }


    public override void SetBlockInfo()
    {
        base.SetBlockInfo();


        var findmodifier = GameRoot.Instance.InGameUpgradeSystem.GetModifier(Config.InGameUpgradeChoice.SpeedBullet);

        if (findmodifier != null)
        {
            var buffpercent = ProjectUtility.PercentCalc(BlockData.Cooldown, findmodifier.Value_1);

            BlockData.Cooldown = BlockData.Cooldown - (float)buffpercent;
        }
    }


    public virtual void BulletInfoInit()
    {
        BulletInfo.AttackDamage = BlockData.Damage;
        BulletInfo.AttackSpeed = 12f;

        // IronHornSpear 업그레이드 적용 (관통 횟수)
        var penetrationModifier = GameRoot.Instance.InGameUpgradeSystem.GetModifier(Config.InGameUpgradeChoice.IronHornSpear);
        if (penetrationModifier != null)
        {
            // Value_2 확률로 성공하면 Value_1(관통 횟수) 설정, 실패하면 0
            BulletInfo.PenetrationCount = ProjectUtility.IsPercentSuccess((float)penetrationModifier.Value_2) ?
                (int)penetrationModifier.Value_1 : 0;
        }
        else
        {
            BulletInfo.PenetrationCount = 0;
        }

        // RubberBullet 업그레이드 적용 (튕김 횟수)
        var rubberBulletModifier = GameRoot.Instance.InGameUpgradeSystem.GetModifier(Config.InGameUpgradeChoice.RubberBullet);
        if (rubberBulletModifier != null)
        {
            BulletInfo.BounceCount = (int)rubberBulletModifier.Value_1;
            BulletInfo.BounceSpeedMultiplier = 1.0f + rubberBulletModifier.Value_2;
        }
        else
        {
            BulletInfo.BounceCount = 0;
            BulletInfo.BounceSpeedMultiplier = 1.0f;
        }
    }

    



}
