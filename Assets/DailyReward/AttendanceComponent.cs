using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class AttendanceComponent : MonoBehaviour
{
    public enum AttendanceState
    {
        None,
        NoneRecvie,
        Recive,
        CurrentClaim
    }

    [SerializeField]
    private List<Image> CommonRewardIconList = new List<Image>();


    [SerializeField]
    private GameObject RewardReciveObj;

    [SerializeField]
    private GameObject CurrentClaimObj;

    [SerializeField]
    private TextMeshProUGUI DailyText;

    private int Order = 0;


    private AttendanceState State = AttendanceState.None;




    public void Set(int order)
    {
        Order = order;

        SetState();

        foreach (var rewardicon in CommonRewardIconList)
        {
            ProjectUtility.SetActiveCheck(rewardicon.gameObject, false);
        }

        ProjectUtility.SetActiveCheck(CurrentClaimObj  , State == AttendanceState.CurrentClaim);
        ProjectUtility.SetActiveCheck(RewardReciveObj  , State == AttendanceState.Recive);

        var td = Tables.Instance.GetTable<AttendanceInfo>().GetData(order);

        if (td != null)
        {
            for (int i = 0; i < td.reward_type.Count; i++)
            {
                CommonRewardIconList[i].sprite = Config.Instance.GetRewardImage(td.reward_type[i], td.reward_idx[i]);
                ProjectUtility.SetActiveCheck(CommonRewardIconList[i].gameObject, true);
            }
        }


        DailyText.text = $"{Tables.Instance.GetTable<Localize>().GetFormat("str_desc_daily", order)}";
    }


    public void SetState()
    {
        var currecivecount = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.AttendanceCount) + 1;

        if(currecivecount == Order && GameRoot.Instance.AttendanceSystem.AttendanceTimeProperty.Value <= 0)
        {
            State = AttendanceState.CurrentClaim;
        }
        else if(currecivecount > Order)
        {
            State = AttendanceState.Recive;
        }
        else
        {
            State = AttendanceState.NoneRecvie;
        }
        

        //State = state;
    }
}
