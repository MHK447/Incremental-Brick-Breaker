using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class PurchaseConfirmItemComponent : MonoBehaviour
{
    [SerializeField]
    private Image ItemImg;

    [SerializeField]
    private TextMeshProUGUI CountText;

    [SerializeField]
    private EquipAttributeComponent AttributeComponent;

    [SerializeField]
    private Image BgImg;

    private int RewardType = 0;
    private int RewardIdx = 0;
    private int RewardValue = 0;

    public void Set(int rewardtype, int rewardidx, int rewardvalue)
    {
        RewardType = rewardtype;
        RewardIdx = rewardidx;
        RewardValue = rewardvalue;

        ItemImg.sprite = Config.Instance.GetRewardImage(rewardtype, rewardidx);
        CountText.text = rewardvalue.ToString();



        BgImg.color = rewardtype == (int)Config.RewardType.HeroEquipment ? Config.Instance.GetImageColor($"item_grade_color_{rewardvalue}") :
        Config.Instance.GetImageColor("currency_bg_color");



        ProjectUtility.SetActiveCheck(AttributeComponent.gameObject, rewardtype == (int)Config.RewardType.HeroEquipment);


        if (rewardtype == (int)Config.RewardType.HeroEquipment)
        {
            var td = Tables.Instance.GetTable<HeroItemInfo>().GetData(rewardidx);

            if (td != null)
            {
                AttributeComponent.Set(rewardvalue, td.item_equip_type);
            }

            CountText.text = "1";
        }


        GameRoot.Instance.UserData.SetReward((int)RewardType, RewardIdx, RewardValue);

    }
}

