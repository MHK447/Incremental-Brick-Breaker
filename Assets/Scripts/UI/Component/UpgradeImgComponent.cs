using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using UniRx;

public class UpgradeImgComponent : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI UpgradeDescText;

    [SerializeField]
    private TextMeshProUGUI UpgradeCostText;

    [SerializeField]
    private TextMeshProUGUI UpgradeCountText;

    private int UpgradeIdx = 0;

    private CompositeDisposable disposables = new CompositeDisposable();

    void Awake()
    {
        var cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable = false;
    }

    public void Set(int upgradeidx)
    {
        UpgradeIdx = upgradeidx;


        var finddata = GameRoot.Instance.UserData.Increaseugprades.Find(x => x.Idx == UpgradeIdx);
        if (finddata != null)
        {
            var td = Tables.Instance.GetTable<IncreaseUpgradeOrder>().GetData(UpgradeIdx);
            var tdinfo = Tables.Instance.GetTable<IncreaseUpgradeInfo>().GetData(UpgradeIdx);

            if (td != null && tdinfo != null && td.cost != null && td.cost.Count > 0)
            {
                int level = finddata.Level.Value;
                int costIdx = Mathf.Clamp(level, 0, td.cost.Count - 1);
                int currentCost = td.cost[costIdx];

                UpgradeCountText.text = $"{level}/{td.increase_max_lv}";
                UpgradeCostText.text = $"{currentCost}";

                var upgradevalue = td.upgrade_value[finddata.Level.Value];

                UpgradeDescText.text =  upgradevalue == -1 ? 
                 Tables.Instance.GetTable<Localize>().GetString(tdinfo.upgrade_name) : 
                 Tables.Instance.GetTable<Localize>().GetFormat(tdinfo.upgrade_name, upgradevalue);

                disposables.Clear();

                GameRoot.Instance.UserData.Money.Subscribe(
                    x =>
                    {
                        UpgradeCostText.color = x >= currentCost ? Color.white : Color.red;
                    }).AddTo(disposables);
            }
        }
    }


    void OnDisable()
    {
        disposables.Clear();
    }

    void OnDestroy()
    {
        disposables.Clear();
    }
}

