using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class TileAddComponent : MonoBehaviour
{
    public enum EquipType
    {
        Four = 1001,
        RightTwo = 1002,
        TopTwo = 1003,
        One = 1004,
    }

    [SerializeField]
    private List<GameObject> AddTileComponentList = new List<GameObject>();

    [SerializeField]
    private GameObject AdObj;


    [HideInInspector]
    public int EquipIdx = 0;

    [HideInInspector]
    public bool IsAd = false;


    public void Set(int equipidx , bool isad = false)
    {
        EquipIdx = equipidx;

        IsAd = isad;

        var td = Tables.Instance.GetTable<EquipInfo>().GetData(equipidx);

        foreach (var item in AddTileComponentList)
        {
            ProjectUtility.SetActiveCheck(item.gameObject, false);
        }

        if (td != null)
        {
            switch (equipidx)
            {
                case (int)EquipType.Four:
                    {
                        foreach (var item in AddTileComponentList)
                        {
                            ProjectUtility.SetActiveCheck(item.gameObject, true);
                        }
                    }
                    break;
                case (int)EquipType.RightTwo:
                    {
                        ProjectUtility.SetActiveCheck(AddTileComponentList[0].gameObject, true);
                        ProjectUtility.SetActiveCheck(AddTileComponentList[1].gameObject, true);
                    }
                    break;
                case (int)EquipType.TopTwo:
                    {
                        ProjectUtility.SetActiveCheck(AddTileComponentList[0].gameObject, true);
                        ProjectUtility.SetActiveCheck(AddTileComponentList[2].gameObject, true);

                    }
                    break;
                case (int)EquipType.One:
                    {
                        ProjectUtility.SetActiveCheck(AddTileComponentList[0].gameObject, true);
                    }
                    break;
            }

#if BANPOFRI_LOG
            ProjectUtility.SetActiveCheck(AdObj, false);
#else
            ProjectUtility.SetActiveCheck(AdObj, isad);
#endif
        }
    }


    public void AdCheck(bool isad)
    {
        IsAd = isad;
        ProjectUtility.SetActiveCheck(AdObj, isad);
    }
}

