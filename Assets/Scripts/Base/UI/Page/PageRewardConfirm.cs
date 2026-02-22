using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using DG.Tweening;
public class RewardData
{
    public int RewardType;
    public int RewardIdx;

    public System.Numerics.BigInteger RewardValue;

    public RewardData(int rewardtype, int rewardidx, System.Numerics.BigInteger rewardvalue)
    {
        RewardType = rewardtype;
        RewardIdx = rewardidx;
        RewardValue = rewardvalue;
    }
}

[UIPath("UI/Page/PageRewardConfirm")]
public class PageRewardConfirm : UIBase
{
    [SerializeField]
    private List<RewardIconComponent> RewardIconComponentList = new List<RewardIconComponent>();


    [SerializeField]
    private GameObject FxObj;

    private List<RewardData> RewardDatas = new List<RewardData>();

    [SerializeField]
    private AdsButton RewardDoubleBtn;

    private bool AdWatch = false;


    protected override void Awake()
    {
        base.Awake();

        RewardDoubleBtn.AddListener(TpMaxProp.AdRewardType.InSufficientSilverCoint, OnClickAdReward);
    }

    public void Init(List<RewardData> rewarddatas, bool isdouble = false)
    {
        SoundPlayer.Instance.PlaySound("popuprewardget");

        RewardDatas = rewarddatas;

        AdWatch = false;

        foreach (var rewardicon in RewardIconComponentList)
        {
            rewardicon.gameObject.SetActive(false);
        }

        ProjectUtility.SetActiveCheck(RewardDoubleBtn.gameObject , isdouble);


        PlayDirection();
    }


    public void PlayDirection()
    {
        ProjectUtility.SetActiveCheck(FxObj, false);

        // 보상 데이터 설정 및 초기 스케일 설정
        for (int i = 0; i < RewardDatas.Count; i++)
        {
            RewardIconComponentList[i].Set(RewardDatas[i].RewardType, RewardDatas[i].RewardIdx, RewardDatas[i].RewardValue.ToString());
            ProjectUtility.SetActiveCheck(RewardIconComponentList[i].gameObject, true);
            RewardIconComponentList[i].transform.localScale = Vector3.zero; // 초기 스케일 0으로 설정
        }

        // 순차적 스케일 애니메이션 시퀀스 생성
        Sequence sequence = DOTween.Sequence();

        for (int i = 0; i < RewardDatas.Count; i++)
        {
            int index = i;
            // 각 보상을 순차적으로 스케일 애니메이션
            sequence.Append(RewardIconComponentList[index].transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack));
            sequence.AppendInterval(0.1f); // 각 보상 사이 간격
        }

        // 마지막에 폭죽 효과 활성화
        sequence.AppendCallback(() =>
        {
            ProjectUtility.SetActiveCheck(FxObj, true);
        });

        sequence.Play();
    }

    public override void Hide()
    {
        base.Hide();

        for (int i = 0; i < RewardDatas.Count; i++)
        {
            GameRoot.Instance.UserData.SetReward(RewardDatas[i].RewardType, RewardDatas[i].RewardIdx, RewardDatas[i].RewardValue);

        }
    }

    public void OnClickAdReward()
    {
        AdWatch = true;

        Hide();
    }

}

