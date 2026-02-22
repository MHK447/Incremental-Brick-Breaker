using UnityEngine;
using BanpoFri;
using UniRx;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class StageResultComponent : MonoBehaviour
{
    [SerializeField]
    private Image RewardImg;

    [SerializeField]
    private TextMeshProUGUI RewardText;


    [HideInInspector]
    public System.Numerics.BigInteger RewardValue = 0;

    public void Set(int rewardtype, int rewardidx, bool isad = false)
    {

        var multivalue = isad ? 2 : 1;

        RewardImg.sprite = Config.Instance.GetRewardImage(rewardtype, rewardidx);

        var td = Tables.Instance.GetTable<StageRewardInfo>().GetData(rewardidx);

        var stagerewardpercent = GameRoot.Instance.StageRewardSystem.StageRewardpercent(GameRoot.Instance.UserData.Stageidx.Value);

        var getmoneyvalue = GameRoot.Instance.StageRewardSystem.GetStageRewardMoney();

        // 올림 처리를 먼저 적용한 후 x2 배수 적용
        var baseValue = getmoneyvalue * (int)stagerewardpercent;
        var roundedValue = (baseValue + 99) / 100; // 올림 처리
        var moneyvalue = roundedValue * multivalue; // x2 적용

        RewardValue = rewardidx == (int)Config.CurrencyID.Money ? moneyvalue
        : td.coin_base_cost * multivalue;


        RewardText.text = ProjectUtility.CalculateMoneyToString(RewardValue);

        RewardText.text = "0";
        GameRoot.Instance.WaitRealTimeAndCallback(0.5f, DirectionAnim);
    }



    public void DirectionAnim()
    {
        if (this.gameObject.activeSelf == false) return;

        // DOTween을 사용한 카운팅 애니메이션 (1.5초 동안)
        float duration = 1.5f;

        DOVirtual.Float(0f, 1f, duration, (value) =>
            {
                // 비율로 현재 값 계산 (BigInteger 직접 계산)
                System.Numerics.BigInteger currentValue = RewardValue * (System.Numerics.BigInteger)(value * 1000) / 1000;
                RewardText.text = ProjectUtility.CalculateMoneyToString(currentValue);
            })
            .SetEase(Ease.OutCubic) // 처음엔 빠르고 끝에 천천히
            .SetUpdate(true)
            .OnComplete(() =>
            {
                // 애니메이션 완료 시 정확한 최종 값 표시
                RewardText.text = ProjectUtility.CalculateMoneyToString(RewardValue);
            });
    }
}
