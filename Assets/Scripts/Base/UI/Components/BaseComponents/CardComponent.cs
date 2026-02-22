using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class CardComponent : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI CardNameText;

    [SerializeField]
    private TextMeshProUGUI CardLevelText;

    [SerializeField]
    private Slider CardSlider;

    [SerializeField]
    private TextMeshProUGUI CardCountText;

    private int CardIdx = 0;

    public void Set(int cardidx)
    {
        CardIdx = cardidx;

        var td = Tables.Instance.GetTable<CardInfo>().GetData(cardidx);

        var finddata = GameRoot.Instance.UserData.Carddatas.Find(x => x.Cardidx == cardidx);

        var level = finddata == null ? 1 : finddata.Cardlevel.Value;

        var cardleveltd = Tables.Instance.GetTable<CardUpgradeLevel>().GetData(level);

        if (td != null)
        {
            var count = finddata == null ? 0 : finddata.Cardcount.Value;

            CardNameText.text = Tables.Instance.GetTable<Localize>().GetString(td.card_name);
            CardLevelText.text = $"Lv.{level}";
            CardCountText.text = $"{count}/{cardleveltd.need_card}";
            CardSlider.value = (float)count / (float)cardleveltd.need_card;
        }
    }
}

