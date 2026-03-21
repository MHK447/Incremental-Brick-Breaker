using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using DG.Tweening;


public class Weapon_ProjectTile : Weapon_Base
{
    protected int ShootCount = 1;
    [SerializeField] private GameObject BulletPrefab;
    public PrefabPool<BulletBase> BulletPool = new();

    [SerializeField]
    protected Transform barrelTransform;


    private EnemyUnitBase Target = null;
    private Transform ResolvedFireTarget = null;
    protected List<BulletBase> ActiveBullets = new List<BulletBase>();

    private EnemyUnitGroup EnemyUnitGroup;

    public override void Set(WeaponData weaponData)
    {
        base.Set(weaponData);

        WeaponData = weaponData;

        EnemyUnitGroup = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.EnemyUnitGroup;

        // 풀 초기화: Init이 호출되지 않으면 Get() 시 큐가 비어 있어 예외 발생
        if (BulletPrefab != null && BulletPool != null)
        {
            const int initialPoolSize = 20;
            BulletPool.Init(BulletPrefab, transform, initialPoolSize);
        }
    }




    public virtual void Clear()
    {
        BulletPool.Clear();
    }



    protected virtual void FireBullet(Vector3 position, Vector3 direction)
    {
        var root = GameRoot.GetInstance();
        if (root == null || root.UserData == null)
        {
            return; // 게임 종료 중
        }

        if (this == null || barrelTransform == null) return;

        // IsWaveRestProperty가 true일 때 활성화된 bullet 모두 삭제
        if (root.UserData.InGamePlayerData.IsDeadProperty.Value)
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

        BulletBase instance = BulletPool.Get();
        if (instance == null) return;

        instance.Set(WeaponData, barrelTransform, targetTransform, OnBulletHit);

        // 발사 시 0.2초 스케일 트윈 연출 (커졌다가 복귀)
        PlayFireScaleTween();

        // 활성화된 bullet 리스트에 추가
        ActiveBullets.Add(instance);
    }

    private const float FireScaleTweenDuration = 0.2f;
    private static readonly Vector3 FireScaleUp = new Vector3(1.2f, 1.2f, 1.2f);

    private void PlayFireScaleTween()
    {
        if (transform == null) return;
        transform.DOKill(complete: true);
        Vector3 originalScale = transform.localScale;
        float half = FireScaleTweenDuration * 0.5f;
        DOTween.Sequence()
            .Append(transform.DOScale(Vector3.Scale(originalScale, FireScaleUp), half).SetEase(Ease.OutQuad))
            .Append(transform.DOScale(originalScale, half).SetEase(Ease.InQuad))
            .SetTarget(transform);
    }

    protected virtual void OnBulletHit(BulletBase bullet)
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
                OnBulletHit(ActiveBullets[i]);
        }

        Target = null;
        ResolvedFireTarget = null;
    }



    public EnemyUnitBase FindTarget()
    {
        if (EnemyUnitGroup == null || barrelTransform == null)
        {
            Target = null;
            ResolvedFireTarget = null;
            return null;
        }

        ResolvedFireTarget = EnemyUnitGroup.FindClosestEnemyTransform(barrelTransform);
        Target = ResolvedFireTarget != null ? ResolvedFireTarget.GetComponent<EnemyUnitBase>() : null;
        return Target;
    }

    protected bool IsValidTarget() => GetTargetTransform() != null;

    private Transform GetTargetTransform()
    {
        if (ResolvedFireTarget == null || !ResolvedFireTarget.gameObject.activeInHierarchy)
            return null;

        var unit = ResolvedFireTarget.GetComponent<EnemyUnitBase>();
        if (unit != null)
            return unit.IsDead ? null : ResolvedFireTarget;

        var spawner = ResolvedFireTarget.GetComponent<EnemyBlockSpawner>();
        if (spawner != null)
            return spawner.IsDead ? null : ResolvedFireTarget;

        return ResolvedFireTarget;
    }

    protected virtual Transform GetTargetTransformForFire() => GetTargetTransform();


    public virtual void BulletInfoInit()
    {
        
    }



    public virtual void Update()
    {
        if (WeaponData == null) return;

        if (GameRoot.Instance == null) return;

        if (GameRoot.Instance.UserData.InGamePlayerData.IsDeadProperty.Value) return;

        WeaponData.WeaponDeltime += Time.deltaTime;

        float cdMult = GameRoot.Instance.UserData.InGamePlayerData.WeaponCooldownMultiplier;
        float adjustedCoolTime = cdMult > 0f ? WeaponData.WeaponCoolTime / cdMult : WeaponData.WeaponCoolTime;

        if (WeaponData.WeaponDeltime >= adjustedCoolTime)
        {
            FindTarget();
            if (ResolvedFireTarget != null)
            {
                FireBullet(barrelTransform.position, ResolvedFireTarget.position - barrelTransform.position);
                WeaponData.WeaponDeltime = 0f;
            }
        }

    }

}

