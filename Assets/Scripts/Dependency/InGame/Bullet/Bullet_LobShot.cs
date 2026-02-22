using UnityEngine;
using BanpoFri;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class Bullet_LobShot : Bullet
{
    private Vector3 startPosition;
    private Vector3 targetPosition; // 타겟의 초기 위치 저장
    private float elapsedTime = 0f;
    private float arcHeight = 2f; // 포물선의 높이
    private bool isInitialized = false;
    private Vector3 previousPosition;   
    private float minDuration = 0.5f; // 최소 비행 시간 (초)
    private bool hasReachedTarget = false; // 목표 도달 여부
    private Coroutine destroyCoroutine = null; // 삭제 코루틴

    public override void Set(BulletInfo bulletinfo, Transform shootertr, Transform targettr, System.Action<Bullet> onhitcallback)
    {
        base.Set(bulletinfo, shootertr, targettr, onhitcallback);

        // 재활용을 위한 초기화
        isInitialized = false;
        elapsedTime = 0f;
        startPosition = Vector3.zero;
        targetPosition = Vector3.zero;
        previousPosition = Vector3.zero;
        hasReachedTarget = false;

        // 이전 코루틴이 있다면 정지
        if (destroyCoroutine != null)
        {
            StopCoroutine(destroyCoroutine);
            destroyCoroutine = null;
        }
    }

    protected override void Move()
    {
        base.Move();


        // 초기화
        if (!isInitialized)
        {
            startPosition = transform.position;
            previousPosition = transform.position;
            elapsedTime = 0f;

            // 타겟 위치를 처음 한 번만 저장
            if (TargetTr != null)
            {
                targetPosition = TargetTr.position;
            }
            else
            {
                // 타겟이 없으면 현재 위치에서 앞쪽으로 설정
                targetPosition = transform.position + transform.right * 5f;
            }

            isInitialized = true;

            BpLog.Log($"[Arrow] 초기화 - 시작위치: {startPosition}, 타겟위치: {targetPosition}, 거리: {Vector3.Distance(startPosition, targetPosition)}");
        }

        // 거리 계산 (저장된 타겟 위치 사용)
        float distance = Vector3.Distance(startPosition, targetPosition);

        // 최소 비행 시간 보장 (너무 빨리 도착하면 화살이 안 보임)
        float duration = Mathf.Max(minDuration, 0.5f);


        // 시간 진행
        elapsedTime += Time.deltaTime;
        float t = Mathf.Clamp01(elapsedTime / duration);

        // 포물선 궤적 계산 (저장된 타겟 위치 사용)
        Vector3 linearPosition = Vector3.Lerp(startPosition, targetPosition, t);

        // 포물선 높이 추가 (중간에 최고점, y축은 -0.5까지 내려감)
        float heightOffset = 2f * Mathf.Sin(t * Mathf.PI);
        Vector3 arcPosition = linearPosition + Vector3.up * heightOffset;


        // 화살 방향 회전 (2D 횡스크롤용)
        Vector3 direction = (arcPosition - previousPosition).normalized;
        if (direction.magnitude > 0.001f) // 거의 0이 아닐 때만
        {
            // 2D 회전 계산 (Z축 기준)
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // 이전 위치 저장 (다음 프레임 방향 계산용)
        previousPosition = arcPosition;

        // 위치 업데이트
        transform.position = arcPosition;

        // 목표 도달 체크 (t >= 1.0)
        if (t >= 1.0f && !hasReachedTarget)
        {
            hasReachedTarget = true;
            // 0.1초 후 삭제 시작
            if (destroyCoroutine == null)
            {
                destroyCoroutine = StartCoroutine(DestroyAfterDelay(0.1f));
            }
        }
    }

    private IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 삭제 처리
        if (!IsCollision)
        {
            IsCollision = true;
            // 관통이 없으면 즉시 충돌 처리
            IsCollision = true;
            DisableTrail();
            OnHitCallback?.Invoke(this);
        }
    }

    protected virtual void DisableTrail()
    {
        if (TrailComponent != null)
        {
            TrailComponent.SetTrailActive(false);
        }
    }
}

