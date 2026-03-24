using UnityEngine;
using System.Collections.Generic;

public class FallWeapon_Bomb : FallWeaponBase
{
    private const float BombEffectAutoRemoveTime = 1f;
    private const float ExplosionRadius = 2f;

    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null || IsCollision)
            return;

        var collidedObj = collision.gameObject;
        if (collidedObj == null)
            return;

        int finalDmg = WeaponData.WeaponDamage
            + GameRoot.Instance.UserData.InGamePlayerData.IncreaDamageBonus;

        if (collidedObj.layer == LayerMask.NameToLayer("Enemy"))
        {
            Explode(finalDmg);
        }
        else if (collidedObj.layer == LayerMask.NameToLayer("EnemyBlockSpawner"))
        {
            Explode(finalDmg);
        }
        else if (collidedObj.layer == LayerMask.NameToLayer("Ground"))
        {
            Explode(finalDmg);
        }
    }

    private void Explode(int finalDmg)
    {
        if (IsCollision)
            return;

        IsCollision = true;
        ApplyRadiusDamage(finalDmg);
        GameRoot.Instance.EffectSystem.MultiPlay<BombEffect>(
            transform.position,
            (effect) => effect.SetAutoRemove(true, BombEffectAutoRemoveTime));
        OnHitCallback?.Invoke(this);
    }

    private void ApplyRadiusDamage(int finalDmg)
    {
        var explodedColliders = Physics2D.OverlapCircleAll(transform.position, ExplosionRadius);
        if (explodedColliders == null || explodedColliders.Length == 0)
            return;

        var hitSet = new HashSet<Collider2D>();
        foreach (var targetCol in explodedColliders)
        {
            if (targetCol == null || !hitSet.Add(targetCol))
                continue;

            var targetObj = targetCol.gameObject;
            if (targetObj == null)
                continue;

            if (targetObj.layer == LayerMask.NameToLayer("Enemy"))
            {
                var enemy = targetObj.GetComponent<EnemyUnitBase>();
                if (enemy != null)
                    enemy.Damage(finalDmg);
            }
            else if (targetObj.layer == LayerMask.NameToLayer("EnemyBlockSpawner"))
            {
                var spawner = targetObj.GetComponent<EnemyBlockSpawner>();
                if (spawner != null)
                    spawner.Damage(finalDmg);
            }
        }
    }
}

