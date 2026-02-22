using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

[EffectPath("Effect/VoltageShockEffect", false, false)]
public class VoltageShockEffect : Effect
{
    [SerializeField]
    private float radius = 2f;

    private Config.UnitType UnitType;

    private double Damage = 0;

    public void Set(Config.UnitType unitType , double damage)
    {
        UnitType = unitType;
        Damage = damage;
    }

    public override void Play(Vector3 worldPos, Transform followTrans)
    {
        base.Play(worldPos, followTrans);
        
        // 반경 내의 모든 Collider2D 찾기
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(worldPos, radius);
        
        foreach (var hitCollider in hitColliders)
        {
            // UnitType에 따라 적 또는 아군에게 데미지
            if (UnitType == Config.UnitType.Enemy)
            {
                // Enemy 레이어인 경우
                if (hitCollider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
                {
                    var enemy = hitCollider.GetComponent<EnemyUnit>();
                    if (enemy != null && !enemy.IsDead)
                    {
                        enemy.Damage(Damage);
                    }
                }
            }
            else if (UnitType == Config.UnitType.Player)
            {
                // Player 레이어인 경우
                if (hitCollider.gameObject.layer == LayerMask.NameToLayer("Player"))
                {
                    var player = hitCollider.GetComponent<PlayerUnit>();
                    if (player != null && !player.IsDead)
                    {
                        player.Damage(Damage);
                    }
                }
            }
        }
    }

}

