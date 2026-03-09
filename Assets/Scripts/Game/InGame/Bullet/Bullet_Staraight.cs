using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class Bullet_Staraight : BulletBase
{
    private Vector3 direction;
    private bool isInitialized = false;

    [SerializeField]
    private TrailComponent TrailComponent;

    public override void Awake()
    {
        base.Awake();

        TrailComponent.InitTrail();
    }

    public override void Set(WeaponData weaponData, Transform shootertr, Transform targettr, System.Action<BulletBase> onhitcallback)
    {
        base.Set(weaponData, shootertr, targettr, onhitcallback);
        
        // 재활용을 위한 초기화
        isInitialized = false;
        direction = Vector3.zero;
    }


    protected override void Move()
    {
        base.Move();

        // 초기화 시점에 타겟 방향을 계산하고 저장
        if (!isInitialized)
        {
            if (TargetTr != null)
            {
                // 타겟 방향으로 초기 방향 설정
                direction = (TargetTr.position - transform.position).normalized;
            }
            else
            {
                // 타겟이 없으면 오른쪽으로 설정
                direction = Vector3.right;
            }
            
            isInitialized = true;
        }

        // 던지는 방향을 바라보도록 회전
        if (direction != Vector3.zero)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // 설정된 방향으로 이동
        float speed = (float)WeaponData.WeaponCoolTime;
        float moveDistance = speed * Time.deltaTime;

        // 이동 전 Raycast로 경로상의 충돌 체크 (근거리 충돌 누락 방지)
        Vector3 currentPos = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(currentPos, direction, moveDistance);


        transform.position += direction * moveDistance;
    }
}

