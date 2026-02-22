using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class Bullet_GravityBall : Bullet
{

    private Vector3 Direction = Vector3.right;
    private bool isGrounded = false;
    private float rollSpeed = 4f; // 굴러가는 속도
    private float spawnTime = 0f; // 생성 시간
    private const float LIFETIME = 5f; // 생존 시간 (3초)
    private float rotationAngle = 0f; // 회전 각도
    private float rotationMultiplier = 2f; // 회전 속도 배수 (더 많이 회전하게 함)

    private float GroundY = 0f;


    public override void Set(BulletInfo bulletinfo, Transform shootertr, Transform targettr, System.Action<Bullet> onhitcallback)
    {
        base.Set(bulletinfo, shootertr, targettr, onhitcallback);

        bool isad = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.PlayerUnitGroup.IsStartInterAd;

        GroundY = isad ?  0.6f : 2f;
        // 타겟 무시하고 오른쪽으로 굴러가도록 설정
        TargetTr = null;
        isGrounded = false;
        Direction = Vector3.right * rollSpeed;
        spawnTime = Time.time; // 생성 시간 기록
        rotationAngle = 0f; // 회전 각도 초기화
    }

    protected override void Move()
    {
        // 3초가 지나면 삭제
        if (Time.time - spawnTime >= LIFETIME)
        {
            if (!IsCollision)
            {
                IsCollision = true;
                if (TrailComponent != null)
                {
                    TrailComponent.SetTrailActive(false);
                }

                DisableTrail();
                OnHitCallback?.Invoke(this);
            }
            return;
        }

        // Ground에 닿지 않았을 때만 중력 적용
        if (!isGrounded)
        {
            ApplyGravity();
        }

        // Ground에 닿았으면 x축으로만 이동
        if (isGrounded)
        {
            Vector3 moveDirection = new Vector3(Direction.x, 0f, 0f);
            transform.position = transform.position + Time.deltaTime * moveDirection;
        }
        else
        {
            transform.position = transform.position + Time.deltaTime * Direction;
        }

        // 바퀴 회전 (반대 방향으로)
        if (isGrounded)
        {
            // 굴러가는 거리에 비례하여 회전 (반시계 방향)
            float distance = Mathf.Abs(Direction.x) * Time.deltaTime;
            float radius = 0.5f; // 공의 반지름 (필요에 따라 조정)
            rotationAngle -= (distance / radius) * Mathf.Rad2Deg * rotationMultiplier;
            transform.rotation = Quaternion.Euler(0, 0, rotationAngle);
        }

        // Ground 체크 및 위치 고정 (y축이 0이 될 때까지 떨어짐)
        if (transform.position.y <= GroundY)
        {
            Vector3 pos = transform.position;
            pos.y = GroundY;
            transform.position = pos;
            Direction.y = 0f; // y축 이동 제거
            isGrounded = true;
        }
    }

    public override void SetColliderSize(float size)
    {
    }

    private void ApplyGravity()
    {
        Direction.y -= 9.8f * Time.deltaTime; // 중력 가속도
    }


    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            var enemy = collision.gameObject.GetComponent<EnemyUnit>();
            if (enemy != null && !enemy.IsDead)
            {
                enemy.Damage(BulletInfo.AttackDamage);
            }

            LigihtninigEffect(enemy, BulletInfo.AttackDamage);
            FrezeeEffectCheck(enemy);
            PoisonEffectCheck(enemy);

            // OnHitCallback?.Invoke(this);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("EnemyBlockSpawner") && ShooterTr.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            var enemy = collision.gameObject.GetComponent<EnemyBlockSpawner>();
            if (enemy != null && !enemy.IsDead)
            {
                enemy.Damage((int)BulletInfo.AttackDamage);

            }
        }
    }

}

