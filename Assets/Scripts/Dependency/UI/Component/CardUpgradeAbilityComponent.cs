using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class CardUpgradeAbilityComponent : MonoBehaviour
{
    [SerializeField]
    private GameObject AbilityLockObj;

    [SerializeField]
    private GameObject AbilityOnObj;


    [SerializeField]
    private TextMeshProUGUI AbilityDescText;

    [SerializeField]
    private TextMeshProUGUI AbilityLockDescText;

    [SerializeField]
    private TextMeshProUGUI LevelText;

    [SerializeField]
    private TextMeshProUGUI LockLevelText;

    private int CardIdx = 0;

    private int Level =0;


    public void Set(int cardidx, int level)
    {
        CardIdx = cardidx;
        Level = level;
        
        var carddata  = GameRoot.Instance.UserData.Carddatas.Find(x=> x.Cardidx == cardidx);

        int cardlevel = carddata == null ? 1 : carddata.Cardlevel.Value;

        var cardtd = Tables.Instance.GetTable<CardInfo>().GetData(CardIdx);
    
        LockLevelText.text = LevelText.text = $"Lv.{Level}";

        var findindex = cardtd.card_ability_level.FindIndex(x => x == Level);

        AbilityDescText.text = AbilityLockDescText.text =
         Tables.Instance.GetTable<Localize>().GetFormat($"card_ability_type_desc_{cardtd.card_ability_type[findindex]}",
        cardtd.card_ability_value[findindex]);


        ProjectUtility.SetActiveCheck(AbilityLockObj, cardlevel < Level);
        ProjectUtility.SetActiveCheck(AbilityOnObj, cardlevel >= Level);
    }
}

