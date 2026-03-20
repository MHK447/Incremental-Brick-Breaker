using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class FallWeaponBase : MonoBehaviour
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
    private TrailComponent TrailComponent = null;


    protected WeaponData WeaponData = new WeaponData();

    public WeaponData GetWeaponData => WeaponData;



    protected InGameBaseStage BaseStage = null;

    protected System.Action<FallWeaponBase> OnHitCallback = null;

    protected bool IsCollision = false;

    protected HashSet<Collider2D> hitTargets = new HashSet<Collider2D>();

    private int maxPenetration = 0;

    private const float FallSpeed = 15f;

    public virtual void Awake()
    {
        TrailComponent.InitTrail(FallSpeed);
    }

    public virtual void Set(WeaponData weaponData, Vector3 startpos, System.Action<FallWeaponBase> onhitcallback)
    {
        IsCollision = false;

        transform.SetParent(null);

        WeaponData = weaponData;

        BaseStage = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage;

        this.transform.position = startpos;

        OnHitCallback = onhitcallback;

        hitTargets.Clear();
        maxPenetration = GameRoot.Instance.UserData.InGamePlayerData.FallPenetrationCount;

        ColliderAction.TriggerEnterAction = OnTriggerEnter2D;

        // 트레일: 재사용 시 이전 꼬리 제거 후 활성화 (떨어질 때 트레일이 나오도록)
        TrailComponent.ClearTrail();
        TrailComponent.SetTrailActive(true);
    }

    public void SetBulletImg(Sprite sprite)
    {
        if (BulletImg != null)
            BulletImg.sprite = sprite;

        SetColliderSize(1f);
    }



    private void Update()
    {
        Move();
    }


    protected virtual void Move()
    {
        this.transform.position += Vector3.down * Time.deltaTime * FallSpeed;
    }

    public virtual void OnTriggerEnter2D(Collider2D collision)
    {
        // 파괴된 콜라이더나 비활성 상태에서 호출될 수 있으므로 방어
        if (collision == null) return;
        var collidedObj = collision.gameObject;
        if (collidedObj == null) return;

        if (IsCollision) return;
        // 이미 맞춘 타겟은 무시
        if (hitTargets.Contains(collision)) return;


        int finalDmg = WeaponData.WeaponDamage
            + GameRoot.Instance.UserData.InGamePlayerData.IncreaDamageBonus
            + GameRoot.Instance.UserData.InGamePlayerData.EquipDamageBonus;

        if (collidedObj.layer == LayerMask.NameToLayer("Enemy"))
        {
            var enemy = collidedObj.GetComponent<EnemyUnitBase>();
            if (enemy != null)
            {
                hitTargets.Add(collision);
                enemy.Damage(finalDmg);

                if (hitTargets.Count > maxPenetration)
                {
                    IsCollision = true;
                    OnHitCallback?.Invoke(this);
                }
            }
        }
        else if (collidedObj.layer == LayerMask.NameToLayer("EnemyBlockSpawner"))
        {
            var enemy = collidedObj.GetComponent<EnemyBlockSpawner>();
            if (enemy != null)
            {
                hitTargets.Add(collision);
                enemy.Damage(finalDmg);

                if (hitTargets.Count > maxPenetration)
                {
                    IsCollision = true;
                    OnHitCallback?.Invoke(this);
                }
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            IsCollision = true;
            OnHitCallback?.Invoke(this);
        }
    }


    /// <summary> 트레일이 사라지는 시간 (히트 후 비활성화 지연용) </summary>
    public float GetTrailTime() => TrailComponent != null ? TrailComponent.GetTrailTime() : 0f;

    public virtual void SetColliderSize(float size)
    {
        if (BulletImg != null && BulletImg.sprite != null)
        {
            // 이미지의 실제 크기를 가져와서 BoxCollider 사이즈 설정
            Vector2 spriteSize = BulletImg.bounds.size;
            Col.size = spriteSize * size;
        }
    }
}

