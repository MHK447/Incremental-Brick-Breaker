using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using DG.Tweening;
using BanpoFri;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Drawing;
using UniRx;

public class PlayerBlockGroup : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer CastleBottomImg;

    [SerializeField]
    private SpriteRenderer MapTopImg;

    [SerializeField]
    private SpriteRenderer MapBottomImg;

    [SerializeField]
    private SpriteRenderer MapImg;

    [HideInInspector]
    public PlayerBlock CastleTop;

    [HideInInspector]
    public HashSet<PlayerBlock> ActiveBlocks = new();
    [HideInInspector]
    public HashSet<PlayerUnit> ActivePlayerUnits = new HashSet<PlayerUnit>();
    [HideInInspector]
    public HashSet<PlayerUnit> DeadPlayerUnits = new HashSet<PlayerUnit>();
    [HideInInspector]
    public List<int> DeadBlockList = new List<int>();

    private CastleHpProgress CastleHpProgress;


    [HideInInspector] public bool TutorialCheckIsSwapOn = false;


    //블록 순서 바꾸는중인지
    [HideInInspector] public bool IsMovingBlock;

    [SerializeField]
    private ParticleSystem OnUpgradeParticle;

    private CompositeDisposable disposables = new CompositeDisposable();

    //skill stats
    [HideInInspector] public bool IsInvincible = false;
    [HideInInspector] public float GlobalCooldownMultiplier = 1;
    [HideInInspector] public float GlobalBlockBonusHpMultiplier = 1;

    //data
    public HashSet<int> TotalSpawnedBlockIndexes = new();
    private int TotalSpawnedBlocks = 0;

    private AsyncOperationHandle<IList<GameObject>> PreloadHandle;

    private bool IsRevival = false;
    private bool IsDeathResultPending = false;


    private void ResetSkillStats()
    {
        IsInvincible = false;
        GlobalCooldownMultiplier = 1;
        GlobalBlockBonusHpMultiplier = 1;
    }

    public event Action BlockStatRefreshEvent;

    public void ShowStatUpgradeParticle()
    {
        OnUpgradeParticle.Play();
    }


    private bool IsFirstRevival = false;



    public void Init()
    {
        ActiveBlocks.Clear();

        IsRevival = false;
        IsDeathResultPending = false;
        IsFirstRevival = GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.RevivalOpen) ? false : true;


        DeadBlockList.Clear();
        IsMovingBlock = false;
        TutorialCheckIsSwapOn = false;
        ResetSkillStats();
        CreateCastleTop();
        SetMapImg();
        //SetInvincible(false, true);

        // OnUpgradeParticle.Stop();
        // OnSpawnParticle.Stop();

        TotalSpawnedBlockIndexes.Clear();
        TotalSpawnedBlocks = 0;

        GameRoot.Instance.UserData.Playerdata.CurShiledProperty.Value = 0;

        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;


        SetHp(GameRoot.Instance.UserData.Playerdata.CurHpProperty.Value, GameRoot.Instance.UserData.Playerdata.StartHpProperty.Value);


        disposables.Clear();

        GameRoot.Instance.UserData.Playerdata.StartHpProperty.Subscribe(x =>
        {
            if (CastleHpProgress != null)
            {
                CastleHpProgress.SetHpText(GameRoot.Instance.UserData.Playerdata.CurHpProperty.Value, x);
            }
        }).AddTo(disposables);
    }


    public void SetHp(int curhp, int starthp)
    {
        if (CastleHpProgress == null)
        {
            GameRoot.Instance.UISystem.LoadFloatingUI<CastleHpProgress>(hpprogress =>
                    {
                        CastleHpProgress = hpprogress;
                        hpprogress.Init(CastleBottomImg.transform);
                        hpprogress.SetHpText(curhp, starthp);
                        ProjectUtility.SetActiveCheck(CastleHpProgress.gameObject, true);
                    });
        }
        else
        {
            CastleHpProgress.SetHpText(curhp, starthp);
            ProjectUtility.SetActiveCheck(CastleHpProgress.gameObject, true);
        }
    }

    public void SetMapImg(bool isDevil = false)
    {
        if (isDevil)
        {
            MapImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_Stage, $"InGameBg_Devil");
            MapTopImg.color = Config.Instance.GetImageColor($"StageColorDevil_Top");
        }
        else
        {
            var stageidx = GameRoot.Instance.UserData.Stageidx.Value;

            var td = Tables.Instance.GetTable<StageInfo>().GetData(stageidx);

            MapTopImg.color = Config.Instance.GetImageColor($"StageColor_{td.ingame_map_idx}_Top");
            MapBottomImg.color = Config.Instance.GetImageColor($"StageColor_{td.ingame_map_idx}_Bottom");
            MapImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_Stage, $"IngameBg_0{td.ingame_map_idx}");
            CastleBottomImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_Stage, $"Ingame_FloorDeco_{td.ingame_map_idx}");

            CastleBottomImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_Stage, $"ingame_map_idx{stageidx}");
        }
    }


    public void HpHeal(int heal)
    {
        if (GameRoot.Instance.UserData.Playerdata.CurHpProperty.Value + heal > GameRoot.Instance.UserData.Playerdata.StartHpProperty.Value)
        {
            GameRoot.Instance.UserData.Playerdata.CurHpProperty.Value = GameRoot.Instance.UserData.Playerdata.StartHpProperty.Value;
        }
        else
        {
            GameRoot.Instance.UserData.Playerdata.CurHpProperty.Value += heal;
        }

        if (CastleHpProgress != null)
        {
            CastleHpProgress.SetHpText(GameRoot.Instance.UserData.Playerdata.CurHpProperty.Value, GameRoot.Instance.UserData.Playerdata.StartHpProperty.Value);
        }


        GameRoot.Instance.EffectSystem.MultiPlay<HealEffect>(new Vector3(transform.position.x, transform.position.y - 0.5f , transform.position.z), x =>
                 {
                     x.SetAutoRemove(true, 2f);
                 });
    }
    public void CreateCastleTop()
    {
        Addressables.InstantiateAsync("PlayerBlock_Top").Completed += (handle) =>
        {
            CastleTop = handle.Result.GetComponent<PlayerBlock>();
            CastleTop.transform.SetParent(transform);
            CastleTop.transform.localPosition = Vector3.zero;
            //SetShieldHeight();
            CastleTop.Set(0, 0, 0, this);
        };
    }


    public void RevivalInvicible(float time)
    {
        IsRevival = true;

        foreach (var block in ActiveBlocks)
        {
            block.IsDead = false;
            ProjectUtility.SetActiveCheck(block.RevivalSprite.gameObject, true);
        }

        RevivalEffect();

        CastleTop.RevivalSprite.gameObject.SetActive(true);

        GameRoot.Instance.UserData.Playerdata.CurHpProperty.Value = GameRoot.Instance.UserData.Playerdata.StartHpProperty.Value;

        CastleHpProgress.SetHpText(GameRoot.Instance.UserData.Playerdata.CurHpProperty.Value
              , GameRoot.Instance.UserData.Playerdata.StartHpProperty.Value);

        GameRoot.Instance.WaitTimeAndCallback(time, () =>
        {
            foreach (var block in ActiveBlocks)
            {
                ProjectUtility.SetActiveCheck(block.RevivalSprite.gameObject, false);
            }

            CastleTop.RevivalSprite.gameObject.SetActive(false);

            IsRevival = false;
        });

    }


    public void AddBlock(int index, int grade)
    {
        var battle = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage;
        bool isFirstBlock = battle.PlayerBlockGroup.TotalSpawnedBlocks == 0;

        //spawn block
        AddBlock_Internal(index, grade);
    }

    private void AddBlock_Internal(int index, int grade)
    {
        var td = Tables.Instance.GetTable<WeaponInfo>().GetData(index);


        var prefab = td == null ? "PlayerBlock_UnitSpawner" : td.prefab;

        var handle = Addressables.InstantiateAsync(prefab, transform);
        GameObject result = handle.WaitForCompletion();

        foreach (var block in ActiveBlocks)
        {
            //이미 있는 블록들한테 점프뛰라고 시킴
            block.OnBlockAdded(0, 1);
        }
        CastleTop.OnBlockAdded(0, 1);
        //SoundPlayer.Instance.PlaySound("effect_add_block");
        //OnSpawnParticle.Play();
        PlayerBlock instance = result.GetComponent<PlayerBlock>();
        // instance.transform.SetParent(transform);
        instance.transform.localPosition = Vector3.zero;
        ActiveBlocks.Add(instance);
        instance.Set(index, 0, grade, this);
        //SetShieldHeight();

        instance.transform.localScale = Vector3.one;

        TotalSpawnedBlockIndexes.Add(index);
        TotalSpawnedBlocks++;

    }

    public void OnBlockMove(int fromIndex, int newIndex)
    {
        if (ActiveBlocks.Count == 1) return;

        if (!IsMovingBlock)
        {
            IsMovingBlock = true;
            GameRoot.Instance.GameSpeedSystem.StopGameSpeed(true);
        }

        newIndex = Mathf.Clamp(newIndex, 0, ActiveBlocks.Count - 1);
        foreach (var block in ActiveBlocks)
        {
            block.OnBlockMove(fromIndex, newIndex);
        }
    }

    public void OnBlockMoveFinish(int fromIndex, int newIndex)
    {
        if (!IsMovingBlock) return;
        if (ActiveBlocks.Count == 1) return;

        newIndex = Mathf.Clamp(newIndex, 0, ActiveBlocks.Count - 1);
        foreach (var block in ActiveBlocks)
        {
            block.OnBlockMoveFinish(fromIndex, newIndex);
        }

        IsMovingBlock = false;

        if (!GameRoot.Instance.TutorialSystem.IsActive("3"))
            GameRoot.Instance.GameSpeedSystem.StopGameSpeed(false);
    }

    public void OnBlockRemove(PlayerBlock block)
    {
        //SoundPlayer.Instance.PlaySound("effect_player_death");
        DeadBlockList.Add(block.BlockIdx);
        ActiveBlocks.Remove(block);
        foreach (var item in ActiveBlocks)
        {
            item.OnBlockRemoved(block.BlockOrder, 1);
        }
        CastleTop.OnBlockRemoved(block.BlockOrder, 1);
    }




    public void ClearData()
    {
        if (CastleTop != null)
        {
            Addressables.ReleaseInstance(CastleTop.gameObject);
            CastleTop = null;
        }
        BlockStatRefreshEvent = null;
        ResetSkillStats();
        foreach (var block in ActiveBlocks)
        {
            block.Clear();
            Addressables.ReleaseInstance(block.gameObject);
        }
        ActiveBlocks.Clear();
        //Addressables.Release(PreloadHandle);

        TotalSpawnedBlocks = 0;
        TotalSpawnedBlockIndexes.Clear();

        if (CastleHpProgress != null)
        {
            ProjectUtility.SetActiveCheck(CastleHpProgress.gameObject, false);
        }
    }

    public void BlockKnockBackCheck(EnemyUnit target)
    {
        var getmodifier = GameRoot.Instance.InGameUpgradeSystem.GetModifier(Config.InGameUpgradeChoice.BlockKnockBack);

        if (getmodifier != null)
        {
            if (getmodifier.Value_1 > 0)
            {
                var isknockbackcheck = ProjectUtility.IsPercentSuccess(getmodifier.Value_2);

                if (isknockbackcheck)
                {
                    target.Damage(getmodifier.Value_1);
                    target.KnockBackStart();
                }
            }
        }
    }


    public void Damage(int damage, EnemyUnit target = null)
    {
        if (GameRoot.Instance.UserData.Playerdata.CurHpProperty.Value <= 0) return;

        if (IsRevival) return;

        ProjectUtility.Vibrate();

        if (target != null)
            BlockKnockBackCheck(target);

        if (GameRoot.Instance.UserData.Playerdata.CurShiledProperty.Value > 0)
        {
            GameRoot.Instance.UserData.Playerdata.CurShiledProperty.Value -= damage;
        }
        else
        {
            GameRoot.Instance.UserData.Playerdata.CurHpProperty.Value -= damage;
        }

        CastleHpProgress.SetHpText(GameRoot.Instance.UserData.Playerdata.CurHpProperty.Value
         , GameRoot.Instance.UserData.Playerdata.StartHpProperty.Value);


        foreach (var block in ActiveBlocks)
        {
            block.DamageColorEffect();
        }

        CastleTop.DamageColorEffect();


        SoundPlayer.Instance.PlaySound("effect_player_damage");

        GameRoot.Instance.DamageTextSystem.ShowDamage(damage,
        new Vector3(CastleTop.transform.position.x, CastleTop.transform.position.y + 0.5f, CastleTop.transform.position.z), UnityEngine.Color.white);


        if (GameRoot.Instance.UserData.Playerdata.CurHpProperty.Value <= 0)
        {
            if (IsDeathResultPending) return;

            IsDeathResultPending = true;
            DeadEffect();

            GameRoot.Instance.WaitTimeAndCallback(2f, () =>
            {
                IsDeathResultPending = false;

                if (GameRoot.Instance.UserData.Playerdata.CurHpProperty.Value > 0)
                {
                    return;
                }

                if (IsFirstRevival)
                {
                    GameRoot.Instance.UISystem.OpenUI<PopupStageResult>(popup => popup.Set(false));
                }
                else
                {
                    IsFirstRevival = true;
                    GameRoot.Instance.UISystem.OpenUI<PopupRevival>(popup => popup.Init());

                }
            });

        }


    }


    public void DeadEffect()
    {
        // BlockOrder 기준으로 정렬 (0이 가장 아래, 숫자가 클수록 위)
        var sortedBlocks = ActiveBlocks.OrderBy(block => block.BlockOrder).ToList();

        float delayPerBlock = 0.1f; // 각 블록 사이의 딜레이
        float scaleDuration = 0.3f; // 스케일 다운 애니메이션 시간

        for (int i = 0; i < sortedBlocks.Count; i++)
        {
            var block = sortedBlocks[i];
            float delay = i * delayPerBlock;

            block.IsDead = true;

            // DOTween으로 스케일 다운 애니메이션
            block.transform.DOKill();
            block.transform.DOScale(Vector3.zero, scaleDuration)
                .SetDelay(delay)
                .SetEase(Ease.InBack)
                .SetUpdate(true);
        }

        // CastleTop도 함께 애니메이션 (블록들 다음에 사라짐)
        if (CastleTop != null && CastleTop.gameObject != null)
        {
            float topDelay = sortedBlocks.Count * delayPerBlock;
            CastleTop.transform.DOKill();
            CastleTop.transform.DOScale(Vector3.zero, scaleDuration)
                .SetDelay(topDelay)
                .SetEase(Ease.InBack)
                .SetUpdate(true);
        }
    }

    public void RevivalEffect()
    {
        // BlockOrder 기준으로 정렬 (0이 가장 아래, 숫자가 클수록 위)
        var sortedBlocks = ActiveBlocks.OrderBy(block => block.BlockOrder).ToList();

        float delayPerBlock = 0.1f; // 각 블록 사이의 딜레이
        float scaleDuration = 0.3f; // 스케일 업 애니메이션 시간

        for (int i = 0; i < sortedBlocks.Count; i++)
        {
            var block = sortedBlocks[i];
            float delay = i * delayPerBlock;

            // 블록을 활성화하고 스케일을 0으로 설정
            if (block != null && block.gameObject != null)
            {
                block.IsDead = false;
                block.gameObject.SetActive(true);
                block.transform.localScale = Vector3.zero;

                // DOTween으로 스케일 업 애니메이션
                block.transform.DOKill();
                block.transform.DOScale(Vector3.one, scaleDuration)
                    .SetDelay(delay)
                    .SetEase(Ease.OutBack)
                    .SetUpdate(true)
                    .OnComplete(() =>
                    {
                        // 애니메이션 완료 후 공격 재시작
                        if (block != null && block.gameObject != null && block.gameObject.activeSelf)
                        {
                            block.RestartAttack();
                        }
                    });
            }
        }

        // CastleTop도 함께 애니메이션 (블록들 다음에 나타남)
        if (CastleTop != null && CastleTop.gameObject != null)
        {
            CastleTop.gameObject.SetActive(true);
            CastleTop.transform.localScale = Vector3.zero;
            CastleTop.transform.DOKill();
            float topDelay = sortedBlocks.Count * delayPerBlock;
            CastleTop.transform.DOScale(Vector3.one, scaleDuration)
                .SetDelay(topDelay)
                .SetEase(Ease.OutBack)
                .SetUpdate(true);
        }
    }


    void OnDestroy()
    {
        disposables.Clear();
    }

    void OnDisable()
    {
        disposables.Clear();
    }



}
