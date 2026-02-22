using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class Bullet_LobShot_Bomb : Bullet_LobShot
{
    private int AttackRange = 2;


    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (IsCollision && remainingPenetrations <= 0) return;

        // 이미 맞춘 타겟은 무시
        if (hitTargets.Contains(collision)) return;

        // 슈터가 정리된 상태라면 더 이상 처리하지 않음
        int shooterLayer = ShooterTr != null && ShooterTr.gameObject != null ? ShooterTr.gameObject.layer : -1;
        if (shooterLayer == -1) return;


        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy") && shooterLayer == LayerMask.NameToLayer("Player"))
        {
            var enemy = collision.gameObject.GetComponent<EnemyUnit>();
            if (enemy != null && !enemy.IsDead)
            {
                hitTargets.Add(collision);
                PlayerFireBall();

                IsCollision = true;
                DisableTrail();
                OnHitCallback?.Invoke(this);
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("EnemyBlockSpawner") && shooterLayer == LayerMask.NameToLayer("Player"))
        {
            var enemy = collision.gameObject.GetComponent<EnemyBlockSpawner>();
            if (enemy != null && !enemy.IsDead)
            {
                hitTargets.Add(collision);
                PlayerFireBall();

                IsCollision = true;
                DisableTrail();
                OnHitCallback?.Invoke(this);
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Player") && shooterLayer == LayerMask.NameToLayer("Enemy"))
        {
            var player = collision.gameObject.GetComponent<PlayerUnit>();
            if (player != null && !player.IsDead)
            {
                hitTargets.Add(collision);
                EnemyFireBall();

                IsCollision = true;
                DisableTrail();
                OnHitCallback?.Invoke(this);


            }
            else if (shooterLayer == LayerMask.NameToLayer("Enemy"))
            {
                var blockGroup = collision.GetComponent<PlayerBlock>();
                if (blockGroup != null)
                {
                    hitTargets.Add(collision);
                    EnemyFireBall();

                    IsCollision = true;
                    DisableTrail();
                    OnHitCallback?.Invoke(this);
                }
            }
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            IsCollision = true;
            DisableTrail();
            OnHitCallback?.Invoke(this);
        }
    }



    public void EnemyFireBall()
    {
        if (TargetTr == null) return;

        // AttackRange 범위 내의 모든 Collider2D 찾기
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(TargetTr.position, AttackRange);

        foreach (var hitCollider in hitColliders)
        {
            // Player 레이어인 경우
            if (hitCollider.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                var player = hitCollider.gameObject.GetComponent<PlayerUnit>();
                if (player != null && !player.IsDead)
                {
                    player.Damage(BulletInfo.AttackDamage);
                }
            }
            // PlayerBlock인 경우
            else if (hitCollider.gameObject.layer == LayerMask.NameToLayer("PlayerBlock"))
            {
                var blockGroup = hitCollider.GetComponent<PlayerBlock>();
                if (blockGroup != null)
                {
                    GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.PlayerBlockGroup.Damage((int)BulletInfo.AttackDamage);
                }
            }
        }

        // 이펙트 재생
        GameRoot.Instance.EffectSystem.MultiPlay<FireBallEffect>(TargetTr.position, x =>
        {
            x.SetAutoRemove(true, 1.5f);
        });
    }


    public void PlayerFireBall()
    {
        if (TargetTr == null) return;

        // AttackRange 범위 내의 모든 Collider2D 찾기
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(TargetTr.position, AttackRange);

        foreach (var hitCollider in hitColliders)
        {
            // Player 레이어인 경우
            if (hitCollider.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                var enemy = hitCollider.gameObject.GetComponent<EnemyUnit>();
                if (enemy != null && !enemy.IsDead)
                {
                    enemy.Damage(BulletInfo.AttackDamage);
                }
            }
            // PlayerBlock인 경우
            else if (hitCollider.gameObject.layer == LayerMask.NameToLayer("EnemyBlockSpawner"))
            {
                var enemyblockspawner = hitCollider.GetComponent<EnemyBlockSpawner>();
                if (enemyblockspawner != null)
                {
                    enemyblockspawner.Damage((int)BulletInfo.AttackDamage);
                }
            }
        }

        // 이펙트 재생
        GameRoot.Instance.EffectSystem.MultiPlay<FireBallEffect>(TargetTr.position, x =>
        {
            x.SetAutoRemove(true, 1.5f);
        });
    }

}

