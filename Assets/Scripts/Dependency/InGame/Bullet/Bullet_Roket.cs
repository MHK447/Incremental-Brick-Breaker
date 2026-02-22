using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System;

public class Bullet_Roket : Bullet_LobShot
{
    public override void Set(BulletInfo bulletinfo, Transform shootertr, Transform targettr, Action<Bullet> onhitcallback)
    {
        base.Set(bulletinfo, shootertr, targettr, onhitcallback);
    }

    public override void OnTriggerEnter2D(Collider2D collision)
    {

        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy") && ShooterTr.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            var enemy = collision.gameObject.GetComponent<EnemyUnit>();
            if (enemy != null)
            {
                BombDamage();
                OnHitCallback?.Invoke(this);

                SoundPlayer.Instance.PlaySound("effect_bomb");

                GameRoot.Instance.EffectSystem.MultiPlay<BombEffect>(new Vector3(collision.transform.position.x, collision.transform.position.y + 0.5f, collision.transform.position.z), x =>
                                {
                                    x.SetAutoRemove(true, 2f);
                                });
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("EnemyBlockSpawner"))
        {
            var enemyblockspawner = collision.gameObject.GetComponent<EnemyBlockSpawner>();
            if (enemyblockspawner != null)
            {
                BombDamage();
                OnHitCallback?.Invoke(this);
                SoundPlayer.Instance.PlaySound("effect_bomb");

                GameRoot.Instance.EffectSystem.MultiPlay<BombEffect>(new Vector3(collision.transform.position.x, collision.transform.position.y + 0.5f, collision.transform.position.z), x =>
                              {
                                  x.SetAutoRemove(true, 2f);
                              });
            }
        }
    }


    public void BombDamage()
    {
        if (ShooterTr.gameObject.layer != LayerMask.NameToLayer("Player"))
            return;

        float bombRadius = 2f;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, bombRadius);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                var enemy = hitCollider.gameObject.GetComponent<EnemyUnit>();
                if (enemy != null && !enemy.IsDead)
                {
                    enemy.Damage(BulletInfo.AttackDamage);

                    LigihtninigEffect(enemy, BulletInfo.AttackDamage);
                    FrezeeEffectCheck(enemy);
                    PoisonEffectCheck(enemy);
                }
            }
            else if (hitCollider.gameObject.layer == LayerMask.NameToLayer("EnemyBlockSpawner"))
            {
                var enemyblockspawner = hitCollider.gameObject.GetComponent<EnemyBlockSpawner>();
                if (enemyblockspawner != null)
                {
                    enemyblockspawner.Damage((int)BulletInfo.AttackDamage);
                }
            }
        }
    }
}



