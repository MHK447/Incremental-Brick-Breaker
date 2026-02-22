using TMPro;
using BanpoFri;
using UnityEngine;
using UnityEngine.UI;

public class ResultrewardComponent : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI AdRewardValueText;

    [SerializeField]
    private TextMeshProUGUI BaseRewardText;

    [SerializeField]
    private Image AdRewardIcon;

    [SerializeField]
    private Image BaseRewardIcon;

    [SerializeField]
    private GameObject AdRewardRoot;
    
    public GameObject GetAdRewardRoot {get {return AdRewardRoot;}}
    public GameObject GetBaseRewardRoot {get {return BaseRewardRoot;}}

    [SerializeField]
    private GameObject BaseRewardRoot;

    private int RewardType = 0;
    private int RewardIdx = 0;
    private double RewardValue = 0;

    private bool IsFreeCheck = false;

    public void Set(int rewardtype, int rewardidx, int rewardvalue, int adRewardValue)
    {
        ProjectUtility.SetActiveCheck(AdRewardRoot, rewardvalue > 0);
        ProjectUtility.SetActiveCheck(BaseRewardRoot, rewardvalue > 0);

        IsFreeCheck = true;
        //!GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.InGameOpen);

        RewardType = rewardtype;
        RewardIdx = rewardidx;
        RewardValue = rewardvalue;

        AdRewardValueText.text = adRewardValue.ToString();
        BaseRewardText.text = System.Math.Round(RewardValue).ToString();

        //AdRewardIcon.sprite = BaseRewardIcon.sprite = AtlasManager.Instance.GetRewardImg(RewardType, RewardIdx);
    }
}
