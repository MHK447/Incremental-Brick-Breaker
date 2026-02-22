using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerBlock_Shiled : PlayerBlock
{
    [SerializeField]
    private SpriteRenderer WeaponImg;


    public override void Set(int blockidx, int order, int grade, PlayerBlockGroup parentGroup)
    {
        base.Set(blockidx, order, grade, parentGroup);


        var td = Tables.Instance.GetTable<WeaponInfo>().GetData(blockidx);

        if (td != null)
        {
            WeaponImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_InGame, $"InGameWeapon_{blockidx}_{grade}");
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

        GameRoot.Instance.UserData.Playerdata.CurShiledProperty.Value += (int)BlockData.BaseValue;

        SoundPlayer.Instance.PlaySound("effect_item_shield");
    }
}

