using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine.UI;
using DG.Tweening;

public class EnemyUnit_Boss_Warrior : EnemyUnit
{
    private bool isCustomLanding = false;

    private BossHpComponent BossHpComponent;

    public override void Set(int idx, int unitdmg, int unithp, int deadexpvalue, int order = 0, float landingY = 0f)
    {
        // 초기 위치를 y=4로 설정
        Vector3 spawnPosition = transform.position;
        spawnPosition.y = 4f;
        transform.position = new Vector3(spawnPosition.x - 3, spawnPosition.y, spawnPosition.z);

        // 커스텀 낙하 연출 플래그 설정
        isCustomLanding = true;

        // base.Set을 호출하되, landingY를 매우 낮은 값으로 설정하여 
        // base 클래스의 스폰 시스템이 랜딩 완료로 인식하지 않도록 함
        // (현재 위치 y=4 > landingY=-10이므로 isSpawning = true로 유지됨)
        base.Set(idx , unitdmg, unithp, deadexpvalue, order, -10f);

        // 낙하 중에는 이동하지 않도록 Idle 상태로 설정
        SetState(StateType.Idle);

        // 커스텀 낙하 연출 시작
        StartBossLanding();

        BossHpComponent = GameRoot.Instance.UISystem.GetUI<PopupInGame>().GetBossHpComponent;
    }

    private void StartBossLanding()
    {
        // y=4에서 y=-0.7까지 빠르게 떨어지는 연출
        Vector3 landingPosition = transform.localPosition;
        landingPosition.y = -0.3f;

        transform.DOLocalMove(landingPosition, 0.4f)
            .SetEase(Ease.InQuad)
            .OnComplete(() =>
            {
                LandingStart();

                // 착지 시 카메라 흔들림 효과
                ShakeCamera();

                GameRoot.Instance.WaitTimeAndCallback(1f, () =>
                {
                    // 체력바 연출 시작 (연출이 끝나면 이동 시작)
                    GameRoot.Instance.UISystem.GetUI<PopupInGame>().StartBossStage(EnemyIdx, InfoData.StartHp, () =>
                    {
                        // 오브젝트 파괴/비활성화 시 콜백 스킵 (NRE 방지)
                        if (this == null || gameObject == null || !gameObject.activeInHierarchy)
                            return;

                        // 체력바 애니메이션이 완료된 후 랜딩 플래그 해제 및 이동 시작
                        isCustomLanding = false;

                        // base 클래스의 스폰 시스템도 완료된 것으로 설정
                        // (EnemyUnit의 isSpawning, isGrounded 상태 업데이트)
                        var enemyUnitType = typeof(EnemyUnit);
                        var isSpawningField = enemyUnitType.GetField("isSpawning", BindingFlags.NonPublic | BindingFlags.Instance);
                        var isGroundedField = enemyUnitType.GetField("isGrounded", BindingFlags.NonPublic | BindingFlags.Instance);
                        var directionField = enemyUnitType.GetField("Direction", BindingFlags.NonPublic | BindingFlags.Instance);

                        if (isSpawningField != null) isSpawningField.SetValue(this, false);
                        if (isGroundedField != null) isGroundedField.SetValue(this, true);
                        if (directionField != null) directionField.SetValue(this, new Vector3(-1f, 0f, 0f));

                        // 타겟 찾기 및 Move 상태로 전환
                        FindTarget();
                        SetState(StateType.Move);
                    });
                });
            });
    }

    private void ShakeCamera()
    {
        Camera mainCam = Camera.main;
        if (mainCam != null)
        {
            // 카메라의 원래 위치 저장
            Vector3 originalPos = mainCam.transform.position;

            // 카메라 흔들림 시퀀스 생성
            Sequence shakeSequence = DOTween.Sequence();

            // 0.3초 동안 카메라 흔들기
            shakeSequence.Append(mainCam.transform.DOShakePosition(0.3f, strength: 0.3f, vibrato: 20, randomness: 90, fadeOut: true));

            // 흔들림 후 원래 위치로 복귀
            shakeSequence.OnComplete(() =>
            {
                mainCam.transform.position = originalPos;
            });
        }
    }

    void Update()
    {
        // 랜딩 중에는 이동하지 않음
        if (isCustomLanding) return;

        // 랜딩이 끝난 후에는 부모 클래스의 Move 호출
        Move();
    }

    public override void Attack()
    {
        // 범위 공격: 주변의 모든 아군 유닛에게 데미지
        bool hasTargetInRange = AttackNearbyPlayerUnits();
        
        // 범위 내에 적이 없으면 Move 상태로 전환
        if (!hasTargetInRange)
        {
            FindTarget();
            SetState(StateType.Move);
        }
    }
    public override void Damage(double damage, PlayerUnit attacker = null)
    {
        if (isCustomLanding) return;

        if (IsDead) return;

        GameRoot.Instance.DamageTextSystem.ShowDamage(damage, new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), Color.white);


        InfoData.CurHp -= damage;

        BossHpComponent.SetHpText(InfoData.CurHp, InfoData.StartHp);

        DamageColorEffect();


        if (InfoData.CurHp <= 0)
        {
            if (InGameHpProgress != null)
            {
                ProjectUtility.SetActiveCheck(InGameHpProgress.gameObject, false);
            }

            Dead(); // 페이드아웃 및 삭제 처리 시작
        }
    }

    public override void Dead(bool isdirection = false)
    {
        base.Dead();


        GameRoot.Instance.UISystem.GetUI<PopupInGame>().BossStageClear();
    }

    private bool AttackNearbyPlayerUnits()
    {
        // 공격 범위 설정 (InfoData.AttackRange 사용)
        float attackRange = InfoData.AttackRange;

        // 활성화된 모든 PlayerUnit 가져오기
        var activePlayerUnits = BaseStage.PlayerUnitGroup.ActiveUnits.ToArray();

        Vector3 currentPosXZ = new Vector3(transform.position.x, 0, transform.position.z);
        
        bool hasPlayerUnitInRange = false;

        foreach (var playerUnit in activePlayerUnits)
        {
            if (playerUnit == null || !playerUnit.gameObject.activeInHierarchy || playerUnit.CurState == StateType.Dead)
                continue;

            // 거리 계산 (y축 제외)
            Vector3 playerPosXZ = new Vector3(playerUnit.transform.position.x, 0, playerUnit.transform.position.z);
            float distance = Vector3.Distance(currentPosXZ, playerPosXZ);

            // 공격 범위 안에 있으면 데미지
            if (distance <= attackRange)
            {
                hasPlayerUnitInRange = true;
                playerUnit.Damage(InfoData.Damage, true, this); // 자신을 공격자로 전달
            }
        }

        // 거리 계산 (y축 제외)
        Vector3 blockPosXZ = new Vector3(BaseStage.PlayerBlockGroup.transform.position.x, 0, BaseStage.PlayerBlockGroup.transform.position.z);
        float blockdistance = Vector3.Distance(currentPosXZ, blockPosXZ);

        bool hasBlockInRange = false;
        if (blockdistance <= InfoData.AttackRange + 1.5f)
        {
            hasBlockInRange = true;
            BaseStage.PlayerBlockGroup.Damage((int)InfoData.Damage);
        }

        // 범위 내에 PlayerUnit이나 Block 중 하나라도 있으면 true 반환
        return hasPlayerUnitInRange || hasBlockInRange;
    }


    public void LandingStart()
    {
        // 모든 활성화된 적 유닛 가져오기
        var activeEnemies = BaseStage.EnemyUnitGroup.ActiveUnits.ToArray();

        foreach (var enemy in activeEnemies)
        {
            // 보스 자신은 제외
            if (enemy == this || enemy == null || !enemy.gameObject.activeInHierarchy)
                continue;

            // 각 적 유닛을 회전시키면서 땅 아래로 떨어뜨리기
            KillEnemyWithFallAnimation(enemy);
        }
    }

    private void KillEnemyWithFallAnimation(EnemyUnit enemy)
    {
        // 범위 내 PlayerUnit들에게 데미지 주기

        // 죽은 상태로 설정하여 더 이상 이동/공격하지 않도록
        enemy.SetState(StateType.Dead);

        // HP바 숨기기
        enemy.HideHpProgress();

        // 땅 아래로 떨어지는 목표 위치 (y축으로 -5)
        Vector3 targetPosition = enemy.transform.position;
        targetPosition.y = -5f;

        // 랜덤 회전 속도 (2~4바퀴)
        float randomRotation = Random.Range(720f, 1440f);

        // 랜덤 떨어지는 시간 (0.6~1.0초)
        float randomDuration = Random.Range(0.6f, 1.0f);

        // 애니메이션 시퀀스 생성
        Sequence fallSequence = DOTween.Sequence();

        // z축 회전하면서 아래로 떨어지기 (x, y축은 고정)
        Vector3 currentRotation = enemy.transform.eulerAngles;
        fallSequence.Append(enemy.transform.DOMove(targetPosition, randomDuration).SetEase(Ease.InQuad));
        fallSequence.Join(enemy.transform.DORotate(new Vector3(currentRotation.x, currentRotation.y, randomRotation), randomDuration, RotateMode.FastBeyond360));

        // 애니메이션 완료 후 유닛 삭제
        fallSequence.OnComplete(() =>
        {
            if (enemy != null && enemy.gameObject != null)
            {
                enemy.DeleteUnit();
            }
        });
    }


}

