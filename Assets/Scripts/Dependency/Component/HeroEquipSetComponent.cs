using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

public class HeroEquipSetComponent : MonoBehaviour
{
    public enum EquipState
    {
        Helmet = 1,
        Armor = 2,
        Shoose = 3,
        Ring = 4,
        Set = 5,
        SetEquipItem = 6,
    }

    private int SetIdx = 0;

    [SerializeField]
    private List<HeroItemComponent> HeroItemComponentList = new List<HeroItemComponent>();

    [SerializeField]
    private HeroItemComponent HeroItemPrefab;

    [SerializeField]
    private Transform HeroItemRoot;

    [SerializeField]
    private TextMeshProUGUI HeroSetNameText;

    private EquipState CurState = EquipState.Set;

    private int SetType = 0;

    public void Set(int setidx, EquipState equipstate)
    {
        SetIdx = setidx;
        CurState = equipstate;
        SetType = -1;
        
        var td = Tables.Instance.GetTable<HeroItemSet>().GetData(SetIdx);
        
        if(td != null)
        {
            SetType = td.set_ability_type;
        }
        foreach (var HeroItemComponent in HeroItemComponentList)
        {
            ProjectUtility.SetActiveCheck(HeroItemComponent.gameObject, false);
        }

        HeroSetNameText.text = Tables.Instance.GetTable<Localize>().GetString($"item_equipment_set_{SetIdx}");

        SetState();
    }


    public void SetState()
    {
        switch (CurState)
        {
            case EquipState.SetEquipItem:
                {
                    var findlist = GameRoot.Instance.UserData.Herogroudata.Heroitemdatas.FindAll(x => Tables.Instance.GetTable<HeroItemInfo>().GetData(x.Heroitemidx).item_set_type == SetIdx);

                    for (int i = 0; i < findlist.Count; i++)
                    {
                        var obj = GetCachedObject(findlist[i].Heroitemidx);

                        if (obj != null)
                        {
                            obj.GetComponent<HeroItemComponent>().Set(findlist[i].Heroitemidx, findlist[i]);
                            ProjectUtility.SetActiveCheck(obj, true);
                        }
                    }
                }
                break;
            case EquipState.Set:
                {
                    var tdlist = Tables.Instance.GetTable<HeroItemInfo>().DataList.Where(x => x.item_set_type == SetType).ToList();

                    for (int i = 0; i < tdlist.Count; i++)
                    {
                        var finddata = GameRoot.Instance.HeroSystem.FindHeroItemGradeValue(tdlist[i].item_idx);

                        var obj = GetCachedObject(tdlist[i].item_idx);

                        if (obj != null)
                        {
                            obj.GetComponent<HeroItemComponent>().Set(tdlist[i].item_idx, finddata);
                            ProjectUtility.SetActiveCheck(obj, true);
                        }
                    }
                }
                break;
            case EquipState.Helmet:
            case EquipState.Shoose:
            case EquipState.Armor:
            case EquipState.Ring:
                {
                    var findlist = GameRoot.Instance.UserData.Herogroudata.Heroitemdatas.FindAll(x =>
                     Tables.Instance.GetTable<HeroItemInfo>().GetData(x.Heroitemidx).item_equip_type == (int)CurState);

                    for (int i = 0; i < findlist.Count; i++)
                    {
                        var obj = GetCachedObject(findlist[i].Heroitemidx);

                        if (obj != null)
                        {
                            obj.GetComponent<HeroItemComponent>().Set(findlist[i].Heroitemidx, findlist[i]);
                            ProjectUtility.SetActiveCheck(obj, true);
                        }

                    }
                    break;
                }

        }
    }




    private GameObject GetCachedObject(int type)
    {
        var inst = HeroItemComponentList.Find(x => !x.gameObject.activeSelf);

        if (inst == null)
        {
            inst = GameObject.Instantiate(HeroItemPrefab);
        }

        inst.transform.SetParent(HeroItemRoot);

        inst.transform.localScale = UnityEngine.Vector3.one;


        if (!HeroItemComponentList.Contains(inst))
        {
            HeroItemComponentList.Add(inst);
        }

        return inst.gameObject;
    }

}

