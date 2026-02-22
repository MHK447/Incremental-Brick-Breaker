using UnityEngine;
using BanpoFri;
using System.Collections;
using System.Collections.Generic;

public class PlayerHero_Base : MonoBehaviour
{
    public enum HeroState
    {
        Idle,
        Taunt,
        Attack,
        Dead,

    }

    [HideInInspector]
    public int HeroIdx = 0;

    private HeroState currentHeroState = HeroState.Idle;


    [SerializeField] private GameObject BulletPrefab;
    public PrefabPool<Bullet> BulletPool = new();

    [SerializeField]
    protected Transform barrelTransform; //총알 스폰 위치

    protected EnemyBlockSpawner EnemyBlockSpawner;

    private EnemyUnit Target = null;
    protected List<Bullet> ActiveBullets = new List<Bullet>();

    [SerializeField]
    private Animator Anim;


    public BulletInfo BulletInfo = new BulletInfo();

    public virtual void Set(int idx)
    {
        HeroIdx = idx;

        var td = Tables.Instance.GetTable<HeroInfo>().GetData(HeroIdx);
        if (td != null)
        {
            var heroAttack = GameRoot.Instance.HeroSystem.GetHeroStatusValue(HeroStatus.Atttack);
            var heroAttackSpeed = GameRoot.Instance.HeroSystem.GetHeroStatusValue(HeroStatus.AtkSpeed);

            BulletInfo.AttackDamage = heroAttack;
            AttackSpawnTime = (float)heroAttackSpeed;
            BulletInfo.AttackRange = 10;
            BulletInfo.AttackSpeed = 12f;

            StartCoroutine(WaitForCastleTopAndAttach());
        }

        var inGameForSpawner = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>();
        EnemyBlockSpawner = inGameForSpawner?.Stage?.EnemyUnitGroup?.EnemyBlockSpawner;

        BulletPool.Init(BulletPrefab, null, 4);
    }

    private IEnumerator WaitForCastleTopAndAttach()
    {
        var inGame = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>();
        var blockGroup = inGame?.Stage?.PlayerBlockGroup;

        while (blockGroup?.CastleTop == null)
        {
            yield return null;
        }

        var castletop = blockGroup.CastleTop;
        transform.SetParent(castletop.transform);
        transform.position = castletop.PlayerHeroTr.transform.position;
    }

    public void SetInfo()
    {

    }

    public void SetState(HeroState state)
    {
        currentHeroState = state;

        // 상태에 따라 애니메이션 설정
        switch (state)
        {
            case HeroState.Idle:
                Anim.Play("Idle", 0, 0f);
                break;
            case HeroState.Taunt:
                Anim.Play("Taunt", 0, 0f);
                break;
            case HeroState.Attack:
                Anim.Play("Attacking", 0, 0f);
                break;
            case HeroState.Dead:
                Anim.Play("Dead", 0, 0f);
                break;

        }
    }

    private float deltime = 0f;
    private float AttackSpawnTime = 0f;

    public virtual void Update()
    {
        if (GameRoot.Instance.UserData.Playerdata.IsWaveRestProperty.Value) return;

        if (GameRoot.Instance.UserData.Playerdata.CurHpProperty.Value <= 0) return;


        deltime += Time.deltaTime;

        if (AttackSpawnTime <= deltime)
        {
            if (IsValidTarget())
            {
                deltime = 0f;
                SetState(HeroState.Attack);
            }
            else
            {
                FindTarget();
            }
        }
    }


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


    public void FindTarget()
    {
        Target = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.EnemyUnitGroup.FindTargetEnemy(transform, (float)BulletInfo.AttackRange);
    }

    protected virtual void Attack()
    {
        StartCoroutine(FireRoutine(Vector3.right));
    }

    protected IEnumerator FireRoutine(Vector3 direction)
    {
        if (!IsValidTarget())
        {
            FindTarget();
        }
        else
            FireBullet(barrelTransform.position, direction);

        yield return null;
    }


    protected virtual void FireBullet(Vector3 position, Vector3 direction)
    {

        // IsWaveRestProperty가 true일 때 활성화된 bullet 모두 삭제
        if (GameRoot.Instance.UserData.Playerdata.IsWaveRestProperty.Value)
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

        SoundPlayer.Instance.PlaySound("weapon_shoot");

        // 활성화된 bullet 리스트에 추가
        ActiveBullets.Add(instance);
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
    }


    protected virtual void OnBulletHit(Bullet bullet)
    {
        ActiveBullets.Remove(bullet);
        BulletPool.Return(bullet);
    }


    // 발사 시점에 가장 가까운 적의 Transform을 반환하는 메서드
    protected Transform GetTargetTransformForFire()
    {
        // 발사 시점에 가장 가까운 적 찾기
        var nearestEnemy = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.EnemyUnitGroup.FindTargetEnemy(transform, (float)BulletInfo.AttackRange);

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

}
