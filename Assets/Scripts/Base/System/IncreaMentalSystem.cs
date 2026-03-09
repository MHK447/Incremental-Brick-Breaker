using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class IncreaMentalSystem
{
    public List<int> UpgradeUnLockOrderList = new List<int>();

    public void Create()
    {
        var tdlist = Tables.Instance.GetTable<IncreaseUpgradeOrder>().DataList;


        if (tdlist.Count != GameRoot.Instance.UserData.Increaseugprades.Count)
        {
            foreach (var td in tdlist)
            {
                var finddata = GameRoot.Instance.UserData.Increaseugprades.Find(x => x.Idx == td.increase_idx);

                if (finddata != null) continue;



                var newdata = new InCreaseUpgradeData();
                newdata.Idx = td.increase_idx;
                newdata.Level.Value = td.order == 1 ? 1 : 0;
                GameRoot.Instance.UserData.Increaseugprades.Add(newdata);
            }
        }

        UpgradeUnLockOrderList.Clear();

        foreach (var data in GameRoot.Instance.UserData.Increaseugprades)
        {
            if (data.Level.Value == 0) continue;

            var td = Tables.Instance.GetTable<IncreaseUpgradeOrder>().GetData(data.Idx);
            if (td != null)
            {
                foreach (var order in td.open_order)
                {
                    if (order > 0)
                    {
                        UpgradeUnLockOrderList.Add(order);
                    }
                }
            }
        }
    }

    public void IncreaseLevelUp(int idx)
    {
        var finddata = FindData(idx);
        if (finddata != null)
        {
            finddata.Level.Value++;

            if(!UpgradeUnLockOrderList.Contains(idx))
            {
                UpgradeUnLockOrderList.Add(idx);
            }
        }
    }


    public InCreaseUpgradeData FindData(int idx)
    {
        return GameRoot.Instance.UserData.Increaseugprades.Find(x => x.Idx == idx);
    }
}

