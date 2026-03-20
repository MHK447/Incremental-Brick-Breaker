using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public enum CarParts
{
    FrontPlate = 1,     // 자동차 앞판
    BodyMiddle = 2,     // 자동차 중간
    Rear = 3,           // 자동차 후면
    FrontWheel,     // 앞 바퀴
    RearWheel = 5,      // 뒷 바퀴
    Chassis = 6,        // 자동차 하부
    SawBlade = 7,       // 톱날
    MachineGun = 8      // 머신건
}

public enum ItemGradeType
{
    Common = 1,
    Rarre = 2,
    Epic = 3,
    Legend = 4,
    Mythic = 5,
}

public enum EquipAbilityType
{
    HpIncrease = 1,
    AttackIncrease = 2,
    AttackSpeedIncrease = 3,
}


public class EquipmentSystem
{


    public void Create()
    {

        if (GameRoot.Instance.UserData.Playerequipdata.Equipitemdatas.Count == 0)
        {
            var tdlist = Tables.Instance.GetTable<EquipItemInfo>().DataList.ToList();


            for (int i = 0; i < (int)CarParts.Chassis; i++)
            {
                var equipitemdata = new EquipItemData();
                equipitemdata.Equipitemidx = tdlist[i].item_idx;
                equipitemdata.Level = 1;
                equipitemdata.Grade = (int)ItemGradeType.Common;
                equipitemdata.Equipitemtype = tdlist[i].item_equip_type;
                GameRoot.Instance.UserData.Playerequipdata.Equipitemdatas.Add(equipitemdata);
            }

            GameRoot.Instance.UserData.Save();
        }
    }


    public int GetItemValue(int itemtype, int itemidx, int grade, int level)
    {
        var td = Tables.Instance.GetTable<EquipItemInfo>().GetData(new KeyValuePair<int, int>(itemtype, itemidx));

        if (td != null)
        {
            return td.ability_value * level * grade;
        }


        return 0;
    }

    public int GetTotalAbilityValue(EquipAbilityType abilityType)
    {
        int total = 0;
        var equipItems = GameRoot.Instance.UserData.Playerequipdata.Equipitemdatas;
        for (int i = 0; i < equipItems.Count; i++)
        {
            var equipItem = equipItems[i];
            var td = Tables.Instance.GetTable<EquipItemInfo>().GetData(new KeyValuePair<int, int>(equipItem.Equipitemtype, equipItem.Equipitemidx));
            if (td == null) continue;
            if (td.item_ability_type != (int)abilityType) continue;

            total += td.ability_value * equipItem.Level * equipItem.Grade;
        }

        return total;
    }

    public int GetAttackBonus()
    {
        return GetTotalAbilityValue(EquipAbilityType.AttackIncrease);
    }

    public int GetHealthBonus()
    {
        return GetTotalAbilityValue(EquipAbilityType.HpIncrease);
    }

    /// <summary> 공격속도 배율 (예: 합계 10이면 1.10f) </summary>
    public float GetAttackSpeedMultiplier()
    {
        int percent = GetTotalAbilityValue(EquipAbilityType.AttackSpeedIncrease);
        return 1f + percent * 0.01f;
    }



    public EquipItemData GetGacahaItemData()
    {
        var getgrade = GetGachaGrade();

        var tdlist = Tables.Instance.GetTable<EquipItemInfo>().DataList;

        var gettd = tdlist[Random.Range(0, tdlist.Count)];

        var level = GameRoot.Instance.UserData.Forgelevelproperty.Value;

        var equiupmenttd = Tables.Instance.GetTable<EquipmentGachaInfo>().GetData(level);

        if (gettd != null)
        {
            var randlevel = Random.Range(equiupmenttd.rand_level_min, equiupmenttd.rand_level_max + 1);

            var equipitemdata = new EquipItemData();
            equipitemdata.Equipitemidx = gettd.item_idx;
            equipitemdata.Level = randlevel;
            equipitemdata.Grade = getgrade;
            equipitemdata.Equipitemtype = gettd.item_equip_type;
            return equipitemdata;
        }

        return null;
    }


    public void EquipItem(EquipItemData equipItemData)
    {
        var finddata = GameRoot.Instance.UserData.Playerequipdata.FindEquipItemData(equipItemData.Equipitemtype);

        if (finddata != null)
        {
            GameRoot.Instance.UserData.Playerequipdata.Equipitemdatas.Remove(finddata);
            GameRoot.Instance.UserData.Playerequipdata.Equipitemdatas.Add(equipItemData);
            GameRoot.Instance.UserData.Save();
            GameRoot.Instance.IncreaMentalSystem.RefreshPlayerBonuses();
        }

    }


    public int GetGachaGrade()
    {
        int grade = 1;

        var level = GameRoot.Instance.UserData.Forgelevelproperty.Value;

        var equiupmenttd = Tables.Instance.GetTable<EquipmentGachaInfo>().GetData(level);

        if (equiupmenttd != null)
        {
            var ratios = equiupmenttd.gacha_ratio;
            if (ratios != null && ratios.Count > 0)
            {
                var totalWeight = ratios.Sum(x => Mathf.Max(0, x));
                if (totalWeight > 0)
                {
                    var roll = Random.Range(0, totalWeight);
                    var cumulative = 0;
                    for (int i = 0; i < ratios.Count; i++)
                    {
                        cumulative += Mathf.Max(0, ratios[i]);
                        if (roll < cumulative)
                        {
                            grade = i + 1;
                            break;
                        }
                    }
                }
            }
        }

        return grade;
    }
}

