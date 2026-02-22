using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class ShopArtifactGachaComponent : MonoBehaviour
{
    [SerializeField]
    private Button OpenTenBtn;

    [SerializeField]
    private Button OpenOneBtn;

    [SerializeField]
    private Button ArtifactInfoBtn;

    [SerializeField]
    private TextMeshProUGUI OpeneOnePriceText;

    [SerializeField]
    private TextMeshProUGUI OpenTenPriceText;

    private int CostValue = 200;

    public void Awake()
    {
        OpenTenBtn.onClick.AddListener(OnClickOpenTen);
        OpenOneBtn.onClick.AddListener(OnClickOpenOne);
        ArtifactInfoBtn.onClick.AddListener(OnClickArtifactInfo);
    }


    public void Init()
    {
        ProjectUtility.SetActiveCheck(this.gameObject, GameRoot.Instance.UserData.Herogroudata.Equipplayeridx > 0);

        OpeneOnePriceText.text = CostValue.ToString();
        OpenTenPriceText.text = (CostValue * 10).ToString();
    }

    public void OnClickOpenTen()
    {

        if (GameRoot.Instance.UserData.Cash.Value >= CostValue * 10)
        {
            GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency, (int)Config.CurrencyID.Cash, -(CostValue * 10));

            var tdlist = Tables.Instance.GetTable<ArtifactGachaRatio>().DataList;
            if (tdlist == null || tdlist.Count == 0)
                return;

            int totalRatio = tdlist.Sum(x => x.ratio);

            // 뽑기 10번마다 ratio로 등급을 새로 정해서 RandHeroItem 1개씩 추가
            List<RewardData> rewarddatas = new List<RewardData>();
            for (int i = 0; i < 10; i++)
            {
                int r = UnityEngine.Random.Range(0, totalRatio);
                int acc = 0;
                int selectedGrade = tdlist[0].grade;
                foreach (var data in tdlist)
                {
                    acc += data.ratio;
                    if (r < acc)
                    {
                        selectedGrade = data.grade;
                        break;
                    }
                }
                rewarddatas.Add(new RewardData((int)Config.RewardType.RandHeroItem, selectedGrade, 1));
            }

            GameRoot.Instance.UISystem.OpenUI<PagePurchaseConfirm>(popup => popup.Set(rewarddatas));
        }
        else
        {
            GameRoot.Instance.UISystem.OpenUI<PopupCashInsufficent>();
        }

    }

    public void OnClickOpenOne()
    {
        if (GameRoot.Instance.UserData.Cash.Value >= CostValue)
        {
            GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency, (int)Config.CurrencyID.Cash,-CostValue);

            var tdlist = Tables.Instance.GetTable<ArtifactGachaRatio>().DataList;
            if (tdlist == null || tdlist.Count == 0)
                return;

            int totalRatio = tdlist.Sum(x => x.ratio);

            // 뽑기 10번마다 ratio로 등급을 새로 정해서 RandHeroItem 1개씩 추가
            List<RewardData> rewarddatas = new List<RewardData>();

            int r = UnityEngine.Random.Range(0, totalRatio);
            int acc = 0;
            int selectedGrade = tdlist[0].grade;
            foreach (var data in tdlist)
            {
                acc += data.ratio;
                if (r < acc)
                {
                    selectedGrade = data.grade;
                    break;
                }
            }
            rewarddatas.Add(new RewardData((int)Config.RewardType.RandHeroItem, selectedGrade, 1));


            GameRoot.Instance.UISystem.OpenUI<PagePurchaseConfirm>(popup => popup.Set(rewarddatas));
        }
        else
        {
            GameRoot.Instance.UISystem.OpenUI<PopupCashInsufficent>();
        }
    }

    public void OnClickArtifactInfo()
    {
        GameRoot.Instance.UISystem.OpenUI<PopupArtifactChance>();
    }
}

