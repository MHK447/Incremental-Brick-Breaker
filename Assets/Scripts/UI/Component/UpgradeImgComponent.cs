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

            if (td != null && tdinfo != null)
            {
                UpgradeCountText.text = $"{finddata.Level.Value}/{td.increase_max_lv}";
                UpgradeCostText.text = $"{td.cost}";

                UpgradeDescText.text = Tables.Instance.GetTable<Localize>().GetString(tdinfo.upgrade_desc);

                disposables.Clear();

                GameRoot.Instance.UserData.Money.Subscribe(
                    x =>
                    {
                        UpgradeCostText.color = x >= td.cost ? Color.white : Color.red;
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

