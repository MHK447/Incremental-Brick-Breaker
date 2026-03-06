using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class EnemyUnitData
{
    public enum EnemyUnitState
    {
        Idle,
        Attack,
        Dead,
        Move,
    }

    public int EnemyIdx { get; set; } = 0;
    public int StartHp { get; set; } = 0;
    public int CurHp { get; set; } = 0;
    public int Dmg { get; set; } = 0;
    public int AtkSpeed { get; set; } = 0;
    public int AtkRange { get; set; } = 0;
    public int MoveSpeed { get; set; } = 0;
}

public class EnemyUnitBase : MonoBehaviour
{
    [SerializeField]
    private Animator Anim;

    public int Hp { get; private set; } = 0;

    public bool IsDead { get { return EnemyUnitData != null && EnemyUnitData.CurHp <= 0; } }

    public int EnemyIdx { get; private set; } = 0;

    protected EnemyUnitData EnemyUnitData { get; private set; } = null;

    [SerializeField]
    private List<SpriteRenderer> UnitSpriteList = new List<SpriteRenderer>();

    
    private EnemyUnitData.EnemyUnitState currentState = EnemyUnitData.EnemyUnitState.Idle;
    private float attackTimer = 0f;
    private PlayerUnit targetPlayer = null;

    public virtual void Set(EnemyUnitData enemydata)
    {
        EnemyUnitData = enemydata;
        EnemyIdx = enemydata.EnemyIdx;
        currentState = EnemyUnitData.EnemyUnitState.Move;
        attackTimer = 0f;
        targetPlayer = null;
    }

    protected virtual void Update()
    {
        if (IsDead || EnemyUnitData == null) return;

        if (targetPlayer == null || !targetPlayer.gameObject.activeInHierarchy)
        {
            targetPlayer = FindObjectOfType<PlayerUnit>();
            if (targetPlayer == null) return;
        }

        float distance = Vector3.Distance(transform.position, targetPlayer.transform.position);
        float atkRange = EnemyUnitData.AtkRange * 0.01f;

        switch (currentState)
        {
            case EnemyUnitData.EnemyUnitState.Move:
                MoveToPlayer(distance, atkRange);
                break;
            case EnemyUnitData.EnemyUnitState.Attack:
                AttackPlayer(distance, atkRange);
                break;
        }
    }

    private void MoveToPlayer(float distance, float atkRange)
    {
        if (distance <= atkRange)
        {
            currentState = EnemyUnitData.EnemyUnitState.Attack;
            attackTimer = 0f;
            if (Anim != null) Anim.SetTrigger("Attack");
            return;
        }

        float moveSpeed = EnemyUnitData.MoveSpeed * 0.01f;
        Vector3 direction = (targetPlayer.transform.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        if (Anim != null) Anim.SetBool("IsMove", true);
    }

    private void AttackPlayer(float distance, float atkRange)
    {
        // 사거리 밖으로 나가면 다시 이동
        if (distance > atkRange * 1.1f)
        {
            currentState = EnemyUnitData.EnemyUnitState.Move;
            if (Anim != null) Anim.SetBool("IsMove", true);
            return;
        }

        float atkCoolTime = EnemyUnitData.AtkSpeed > 0 ? 1f / (EnemyUnitData.AtkSpeed * 0.01f) : 1f;
        attackTimer += Time.deltaTime;

        if (attackTimer >= atkCoolTime)
        {
            attackTimer = 0f;
            targetPlayer.Damage(EnemyUnitData.Dmg);
            if (Anim != null) Anim.SetTrigger("Attack");
        }
    }

    public virtual void Damage(int damage)
    {
        if (IsDead || EnemyUnitData == null) return;

        EnemyUnitData.CurHp -= damage;

        DamageColorEffect();

        GameRoot.Instance.DamageTextSystem.ShowDamage(damage, transform.position, Color.red);

        if (EnemyUnitData.CurHp <= 0)
        {
            EnemyUnitData.CurHp = 0;
            currentState = EnemyUnitData.EnemyUnitState.Dead;
            if (Anim != null) Anim.SetTrigger("Dead");

            ProjectUtility.SetActiveCheck(this.gameObject , false);
        }
    }

    private bool IsDamageDirect = false;

    public virtual void DamageColorEffect()
    {
        if (!IsDamageDirect)
        {
            IsDamageDirect = true;

            // 피격 효과 적용
            foreach (var sprite in UnitSpriteList)
            {
                if (sprite != null)
                {
                    sprite.EnableHitEffect();
                }
            }

            GameRoot.Instance.WaitTimeAndCallback(0.15f, () =>
            {
                if (this != null)
                {
                    // 효과 종료 후 원래 머티리얼로 복귀
                    foreach (var sprite in UnitSpriteList)
                    {
                        if (sprite != null)
                        {
                            sprite.DisableHitEffect();
                        }
                    }

                    IsDamageDirect = false;
                }
            });
        }
    }

    // protected virtual IEnumerator FadeOutAndDelete()
    // {
    //     // DOTween을 사용하여 0.3초간 스케일을 0으로 만들기
    //     transform.DOScale(Vector3.zero, 0.3f)
    //         .SetEase(Ease.InOutQuad)
    //         .OnComplete(() =>
    //         {
    //             DeleteUnit();
    //             fadeOutCoroutine = null;
    //         });

    //     yield return null;
    // }



}
