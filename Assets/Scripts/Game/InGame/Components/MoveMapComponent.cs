using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveMapComponent : MonoBehaviour
{
    [Header("MoveMapComponent")]
    //===외부 요소===
    public float scrollSpeed = 2f;          // 원본 초당 이동 거리
    public Vector3 customRepeatPosition;    // 반복 시작 위치 (Local)
    public Vector3 EndPos;                  // 종료 위치 (Local)
    private Vector3 startPos;                // 시작 위치 (Local)
    public bool autoStart = true;           // 자동 시작 여부

    public bool ignoreRepeat = false;

    //===내부 요소===
    private float CurScrollSpeed = 0f;      // 비율 초당 이동 거리

    public bool IsMove { get; private set; } = false;
    public bool IsPaused { get; private set; } = false;
    public float MoveTime { get; private set; } = 0f;
    private float Deltime = 0f;

    private float TotalDistanceMoved = 0f; // 총 이동 거리
    private int Movecount = 0; // 반복 상태를 저장


    void Awake()
    {
        startPos = transform.localPosition;
        EndPos =  new Vector3(EndPos.x, transform.localPosition.y, EndPos.z);
        customRepeatPosition = new Vector3(customRepeatPosition.x, transform.localPosition.y, customRepeatPosition.z);
    }

    //===함수===
    #region 라이프 사이클
    private void OnEnable()
    {
        InitializePosition();

        if (autoStart)
        {
            StartInfiniteMove();
        }
    }

    void Update()
    {
        // 움직임이 비활성화되었거나 일시정지 상태면 리턴
        if (!IsMove || IsPaused) return;

        Deltime += Time.deltaTime;

        // 시간 제한이 있는 경우 시간 체크
        if (MoveTime > 0f)
        {
            if (Deltime >= MoveTime)
            {
                StopMove();
                return;
            }
        }

        // 진행도 계산 (무한 움직임의 경우 항상 1.0)
        float progress = MoveTime > 0f ? Mathf.Clamp01(Deltime / MoveTime) : 1f;

        // Ease InOut Quad 적용 (무한 움직임의 경우 일정한 속도)
        float easeValue = MoveTime > 0f ? EaseInOutQuad(progress) : 1f;

        // 이동 거리 계산
        float distanceToMove = CurScrollSpeed * easeValue * Time.deltaTime;
        TotalDistanceMoved += distanceToMove;

        // 현재 X 좌표 계산
        float currentX = Movecount == 0 ? startPos.x - TotalDistanceMoved : customRepeatPosition.x - TotalDistanceMoved;

        // NaN 방지
        if (float.IsNaN(currentX))
        {
            currentX = startPos.x;
        }

        transform.localPosition = new Vector3(currentX, startPos.y, startPos.z);

        // 반복 위치 확인 (ignoreRepeat가 false인 경우에만)
        if (!ignoreRepeat)
        {
            CheckRepeatPosition();
        }
    }
    #endregion

    // 초기 위치 및 상태 설정
    private void InitializePosition()
    {
        Movecount = 0;
        TotalDistanceMoved = 0f;
        transform.localPosition = startPos;
        SetSpeedRatio(1f); // 기본 속도 설정
    }

    // 무한 움직임 시작
    public void StartInfiniteMove()
    {
        IsMove = true;
        IsPaused = false;
        MoveTime = -1f; // 무한 시간을 의미
        Deltime = 0f;
    }

    // 기존 시간 제한 움직임 (호환성 유지)
    public void StartMove(float movetime)
    {
        IsMove = true;
        IsPaused = false;
        Deltime = 0f;
        MoveTime = movetime;
    }

    // 일시정지
    public void PauseMove()
    {
        IsPaused = true;
    }

    // 재개
    public void ResumeMove()
    {
        IsPaused = false;
    }

    // 움직임 완전 정지
    public void StopMove()
    {
        IsMove = false;
        IsPaused = false;
        MoveTime = 0f;
        Deltime = 0f;
    }

    public void SetSpeedRatio(float speedratio)
    {
        CurScrollSpeed = scrollSpeed * speedratio;
    }

    // 반복 위치 체크 및 리셋
    private void CheckRepeatPosition()
    {
        if (transform.localPosition.x <= EndPos.x)
        {
            float overflow = Mathf.Abs(transform.localPosition.x - EndPos.x);
            TotalDistanceMoved = overflow;
            Movecount = 1;

            transform.localPosition = new Vector3(customRepeatPosition.x, customRepeatPosition.y, customRepeatPosition.z);
        }
    }

    // Ease InOut Quad 함수
    private float EaseInOutQuad(float t)
    {
        if (t < 0.5f)
            return 2f * t * t; // Ease In
        return -1f + (4f - 2f * t) * t; // Ease Out
    }
}
