using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class AdsEquipcomponent : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI CountText;

    [SerializeField]
    private Image RewardImg;

    [SerializeField]
    private GameObject RewardCheckObj;

    [SerializeField]
    private GameObject FreeObj;


    [SerializeField]
    private Button RewardBtn;

    private int Order = 0;


    void Awake()
    {
        RewardBtn.onClick.AddListener(OnClickReward);
    }

    public void Set(int order)
    {
        Order = order;

        var td = Tables.Instance.GetTable<AdsEquipInfo>().GetData(order);

        CountText.text = td.reward_value.ToString();

        RewardImg.sprite = Config.Instance.GetRewardImage(td.reward_type, td.reward_idx);

        var count = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.AdsEquipCount);

        ProjectUtility.SetActiveCheck(RewardCheckObj, count >= order);

        ProjectUtility.SetActiveCheck(FreeObj, 1 == order);

    }


    public void OnClickReward()
    {
        var getcount = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.AdsEquipCount) + 1;

        if (Order == getcount)
        {

            if(1 == Order)
            {
                GetReward();
            }
            else
            {
                GameRoot.Instance.PluginSystem.ADProp.ShowRewardAD(TpMaxProp.AdRewardType.DailyEquipmentAdReward, (result) =>
                {
                    if (result)
                    {
                        GetReward();
                    }
                });
            }
        }
    }

    public void GetReward()
    {

        var td = Tables.Instance.GetTable<AdsEquipInfo>().GetData(Order);


        GameRoot.Instance.UserData.AddRecordCount(Config.RecordCountKeys.AdsEquipCount, 1);
        Set(Order);

        List<RewardData> rewarddatas = new List<RewardData>();
        rewarddatas.Add(new RewardData(td.reward_type, td.reward_idx, td.reward_value));

        GameRoot.Instance.UISystem.OpenUI<PagePurchaseConfirm>(popup => popup.Set(rewarddatas));

        GameRoot.Instance.UISystem.GetUI<PageLobbyBattle>()?.AdsEquipBtnCheck();
    }

}

