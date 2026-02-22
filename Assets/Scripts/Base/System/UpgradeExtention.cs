using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
public static class UpgradeExtention
{
    public class CostUpgradeData
    {
        public int Level = 0;
        public System.Numerics.BigInteger Cost = 0;
    }

    static Dictionary<int, Dictionary<int, CostUpgradeData>> CostUpgradeDatas = new Dictionary<int, Dictionary<int, CostUpgradeData>>();



    public static System.Numerics.BigInteger GetCost(this BanpoFri.UpgradeInfoData tableData, int idx, int level)
    {

        if (CostUpgradeDatas.ContainsKey(idx) && CostUpgradeDatas[idx].ContainsKey(level))
        {
            return CostUpgradeDatas[idx][level].Cost;
        }


        return 0;
    }


    public static void CalculateUpgradeTable(this BanpoFri.UpgradeInfoData tableData, int idx, int maxlevel)
    {
        if (CostUpgradeDatas.ContainsKey(idx))
        {
            var startvalue = CostUpgradeDatas[idx].Count;

            for (int i = startvalue; i < maxlevel; ++i)
            {
                var nextcost = CostUpgradeDatas[idx][i - 1].Cost + ProjectUtility.PercentCalc(CostUpgradeDatas[idx][i - 1].Cost, tableData.inceease_upgrade_cost);

                CostUpgradeDatas[idx].Add(i, new CostUpgradeData() { Level = i, Cost = nextcost });
            }

        }
        else //first
        {
            CostUpgradeDatas[idx] = new Dictionary<int, CostUpgradeData>();
            
            for (int i = 1; i < maxlevel; ++i)
            {
                if (i == 1)
                {
                    CostUpgradeDatas[idx].Add(i, new CostUpgradeData() { Level = i, Cost = tableData.base_upgrade_cost });
                }
                else
                {
                    var nextcost = CostUpgradeDatas[idx][i - 1].Cost + ProjectUtility.PercentCalc(CostUpgradeDatas[idx][i - 1].Cost, tableData.inceease_upgrade_cost);

                    CostUpgradeDatas[idx].Add(i, new CostUpgradeData() { Level = i, Cost = nextcost });
                }
            }
        }
    }



}
