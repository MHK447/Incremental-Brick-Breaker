using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;
public class LobbyStageRewardComponent : MonoBehaviour
{
    [SerializeField]
    private Image NoneRewardImg;

    [SerializeField]
    private Image RewardImg;


    [SerializeField]
    private TextMeshProUGUI NonRewardStageText;


    [SerializeField]
    private TextMeshProUGUI NoneRewardValueText;

    [SerializeField]
    private TextMeshProUGUI RewardValueText;


    [SerializeField]
    private GameObject RewardRoot;

    [SerializeField]
    private GameObject NoneRewardRoot;

    [SerializeField]
    private GameObject RewardDoneObj;

    [SerializeField]
    private GameObject NoneRewardDoneObj;

    [SerializeField]
    private Button RewardBtn;

    [SerializeField]
    private GameObject CompleteAdObj;

    [SerializeField]
    private GameObject NoneAdObj;

    private int CurStageIdx = 0;

    void Awake()
    {
        RewardBtn.onClick.AddListener(OnClickReward);
    }

    public void Set(int stage)
    {
        CurStageIdx = stage;

        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;

        var td = Tables.Instance.GetTable<StageInfo>().GetData(CurStageIdx);

        if (td != null)
        {

            var recordcount = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.StageReward, CurStageIdx);

            ProjectUtility.SetActiveCheck(RewardRoot, stageidx >= CurStageIdx && recordcount <= 0 && td.stage_reward_type > 0);
            ProjectUtility.SetActiveCheck(NoneRewardRoot, stageidx < CurStageIdx && recordcount <= 0 && td.stage_reward_type > 0);

            ProjectUtility.SetActiveCheck(RewardDoneObj, recordcount > 0);
            ProjectUtility.SetActiveCheck(NoneRewardDoneObj, recordcount <= 0);

            NonRewardStageText.text = CurStageIdx.ToString();

            NoneRewardValueText.text = RewardValueText.text = td.stage_reward_value.ToString();

            NoneRewardImg.sprite = RewardImg.sprite = Config.Instance.GetRewardImage(td.stage_reward_type, td.stage_reward_idx);

            ProjectUtility.SetActiveCheck(CompleteAdObj, td.stage_ad_check > 0 && !RewardDoneObj.gameObject.activeSelf);
            ProjectUtility.SetActiveCheck(NoneAdObj, td.stage_ad_check > 0 && !RewardDoneObj.gameObject.activeSelf);
        }
    }



    public void OnClickReward()
    {
        if (RewardRoot.activeSelf)
        {
            if (CompleteAdObj.gameObject.activeSelf)
            {
                GameRoot.Instance.PluginSystem.ADProp.ShowRewardAD(TpMaxProp.AdRewardType.StageClearAdReward, (result) =>
                {
                    GetReward();
                });
            }
            else
            {
                GetReward();
            }
        }
    }


    public void GetReward()
    {
        GameRoot.Instance.UserData.AddRecordCount(Config.RecordCountKeys.StageReward, 1, CurStageIdx);

        Set(CurStageIdx);

        var td = Tables.Instance.GetTable<StageInfo>().GetData(CurStageIdx);

        if (td != null)
        {
            switch (td.stage_reward_type)
            {
                case (int)Config.RewardType.Currency:
                    {
                        GameRoot.Instance.EffectSystem.MultiPlay<RewardEffect>(transform.position, x =>
                        {
                            var endtr = GameRoot.Instance.UISystem.GetUI<HudCurrencyTop>().GetRewardEndTr(td.stage_reward_type, td.stage_reward_idx);

                            x.Set(td.stage_reward_type, td.stage_reward_idx, endtr, () =>
                            {
                                GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency, td.stage_reward_idx, td.stage_reward_value);
                            });
                            x.SetAutoRemove(true, 4f);
                        });
                    }
                    break;
                case (int)Config.RewardType.RandHeroItem:
                case (int)Config.RewardType.Item:
                    {

                        List<RewardData> rewarddatas = new List<RewardData>();
                        rewarddatas.Add(new RewardData(td.stage_reward_type, td.stage_reward_idx, td.stage_reward_value));

                        GameRoot.Instance.UISystem.OpenUI<PagePurchaseConfirm>(popup => popup.Set(rewarddatas));

                    }
                    break;
                case (int)Config.RewardType.Card:
                    {
                        GameRoot.Instance.UISystem.OpenUI<PageGachaSkillCard>(page => page.ChoiceCard(td.stage_reward_idx, td.stage_reward_value));
                    }
                    break;
            }
        }
    }
}

