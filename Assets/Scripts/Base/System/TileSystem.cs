using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class TileSystem
{
    public enum TileWeaponType
    {
        Unit = 1,
        Weapon = 2,
    }






    public List<List<Vector2>> TileTypeList = new List<List<Vector2>>
    {
        new List<Vector2> { new Vector2(0, 0), new Vector2(0, 1)}, //1
        new List<Vector2> { new Vector2(0, 0), new Vector2(-1, 0) }, //2
        new List<Vector2> { new Vector2(0, 0), new Vector2(1, 0)  , new Vector2(0, 1) }, //3
        new List<Vector2> { new Vector2(0, 0), new Vector2(0, 1)  , new Vector2(0, 2) }, //4 
        new List<Vector2> { new Vector2(0, 0), new Vector2(-1, 0), new Vector2(0, 1) , new Vector2(-1, 1) }, //5
        new List<Vector2> { new Vector2(0, 0), new Vector2(-1, 1) , new Vector2(0, 1) }, //6
        new List<Vector2> { new Vector2(0, 0), new Vector2(0, 1) , new Vector2(1, 1) , new Vector2(2, 1) },
        new List<Vector2> { new Vector2(0, 0), new Vector2(1, 0) , new Vector2(0, 1) ,},
        new List<Vector2> { new Vector2(0, 0)},
        new List<Vector2> { new Vector2(0, 0), new Vector2(1, 1) , new Vector2(0, 1) }, //6},
    };


    public List<List<Vector2>> TileUnLockCheckList = new List<List<Vector2>>
    {
        new List<Vector2> { new Vector2(0, 0), new Vector2(0, 1) , new Vector2(1, 0), new Vector2(1, 1) },
        new List<Vector2> { new Vector2(0, 0), new Vector2(1, 0) },
        new List<Vector2> { new Vector2(0, 0), new Vector2(0, 1) },
        new List<Vector2> { new Vector2(0, 0)},
    };

    public List<EquipInfoData> EquipItemList = new List<EquipInfoData>();


    public void SpawnTileWeapon(int equipidx, int grade)
    {
        var td = Tables.Instance.GetTable<EquipInfo>().GetData(equipidx);


        if (td != null)
        {
            switch (td.item_type)
            {
                case (int)TileWeaponType.Unit:
                    {
                    }
                    break;
                case (int)TileWeaponType.Weapon:
                    break;

            }
        }

    }



}

