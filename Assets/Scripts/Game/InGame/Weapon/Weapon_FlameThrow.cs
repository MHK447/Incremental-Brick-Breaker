using UnityEngine;
using BanpoFri;

public class Weapon_FlameThrow : Weapon_Base
{
    [SerializeField]
    private Transform GunImgTr;

    [SerializeField] private FlameThrowEffect embeddedFlameEffect;
    [SerializeField] private Transform flameFollowTransform;

    [Tooltip("분사 유지 시간(초). 이후 이펙트를 끄고 다시 쿨타임을 채웁니다.")]
    [SerializeField] private float flameDuration = 2f;

    [Tooltip("GunImgTr 회전 보간 속도")]
    [SerializeField] private float gunRotSpeed = 10f;

    private bool isFlaming;
    private float flameTimer;
    private bool waitingForFlameLoad;
    private FlameThrowEffect activePooledEffect;
    private EnemyUnitGroup enemyUnitGroup;
    private Transform currentTarget;

    private void Awake()
    {
        if (flameFollowTransform == null)
            flameFollowTransform = transform;
        if (embeddedFlameEffect == null)
            embeddedFlameEffect = GetComponentInChildren<FlameThrowEffect>(true);
    }

    public override void Set(WeaponData weaponData)
    {
        base.Set(weaponData);
        StopFlameInternal();
        isFlaming = false;
        flameTimer = 0f;
        waitingForFlameLoad = false;
        currentTarget = null;
        if (WeaponData != null)
            WeaponData.WeaponDeltime = 0f;

        enemyUnitGroup = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.EnemyUnitGroup;
    }

    private void OnDisable()
    {
        StopFlameInternal();
        isFlaming = false;
        waitingForFlameLoad = false;
    }

    private void Update()
    {
        if (WeaponData == null || GameRoot.Instance == null)
            return;
        if (GameRoot.Instance.UserData.InGamePlayerData.IsDeadProperty.Value)
            return;

        bool canUseEmbedded = embeddedFlameEffect != null;
        bool canUsePool = embeddedFlameEffect == null && GameRoot.Instance.EffectSystem != null;
        if (!canUseEmbedded && !canUsePool)
            return;

        float cdMult = GameRoot.Instance.UserData.InGamePlayerData.WeaponCooldownMultiplier;
        float adjustedCoolTime = cdMult > 0f ? WeaponData.WeaponCoolTime / cdMult : WeaponData.WeaponCoolTime;

        UpdateTarget();
        RotateGunToTarget();

        if (isFlaming)
        {
            if (!waitingForFlameLoad)
                flameTimer += Time.deltaTime;

            if (flameTimer >= flameDuration)
            {
                StopFlameInternal();
                isFlaming = false;
                waitingForFlameLoad = false;
                WeaponData.WeaponDeltime = 0f;
            }
        }
        else
        {
            WeaponData.WeaponDeltime += Time.deltaTime;
            if (WeaponData.WeaponDeltime >= adjustedCoolTime)
            {
                StartFlameInternal();
                isFlaming = true;
                flameTimer = 0f;
            }
        }
    }

    private void UpdateTarget()
    {
        if (enemyUnitGroup == null) return;
        currentTarget = enemyUnitGroup.FindClosestEnemyTransform(flameFollowTransform);
    }

    private void RotateGunToTarget()
    {
        if (GunImgTr == null || currentTarget == null) return;

        Vector3 dir = currentTarget.position - GunImgTr.position;
        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        Quaternion targetRot = Quaternion.Euler(0f, 0f, targetAngle);
        GunImgTr.rotation = Quaternion.Lerp(GunImgTr.rotation, targetRot, Time.deltaTime * gunRotSpeed);
    }

    private void StartFlameInternal()
    {
        if (embeddedFlameEffect != null)
        {
            embeddedFlameEffect.Init(WeaponData.WeaponDamage);
            ProjectUtility.SetActiveCheck(embeddedFlameEffect.gameObject, true);
            embeddedFlameEffect.Play(flameFollowTransform.position, flameFollowTransform);
            waitingForFlameLoad = false;
            return;
        }

        waitingForFlameLoad = true;
        activePooledEffect = null;
        var pos = flameFollowTransform.position;
        GameRoot.Instance.EffectSystem.MultiPlay<FlameThrowEffect>(pos, e =>
        {
            if (e == null)
            {
                waitingForFlameLoad = false;
                return;
            }

            if (this == null || !isFlaming || WeaponData == null
                || GameRoot.Instance.UserData.InGamePlayerData.IsDeadProperty.Value)
            {
                e.Stop();
                GameRoot.Instance.EffectSystem.ReturnMultiEffect(e);
                waitingForFlameLoad = false;
                return;
            }

            activePooledEffect = e;
            e.Init(WeaponData.WeaponDamage);
            waitingForFlameLoad = false;
            flameTimer = 0f;
        }, flameFollowTransform);
    }

    private void StopFlameInternal()
    {
        waitingForFlameLoad = false;

        if (embeddedFlameEffect != null)
        {
            embeddedFlameEffect.Stop();
            ProjectUtility.SetActiveCheck(embeddedFlameEffect.gameObject, false);
            return;
        }

        if (activePooledEffect != null)
        {
            activePooledEffect.Stop();
            if (GameRoot.Instance != null && GameRoot.Instance.EffectSystem != null)
                GameRoot.Instance.EffectSystem.ReturnMultiEffect(activePooledEffect);
            activePooledEffect = null;
        }
    }
}
