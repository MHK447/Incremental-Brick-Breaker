using UnityEngine;
using UnityEngine.UI;
using BanpoFri;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Unity.Mathematics;

public class EnemyUnit : UnitBase
{
    [HideInInspector]
    public int EnemyIdx = 0;


    protected PlayerUnit UnitTarget;
    protected PlayerBlockGroup BlockTarget;


    // 스폰 관련 변수
    private bool isSpawning = true; // 스폰 중인지 여부
    private bool isGrounded = false; // 착지했는지 여부
    private int spawnOrder = 0; // 스폰 순서
    private float groundY = 0f; // 착지할 y 값
    private Vector3 Direction;

    private int DeadExpValue = 0;

    [HideInInspector]
    public bool IsBossUnit = false;




    public virtual void Set(int idx, int unitdmg, int unithp, int deadexpvalue, int order = 0, float landingY = 0f)
    {
        // 재활용을 위한 초기화
        ResetUnit();

        EnemyIdx = idx;
        DeadExpValue = deadexpvalue;
        spawnOrder = order;
        groundY = landingY; // 착지 y값 설정

        ShadowTr.localPosition = InitShadowPos;

        BaseStage = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage;
        BlockTarget = BaseStage.PlayerBlockGroup;
        SetInfo(unitdmg, unithp);

        UnitTarget = null;

        UnitColliderAction.AttackAction = Attack;

        // 스프라이트 알파값 복원
        foreach (var sprite in UnitSpriteList)
        {
            if (sprite == null) continue;
            Color color = sprite.color;
            color.a = 1f;
            sprite.color = color;
        }

        transform.localScale = new Vector3(-1f, 1f, 1);

        // 스폰 시작 (상태는 InitializeSpawn에서 설정)
        InitializeSpawn();
    }

    private void InitializeSpawn()
    {
        // 현재 위치가 groundY보다 높은지 확인
        float currentY = transform.position.y;

        if (currentY > groundY)
        {
            // 떨어져야 하는 경우
            isSpawning = true;
            isGrounded = false;

            // 초기 방향 설정 (포물선 운동 시작 - 왼쪽 위로)
            Direction = new Vector3(-2f, 1.5f, 0f);

            // 스폰 중에는 Idle 애니메이션
            SetState(StateType.Idle);
        }
        else
        {
            // 이미 땅에 있는 경우 바로 Move 상태로
            isSpawning = false;
            isGrounded = true;

            // 수평 이동 방향
            Direction = new Vector3(-1f, 0f, 0f);

            // y 위치를 groundY로 보정
            transform.position = new Vector3(transform.position.x, groundY, transform.position.z);

            // StartAnimName 초기화 후 Move 상태로 설정
            StartAnimName = "";
            SetState(StateType.Move);
        }

        Debug.Log($"[EnemySpawn] InitializeSpawn - EnemyIdx: {EnemyIdx}, SpawnOrder: {spawnOrder}, GroundY: {groundY}, StartPos: {transform.position}, IsSpawning: {isSpawning}");
    }



    public void HPMaxIncreaseCheck()
    {
        var getmodifier = GameRoot.Instance.InGameUpgradeSystem.GetModifier(Config.InGameUpgradeChoice.IncreaseHpMax);

        if (getmodifier != null)
        {
            if (getmodifier.Value_1 > 0)
            {
                var ishpmaxp = ProjectUtility.IsPercentSuccess(getmodifier.Value_2);

                if (ishpmaxp)
                {
                    GameRoot.Instance.UserData.Playerdata.StartHpProperty.Value += (int)getmodifier.Value_1;
                }
            }
        }
    }
    public void InGameUpgradeVoltEffectCheck()
    {
        var getmodifier = GameRoot.Instance.InGameUpgradeSystem.GetModifier(Config.InGameUpgradeChoice.ElectricShock);

        if (getmodifier != null)
        {
            if (getmodifier.Value_1 > 0)
            {
                var isvoltcheck = ProjectUtility.IsPercentSuccess(getmodifier.Value_2);

                if (isvoltcheck)
                {
                    GameRoot.Instance.EffectSystem.MultiPlay<VoltageShockEffect>(transform.position, x =>
                    {
                        //적에 전체체력에 퍼센트

                        var damagevalue = ProjectUtility.PercentCalc(InfoData.StartHp, getmodifier.Value_1);

                        if (damagevalue == 0) damagevalue = 0.1f;

                        SoundPlayer.Instance.PlaySound("effect_bomb");

                        x.Set(Config.UnitType.Enemy, damagevalue);
                        x.SetAutoRemove(true, 1.5f);
                    });
                }
            }
        }
    }






    public virtual void SetInfo(int unitdmg, int unithp)
    {
        base.SetInfo();

        var td = Tables.Instance.GetTable<EnemyInfo>().GetData(EnemyIdx);

        if (td != null)
        {
            DataInit(unitdmg, unithp);
            SetHp(InfoData.StartHp);
            SyncAnimationSpeed();
        }
    }

    public virtual void DataInit(int unitdmg, int unithp)
    {
        var td = Tables.Instance.GetTable<EnemyInfo>().GetData(EnemyIdx);

        if (td != null)
        {
            InfoData.AttackRange = (float)td.atk_range_factor / 100f;
        }

        InfoData.AttackSpeed = (float)td.attack_speed / 100f;
        // WaveInfo에서 전달받은 HP와 Damage 사용
        InfoData.StartHp = unithp;
        InfoData.CurHp = InfoData.StartHp;
        InfoData.Damage = unitdmg;
        InfoData.MoveSpeed = (float)td.move_speed / 100f;

        IsBossUnit = td.boss_unit > 0 ? true : false;
    }

    public void FindTarget()
    {
        if (this == null || gameObject == null || !gameObject.activeInHierarchy)
            return;
        if (BaseStage == null || BaseStage.PlayerUnitGroup == null)
        {
            UnitTarget = null;
            return;
        }

        // 활성화된 PlayerUnit만 필터링
        var activePlayerUnits = BaseStage.PlayerUnitGroup.ActiveUnits.ToArray();

        if (activePlayerUnits.Length > 0)
        {
            // AttackRange 안에 있는 가장 가까운 PlayerUnit 찾기
            PlayerUnit closestUnit = null;
            float closestDistance = float.MaxValue;

            Vector3 myPos = transform.position;

            foreach (var unit in activePlayerUnits)
            {
                if (unit == null || unit.gameObject == null || !unit.gameObject.activeInHierarchy)
                    continue;

                // y축 offset 적용 (1만큼 거리 감소 효과)
                float yOffset = 1f;
                Vector3 unitPos = unit.transform.position;
                float yDifference = Mathf.Abs(myPos.y - unitPos.y);
                float adjustedYDifference = Mathf.Max(0f, yDifference - yOffset);

                // x, z축 거리 계산
                Vector2 currentPosXZ = new Vector2(myPos.x, myPos.z);
                Vector2 unitPosXZ = new Vector2(unitPos.x, unitPos.z);
                float xzDistance = Vector2.Distance(currentPosXZ, unitPosXZ);

                // y축 offset 적용한 전체 거리 계산
                float distance = Mathf.Sqrt(xzDistance * xzDistance + adjustedYDifference * adjustedYDifference);

                // AttackRange 내에 있고, 가장 가까운 유닛만 선택
                if (distance <= InfoData.AttackRange && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestUnit = unit;
                }
            }

            UnitTarget = closestUnit;
        }
        else
        {

            // PlayerUnit이 없으면 BlockTarget을 타겟으로 설정
            UnitTarget = null;
        }
    }


    public void Move()
    {
        if (CurState == StateType.Dead) return;
        if (CurState == StateType.KnockBack) return; // 넉백 중에는 이동/공격 불가
        if (CurState == StateType.Sturn) return; // 스턴 중에는 이동/공격 불가
        if (CurState == StateType.Attack) return; // 공격 중에는 Move 로직 실행하지 않음 (상태 전환 방지)

        // UnitTarget이 있지만 비활성화되었거나 죽은 상태면 타겟 해제 (성 공격으로 복귀)
        if (UnitTarget != null && (!UnitTarget.gameObject.activeInHierarchy || UnitTarget.CurState == StateType.Dead))
        {
            UnitTarget = null;
        }

        // UnitTarget이 null인 경우 플레이어 유닛을 찾아서 타겟으로 설정
        if (UnitTarget == null)
        {
            FindTarget();
        }

        // 현재 타겟 결정 (PlayerUnit이 있으면 우선, 없으면 BlockTarget)
        Vector3 targetPosition;

        if (UnitTarget != null && UnitTarget.gameObject.activeInHierarchy && UnitTarget.CurState != StateType.Dead)
        {
            targetPosition = UnitTarget.transform.position;
        }
        else if (BlockTarget != null)
        {
            targetPosition = BlockTarget.transform.position;
        }
        else
        {
            // Move 상태가 아니면 Move 상태로 전환
            if (CurState != StateType.Move)
            {
                SetState(StateType.Move);
            }

            // 타겟이 없으면 기본 방향(왼쪽)으로 보게 설정
            transform.localScale = new Vector3(-1f, 1f, 1);
            return;
        }

        // 타겟 방향에 따라 유닛 방향 전환
        UpdateFacingDirection(targetPosition);

        // 타겟까지의 거리 계산 (y축 제외)
        Vector3 currentPosXZ = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 targetPosXZ = new Vector3(targetPosition.x, 0, targetPosition.z);
        float distanceToTarget = Vector3.Distance(currentPosXZ, targetPosXZ);

        // 공격 범위 안에 있으면 공격
        if (distanceToTarget <= InfoData.AttackRange)
        {
            SetState(StateType.Attack);
        }
        else
        {
            // Move 상태가 아니면 Move 상태로 전환
            if (CurState != StateType.Move)
            {
                SetState(StateType.Move);
            }

            // 공격 범위 밖이면 이동 (y축은 이동하지 않음)
            Vector3 direction = (targetPosition - transform.position);
            direction.y = 0; // y축 이동 제거
            direction.Normalize();

            // SnailSlime 효과 적용된 이동속도 계산
            float snailspeed = GetSnailSlimeAppliedMoveSpeed();

            // y축 위치는 유지하면서 이동
            Vector3 movement = direction * (InfoData.MoveSpeed - snailspeed) * Time.deltaTime;
            transform.position += movement;
        }
    }

    private float GetSnailSlimeAppliedMoveSpeed()
    {
        var snailSlimeModifier = GameRoot.Instance.InGameUpgradeSystem.GetModifier(Config.InGameUpgradeChoice.SnailSlime);

        if (snailSlimeModifier != null && snailSlimeModifier.Value_1 > 0)
        {
            return ProjectUtility.PercentCalc(InfoData.MoveSpeed, snailSlimeModifier.Value_1);
        }

        return 0;
    }

    public override void Attack()
    {
        base.Attack();

        // 타겟 공격 (타겟이 살아있을 때만)
        if (UnitTarget != null && UnitTarget.gameObject.activeInHierarchy && UnitTarget.CurState != StateType.Dead)
        {
            UnitTarget.Damage(InfoData.Damage, false, this); // 자신을 공격자로 전달
        }
        else if (BlockTarget != null)
        {
            // BlockTarget까지의 거리 계산 (y축 제외)
            Vector3 currentPosXZ = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 blockPosXZ = new Vector3(BlockTarget.transform.position.x, 0, BlockTarget.transform.position.z);
            float distanceToBlock = Vector3.Distance(currentPosXZ, blockPosXZ);

            // 공격 범위 안에 있을 때만 공격
            if (distanceToBlock <= InfoData.AttackRange)
            {
                BlockTarget.Damage((int)InfoData.Damage, this);
            }
            else
            {
                // 공격 범위를 벗어났으면 Move 상태로 전환
                SetState(StateType.Move);
            }
        }
        else
        {
            // 타겟이 없으면 Move 상태로 전환
            SetState(StateType.Move);
        }
    }

    // Update 메서드에서 Move 호출
    public virtual void Update()
    {
        // 스폰 중일 때는 중력 적용 및 착지 체크
        if (isSpawning && !isGrounded)
        {
            ApplyGravity();
            transform.position += Direction * Time.deltaTime;
            CheckGrounded();
        }
        else
        {
            // 공격 중일 때 타겟이 죽었는지 확인
            if (CurState == StateType.Attack)
            {
                if (UnitTarget != null && (!UnitTarget.gameObject.activeInHierarchy || UnitTarget.CurState == StateType.Dead))
                {
                    // 범위 내에 다른 플레이어 유닛이 있는지 확인
                    PlayerUnit newTarget = FindPlayerUnitInRange();

                    if (newTarget != null)
                    {
                        // 범위 내에 플레이어 유닛이 있으면 타겟만 교체하고 공격 상태 유지
                        UnitTarget = newTarget;
                    }
                    else
                    {
                        // 범위 내에 플레이어 유닛이 없으면 Move 상태로 전환 (성 공격으로 복귀)
                        UnitTarget = null;
                        SetState(StateType.Move);
                    }
                }
                else if (UnitTarget == null && BlockTarget != null)
                {
                    // BlockTarget을 공격 중일 때 거리 체크
                    Vector3 currentPosXZ = new Vector3(transform.position.x, 0, transform.position.z);
                    Vector3 blockPosXZ = new Vector3(BlockTarget.transform.position.x, 0, BlockTarget.transform.position.z);
                    float distanceToBlock = Vector3.Distance(currentPosXZ, blockPosXZ);

                    // 공격 범위를 벗어났으면 Move 상태로 전환
                    if (distanceToBlock > InfoData.AttackRange)
                    {
                        SetState(StateType.Move);
                    }
                }
            }

            // 착지 후에는 정상 이동
            Move();
        }
    }

    // 공격 범위 내에서 플레이어 유닛을 찾는 메서드
    private PlayerUnit FindPlayerUnitInRange()
    {
        var activePlayerUnits = BaseStage.PlayerUnitGroup.ActiveUnits.ToArray();

        if (activePlayerUnits.Length > 0)
        {
            // 공격 범위 내에서 가장 가까운 플레이어 유닛 찾기
            PlayerUnit closestUnit = null;
            float closestDistance = float.MaxValue;

            foreach (var unit in activePlayerUnits)
            {
                if (unit.gameObject.activeInHierarchy && unit.CurState != StateType.Dead)
                {
                    // y축 제외한 거리 계산
                    Vector3 currentPosXZ = new Vector3(transform.position.x, 0, transform.position.z);
                    Vector3 targetPosXZ = new Vector3(unit.transform.position.x, 0, unit.transform.position.z);
                    float distance = Vector3.Distance(currentPosXZ, targetPosXZ);

                    if (distance <= InfoData.AttackRange && distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestUnit = unit;
                    }
                }
            }

            return closestUnit;
        }

        return null;
    }

    private void CheckGrounded()
    {
        float currentY = transform.position.y;

        // y축이 groundY 이하로 떨어지고, 아래로 떨어지고 있을 때만 착지로 간주
        if (currentY <= groundY && Direction.y <= 0)
        {
            isGrounded = true;
            isSpawning = false;

            // 착지 후 수평 이동으로 전환
            Direction = new Vector3(-1f, 0f, 0f);

            // y 위치 보정 (땅에 정확히 맞춤)
            Vector3 landingPos = new Vector3(transform.position.x, groundY, transform.position.z);
            transform.position = landingPos;

            Debug.Log($"[EnemySpawn] Landed - EnemyIdx: {EnemyIdx}, SpawnOrder: {spawnOrder}, Landing Pos: {landingPos}");

            // 착지 후 가까운 플레이어 유닛을 타겟으로 설정
            FindTarget();

            // StartAnimName 초기화 후 Move 상태로 전환하여 Walking 애니메이션 재생
            StartAnimName = "";
            SetState(StateType.Move);
        }
    }

    private void ApplyGravity()
    {
        // 중력 적용 (아래로 가속)
        Direction.y += -9.8f * Time.deltaTime;

        // x축 속도는 일정하게 유지
        // Direction.x는 이미 초기값으로 설정되어 있으므로 그대로 유지
    }


    public virtual void Damage(double damage, PlayerUnit attacker = null)
    {
        if (IsDead) return;

        if (InGameHpProgress == null)
        {
            Debug.LogWarning("InGameHpProgress is null when taking damage!");
            return;
        }

        if (CurState != StateType.Dead && InGameHpProgress != null && !InGameHpProgress.gameObject.activeSelf)
        {
            InGameHpProgress.UpdatePos(); // 활성화 직후 위치 즉시 업데이트
            ProjectUtility.SetActiveCheck(InGameHpProgress.gameObject, true);
        }

        // 어그로 시스템: 공격자가 있고 살아있으면 타겟으로 변경
        if (attacker != null && attacker.gameObject.activeInHierarchy && attacker.CurState != StateType.Dead)
        {
            // 1. 현재 타겟이 없거나 (성을 공격 중)
            // 2. 현재 타겟이 죽었을 때
            // -> 공격자를 새로운 타겟으로 설정
            if (UnitTarget == null || UnitTarget.CurState == StateType.Dead)
            {
                UnitTarget = attacker;
                Debug.Log($"[EnemyUnit] Target changed to attacker: {attacker.name}");
            }
        }

        GameRoot.Instance.DamageTextSystem.ShowDamage(damage, new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), Color.white);


        InfoData.CurHp -= damage;
        SoundPlayer.Instance.PlaySound("effect_damage");

        InGameHpProgress.SetHpText(InfoData.CurHp, InfoData.StartHp);

        DamageColorEffect();

        // 넉백 체크
        KnockBack();

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

        GoblinBagEffectCheck();
        InGameUpgradeVoltEffectCheck();
        HPMaxIncreaseCheck();
        DeadEffect();
    }

    public override void SetAnim()
    {
        base.SetAnim();
    }


    private void UpdateFacingDirection(Vector3 targetPosition)
    {
        // 타겟이 왼쪽에 있으면 -1, 오른쪽에 있으면 1
        if (targetPosition.x < transform.position.x)
        {
            // 타겟이 왼쪽에 있음
            transform.localScale = new Vector3(-1f, 1f, 1);
        }
        else
        {
            // 타겟이 오른쪽에 있음
            transform.localScale = new Vector3(1f, 1f, 1);
        }
    }


    public override void DeleteUnit()
    {
        base.DeleteUnit();

        if (InGameHpProgress != null)
        {
            ProjectUtility.SetActiveCheck(InGameHpProgress.gameObject, false);
        }

        ProjectUtility.SetActiveCheck(gameObject, false);
        BaseStage.EnemyUnitGroup.DeleteUnit(this);
    }

    public void KnockBack()
    {
        var findmodifier = GameRoot.Instance.InGameUpgradeSystem.GetModifier(Config.InGameUpgradeChoice.KnockBackGun);

        if (findmodifier != null && !IsBossUnit)
        {
            var isknockbackcheck = ProjectUtility.IsPercentSuccess(findmodifier.Value_2);

            if (isknockbackcheck)
            {
                // 넉백 거리 (Value_2를 100으로 나누어 적절한 거리로 변환, 0.5~2.0 범위로 제한)
                float knockbackDistance = findmodifier.Value_2 > 0 ? Mathf.Clamp(findmodifier.Value_1, 0.5f, 2.0f) : 1f;

                SetState(StateType.KnockBack);

                // 현재 위치에서 오른쪽으로 밀려남
                Vector3 knockbackPosition = transform.position + Vector3.right * knockbackDistance;

                // DOTween을 사용하여 부드럽게 넉백하고, 완료되면 Idle 상태로 전환
                transform.DOMove(knockbackPosition, 0.2f).SetEase(Ease.OutQuad).OnComplete(() =>
                {
                    if (this != null && CurState == StateType.KnockBack)
                    {
                        SetState(StateType.Idle);
                    }
                });
            }
        }
    }

    public void KnockBackStart()
    {
        // 넉백 거리 (Value_2를 100으로 나누어 적절한 거리로 변환, 0.5~2.0 범위로 제한)
        float knockbackDistance = 1f;

        SetState(StateType.KnockBack);

        // 현재 위치에서 오른쪽으로 밀려남
        Vector3 knockbackPosition = transform.position + Vector3.right * knockbackDistance;

        // DOTween을 사용하여 부드럽게 넉백하고, 완료되면 Idle 상태로 전환
        transform.DOMove(knockbackPosition, 0.2f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            if (this != null && CurState == StateType.KnockBack)
            {
                SetState(StateType.Idle);
            }
        });
    }

    public bool BlockRangeTarget()
    {
        if (BlockTarget == null) return false;


        var distance = Vector3.Distance(transform.position, BlockTarget.transform.position);

        if (distance <= InfoData.AttackRange + 0.5f)
        {
            return true;
        }

        return false;
    }


    public void GoblinBagEffectCheck()
    {
        var findmodifier = GameRoot.Instance.InGameUpgradeSystem.GetModifier(Config.InGameUpgradeChoice.GoblinBag);

        if (findmodifier != null)
        {
            if (ProjectUtility.IsPercentSuccess(findmodifier.Value_2))
            {
                SpriteThrowEffectParameters coinparameters = new()
                {
                    sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, "Common_Currency_Money"),
                    scale = 0.7f,
                    duration = 1.2f,
                };
                GameRoot.Instance.EffectSystem.MultiPlay<SpriteThrowEffect>(transform.position, (x) =>
                  {
                      var target = GameRoot.Instance.UISystem.GetUI<PopupInGame>().SilverCoinRoot;

                      x.ShowWorldPos(transform.position, target, () =>
                                       {
                                           GameRoot.Instance.UserData.Ingamesilvercoin.Value += (int)findmodifier.Value_1; ;
                                           target.DOScale(1.3f, 0.15f).SetEase(DG.Tweening.Ease.OutCubic).SetUpdate(true).SetLoops(2, DG.Tweening.LoopType.Yoyo);
                                       }, coinparameters);


                      x.SetAutoRemove(true, 2f);
                  });
            }
        }
    }


    public void DeadEffect()
    {
        GameRoot.Instance.UserData.Playerdata.KillCountProperty.Value += 1;

        if (GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.ExpOpen))
        {
            var target = GameRoot.Instance.UISystem.GetUI<PopupInGame>().ExpIconRoot;

            GameRoot.Instance.UserData.Playerdata.InGameExpProperty.Value += DeadExpValue;
        }

        // KillUnitSilverCoinIncrease 버프 적용
        KillUnitSilverCoinEffect();
        // 세트 버프: Kill20CountAddSilverCoin - 20킬마다 추가 은화
        Kill20SetSilverCoinEffect();
    }

    public void Kill20SetSilverCoinEffect()
    {
        var killCount = GameRoot.Instance.UserData.Playerdata.KillCountProperty.Value;
        if (killCount % 20 != 0) return;

        var bonus = GameRoot.Instance.HeroSystem.GetSetBuffValue(HeroItemSetType.Kill20CountAddSilverCoin);
        if (bonus <= 0) return;

        SpriteThrowEffectParameters coinparameters = new()
        {
            sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, "Common_Currency_Money"),
            scale = 0.7f,
            duration = 1.2f,
        };
        var target = GameRoot.Instance.UISystem.GetUI<PopupInGame>().SilverCoinRoot;
        GameRoot.Instance.EffectSystem.MultiPlay<SpriteThrowEffect>(transform.position, (x) =>
        {
            x.ShowWorldPos(transform.position, target, () =>
            {
                GameRoot.Instance.UserData.Ingamesilvercoin.Value += bonus;
                target.DOScale(1.3f, 0.15f).SetEase(DG.Tweening.Ease.OutCubic).SetUpdate(true).SetLoops(2, DG.Tweening.LoopType.Yoyo);
            }, coinparameters);
            x.SetAutoRemove(true, 2f);
        });
    }

    public void KillUnitSilverCoinEffect()
    {
        var killCoinBonus = GameRoot.Instance.HeroSystem.GetGradeBuffTypeValue(GradeBuffType.KillUnitSilverCoinIncrease);

        if (killCoinBonus > 0)
        {
            SpriteThrowEffectParameters coinparameters = new()
            {
                sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, "Common_Currency_Money"),
                scale = 0.7f,
                duration = 1.2f,
            };
            GameRoot.Instance.EffectSystem.MultiPlay<SpriteThrowEffect>(transform.position, (x) =>
              {
                  var target = GameRoot.Instance.UISystem.GetUI<PopupInGame>().SilverCoinRoot;

                  x.ShowWorldPos(transform.position, target, () =>
                                   {
                                       GameRoot.Instance.UserData.Ingamesilvercoin.Value += killCoinBonus;
                                       target.DOScale(1.3f, 0.15f).SetEase(DG.Tweening.Ease.OutCubic).SetUpdate(true).SetLoops(2, DG.Tweening.LoopType.Yoyo);
                                   }, coinparameters);


                  x.SetAutoRemove(true, 2f);
              });
        }
    }


}
