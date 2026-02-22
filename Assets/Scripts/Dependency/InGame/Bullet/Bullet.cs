using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using BanpoFri;
using System.Linq;

public class Bullet : MonoBehaviour
{
    [SerializeField]
    private ColliderAction ColliderAction = null;

    [SerializeField]
    protected SpriteRenderer BulletImg;

    [SerializeField]
    private Transform BulletRootTr;

    [SerializeField]
    private BoxCollider2D Col;


    [SerializeField]
    private bool IsRotation = false;

    [SerializeField]
    private float RotationSpeed = 360f;

    protected BulletInfo BulletInfo = new BulletInfo();

    [HideInInspector]
    public Transform TargetTr = null;

    [HideInInspector]
    public Transform ShooterTr = null;

    private int shooterLayer = -1; // 캐싱된 발사자 레이어

    protected InGameBaseStage BaseStage = null;

    protected System.Action<Bullet> OnHitCallback = null;

    protected bool IsCollision = false;

    protected int remainingPenetrations = 0; // 남은 관통 횟수
    protected HashSet<Collider2D> hitTargets = new HashSet<Collider2D>(); // 이미 맞춘 타겟들

    [SerializeField]
    private float InitialScale = 0.3f; // 초기 스케일 (작게 시작)

    [SerializeField]
    private float ScaleUpDuration = 0.1f; // 스케일 확대 시간

    private float _scaleTimer = 0f;

    public TrailComponent TrailComponent;

    public virtual void Awake()
    {
        if (TrailComponent != null)
        {
            TrailComponent.InitTrail(Color.white);
        }
    }

    public virtual void Set(BulletInfo bulletinfo, Transform shootertr, Transform targettr, System.Action<Bullet> onhitcallback)
    {
        IsCollision = false;

        transform.SetParent(null);

        // 발사 방향으로 약간 앞에서 시작 (발사자와 겹치지 않도록)
        Vector3 direction = (targettr.position - shootertr.position).normalized;
        transform.position = shootertr.transform.position + direction * 0.15f;

        transform.rotation = Quaternion.identity;

        // 작은 스케일로 시작
        //transform.localScale = Vector3.one * InitialScale;
        _scaleTimer = 0f;

        BulletInfo = bulletinfo;

        // 트레일을 속도에 맞게 재초기화
        if (TrailComponent != null)
        {
            TrailComponent.ClearTrail(); // 이전 트레일 데이터 제거
            TrailComponent.InitTrail(Color.white, (float)bulletinfo.AttackSpeed);
            TrailComponent.SetTrailActive(true); // 트레일 활성화
        }

        BaseStage = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage;

        ShooterTr = shootertr;
        shooterLayer = ShooterTr != null && ShooterTr.gameObject != null ? ShooterTr.gameObject.layer : -1;

        TargetTr = targettr;

        OnHitCallback = onhitcallback;

        // 관통 횟수 초기화
        remainingPenetrations = bulletinfo.PenetrationCount;
        hitTargets.Clear();

        ColliderAction.TriggerEnterAction = OnTriggerEnter2D;
    }

    public void SetBulletImg(Sprite sprite)
    {
        BulletImg.sprite = sprite;

        SetColliderSize(1f);
    }



    private void Update()
    {
        // // 타겟이 없으면 총알 제거
        // if (TargetTr == null || !TargetTr.gameObject.activeSelf)
        // {
        //     if (!IsCollision)
        //     {
        //         IsCollision = true;
        //         DisableTrail();
        //         OnHitCallback?.Invoke(this);
        //     }
        //     return;
        // }


        Move();



        if (IsRotation)
        {
            BulletRootTr.transform.Rotate(0, 0, -RotationSpeed * Time.deltaTime);
        }
    }


    protected virtual void Move() { }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        // 파괴된 콜라이더나 비활성 상태에서 호출될 수 있으므로 방어
        if (collision == null) return;
        var collidedObj = collision.gameObject;
        if (collidedObj == null) return;

        if (IsCollision && remainingPenetrations <= 0) return;
        if (shooterLayer == -1) return;

        // 이미 맞춘 타겟은 무시
        if (hitTargets.Contains(collision)) return;
        

        if (collidedObj.layer == LayerMask.NameToLayer("Enemy") && shooterLayer == LayerMask.NameToLayer("Player"))
        {
            var enemy = collidedObj.GetComponent<EnemyUnit>();
            if (enemy != null && !enemy.IsDead)
            {
                hitTargets.Add(collision);
                enemy.Damage(BulletInfo.AttackDamage);

                LigihtninigEffect(enemy, BulletInfo.AttackDamage);
                FrezeeEffectCheck(enemy);
                PoisonEffectCheck(enemy);

                // 관통 횟수가 남아있으면 관통
                if (remainingPenetrations > 0)
                {
                    remainingPenetrations--;
                    if (remainingPenetrations < 0)
                    {
                        IsCollision = true;
                        DisableTrail();
                        OnHitCallback?.Invoke(this);
                    }
                }
                else
                {
                    // 관통이 없으면 즉시 충돌 처리
                    IsCollision = true;
                    DisableTrail();
                    OnHitCallback?.Invoke(this);
                }
            }
        }
        else if (collidedObj.layer == LayerMask.NameToLayer("EnemyBlockSpawner") && shooterLayer == LayerMask.NameToLayer("Player"))
        {
            var enemy = collidedObj.GetComponent<EnemyBlockSpawner>();
            if (enemy != null && !enemy.IsDead)
            {
                hitTargets.Add(collision);
                enemy.Damage((int)BulletInfo.AttackDamage);

                // 관통 횟수가 남아있으면 관통
                if (remainingPenetrations > 0)
                {
                    remainingPenetrations--;
                    if (remainingPenetrations < 0)
                    {
                        IsCollision = true;
                        DisableTrail();
                        OnHitCallback?.Invoke(this);
                    }
                }
                else
                {
                    // 관통이 없으면 즉시 충돌 처리
                    IsCollision = true;
                    DisableTrail();
                    OnHitCallback?.Invoke(this);
                }
            }
        }
        else if (collidedObj.layer == LayerMask.NameToLayer("Player") && shooterLayer == LayerMask.NameToLayer("Enemy"))
        {
            var player = collidedObj.GetComponent<PlayerUnit>();
            if (player != null && !player.IsDead)
            {
                hitTargets.Add(collision);
                player.Damage(BulletInfo.AttackDamage);

                // 관통 횟수가 남아있으면 관통
                if (remainingPenetrations > 0)
                {
                    remainingPenetrations--;
                    if (remainingPenetrations <= 0)
                    {
                        IsCollision = true;
                        DisableTrail();
                        OnHitCallback?.Invoke(this);
                    }
                }
                else
                {
                    // 관통이 없으면 즉시 충돌 처리
                    IsCollision = true;
                    DisableTrail();
                    OnHitCallback?.Invoke(this);
                }
            }
            else if (shooterLayer == LayerMask.NameToLayer("Enemy"))
            {
                var blockGroup = collision.GetComponent<PlayerBlock>();
                if (blockGroup != null)
                {
                    hitTargets.Add(collision);

                    GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.PlayerBlockGroup.Damage((int)BulletInfo.AttackDamage);

                    // 관통 횟수가 남아있으면 관통
                    if (remainingPenetrations > 0)
                    {
                        remainingPenetrations--;
                        if (remainingPenetrations <= 0)
                        {
                            IsCollision = true;
                            DisableTrail();
                            OnHitCallback?.Invoke(this);
                        }
                    }
                    else
                    {
                        // 관통이 없으면 즉시 충돌 처리
                        IsCollision = true;
                        DisableTrail();
                        OnHitCallback?.Invoke(this);
                    }
                }
            }
        }
        else if(collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            IsCollision = true;
            DisableTrail();
            OnHitCallback?.Invoke(this);
        }
    }


    public virtual void SetColliderSize(float size)
    {
        if (BulletImg != null && BulletImg.sprite != null)
        {
            // 이미지의 실제 크기를 가져와서 BoxCollider 사이즈 설정
            Vector2 spriteSize = BulletImg.bounds.size;
            Col.size = spriteSize * size;
        }
    }


    public void LigihtninigEffect(EnemyUnit target, double damage)
    {
        if (shooterLayer != LayerMask.NameToLayer("Player"))
            return;

        var findmodifier = GameRoot.Instance.InGameUpgradeSystem.GetModifier(Config.InGameUpgradeChoice.LightningStatue);
        if (findmodifier == null)
            return;

        if (!ProjectUtility.IsPercentSuccess(findmodifier.Value_2))
            return;

        var cachedTarget = target;
        var effectPos = new Vector3(target.transform.position.x, target.transform.position.y - 0.5f, this.transform.position.z);

        GameRoot.Instance.EffectSystem.Play<LightningEffect>(effectPos, x =>
        {
            // Play effect sound if available
            SoundPlayer.Instance?.PlaySound("effect_bomb");

            // Effect instance missing; nothing more to do
            if (x == null)
                return;

            // Target might already be destroyed when the effect finishes loading
            if (cachedTarget == null || cachedTarget.IsDead)
            {
                x.SetAutoRemove(true, 3f);
                return;
            }

            var newdamage = ProjectUtility.PercentCalc(damage, findmodifier.Value_1);
            cachedTarget.Damage(newdamage);
            x.SetAutoRemove(true, 3f);
        });
    }


    public void FrezeeEffectCheck(EnemyUnit target)
    {
        if (shooterLayer == LayerMask.NameToLayer("Player") && !target.IsBossUnit)
        {
            var findmodifier = GameRoot.Instance.InGameUpgradeSystem.GetModifier(Config.InGameUpgradeChoice.FreezeBullet);
            if (findmodifier != null)
            {
                if (ProjectUtility.IsPercentSuccess(findmodifier.Value_2))
                {
                    GameRoot.Instance.AlimentSystem.AddAliment(AlimentType.Freeze, findmodifier.Value_1, target, 0, 1f);
                }
            }
        }
    }


    public void PoisonEffectCheck(EnemyUnit target)
    {
        var findmodifier = GameRoot.Instance.InGameUpgradeSystem.GetModifier(Config.InGameUpgradeChoice.PoisonBullet);
        if (findmodifier != null && !target.IsBossUnit)
        {
            if (ProjectUtility.IsPercentSuccess(findmodifier.Value_2))
            {
                var damage = ProjectUtility.PercentCalc(BulletInfo.AttackDamage, findmodifier.Value_1);

                GameRoot.Instance.AlimentSystem.AddAliment(AlimentType.Poison, 2f, target, damage, 1f, 3); // 최대 3회 중첩
            }
        }
    }

    protected virtual void DisableTrail()
    {
        if (TrailComponent != null)
        {
            TrailComponent.SetTrailActive(false);
        }
    }

}
