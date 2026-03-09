using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class IncreaseUpgradeGroup : MonoBehaviour
{
    [SerializeField]
    private List<IncreaUpgradeComponent> IncreaseUpgradeGroupList = new List<IncreaUpgradeComponent>();



    public void Init()
    {
        foreach (var increaupgrade in IncreaseUpgradeGroupList)
        {
            increaupgrade.Init(UnLockUpgrade);
        }


        
    }


    public void UnLockUpgrade(int idx)
    {
        var td = Tables.Instance.GetTable<IncreaseUpgradeOrder>().GetData(idx);

        if(td != null)
        {
            foreach(var order in td.open_order)
            {
                var finddata = IncreaseUpgradeGroupList.Find(x=> x.GetOrderIdx == order);

                if(finddata != null)
                {
                    finddata.UnLock();
                }
            }
        }
    }
}

