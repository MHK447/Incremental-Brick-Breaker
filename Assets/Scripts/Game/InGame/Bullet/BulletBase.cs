using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using BanpoFri;
using System.Linq;

public class BulletBase : MonoBehaviour
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

    protected WeaponData WeaponData = new WeaponData();

    public WeaponData GetWeaponData => WeaponData;

    [HideInInspector]
    public Transform TargetTr = null;

    [HideInInspector]
    public Transform ShooterTr = null;

    private int shooterLayer = -1; // 캐싱된 발사자 레이어

    protected InGameBaseStage BaseStage = null;

    protected System.Action<BulletBase> OnHitCallback = null;

    protected bool IsCollision = false;

    protected HashSet<Collider2D> hitTargets = new HashSet<Collider2D>(); // 이미 맞춘 타겟들


    public virtual void Awake()
    {

    }

    public virtual void Set(WeaponData weaponData, Transform shootertr, Transform targettr, System.Action<BulletBase> onhitcallback)
    {
        IsCollision = false;

        transform.SetParent(null);

        // 발사 방향으로 약간 앞에서 시작 (발사자와 겹치지 않도록)
        Vector3 direction = (targettr.position - shootertr.position).normalized;
        transform.position = shootertr.transform.position + direction * 0.15f;

        transform.rotation = Quaternion.identity;

        WeaponData = weaponData;

        BaseStage = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage;

        ShooterTr = shootertr;
        shooterLayer = ShooterTr != null && ShooterTr.gameObject != null ? ShooterTr.gameObject.layer : -1;

        TargetTr = targettr;

        OnHitCallback = onhitcallback;

        // 관통 횟수 초기화
        hitTargets.Clear();

        ColliderAction.TriggerEnterAction = OnTriggerEnter2D;
    }

    public void SetBulletImg(Sprite sprite)
    {
        if (BulletImg != null)
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

        if (IsCollision) return;
        if (shooterLayer == -1) return;

        // 이미 맞춘 타겟은 무시
        if (hitTargets.Contains(collision)) return;


        if (collidedObj.layer == LayerMask.NameToLayer("Enemy") && shooterLayer == LayerMask.NameToLayer("Player"))
        {
            var enemy = collidedObj.GetComponent<EnemyUnitBase>();
            if (enemy != null)
            {
                hitTargets.Add(collision);
                int finalDmg = WeaponData.WeaponDamage
                    + GameRoot.Instance.UserData.InGamePlayerData.IncreaDamageBonus
                    + GameRoot.Instance.UserData.InGamePlayerData.EquipDamageBonus;
                enemy.Damage(finalDmg);

                IsCollision = true;
                OnHitCallback?.Invoke(this);
            }
        }
        else if (collidedObj.layer == LayerMask.NameToLayer("EnemyBlockSpawner") && shooterLayer == LayerMask.NameToLayer("Player"))
        {
            var enemy = collidedObj.GetComponent<EnemyBlockSpawner>();
            if (enemy != null)
            {
                hitTargets.Add(collision);
                int finalDmg = WeaponData.WeaponDamage
                    + GameRoot.Instance.UserData.InGamePlayerData.IncreaDamageBonus
                    + GameRoot.Instance.UserData.InGamePlayerData.EquipDamageBonus;
                enemy.Damage(finalDmg);

                IsCollision = true;
                OnHitCallback?.Invoke(this);

            }
        }
        else if (collidedObj.layer == LayerMask.NameToLayer("Player") && shooterLayer == LayerMask.NameToLayer("Enemy"))
        {
            var player = collidedObj.GetComponent<PlayerUnit>();
            if (player != null)
            {
                hitTargets.Add(collision);
                player.Damage(WeaponData.WeaponDamage);

                // 관통이 없으면 즉시 충돌 처리
                IsCollision = true;
                OnHitCallback?.Invoke(this);
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            IsCollision = true;
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

    // protected virtual void DisableTrail()
    // {
    //     if (TrailComponent != null)
    //     {
    //         TrailComponent.SetTrailActive(false);
    //     }
    // }

}
