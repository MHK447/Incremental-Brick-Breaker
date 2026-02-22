using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using DG.Tweening;


public class PlayerBlockSIlverCoin : PlayerBlock
{
    [SerializeField]
    private SpriteRenderer LightImg;



    public override void Set(int blockidx, int order, int grade, PlayerBlockGroup parentGroup)
    {
        base.Set(blockidx, order, grade, parentGroup);

        LightImg.color = Config.Instance.GetImageColor($"silvercoinblock_light_{grade}");
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
                                   var cardhealvalue = GameRoot.Instance.CardSystem.GetCardValue(BlockIdx, UnitStatusType.SILVERCOIN);

                                   target.DOScale(1.3f, 0.15f).SetEase(DG.Tweening.Ease.OutCubic).SetUpdate(true).SetLoops(2, DG.Tweening.LoopType.Yoyo);
                                   GameRoot.Instance.UserData.Ingamesilvercoin.Value += (int)BlockData.BaseValue + (int)cardhealvalue;
                               }, coinparameters);


              x.SetAutoRemove(true, 2f);
          });
    }
}

