using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

[UIPath("UI/Page/PageEquipmentUpgrade")]
public class PageEquipmentUpgrade : CommonUIBase
{
    [SerializeField]
    private List<EquipUpgradeComponent> EquipItemComponentList = new List<EquipUpgradeComponent>();

    [SerializeField]
    private GameObject EquipItemPrefab;

    [SerializeField]
    private Transform EquipItemRoot;

    [SerializeField]
    private Animator animatorAnim;

    public void Set(List<HeroItemData> itemdatlist)
    {
        animatorAnim.Play("Success", 0, 0f);


        foreach(var item in EquipItemComponentList)
        {
            ProjectUtility.SetActiveCheck(item.gameObject, false);
        }

        for (int i = 0; i < itemdatlist.Count; i++)
        {
            var obj = GetCachedObject();

            if (obj != null)
            {
                obj.GetComponent<EquipUpgradeComponent>().Set(itemdatlist[i].Heroitemidx, itemdatlist[i].Grade);
                ProjectUtility.SetActiveCheck(obj, true);
            }
        }
    }


    private GameObject GetCachedObject()
    {
        var inst = EquipItemComponentList.Find(x => !x.gameObject.activeSelf);

        if (inst == null)
        {
            inst = GameObject.Instantiate(EquipItemPrefab).GetComponent<EquipUpgradeComponent>();
        }

        inst.transform.SetParent(EquipItemRoot);

        inst.transform.localScale = UnityEngine.Vector3.one;


        if (!EquipItemComponentList.Contains(inst))
        {
            EquipItemComponentList.Add(inst);
        }

        return inst.gameObject;
    }

}

