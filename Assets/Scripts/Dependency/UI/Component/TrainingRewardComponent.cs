using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public enum TraniningRewardType
{
    Reward = 1,
    Class = 101,
}

public class TrainingRewardComponent : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI RewardValueText;

    [SerializeField]
    private TextMeshProUGUI UpgradeCountText;

    [SerializeField]
    private Image RewardImg;

    [SerializeField]
    private Image ClassImg;

    [SerializeField]
    private Image ClassUpFx;

    [SerializeField]
    private TextMeshProUGUI ClassDescText;

    [SerializeField]
    private Transform RewardRoot;

    [SerializeField]
    private Transform TrainingRoot;




    public void Set(int idx)
    {
        var td = Tables.Instance.GetTable<TrainingClassPass>().GetData(idx);

        UpgradeCountText.text = td.upgrade_count.ToString();

        ProjectUtility.SetActiveCheck(RewardRoot.gameObject, td.reward_type == (int)TraniningRewardType.Reward);
        ProjectUtility.SetActiveCheck(TrainingRoot.gameObject, td.reward_type == (int)TraniningRewardType.Class);

        RewardValueText.text = ProjectUtility.CalculateMoneyToString(td.reward_value);

        if (td.reward_type == (int)TraniningRewardType.Reward)
        {
            RewardImg.sprite = Config.Instance.GetRewardImage(td.reward_type, td.reward_index);

        }
        else
        {
            bool isclassup =  idx % 10 == 0 ? true : false;

            ClassImg.sprite = isclassup ?  AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, "Common_Ambulum_Epic")
             : AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, $"Common_Ambulum_Normal");

            ClassDescText.text = $"{Tables.Instance.GetTable<Localize>().GetString($"str_training_class_desc_{td.group}")} {td.reward_value}";
        }
    }


}

