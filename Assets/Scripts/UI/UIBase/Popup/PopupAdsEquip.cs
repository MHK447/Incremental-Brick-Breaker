using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using UniRx;

[UIPath("UI/Popup/PopupAdsEquip")]
public class PopupAdsEquip : CommonUIBase
{
    [SerializeField]
    private List<AdsEquipcomponent> AdsEquipComponents = new List<AdsEquipcomponent>();

    [SerializeField]
    private TextMeshProUGUI InitalizeTimeText;

    private CompositeDisposable disposables = new CompositeDisposable();

    public void Init()
    {
        var tdlist = Tables.Instance.GetTable<AdsEquipInfo>().DataList.ToList();


        foreach (var equipcomponent in AdsEquipComponents)
        {
            ProjectUtility.SetActiveCheck(equipcomponent.gameObject, false);
        }

        for (int i = 0; i < tdlist.Count; i++)
        {
            AdsEquipComponents[i].Set(tdlist[i].order);
            ProjectUtility.SetActiveCheck(AdsEquipComponents[i].gameObject, true);
        }

        disposables.Clear();

        GameRoot.Instance.DailyResetSystem.FreeDailyResetRemindTime.Subscribe(x =>
        {
            InitalizeTimeText.text = Tables.Instance.GetTable<Localize>().GetFormat("str_desc_initalize", ProjectUtility.GetTimeStringFormattingShort(x));
        }).AddTo(disposables);
    }


    void OnDestroy()
    {
        disposables.Clear();

    }

    void OnDisable()
    {
        disposables.Clear();
    }

}

