using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

[EffectPath("Effect/WeaponEffect", false, false)]
public class WeaponEffect : Effect
{
    [SerializeField]
    private SpriteRenderer WeaponImg;

    [SerializeField]
    private TrailComponent TrailComponent;

    private int Damage = 0;

    void Awake()
    {
        TrailComponent.InitTrail();
    }



    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            var enemy = collision.gameObject.GetComponent<EnemyUnitBase>();

            if(enemy != null)
            {
                enemy.Damage(Damage);
                SetAutoRemove(true , 0f);
            }  
        }
    }


}

