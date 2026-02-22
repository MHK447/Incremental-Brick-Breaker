using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using UniRx;

public class EquipmentMaterialGroup : MonoBehaviour
{
    [SerializeField]
    private List<TextMeshProUGUI> materialTextList = new List<TextMeshProUGUI>();

    private CompositeDisposable disposables = new CompositeDisposable();




    public void Init()
    {
        var itemtypelist = Tables.Instance.GetTable<ItemInfo>().DataList.ToList();

        disposables.Clear();

        for(int i = 0; i < itemtypelist.Count; i++)
        {
            var itemdata = GameRoot.Instance.ItemSystem.GetItemData(itemtypelist[i].type, itemtypelist[i].idx);
            var index = i; // 로컬 변수로 복사하여 클로저 문제 해결

            if(itemdata != null)
            {
                materialTextList[index].text = $"x{itemdata.Itemcnt.Value}";

                itemdata.Itemcnt.Subscribe(x =>
                {
                    materialTextList[index].text = $"x{x}";
                }).AddTo(disposables);
                
            }
        }
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

