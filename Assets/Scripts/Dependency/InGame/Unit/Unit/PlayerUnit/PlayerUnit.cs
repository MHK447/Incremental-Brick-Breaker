using UnityEngine;
using BanpoFri;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using DG.Tweening;
public class PlayerUnit : UnitBase
{
    [HideInInspector]
    public int PlayerUnitIdx = 0;

    [HideInInspector]
    public int PlayerGrade = 0;

    protected EnemyUnit UnitTarget;

    protected EnemyBlockSpawner EnemyBlockSpawner;

    // 버서커 상태 추적
    private bool isBerserkerMode = false;

    // 스폰 관련 변수
    private bool isSpawning = true; // 스폰 중인지 여부
    private bool isGrounded = false; // 착지했는지 여부
    private int blockOrder = 0; // 스폰된 블록의 순서
    private float groundY = 0.8f; // 착지할 y 값 (BlockOrder * 0.8f)
    private float verticalVelocity = 0f; // y축 속도

    // Taunt 상태 추적
    private bool isTaunting { get { return CurState == StateType.Taunt; } }


    protected Vector3 Direction;

    private GameObject UnitRoot;

    private Vector3 OriginalScale;


    public virtual void Set(int idx, int grade, int order = 0)
    {
        // 재활용을 위한 초기화
        ResetUnit();

        // DOTween 애니메이션 확실히 정리 (승리 애니메이션 등에서 시작된 애니메이션 정리)
        transform.DOKill();

        PlayerUnitIdx = idx;
        PlayerGrade = grade;
        blockOrder = order;

        ShadowTr.localPosition = InitShadowPos;

        UnitTarget = null;

        // 버서커 상태 초기화
        isBerserkerMode = false;

        // 스폰 상태 초기화 (InitializeSpawn에서 설정될 것이므로 여기서는 기본값만)
        verticalVelocity = 0f;

        // 초기 로컬 스케일 설정
        transform.localScale = new Vector3(1, 1, 1);

        // 초기 로테이션 설정
        transform.rotation = Quaternion.Euler(0, 0, 0);

        SetInfo();

        BaseStage = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage;

        EnemyBlockSpawner = BaseStage.EnemyUnitGroup.EnemyBlockSpawner;

        // 스폰 초기화 (SetUnitRoot 전에 먼저 호출하여 상태 설정)
        InitializeSpawn();

        SetUnitRoot();

        

        // 초기 상태는 Addressable 로딩 완료 후 설정됨
    }


    public void SetUnitRoot()
    {
        if (UnitRoot != null)
        {
            Addressables.ReleaseInstance(UnitRoot);
            UnitRoot = null;
        }


        Addressables.InstantiateAsync($"Unit_{PlayerUnitIdx}_{PlayerGrade}").Completed += (handle) =>
        {
            UnitRoot = handle.Result;
            UnitRoot.transform.SetParent(transform, false);
            UnitRoot.transform.localPosition = Vector3.zero;
            UnitRoot.transform.localRotation = Quaternion.Euler(0, 0, 0);
            Anim = UnitRoot.GetComponent<Animator>();
            UnitColliderAction = UnitRoot.GetComponent<ColliderAction>();
            UnitColliderAction.AttackAction = Attack;

            // OriginalScale 저장 시 정상적인 스케일인지 확인
            if (Anim.transform.localScale.magnitude < 0.5f)
            {
                // 비정상적으로 작은 스케일이면 기본값으로 설정
                Anim.transform.localScale = Vector3.one;
            }
            OriginalScale = Anim.transform.localScale;


            UnitSpriteList.Clear();

            // UnitRoot의 자식에서 모든 SpriteRenderer 가져오기
            SpriteRenderer[] sprites = UnitRoot.GetComponentsInChildren<SpriteRenderer>();
            UnitSpriteList.AddRange(sprites);

            // SpriteRenderer 가져오기 및 알파값 복원
            foreach (var sprite in UnitSpriteList)
            {
                if (sprite == null) continue;
                Color color = sprite.color;
                color.a = 1f;
                sprite.color = Color.white;
            }

            SyncAnimationSpeed();

            // Animator 초기화를 위해 한 프레임 대기 후 상태 설정
            GameRoot.Instance.StartCoroutine(SetInitialStateAfterFrame());
            SetHp(InfoData.CurHp);
        };




    }

    private void InitializeSpawn()
    {
        // 스테이지 y 오프셋 반영 (NoInterstitialAds 시 0, 아니면 1.5)
        float stageY = BaseStage.PlayerUnitGroup.IsStartInterAd ? 0f : 0.62f;

        // BlockOrder에 따라 착지 y값 설정 (블록 높이와 동일), 스테이지 높이만큼 보정
        float localGroundY = blockOrder * stageY;
        if (localGroundY > stageY || localGroundY <  stageY + 0.6f)
        {
            localGroundY = Random.Range(stageY, stageY + 0.6f);
        }
        groundY = stageY + localGroundY;

        // 현재 태어난 위치의 y값을 확인
        float currentY = transform.position.y;

        // y축 속도 초기화
        verticalVelocity = 0f;

        if (currentY > groundY)
        {
            // 현재 y 위치가 groundY보다 높으면 떨어지기 시작
            isSpawning = true;
            isGrounded = false;

            // 초기 방향 설정 (수평 이동)
            Direction = new Vector3(1f, 0f, 0f);
        }
        else
        {
            // 현재 y 위치가 groundY 이하면 바로 착지 상태로 시작
            isSpawning = false;
            isGrounded = true;

            // 수평 이동으로 시작
            Direction = new Vector3(1f, 0f, 0f);

            // y 위치를 groundY로 보정
            transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
        }
    }

    private IEnumerator SetInitialStateAfterFrame()
    {
        // 한 프레임 대기하여 Animator가 완전히 초기화되도록 함
        yield return new WaitForSeconds(0.01f);

        // Animator Controller가 준비되었는지 확인
        if (Anim != null && Anim.runtimeAnimatorController != null)
        {
            // StartAnimName을 초기화하여 애니메이션이 강제로 재생되도록 함
            StartAnimName = "";
            SetState(StateType.Move);
        }
    }


    public override void SetState(StateType state)
    {
        // Attack 상태로 전환될 때 스케일 보정
        if (state == StateType.Attack && Anim != null && OriginalScale != Vector3.zero)
        {
            if (Anim.transform.localScale.magnitude < 0.5f)
            {
                Anim.transform.localScale = OriginalScale;
            }
        }

        base.SetState(state);
    }

    public override void SetInfo()
    {
        base.SetInfo();

        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;

        var td = Tables.Instance.GetTable<UnitInfo>().GetData(PlayerUnitIdx);

        DataInit();
        SetHp(InfoData.StartHp);

        // 중복 구독 방지: 기존 구독 해제 후 재구독
        GameRoot.Instance.InGameUpgradeSystem.StateRefreshEvent -= SetInfo;
        GameRoot.Instance.InGameUpgradeSystem.StateRefreshEvent += SetInfo;
    }

    public override void DataInit()
    {
        base.DataInit();

        var td = Tables.Instance.GetTable<UnitInfo>().GetData(PlayerUnitIdx);

        var baseattackspeed = (float)td.base_atk_speed / 100f;

        if (PlayerGrade > 1)
        {
            baseattackspeed += ProjectUtility.PercentCalc(baseattackspeed, PlayerGrade * 10f);
        }

        if (td != null)
        {
            InfoData.AttackRange = (float)td.attack_range / 100f;
            InfoData.AttackSpeed = baseattackspeed;
            InfoData.MoveSpeed = (float)td.base_move_speed / 100f;

            var traininghpvalue = GameRoot.Instance.TrainingSystem.GetBuffValue(TrainingSystem.TrainingType.UnitHpIncrease);

            var cardhpvalue = GameRoot.Instance.CardSystem.GetCardValue(PlayerUnitIdx, UnitStatusType.Hp);

            var hp = (int)((td.base_hp + traininghpvalue + cardhpvalue) * PlayerGrade);

            var hpbuff = 0;

            if (PlayerUnitIdx == 1)
            {
                hpbuff = GameRoot.Instance.HeroSystem.GetGradeBuffTypeValue(GradeBuffType.WarriorHpIncrease);

                hpbuff = (int)ProjectUtility.PercentCalc(hp, hpbuff);
            }

            hp += hpbuff;

            InfoData.StartHp = InfoData.CurHp = hp;

            var trainingvalue = GameRoot.Instance.TrainingSystem.GetBuffValue(TrainingSystem.TrainingType.UnitAttackDamage);

            var carddamage = GameRoot.Instance.CardSystem.GetCardValue(PlayerUnitIdx, UnitStatusType.Attack);

            var unitbuff = 0;

            var damage = ((int)(td.base_dmg + trainingvalue + carddamage) * PlayerGrade);

            if (PlayerUnitIdx == 1)
            {
                unitbuff = GameRoot.Instance.HeroSystem.GetGradeBuffTypeValue(GradeBuffType.WarriorUnitAttackIncrease);

                unitbuff = (int)ProjectUtility.PercentCalc(damage , unitbuff);
            }

            damage += unitbuff;

            InfoData.Damage = damage;
        }
    }


    /// <summary> 사거리 안일 때 Attack 상태로 전환해도 되는지 (Magic은 쿨타임 체크용 오버라이드)
    /// </summary>
    protected virtual bool CanTransitionToAttack()
    {
        return true;
    }

    public void FindTarget()
    {
        // 모든 PlayerUnit을 찾기
        EnemyUnit[] allPlayerUnits = BaseStage.EnemyUnitGroup.ActiveUnits.ToArray();

        // 활성화되어 있고 Dead 상태가 아닌 PlayerUnit만 필터링
        var activePlayerUnits = allPlayerUnits.Where(unit => unit.gameObject.activeInHierarchy && unit.CurState != StateType.Dead).ToArray();

        if (activePlayerUnits.Length > 0)
        {
            // 가장 가까운 PlayerUnit 찾기
            EnemyUnit closestUnit = null;
            float closestDistance = float.MaxValue;

            foreach (var unit in activePlayerUnits)
            {
                Vector3 adjustedPosition = unit.transform.position;
                adjustedPosition.y += 1f; // y축에 +1을 더해서 더 가까워지게
                float distance = Vector3.Distance(transform.position, adjustedPosition);
                if (distance < closestDistance)
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
        if (CurState == StateType.Attack) return; // 공격 중에는 이동 로직 실행 안함
        if (isTaunting) return; // Taunt 중에는 이동 로직 실행 안함

        // 타겟이 없거나 Dead 상태면 새로 찾기
        if (UnitTarget == null || UnitTarget.CurState == StateType.Dead)
        {
            FindTarget();
        }

        // 현재 타겟 결정 (PlayerUnit이 우선, 없으면 BlockTarget)
        if (UnitTarget != null && UnitTarget.gameObject.activeInHierarchy && UnitTarget.CurState != StateType.Dead)
        {
            Vector3 targetPosition = UnitTarget.transform.position;

            // 타겟 방향에 따라 유닛 방향 전환
            UpdateFacingDirection(targetPosition);

            // 타겟까지의 거리 계산 (y축 포함)
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

            // 공격 범위 안에 있으면 공격 (쿨타임 등 조건은 CanTransitionToAttack에서 처리)
            if (distanceToTarget <= InfoData.AttackRange)
            {
                if (CanTransitionToAttack())
                    SetState(StateType.Attack);
            }
            else
            {
                // Move 상태가 아니면 Move 상태로 전환
                if (CurState != StateType.Move)
                {
                    SetState(StateType.Move);
                }

                // 공격 범위 밖이면 타겟 방향으로 이동 (y축 제외)
                Vector3 direction;
                if (this is PlayerUnit_Magic)
                {
                    // Magic 유닛만 xz 평면 기준으로 보정하여 0벡터 이동을 방지
                    direction = targetPosition - transform.position;
                    direction.y = 0f;
                    if (direction.sqrMagnitude > 0f)
                    {
                        direction.Normalize();
                    }
                    else
                    {
                        direction = Direction.normalized;
                    }
                }
                else
                {
                    direction = (targetPosition - transform.position).normalized;
                    direction.y = 0; // y축 이동 제거
                }

                // InfoData.MoveSpeed를 사용하여 이동
                float moveSpeed = InfoData.MoveSpeed;
                Vector3 newPos = transform.position + Time.deltaTime * direction * moveSpeed;
                newPos.y = groundY; // y축 위치 고정
                transform.position = newPos;
            }
        }
        else
        {
            // 타겟이 없을 때 EnemyBlockSpawner 체크
            if (EnemyBlockSpawner != null && EnemyBlockSpawner.CurHp > 0 && EnemyBlockSpawner.IsSpawn)
            {
                Vector3 spawnerPosition = EnemyBlockSpawner.transform.position;

                // EnemyBlockSpawner 방향에 따라 유닛 방향 전환
                UpdateFacingDirection(spawnerPosition);

                // EnemyBlockSpawner까지의 거리 계산 (y축 포함)
                float distanceToSpawner = Vector3.Distance(transform.position, spawnerPosition);

                // 공격 범위 안에 들어왔을 때만 공격 상태로 전환 (쿨타임 등은 CanTransitionToAttack에서 처리)
                if (distanceToSpawner <= InfoData.AttackRange)
                {
                    if (CanTransitionToAttack())
                        SetState(StateType.Attack);
                }
                else
                {
                    // 사거리 밖에 있으면 Move 상태로 전환하고 이동
                    if (CurState != StateType.Attack)
                    {
                        // 공격 상태가 아니면 Move 상태로 전환
                        if (CurState != StateType.Move)
                        {
                            SetState(StateType.Move);
                        }
                    }
                    else
                    {
                        // 현재 공격 상태인데 사거리 밖으로 나갔으면 Move 상태로 전환
                        SetState(StateType.Move);
                    }

                    // 공격 범위 밖이면 EnemyBlockSpawner 방향으로 이동 (y축 제외)
                    Vector3 direction;
                    if (this is PlayerUnit_Magic)
                    {
                        // Magic 유닛만 xz 평면 기준으로 보정하여 0벡터 이동을 방지
                        direction = spawnerPosition - transform.position;
                        direction.y = 0f;
                        if (direction.sqrMagnitude > 0f)
                        {
                            direction.Normalize();
                        }
                        else
                        {
                            direction = Direction.normalized;
                        }
                    }
                    else
                    {
                        direction = (spawnerPosition - transform.position).normalized;
                        direction.y = 0; // y축 이동 제거
                    }

                    // InfoData.MoveSpeed를 사용하여 이동
                    float moveSpeed = InfoData.MoveSpeed;
                    Vector3 newPos = transform.position + Time.deltaTime * direction * moveSpeed;
                    newPos.y = groundY; // y축 위치 고정
                    transform.position = newPos;
                }
            }
            else
            {
                // Move 상태가 아니면 Move 상태로 전환
                if (CurState != StateType.Move)
                {
                    SetState(StateType.Move);
                }

                // 타겟이 없으면 기본 방향(오른쪽)으로 보고 이동
                // DOTween 스케일 애니메이션이 실행 중이면 정리
                if (DOTween.IsTweening(transform))
                {
                    transform.DOKill();
                }
                transform.localScale = new Vector3(1, 1, 1);
                Vector3 moveDirection = Direction;
                moveDirection.y = 0; // y축 이동 제거
                moveDirection = moveDirection.normalized; // 방향 정규화
                Vector3 newPos = transform.position + Time.deltaTime * moveDirection * InfoData.MoveSpeed;
                newPos.y = groundY; // y축 위치 고정
                transform.position = newPos;
            }
        }
    }

    private void UpdateFacingDirection(Vector3 targetPosition)
    {
        // DOTween 스케일 애니메이션이 실행 중이면 정리
        if (DOTween.IsTweening(transform))
        {
            transform.DOKill();
        }

        // Anim transform의 DOTween도 정리 (버서커 모드가 아닐 때만)
        // 버서커 모드일 때는 스케일 애니메이션을 보존
        if (Anim != null && DOTween.IsTweening(Anim.transform) && !isBerserkerMode)
        {
            Anim.transform.DOKill();
        }

        // 타겟이 왼쪽에 있으면 -1, 오른쪽에 있으면 1
        if (targetPosition.x < transform.position.x)
        {
            // 타겟이 왼쪽에 있음
            transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            // 타겟이 오른쪽에 있음
            transform.localScale = new Vector3(1, 1, 1);
        }

        // Anim transform의 스케일도 보정 (애니메이션 클립에서 스케일을 변경했을 수 있으므로)
        // 단, 버서커 모드일 때는 진행 중인 스케일 애니메이션을 방해하지 않음
        if (Anim != null && OriginalScale != Vector3.zero && !isBerserkerMode)
        {
            // 현재 Anim의 스케일이 비정상적으로 작으면 원래 스케일로 복원
            if (Anim.transform.localScale.magnitude < 0.5f)
            {
                Anim.transform.localScale = OriginalScale;
            }
        }
    }

    public override void Attack()
    {
        base.Attack();

        // 공격 시 Anim transform의 스케일이 비정상적으로 작으면 복원
        if (Anim != null && OriginalScale != Vector3.zero)
        {
            if (Anim.transform.localScale.magnitude < 0.5f)
            {
                Anim.transform.localScale = OriginalScale;
            }
        }

        // 타겟 공격 (타겟이 살아있을 때만)
        if (UnitTarget != null && UnitTarget.gameObject.activeInHierarchy && UnitTarget.CurState != StateType.Dead)
        {
            UnitTarget.Damage(InfoData.Damage, this); // 자신을 공격자로 전달
        }
        // 타겟이 없고 EnemyBlockSpawner가 공격 가능하면 EnemyBlockSpawner 공격
        else if (EnemyBlockSpawner != null && EnemyBlockSpawner.CurHp > 0 && EnemyBlockSpawner.IsSpawn)
        {
            float distanceToSpawner = Vector3.Distance(transform.position, EnemyBlockSpawner.transform.position);
            if (distanceToSpawner <= InfoData.AttackRange)
            {
                EnemyBlockSpawner.Damage((int)InfoData.Damage);
            }
        }

    }

    public virtual void Damage(double damage, bool isdirection = false, EnemyUnit attacker = null)
    {
        var hpProgress = InGameHpProgress;

        if (CurState != StateType.Dead && hpProgress != null && hpProgress.gameObject != null && !hpProgress.gameObject.activeSelf)
        {
            hpProgress.UpdatePos(); // 활성화 직후 위치 즉시 업데이트
            ProjectUtility.SetActiveCheck(hpProgress.gameObject, true);
        }

        // 공격자가 있고 살아있으면 타겟으로 설정 (다른 적에게 공격받고 있으면 해당 적을 다시 공격)
        if (attacker != null && attacker.gameObject.activeInHierarchy && attacker.CurState != StateType.Dead)
        {
            UnitTarget = attacker;
        }

        GameRoot.Instance.DamageTextSystem.ShowDamage(damage,
        new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), Color.white);

        InfoData.CurHp -= damage;
        SoundPlayer.Instance.PlaySound("effect_damage");

        // HP UI가 아직 생성되지 않았거나 파괴된 경우 안전하게 초기화 후 업데이트
        if (hpProgress != null && hpProgress.gameObject != null)
        {
            hpProgress.SetHpText(InfoData.CurHp, InfoData.StartHp);
        }
        else
        {
            SetHp(InfoData.CurHp);
            hpProgress = InGameHpProgress;
        }

        DamageColorEffect();

        // HP 변경 후 버서커 상태 체크
        UnitBurskerCheck();

        if (InfoData.CurHp <= 0)
        {
            if (InGameHpProgress != null)
            {
                ProjectUtility.SetActiveCheck(InGameHpProgress.gameObject, false);
            }

            Dead(isdirection); // 페이드아웃 및 삭제 처리 시작
        }

    }

    // Update 메서드에서 Move 호출
    public virtual void Update()
    {
        // 스폰 중일 때는 중력 적용 및 착지 체크
        if (isSpawning && !isGrounded)
        {
            ApplyGravity();

            // 수평 이동 (InfoData.MoveSpeed 적용)
            Vector3 horizontalDirection = new Vector3(Direction.x, 0, Direction.z).normalized;
            Vector3 horizontalMove = horizontalDirection * InfoData.MoveSpeed * Time.deltaTime;
            // 수직 이동 (중력 적용)
            Vector3 verticalMove = new Vector3(0, verticalVelocity, 0) * Time.deltaTime;

            Vector3 newPosition = transform.position + horizontalMove + verticalMove;

            // y축이 groundY 아래로 내려가지 않도록 제한
            if (newPosition.y < groundY)
            {
                newPosition.y = groundY;
            }

            transform.position = newPosition;

            CheckGrounded();
        }
        else
        {
            // 공격 중일 때 타겟이 죽었는지 확인
            if (CurState == StateType.Attack)
            {
                if (UnitTarget == null || !UnitTarget.gameObject.activeInHierarchy || UnitTarget.CurState == StateType.Dead)
                {
                    // 범위 내에 다른 적이 있는지 확인
                    EnemyUnit newTarget = FindTargetInRange();

                    if (newTarget != null)
                    {
                        // 범위 내에 적이 있으면 타겟만 교체하고 공격 상태 유지
                        UnitTarget = newTarget;
                    }
                    else if (EnemyBlockSpawner != null && EnemyBlockSpawner.CurHp > 0 && EnemyBlockSpawner.IsSpawn)
                    {
                        // EnemyBlockSpawner가 살아있으면 사거리 체크
                        float distanceToSpawner = Vector3.Distance(transform.position, EnemyBlockSpawner.transform.position);
                        if (distanceToSpawner > InfoData.AttackRange)
                        {
                            // 사거리 밖으로 나갔으면 Move 상태로 전환
                            UnitTarget = null;
                            SetState(StateType.Move);
                        }
                    }
                    else if (EnemyBlockSpawner == null || EnemyBlockSpawner.CurHp <= 0 || !EnemyBlockSpawner.IsSpawn)
                    {
                        // 범위 내에 적이 없으면 Move 상태로 전환
                        UnitTarget = null;
                        SetState(StateType.Move);
                    }
                }
            }

            // 착지 후에는 정상 이동 (Taunt 중이 아닐 때만)
            if (!isTaunting)
            {
                Move();
            }
        }
    }

    // 공격 범위 내에서 타겟을 찾는 메서드
    private EnemyUnit FindTargetInRange()
    {
        EnemyUnit[] allEnemyUnits = BaseStage.EnemyUnitGroup.ActiveUnits.ToArray();
        var activeEnemyUnits = allEnemyUnits.Where(unit => unit.gameObject.activeInHierarchy && unit.CurState != StateType.Dead).ToArray();

        if (activeEnemyUnits.Length > 0)
        {
            // 공격 범위 내에서 가장 가까운 적 찾기
            EnemyUnit closestUnit = null;
            float closestDistance = float.MaxValue;

            foreach (var unit in activeEnemyUnits)
            {
                float distance = Vector3.Distance(transform.position, unit.transform.position);
                if (distance <= InfoData.AttackRange && distance < closestDistance)
                {
                    closestDistance = distance;
                    closestUnit = unit;
                }
            }

            return closestUnit;
        }

        return null;
    }

    // 적이 모두 죽었는지 확인하는 메서드
    private bool AreAllEnemiesDead()
    {
        if (BaseStage == null || BaseStage.EnemyUnitGroup == null) return false;

        EnemyUnit[] allEnemyUnits = BaseStage.EnemyUnitGroup.ActiveUnits.ToArray();
        var activeEnemyUnits = allEnemyUnits.Where(unit => unit.gameObject.activeInHierarchy && unit.CurState != StateType.Dead).ToArray();

        // 일반 적 유닛이 모두 죽었는지 확인
        if (activeEnemyUnits.Length > 0) return false;

        // EnemyBlockSpawner가 활성화되어 있고 아직 살아있으면 false
        var enemyUnitGroup = BaseStage.EnemyUnitGroup;
        if (enemyUnitGroup.IsEnemyBlockSpawnerActive && enemyUnitGroup.EnemyBlockSpawner != null && !enemyUnitGroup.EnemyBlockSpawner.IsDead)
            return false;

        return true;
    }

    private void CheckGrounded()
    {
        float currentY = transform.position.y;

        // y축이 groundY 이하로 떨어지고, 아래로 떨어지고 있을 때만 착지로 간주
        if (currentY <= groundY && verticalVelocity < 0)
        {
            isGrounded = true;
            isSpawning = false;

            // 수직 속도 초기화
            verticalVelocity = 0f;

            // 착지 후 수평 이동으로 전환
            Direction = new Vector3(1f, 0f, 0f);

            // y 위치를 groundY로 부드럽게 고정 (groundY 아래로 내려가지 않도록)
            if (currentY < groundY)
            {
                transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
            }

            // 착지 후 Move 상태로 전환 (아직 Move 상태가 아니라면)
            if (CurState != StateType.Move)
            {
                SetState(StateType.Move);
            }
        }
    }


    private void ApplyGravity()
    {
        // 중력 가속도 적용 (게임에 맞게 조정된 값)
        float gravity = -12f; // 중력 값 (자연스러운 낙하)
        verticalVelocity += gravity * Time.deltaTime;

        // 최대 낙하 속도 제한 (너무 빠르게 떨어지지 않도록)
        float maxFallSpeed = -8f;
        if (verticalVelocity < maxFallSpeed)
        {
            verticalVelocity = maxFallSpeed;
        }
    }


    public override void SetAnim()
    {
        base.SetAnim();
    }

    public override void DeleteUnit()
    {
        base.DeleteUnit();

        if (InGameHpProgress != null)
        {
            ProjectUtility.SetActiveCheck(InGameHpProgress.gameObject, false);
        }

        ProjectUtility.SetActiveCheck(gameObject, false);
        BaseStage.PlayerUnitGroup.DeleteUnit(this);
    }




    public void UnitBurskerCheck()
    {
        var findmodifier = GameRoot.Instance.InGameUpgradeSystem.GetModifier(Config.InGameUpgradeChoice.BerserkerDoll);

        if (findmodifier == null) return;


        var cuhppercent = (int)(InfoData.CurHp / InfoData.StartHp * 100);


        if (cuhppercent <= findmodifier.Value_2 && !isBerserkerMode)
        {
            // 버서커 모드 활성화
            if (!isBerserkerMode)
            {
                isBerserkerMode = true;
            }

            var dmgbuffvalue = ProjectUtility.PercentCalc(
            InfoData.Damage, findmodifier.Value_1);

            InfoData.Damage += dmgbuffvalue;

            // Anim과 OriginalScale이 유효한지 확인
            if (Anim != null && OriginalScale != Vector3.zero)
            {
                // 기존 애니메이션 정리 후 새로운 애니메이션 시작
                Anim.transform.DOKill();
                Anim.transform.localScale = OriginalScale;
                Anim.transform.DOScale(OriginalScale * 1.5f, 0.3f).SetEase(Ease.OutBack);
            }


            var atkspeedbuffvalue = ProjectUtility.PercentCalc(
            InfoData.AttackSpeed, findmodifier.Value_1);

            InfoData.AttackSpeed += atkspeedbuffvalue;

            SyncAnimationSpeed();

            SetImageColor(Color.red);
        }

    }


    public void SetImageColor(Color color)
    {
        foreach (var sprite in UnitSpriteList)
        {
            if (sprite == null) continue;
            sprite.color = color;
        }
    }



}
