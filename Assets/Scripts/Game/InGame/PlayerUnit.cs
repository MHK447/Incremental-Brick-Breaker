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

    [SerializeField]
    private List<SpriteRenderer> PlayerSpriteList = new List<SpriteRenderer>();

    [SerializeField]
    private Animator Anim;

    private InGamePlayerData InGamePlayerData;

    private PlayerState State = PlayerState.Idle;

    public void Init()
    {
        WeaponController.Init();

        InGamePlayerData = GameRoot.Instance.UserData.InGamePlayerData;

        SetPlayerData();

        ChangeState(PlayerState.Run);
    }


    public void SetPlayerData()
    {
        InGamePlayerData.CurHppProperty.Value = InGamePlayerData.StartHpProperty.Value = 50;
        InGamePlayerData.CriticalChanceProperty.Value = 30;
        InGamePlayerData.CriticalDamageProperty.Value = 10;

    }

    public void Damage(int damage)
    {
        InGamePlayerData.CurHppProperty.Value -= damage;

        DamageColorEffect();

        GameRoot.Instance.DamageTextSystem.ShowDamage(damage, transform.position, Color.red);

        if (InGamePlayerData.CurHppProperty.Value <= 0)
        {
            Dead();
        }
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

