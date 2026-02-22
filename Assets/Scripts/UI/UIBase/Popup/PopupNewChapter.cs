using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

[UIPath("UI/Popup/PopupNewChapter")]
public class PopupNewChapter : UIBase
{
    [SerializeField]
    private Image ChapterImg;


    public void Set(int ingamemapidx)
    {
        ChapterImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Map, $"InGameMap_0{ingamemapidx}");
        SoundPlayer.Instance.PlaySound("Win");
    }




    public override void Hide()
    {
        base.Hide();


        GameRoot.Instance.EffectSystem.MultiPlay<RewardEffect>(transform.position, x =>
                          {
                              //적에 전체체력에 퍼센트

                              var endtr = GameRoot.Instance.UISystem.GetUI<HudCurrencyTop>().GetRewardEndTr((int)Config.RewardType.Currency, (int)Config.CurrencyID.Money);


                              x.Set((int)Config.RewardType.Currency, (int)Config.CurrencyID.Money, endtr, () =>
                              {
                                  GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency, (int)Config.CurrencyID.Money, 300);
                              });
                              x.SetAutoRemove(true, 4f);
                          });


        GameRoot.Instance.EffectSystem.MultiPlay<RewardEffect>(transform.position, x =>
          {
              //적에 전체체력에 퍼센트

              var endtr = GameRoot.Instance.UISystem.GetUI<HudCurrencyTop>().GetRewardEndTr((int)Config.RewardType.Currency, (int)Config.CurrencyID.Material);


              x.Set((int)Config.RewardType.Currency, (int)Config.CurrencyID.Material, endtr, () =>
              {
                  GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency, (int)Config.CurrencyID.Material, 40);
              });
              x.SetAutoRemove(true, 4f);
          });

    }
}

