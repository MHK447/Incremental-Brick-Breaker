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

        var td = Tables.Instance.GetTable<IncreaUpgradeOrder>().GetData(UpgradeIdx);
        if (td == null) return;

        var finddata = GameRoot.Instance.UserData.Increaseugprades.Find(x => x.Order == td.order);
        if (finddata != null)
        {
            var tdinfo = Tables.Instance.GetTable<IncreaUpgradeInfo>().GetData(td.increase_idx);

            if (tdinfo != null && td.cost != null && td.cost.Count > 0)
            {
                var level = finddata.Level.Value == 0 ? 0 : finddata.Level.Value - 1;
                
                int costIdx = Mathf.Clamp(level, 0, td.cost.Count - 1);
                int currentCost = td.cost[costIdx];

                UpgradeCountText.text = $"{level}/{td.increase_max_lv}";
                UpgradeCostText.text = $"{currentCost}";

                var upgradevalue = td.upgrade_value[level];

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

