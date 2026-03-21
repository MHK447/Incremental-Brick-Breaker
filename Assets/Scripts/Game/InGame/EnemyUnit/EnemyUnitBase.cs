using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class EnemyUnitData
{

    public int EnemyIdx { get; set; } = 0;
    public int StartHp { get; set; } = 0;
    public int CurHp { get; set; } = 0;
    public int Dmg { get; set; } = 0;
    public float AtkSpeed { get; set; } = 0f;
    public int AtkRange { get; set; } = 0;
    public int MoveSpeed { get; set; } = 0;

    public float Attackdeltime = 0f;

}

public class EnemyUnitBase : MonoBehaviour
{

    public enum EnemyUnitState
    {
        Idle,
        Attack,
        Dead,
        Move,
    }
    public EnemyUnitState CurrentState { get; private set; } = EnemyUnitState.Idle;
    [SerializeField]
    private Animator Anim;

    public int Hp { get; private set; } = 0;

    public bool IsDead { get { return EnemyUnitData != null && EnemyUnitData.CurHp <= 0; } }

    public int EnemyIdx { get; private set; } = 0;

    protected EnemyUnitData EnemyUnitData { get; private set; } = null;

    [SerializeField]
    private List<SpriteRenderer> UnitSpriteList = new List<SpriteRenderer>();


    private InGameHpProgress HpProgress;
    private bool HpProgressLoading = false;


    private EnemyUnitState currentState = EnemyUnitState.Idle;
    private float attackTimer = 0f;
    private PlayerUnit targetPlayer = null;

    private EnemyUnitGroup EnemyUnitGroup = null;

    public bool IsStationary { get; set; } = false;

    public virtual void Set(EnemyUnitData enemydata)
    {
        EnemyUnitData = enemydata;
        EnemyIdx = enemydata.EnemyIdx;
        currentState = EnemyUnitState.Move;
        attackTimer = 0f;
        IsStationary = false;
        targetPlayer = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.Player;
        EnemyUnitGroup = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.EnemyUnitGroup;
        EnsureHpProgress();
    }

    private void BindHpProgress()
    {
        if (HpProgress == null || EnemyUnitData == null || EnemyUnitData.StartHp <= 0)
            return;

        HpProgress.Init(transform);
        ProjectUtility.SetActiveCheck(HpProgress.gameObject, true);
        HpProgress.SetHpText(EnemyUnitData.CurHp, EnemyUnitData.StartHp);
        HpProgress.SetOffset(new Vector3(0f, 0.7f, 0f));
    }

    private void EnsureHpProgress()
    {
        if (HpProgress != null)
        {
            BindHpProgress();
            return;
        }

        if (HpProgressLoading || EnemyUnitData == null || EnemyUnitData.StartHp <= 0)
            return;

        HpProgressLoading = true;
        GameRoot.Instance.UISystem.LoadFloatingUI<InGameHpProgress>(ui =>
        {
            HpProgressLoading = false;
            if (ui == null)
                return;
            if (this == null)
            {
                ui.Hide();
                return;
            }

            HpProgress = ui;
            BindHpProgress();
        }, false);
    }

    protected virtual void Update()
    {
        if (IsDead || EnemyUnitData == null) return;
        if (IsStationary) return;

        if (targetPlayer == null || !targetPlayer.gameObject.activeInHierarchy)
        {
            targetPlayer = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.Player;
            if (targetPlayer == null) return;
        }

        float distance = Vector3.Distance(transform.position, targetPlayer.transform.position);
        float atkRange = EnemyUnitData.AtkRange * 0.01f;

        switch (currentState)
        {
            case EnemyUnitState.Move:
                MoveToPlayer(distance, atkRange);
                break;
            case EnemyUnitState.Attack:
                AttackPlayer(distance, atkRange);
                break;
            case EnemyUnitState.Idle:
                IdleAfterAttack(distance, atkRange);
                break;
        }
    }

    private void MoveToPlayer(float distance, float atkRange)
    {
        if (distance <= atkRange)
        {
            currentState = EnemyUnitState.Attack;
            attackTimer = 0f;
            return;
        }

        float moveSpeed = EnemyUnitData.MoveSpeed * 0.01f;
        Vector3 direction = (targetPlayer.transform.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        if (Anim != null) Anim.Play("Move");
    }

    private void AttackPlayer(float distance, float atkRange)
    {
        // 사거리 밖으로 나가면 다시 이동
        if (distance > atkRange * 1.1f)
        {
            currentState = EnemyUnitState.Move;
            if (Anim != null) Anim.Play("Walk");
            return;
        }

        attackTimer += Time.deltaTime;

        if (attackTimer >= EnemyUnitData.AtkSpeed)
        {
            attackTimer = 0f;
            ChangeState(EnemyUnitState.Attack);
        }
    }

    public void AttackAfter()
    {
        attackTimer = EnemyUnitData.AtkSpeed;
        targetPlayer.Damage(EnemyUnitData.Dmg);

        ChangeState(EnemyUnitState.Move);
    }

    public void DeadAfter()
    {
        ProjectUtility.SetActiveCheck(this.gameObject, false);
    }

    private void IdleAfterAttack(float distance, float atkRange)
    {
        // 사거리 밖이면 다시 이동
        if (distance > atkRange * 1.1f)
        {
            ChangeState(EnemyUnitState.Move);
            return;
        }

        ChangeState(EnemyUnitState.Move);

        float atkCoolTime = EnemyUnitData.AtkSpeed > 0 ? 1f / (EnemyUnitData.AtkSpeed * 0.01f) : 1f;
        attackTimer += Time.deltaTime;

        if (attackTimer >= atkCoolTime)
        {
            attackTimer = atkCoolTime; // 다음 프레임에 바로 공격
            ChangeState(EnemyUnitState.Attack);
        }
    }

    public virtual void Damage(int damage)
    {
        if (IsDead || EnemyUnitData == null) return;

        EnemyUnitData.CurHp -= damage;
        if (HpProgress != null && EnemyUnitData.StartHp > 0)
            HpProgress.SetHpText(EnemyUnitData.CurHp, EnemyUnitData.StartHp);

        DamageColorEffect();

        GameRoot.Instance.DamageTextSystem.ShowDamage(damage, transform.position, Color.red);

        if (EnemyUnitData.CurHp <= 0)
        {
            EnemyUnitData.CurHp = 0;
            if (HpProgress != null && EnemyUnitData.StartHp > 0)
                HpProgress.SetHpText(EnemyUnitData.CurHp, EnemyUnitData.StartHp);
            ChangeState(EnemyUnitState.Dead);

            var playerData = GameRoot.Instance.UserData.InGamePlayerData;

            int coinReward = EnemyUnitData.Dmg + playerData.CoinValueBonus;

            int totalCoinCount = 1 + playerData.BonusCoinDropCount;
            for (int i = 0; i < totalCoinCount; i++)
            {
                Vector3 dropPos = transform.position + new Vector3(Random.Range(-0.25f, 0.25f), Random.Range(-0.25f, 0.25f), 0f);
                GameRoot.Instance.EffectSystem.MultiPlay<EnemyKillRewardEffect>(dropPos, x =>
                {
                    x.Set((int)Config.RewardType.Currency, (int)Config.CurrencyID.Money, coinReward);
                });
            }

            if (playerData.BonusGemDropRate > 0f && Random.value < playerData.BonusGemDropRate)
            {
                GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency, (int)Config.CurrencyID.Cash, 1);
            }

            if (EnemyUnitGroup != null)
                EnemyUnitGroup.DeleteUnit(this);
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


    public void ChangeState(EnemyUnitState state)
    {
        currentState = state;
        if (Anim != null)
        {
            switch (state)
            {
                case EnemyUnitState.Idle:
                    Anim.Play("Idle");
                    break;
                case EnemyUnitState.Attack:
                    Anim.Play("Attack");
                    break;
                case EnemyUnitState.Dead:
                    Anim.Play("Dead");
                    break;
                case EnemyUnitState.Move:
                    Anim.Play("Walk");
                    break;
            }
        }
    }

    private void OnDestroy()
    {
        if (HpProgress != null)
            HpProgress.Hide();
    }

}
