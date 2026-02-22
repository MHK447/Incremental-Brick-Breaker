using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class AlimentData
{
    public AlimentType Type = AlimentType.None;

    public float Time = 0f;

    public EnemyUnit Target = null;

    public double Damage = 0f;
    public float DamageDelay = 0f;

    public int StackCount = 1; // 중첩 카운트
    public int MaxStackCount = 1; // 최대 중첩 수

    protected float deltatime = 0f;


    public virtual void Set(AlimentType type, float time, EnemyUnit target, double damage, float damagedelay, int maxStackCount = 1)
    {
        this.Type = type;
        this.Time = time;
        this.Target = target;
        this.Damage = damage;
        this.DamageDelay = damagedelay;
        this.MaxStackCount = maxStackCount;

        deltatime = 0f;
    }


    public virtual void Update()
    {
        if (Time <= 0f) return;

        // 대상이 비활성화/삭제/사망한 경우 즉시 종료
        if (Target == null || Target.IsDead || Target.gameObject == null || !Target.gameObject.activeInHierarchy)
        {
            Time = 0f;
            return;
        }

        deltatime += UnityEngine.Time.deltaTime;
        if (deltatime >= DamageDelay)
        {
            Time -= DamageDelay;
            deltatime = 0f;
            OnDamage();
        }
    }


    public virtual void OnDamage()
    {
        if (Target == null || Target.IsDead || Target.gameObject == null || !Target.gameObject.activeInHierarchy) return;


        Effect();
    }

    /// <summary>
    /// 리스트에서 제거되기 직전 호출 (타겟 사망/비활성화 또는 시간 소진 시).
    /// Freeze 등 유닛 상태/비주얼을 복구할 때 사용.
    /// </summary>
    public virtual void OnRemove()
    {
    }

    public void Effect()
    {
        if (Target == null) return;

        switch (Type)
        {
            case AlimentType.Poison:
                {
                    GameRoot.Instance.EffectSystem.MultiPlay<PoisonEffect>(Target.transform.position, (x) =>
                    {
                        x.SetAutoRemove(true, 1.5f);
                    });
                    break;
                }
        }
    }

}
