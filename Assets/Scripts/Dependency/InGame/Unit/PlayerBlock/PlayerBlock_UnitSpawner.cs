using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using DG.Tweening;

public class PlayerBlock_UnitSpawner : PlayerBlock
{
    private int UnitIdx = 0;
    private int Grade = 0;

    private float CoolTime = 0f;

    private PlayerUnitGroup PlayerUnitGroup;

    [SerializeField]
    private SpriteRenderer SpawnUnitImg;

    [SerializeField]
    private Animator Anim;



    public override void Set(int blockidx, int order, int grade, PlayerBlockGroup parentGroup)
    {
        base.Set(blockidx, order, grade, parentGroup);

        var td = Tables.Instance.GetTable<EquipInfo>().GetData(blockidx);

        if (td != null)
        {
            PlayerUnitGroup = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.PlayerUnitGroup;

            UnitIdx = td.idx;
            Grade = grade;

            SpawnUnitImg.sprite = 
            AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_InGame, $"BlockUnitImg_{UnitIdx}_{Grade}");
        }
    }

    protected override void Attack()
    {
        base.Attack();

        // 스폰 블록 스케일 애니메이션 (1 -> 1.3 -> 1)
        transform.DOScale(1.3f, 0.15f)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                transform.DOScale(1f, 0.15f).SetEase(Ease.InQuad);
            });

        PlayerUnitGroup.AddUnit(UnitIdx, Grade, barrelTransform, BlockOrder);

        // SlimeClone 업그레이드가 있으면 확률적으로 한 번 더 소환
        var slimeCloneModifier = GameRoot.Instance.InGameUpgradeSystem.InGameUpgradeList.Find(x => x.StatType == Config.InGameUpgradeChoice.SlimeClone);
        if (slimeCloneModifier != null)
        {
            float spawnChance = slimeCloneModifier.Value_2;
            if (ProjectUtility.IsPercentSuccess(spawnChance))
            {
                DOVirtual.DelayedCall(0.5f, () =>
                {
                    GameRoot.Instance.EffectSystem.Play<DoubleSpawnEffect>(this.transform.position, x =>
                    {
                        x.Init();
                        x.SetAutoRemove(true, 2f);
                    });

                    if (this != null && PlayerUnitGroup != null && !GameRoot.Instance.UserData.Playerdata.IsWaveRestProperty.Value)
                    {
                        PlayerUnitGroup.AddUnit(UnitIdx, Grade, barrelTransform, BlockOrder);
                    }
                });
            }
        }
    }

}

