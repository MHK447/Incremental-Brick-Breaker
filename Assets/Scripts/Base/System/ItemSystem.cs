using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UniRx;


public class ItemSystem 
{
    public enum ItemType
    {
        EquipUpgradeItem = 1,
    }


    public void Create()
    {
        var tdlist = Tables.Instance.GetTable<ItemInfo>().DataList;


        foreach(var td in tdlist)
        {
            var itemdata = GameRoot.Instance.UserData.Itemdatas.Find(x => x.Itemidx == td.idx);

            if(itemdata == null)
            {
                itemdata = new ItemData()
                {
                    Itemidx = td.idx,
                    Itemcnt = new ReactiveProperty<int>(0),
                    Itemtype = td.type,
                };

                GameRoot.Instance.UserData.Itemdatas.Add(itemdata);
            }
        }
    }



    public ItemData GetItemData(int itemtype , int itemidx)
    {
        return GameRoot.Instance.UserData.Itemdatas.Find(x => x.Itemtype == itemtype && x.Itemidx == itemidx);
    }
}
