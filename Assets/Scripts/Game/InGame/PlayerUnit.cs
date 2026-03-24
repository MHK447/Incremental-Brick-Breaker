using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;


public class PlayerUnit : MonoBehaviour
{
    public enum PlayerState
    {
        Idle,
        Run,
        Dead,
    }

    [SerializeField]
    private WeaponController WeaponController;

    public WeaponController GetWeaponController { get { return WeaponController; } }

    [SerializeField]
    private List<SpriteRenderer> PlayerSpriteList = new List<SpriteRenderer>();

    [SerializeField]
    private Animator Anim;

    private InGamePlayerData InGamePlayerData;

    private PlayerState State = PlayerState.Idle;

    private float healthRegenAccumulator;

    public void Init()
    {
        if(!GameRoot.Instance.IncreaMentalSystem.IsUnlocked(IncreaMentalType.TruckUnlock))
        {
            ProjectUtility.SetActiveCheck(this.gameObject, false);
            return;
        }

        WeaponController.Init();

        InGamePlayerData = GameRoot.Instance.UserData.InGamePlayerData;

        SetPlayerData();

        ChangeState(PlayerState.Run);
    }

    void Update()
    {
        if (State == PlayerState.Dead) return;
        if (!gameObject.activeInHierarchy || InGamePlayerData == null) return;

        int regenPerSec = InGamePlayerData.HealthRegenBonus;
        if (regenPerSec <= 0) return;

        int maxHp = InGamePlayerData.StartHpProperty.Value;
        int cur = InGamePlayerData.CurHppProperty.Value;
        if (cur >= maxHp) return;

        healthRegenAccumulator += regenPerSec * Time.deltaTime;
        if (healthRegenAccumulator < 1f) return;

        int heal = Mathf.FloorToInt(healthRegenAccumulator);
        healthRegenAccumulator -= heal;
        InGamePlayerData.CurHppProperty.Value = Mathf.Min(maxHp, cur + heal);
    }

    /// <summary> 보너스 체력(인크/장비) 변경 후 최대 체력·현재 체력 동기화 </summary>
    public void ApplyMaxHealthFromData()
    {
        if (InGamePlayerData == null)
            InGamePlayerData = GameRoot.Instance?.UserData?.InGamePlayerData;
        if (InGamePlayerData == null) return;
        if (!gameObject.activeInHierarchy) return;

        int newMax = 50 + InGamePlayerData.BonusHealth + InGamePlayerData.EquipHealthBonus;
        int oldMax = InGamePlayerData.StartHpProperty.Value;
        int delta = newMax - oldMax;

        InGamePlayerData.StartHpProperty.Value = newMax;

        if (InGamePlayerData.IsDeadProperty.Value) return;

        if (oldMax <= 0)
        {
            InGamePlayerData.CurHppProperty.Value = newMax;
            return;
        }

        int newCur = InGamePlayerData.CurHppProperty.Value + delta;
        InGamePlayerData.CurHppProperty.Value = Mathf.Clamp(newCur, 1, newMax);
    }

    public void SetPlayerData()
    {
        healthRegenAccumulator = 0f;
        int baseHp = 50 + InGamePlayerData.BonusHealth + InGamePlayerData.EquipHealthBonus;
        InGamePlayerData.CurHppProperty.Value = InGamePlayerData.StartHpProperty.Value = baseHp;
        InGamePlayerData.CriticalChanceProperty.Value = 30;
        InGamePlayerData.CriticalDamageProperty.Value = 10;
        InGamePlayerData.FallWeaponIdxProperty.Value =
            GameRoot.Instance.IncreaMentalSystem.IsBombUpgraded()
                ? InGamePlayerData.FallWeaponIdx_Bomb
                : InGamePlayerData.FallWeaponIdx_Default;
    }

    public void Damage(int damage, EnemyUnitBase attacker = null)
    {
        InGamePlayerData.CurHppProperty.Value -= damage;

        DamageColorEffect();

        GameRoot.Instance.DamageTextSystem.ShowDamage(damage, transform.position, Color.red);

        TryTriggerDefenseGuard(damage, attacker);

        if (InGamePlayerData.CurHppProperty.Value <= 0)
        {
            Dead();
        }
    }

    private void TryTriggerDefenseGuard(int receivedDamage, EnemyUnitBase attacker)
    {
        if (attacker == null) return;
        if (receivedDamage <= 0) return;
        if (GameRoot.Instance == null || GameRoot.Instance.IncreaMentalSystem == null) return;
        if (!GameRoot.Instance.IncreaMentalSystem.IsDefenseGuardUnlocked()) return;
        if (Random.value > 0.1f) return;

        attacker.ApplyDefenseGuardKnockback(transform.position);

        int rollbackDamage = Mathf.Max(1, Mathf.FloorToInt(receivedDamage * 0.5f));
        attacker.Damage(rollbackDamage);
    }



    public void Dead()
    {
        InGamePlayerData.IsDeadProperty.Value = true;

        ChangeState(PlayerState.Dead);
    }


    public void DeadAfter()
    {
        ProjectUtility.SetActiveCheck(this.gameObject, false);
    }

    private bool IsDamageDirect = false;


    public void DamageColorEffect()
    {
        if (!IsDamageDirect)
        {
            IsDamageDirect = true;

            // 피격 효과 적용
            foreach (var sprite in PlayerSpriteList)
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
                    foreach (var sprite in PlayerSpriteList)
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


    public void ChangeState(PlayerState state)
    {
        State = state;

        switch (state)
        {
            case PlayerState.Idle:
                Anim.Play("Idle");
                break;
            case PlayerState.Run:
                Anim.Play("Walk");
                break;
            case PlayerState.Dead:
                Anim.Play("Dead");
                break;
        }
    }
}

