using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using Unity.VisualScripting;

public class EquipmnentUpgradeGroup : MonoBehaviour
{
    [SerializeField]
    private List<EquipmentComponent> EquipmentComponentList = new List<EquipmentComponent>();




    public void Init()
    {
        foreach(var component in EquipmentComponentList)
        {
            component.Init();
        }
    }
}

