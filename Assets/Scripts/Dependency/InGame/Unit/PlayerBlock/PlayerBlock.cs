using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using BanpoFri;
using UniRx;
using UnityEngine;
using System.Drawing;

public class PlayerBlock : MonoBehaviour
{
    //common refs
    [HideInInspector] public InGameBaseStage Battle;
    [HideInInspector] protected PlayerBlockGroup Group;

    [SerializeField]
    protected Transform barrelTransform; //총알 스폰 위치

    [SerializeField]
    private SpriteRenderer CooldownVisualizer;

    [SerializeField]
    private SpriteRenderer BlockImg;

    private MaterialPropertyBlock PropertyBlock;



    [HideInInspector] public int BlockType;
    protected bool IsMaxLevel = false;


    [HideInInspector]
    public PlayerBlockData BlockData = new();

    //block state
    [HideInInspector] public bool IsDead = false;
    // 블럭 배치 순서 (0은 밑바닥, 층마다 0.8 증가)
    [HideInInspector] public int BlockOrder = 0;
    // 블럭 생성, 제거 애니매이션 중인지
    private bool isAnimating = false;
    //목표 위치
    private float TargetHeight = 0;
    //hp 표시할지
    protected bool HasHP = true;
    //성 꼭대기인지
    protected bool IsCastleTop = false;
    protected float AnimationSpeed = 1; //애니매이션 클립을 몇배속해야 쿨다운과 일치하는지

    private Coroutine AttckCorutine; // 코루틴 참조 추가

    protected int BlockGrade = 1;

    public SpriteRenderer RevivalSprite;

    //others
    private CompositeDisposable disposables = new CompositeDisposable();

    [HideInInspector]
    public int BlockIdx = 0;

    public Transform PlayerHeroTr;

    protected string AttackSoundEffectKey = "effect_player_attack";

    private bool IsSwapTutorialOn = false;

    public virtual void Awake()
    {
        PropertyBlock = new();
    }

    protected virtual void Update()
    {
        UpdatePosition();
        UpdateCooldownVisualizer();
    }

    private void UpdatePosition()
    {
        if (isAnimating) return;
        Vector3 position = transform.localPosition;
        position.y = position.y + (TargetHeight - position.y) * Time.unscaledDeltaTime * 20;
        transform.localPosition = position;
    }

    protected virtual void PreInitialize() { }
    protected virtual void PostInitialize() { }

    public virtual void Set(int blockidx, int order, int grade, PlayerBlockGroup parentGroup)
    {
        BlockIdx = blockidx;
        BlockGrade = grade;
        IsCastleTop = false;
        HasHP = true;
        IsMaxLevel = false;
        IsSwapTutorialOn = false;
        disposables.Clear();

        if (RevivalSprite != null)
            ProjectUtility.SetActiveCheck(RevivalSprite.gameObject, false);


        Group = parentGroup;
        BlockOrder = order;

        GameRoot.Instance.InGameUpgradeSystem.StateRefreshEvent += SetBlockInfo;


        PreInitialize();

        SetBlockInfo();


        //initialize
        SpawnAnimation(order);

        PostInitialize();

    }

    public virtual void Clear()
    {
    }



    protected virtual void OnSpawn()
    {
        if (AttckCorutine != null)
        {
            GameRoot.Instance.StopCoroutine(AttckCorutine);
            AttckCorutine = null;
        }
        //터렛 행동 시작
        AttckCorutine = GameRoot.Instance.StartCoroutine(AttackRoutine());
    }

    // Revival 후 공격 재시작을 위한 public 메서드
    public void RestartAttack()
    {
        OnSpawn();
    }

    private IEnumerator AttackRoutine()
    {
        while (!IsDead && this != null && gameObject != null && gameObject.activeSelf)
        {
            var root = GameRoot.GetInstance();
            if (root == null || root.UserData == null || root.UserData.Playerdata == null)
            {
                yield return null;
                continue;
            }

            // 웨이브가 진행 중일 때만 공격
            bool isWaveActive = !root.UserData.Playerdata.IsWaveRestProperty.Value;

            if (isWaveActive && BlockData.RemainingCooldown <= 0)
            {
                BlockData.RemainingCooldown = BlockData.Cooldown;
                OnAttackStarted();
                //SoundPlayer.Instance.PlaySound(AttackSoundEffectKey);

                if (this != null)
                    Attack();
            }
            yield return null;

            // yield return null 이후 다시 한번 체크
            if (this == null || gameObject == null || !gameObject.activeSelf || IsDead)
                yield break;

            // 웨이브가 진행 중일 때만 쿨다운 감소
            if (isWaveActive)
            {
                BlockData.RemainingCooldown -= Time.deltaTime;
            }
            else
            {
                // 쉬는 시간일 때는 쿨다운을 최대값으로 설정 (다음 웨이브 시작 시 바로 공격 안하도록)
                BlockData.RemainingCooldown = BlockData.Cooldown;
            }
        }
    }
    //쿨다운이 다됐을때 불림. 애니매이션 이벤트안쓸때 이거씀
    protected virtual void OnAttackStarted() { }
    //공격 구현 (애니매이션 이벤트에서 호출됨)
    protected virtual void Attack()
    {
    }

    //터렛 다운, 공격정보들 적용. 스탯 바뀔때 불러주기


    protected virtual void OnBlockLevel3()
    {

    }

    protected virtual void OnBlockLevel4()
    {
        IsMaxLevel = true;
    }

    //쿨다운과 애니매이션 속도 일치하도록 배속 맞춤
    protected void SyncAnimationSpeed()
    {
        if (this == null) return;
    }

    public void Damage(float damage)
    {
    }



    public virtual void Dead()
    {
        IsDead = true;

        //decrement this block count
        //var findblockdata = GameRoot.Instance.BlockSystem.FindBlockData(BlockIdx);
        //if (findblockdata != null) findblockdata.BlockCountProperty.Value -= 1;

        gameObject.SetActive(false);

        GameRoot.Instance.InGameUpgradeSystem.StateRefreshEvent -= SetBlockInfo;
        Group.OnBlockRemove(this);
    }

    void OnDestroy()
    {
        transform.DOKill();

        var root = GameRoot.GetInstance();
        if (AttckCorutine != null && root != null)
        {
            root.StopCoroutine(AttckCorutine);
            AttckCorutine = null;
        }
    }

    public virtual void SetBlockInfo()
    {
        var td = Tables.Instance.GetTable<EquipInfo>().GetData(BlockIdx);

        if (td != null)
        {
            var cardcooltime = GameRoot.Instance.CardSystem.GetCardValue(BlockIdx, UnitStatusType.AttackSpeed);

            var cardrange = GameRoot.Instance.CardSystem.GetCardValue(BlockIdx, UnitStatusType.AttackRange);

            var weaponcooltime = (td.cooltime + cardcooltime) * 0.01f;

            BlockData.Cooldown = (float)weaponcooltime;
            BlockData.AttackRange = (float)(td.attack_range + cardrange);

            var trainingdamagevalue = GameRoot.Instance.TrainingSystem.GetBuffValue(TrainingSystem.TrainingType.BlockAttackDamage);

            var carddamagevalue = GameRoot.Instance.CardSystem.GetCardValue(BlockIdx, UnitStatusType.Attack);

            BlockData.Damage = (td.base_dmg + trainingdamagevalue + carddamagevalue) * BlockGrade;
            BlockData.BaseValue = td.base_value_1 * BlockGrade;
        }

        SyncAnimationSpeed();

    }

    void OnDisable()
    {
        // // 코루틴 중단
        // if (AttckCorutine != null && !GameRoot.IsApplicationQuitting)
        // {
        //     GameRoot.Instance.StopCoroutine(AttckCorutine);
        //     AttckCorutine = null;
        // }

    }

    #region Block Order Move

    //다른 블럭이 움직일때
    public void OnBlockMove(int fromOrder, int newOrder)
    {
        // 움직이는게 나임
        if (fromOrder == BlockOrder)
        {
            TargetHeight = newOrder;
            return;
        }

        // 내가 그 블럭보다 위에있는데 옮기는 위치가 나보다 같거나 위일때, 아래로 움직임
        if (BlockOrder > fromOrder && newOrder >= BlockOrder)
        {
            TargetHeight = BlockOrder - 1;
            return;
        }

        // 내가 그 블럭보다 아래있고 옮기는 위치가 같거나 아래일때, 위로 움직임
        if (BlockOrder < fromOrder && newOrder <= BlockOrder)
        {
            TargetHeight = BlockOrder + 1;
            return;
        }

        TargetHeight = BlockOrder;
    }

    //다른 블럭 이동이 끝났을때
    public void OnBlockMoveFinish(int fromOrder, int newOrder)
    {

        if (fromOrder != newOrder)
        {
            Group.TutorialCheckIsSwapOn = true;
        }

        // 움직이는게 나임
        if (fromOrder == BlockOrder)
        {
            transform.DOKill();
            transform.DOScale(1f, 0.2f)
                .SetUpdate(true)
                .SetEase(Ease.OutCubic);

            BlockOrder = newOrder;
            TargetHeight = BlockOrder;
            return;
        }


        // 내가 그 블럭보다 위에있는데 옮기는 위치가 나보다 같거나 위일때, 아래로 움직임
        if (BlockOrder > fromOrder && newOrder >= BlockOrder)
        {
            BlockOrder--;
            TargetHeight = BlockOrder;

            return;
        }

        // 내가 그 블럭보다 아래있고 옮기는 위치가 같거나 아래일때, 위로 움직임
        if (BlockOrder < fromOrder && newOrder <= BlockOrder)
        {
            BlockOrder++;
            TargetHeight = BlockOrder;
            return;
        }


    }

    private bool IsDamageDirect = false;



    public virtual void DamageColorEffect()
    {
        if (!IsDamageDirect)
        {
            IsDamageDirect = true;


            BlockImg.EnableHitEffect();



            GameRoot.Instance.WaitTimeAndCallback(0.15f, () =>
            {
                if (this != null)
                {
                    // 효과 종료 후 원래 머티리얼로 복귀

                    BlockImg.DisableHitEffect();

                    IsDamageDirect = false;
                }
            });
        }
    }

    protected virtual void UpdateCooldownVisualizer()
    {
        if (CooldownVisualizer == null) return;
        CooldownVisualizer.GetPropertyBlock(PropertyBlock);
        PropertyBlock.SetFloat("_FillAmount", 1 - Mathf.Clamp01(BlockData.RemainingCooldown / (float)BlockData.Cooldown));
        CooldownVisualizer.SetPropertyBlock(PropertyBlock);
    }



    //다른 블럭이 추가됐을때 애니매이션
    public void OnBlockAdded(int addedBlockOrder, int count)
    {
        if (addedBlockOrder > BlockOrder) return;
        BlockOrder += count;
        TargetHeight = BlockOrder;

        //animation
        transform.DOKill();
        isAnimating = true;
        transform.localScale = Vector3.one;
        transform.DOLocalMoveY(BlockOrder, 0.2f)
            .SetEase(Ease.OutBack);
        transform.DOScaleX(1.2f, 0.1f)
            .SetEase(Ease.OutCubic)
            .SetDelay(BlockOrder * 0.07f)
            .SetLoops(2, LoopType.Yoyo)
            .OnKill(() => isAnimating = false)
            .OnComplete(() => isAnimating = false);
    }

    //다른 블럭이 사라졌을때 애니매이션
    public void OnBlockRemoved(int removedBlockOrder, int count)
    {
        if (removedBlockOrder >= BlockOrder) return;
        BlockOrder -= count;
        TargetHeight = BlockOrder;

        //animation
        transform.DOKill();
        isAnimating = true;
        transform.localScale = Vector3.one;
        transform.DOLocalMoveY(BlockOrder, 0.2f)
            .SetEase(Ease.OutCubic);
        transform.DOScaleX(1.2f, 0.1f)
            .SetEase(Ease.OutCubic)
            .SetDelay(BlockOrder * 0.07f)
            .SetLoops(2, LoopType.Yoyo)
            .OnKill(() => isAnimating = false)
            .OnComplete(() => isAnimating = false);
    }

    //처음 스폰될때 애니매이션
    protected void SpawnAnimation(int order)
    {
        isAnimating = true;
        TargetHeight = order;

        transform.DOKill();
        transform.localScale = new(1, 0, 1);
        transform.localPosition = Vector3.up * order;
        transform.DOScaleY(1, 0.2f)
            .SetEase(Ease.OutQuart)
            .OnComplete(() =>
            {
                isAnimating = false;
                OnSpawn();
            })
            .OnKill(() =>
            {
                isAnimating = false;
                OnSpawn();
            });
        transform.DOScaleX(1.2f, 0.1f)
            .SetEase(Ease.OutCubic)
            .SetDelay(BlockOrder * 0.07f)
            .SetLoops(2, LoopType.Yoyo);
    }


    #endregion
}
