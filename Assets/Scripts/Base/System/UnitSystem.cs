using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public enum UnitStatusType
{
    Attack = 1,
    Hp,
    AttackSpeed,
    AttackRange,
    Shield,
    SILVERCOIN,
    HpHeal,
}


public class UnitSystem
{
    public enum UnitType
    {
        Unit = 1,
        Wall = 2,
        Tower = 3,
    }


    public int SelectUnitIdx = 0;

    public void Create()
    {
        if (GameRoot.Instance.UserData.Unitgroupdata.Unitdatas.Count == 0)
        {
            GameRoot.Instance.UserData.Unitgroupdata.Unitdatas.Add(new UnitData()
            {
                Unitidx = (int)Config.WeaponUnit.Knife,
                Unitlevel = 1
            });

            GameRoot.Instance.UserData.Unitgroupdata.Equipidxs.Add((int)Config.WeaponUnit.Knife);
            GameRoot.Instance.UserData.Save();
        }
    }


    public void EquipUnit(int unequipidx, int equipidx)
    {
        if (SelectUnitIdx == 0) return;

        var selectunitdata = GameRoot.Instance.UserData.Unitgroupdata.FindUnit(equipidx);

        if (selectunitdata == null) return;

        var unequipidxidx = GameRoot.Instance.UserData.Unitgroupdata.Equipidxs.IndexOf(unequipidx);

        if (unequipidxidx != -1)
        {
            GameRoot.Instance.UserData.Unitgroupdata.Equipidxs[unequipidxidx] = selectunitdata.Unitidx;
        }

        GameRoot.Instance.UserData.Save();

        SelectUnitIdx = -1;

    }


    public double GetUnitStatus(int unitidx, UnitStatusType statusType)
    {

        //add training
        return GameRoot.Instance.CardSystem.GetStatusValue(unitidx, statusType);
    }

}