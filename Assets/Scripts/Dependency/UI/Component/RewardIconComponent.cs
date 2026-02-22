using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class RewardIconComponent : MonoBehaviour
{
    [SerializeField]
    private Image RewardImg;

    [SerializeField]
    private TextMeshProUGUI RewardValueText;



    public void Set(int rewardtype , int rewardidx , string rewardvalue)
    {
        RewardImg.sprite = Config.Instance.GetRewardImage(rewardtype , rewardidx);

        RewardValueText.text = rewardvalue;
    }
}

