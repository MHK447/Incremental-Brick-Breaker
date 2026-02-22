using UnityEngine;
using System.Collections.Generic;
using BanpoFri;
using System.Linq;
using UnityEngine.UI;
public class UnitUpgradeLevelGroup : MonoBehaviour
{
    [SerializeField]
    private List<UnitUpgradeLevelComponent> UnitUpgradeLevelComponentList = new List<UnitUpgradeLevelComponent>();

    public void Set(int curlevel)
    {
        for(int i = 0; i < UnitUpgradeLevelComponentList.Count; i++)
        {
            UnitUpgradeLevelComponentList[i].Activecheck(curlevel >= i + 1);
        }

        if(curlevel >= 0 && curlevel < UnitUpgradeLevelComponentList.Count)
        {
            UnitUpgradeLevelComponentList[curlevel].AlpahScaleCheck();
        }
    }
}

