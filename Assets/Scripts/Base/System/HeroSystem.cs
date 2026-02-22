using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;
using UniRx;
public enum HeroEquipType
{
    Helmat = 1,
    Armor = 2,
    Shoose = 3,
    Ring = 4,
}

public enum HeroStatus
{
    Atttack = 0,
    Hp = 1,
    AtkSpeed = 2,
}

public enum GradeBuffType
{
    HeroAttackIncrease = 1,
    HeroHpIncrease = 2,
    HeroAttackSpeedIncrease = 3,
    KillUnitSilverCoinIncrease = 4,
    WarriorUnitAttackIncrease = 5,
    TowerAttackIncrease = 6,
    TowerAttackSpeedIncrease = 7,
    WarriorHpIncrease = 8,
}



public enum HeroItemSetType
{
    StartSilvercoinIncrease = 1,
    Kill20CountAddSilverCoin = 2,
    FirstStartRandWeapon = 3,
    FirstStartRandTile = 4,
}

public class HeroSystem
{
    public int equip_item_level_gold_cost = 0;
    public int equip_item_level_count = 0;

    private bool isCreated = false;

    public void Create()
    {
        if (isCreated) return;
        isCreated = true;

        if (GameRoot.Instance.UserData.Herogroudata.Equipheroitems.Count == 0)
        {
            for (int i = 0; i < (int)HeroEquipType.Ring; i++)
            {
                GameRoot.Instance.UserData.Herogroudata.Equipheroitems.Add(new HeroEquipItemData()
                {
                    Level = new ReactiveProperty<int>(1),
                    Heroitemtype = i + 1,
                });
            }
        }

        foreach (var equipheroitem in GameRoot.Instance.UserData.Herogroudata.Equipheroitems)
        {
            var finddata = GameRoot.Instance.UserData.Herogroudata.Heroitemdatas.Find(x => x.Isequip.Value &&
            Tables.Instance.GetTable<HeroItemInfo>().GetData(x.Heroitemidx).item_equip_type == equipheroitem.Heroitemtype);

            if (finddata != null)
            {
                equipheroitem.Heroitemdata = finddata;
            }
        }

        equip_item_level_gold_cost = Tables.Instance.GetTable<Define>().GetData("equip_item_level_gold_cost").value;
        equip_item_level_count = Tables.Instance.GetTable<Define>().GetData("equip_item_level_count").value;

    }



    public HeroEquipItemData FindHeroEquipItemData(int equiptype)
    {
        return GameRoot.Instance.UserData.Herogroudata.Equipheroitems.Find(x => x.Heroitemtype == equiptype);
    }

    public HeroItemData FindHeroItemData(int heroitemidx)
    {
        return GameRoot.Instance.UserData.Herogroudata.Heroitemdatas.Find(x => x.Heroitemidx == heroitemidx);
    }

    public HeroItemData FindHeroItemGradeValue(int itemidx)
    {
        return GameRoot.Instance.UserData.Herogroudata.Heroitemdatas
            .Where(x => x.Heroitemidx == itemidx)
            .OrderByDescending(x => x.Grade)
            .FirstOrDefault();
    }


    public double GetHeroStatusValue(HeroStatus heroStatus)
    {
        var td = Tables.Instance.GetTable<HeroInfo>().GetData(GameRoot.Instance.UserData.Herogroudata.Equipplayeridx);

        double herostatusvalue = 0;

        if (td != null)
        {
            foreach (var equipheroitem in GameRoot.Instance.UserData.Herogroudata.Equipheroitems)
            {
                if (equipheroitem.Heroitemdata != null && equipheroitem.Isequip.Value)
                {
                    var heroiteminfotd = Tables.Instance.GetTable<HeroItemInfo>().GetData(equipheroitem.Heroitemdata.Heroitemidx);

                    if (heroiteminfotd != null)
                    {
                        herostatusvalue += GetHeroItemStatusValue((HeroEquipType)heroiteminfotd.item_equip_type, heroStatus);
                    }
                }
            }

            switch (heroStatus)
            {
                case HeroStatus.Atttack:
                    {
                        herostatusvalue += td.base_atk_dmg;
                        var percentcalc = (int)ProjectUtility.PercentCalc(herostatusvalue, GetGradeBuffTypeValue(GradeBuffType.HeroAttackIncrease));
                        herostatusvalue += percentcalc;
                    }
                    break;
                case HeroStatus.Hp:
                    {
                        herostatusvalue += td.base_hp;
                        var percentcalc = (int)ProjectUtility.PercentCalc(herostatusvalue, GetGradeBuffTypeValue(GradeBuffType.HeroHpIncrease));
                        herostatusvalue += percentcalc;
                    }
                    break;
                case HeroStatus.AtkSpeed:
                    {
                        herostatusvalue = td.base_atk_speed - herostatusvalue;
                        var percentcalc = (int)ProjectUtility.PercentCalc(herostatusvalue, GetGradeBuffTypeValue(GradeBuffType.HeroAttackSpeedIncrease));
                        herostatusvalue -= percentcalc;
                    }
                    break;
            }
        }


        return herostatusvalue;
    }


    public int GetHeroItemStatusValue(HeroItemData itemdata, int statustypevalue)
    {
        var td = Tables.Instance.GetTable<HeroItemInfo>().GetData(itemdata.Heroitemidx);

        int value = 0;

        if (td != null)
        {
            int arrayIndex = statustypevalue - 1;

            // 배열 범위 체크
            if (td.item_ability_value == null || arrayIndex < 0 || arrayIndex >= td.item_ability_value.Count ||
                td.levelup_increase_value == null || arrayIndex >= td.levelup_increase_value.Count)
            {
                return 0;
            }

            var grade = itemdata.Grade;

            var level = FindHeroEquipItemData(td.item_equip_type).Level.Value;

            int levelincrasevalue = level * td.levelup_increase_value[arrayIndex];

            int gradebuffvalue = grade > 1 ? (td.item_ability_value[arrayIndex] * grade) / 2 : 0;

            value = td.item_ability_value[arrayIndex] + levelincrasevalue + gradebuffvalue;
        }

        return value;
    }

    public int GetHeroItemStatusValue(HeroEquipType equiptype, HeroStatus heroStatus)
    {
        int value = 0;

        var finddata = GameRoot.Instance.UserData.Herogroudata.Equipheroitems.Find(x => x.Heroitemtype == (int)equiptype);

        if (finddata != null && finddata.Heroitemdata != null)
        {
            var buffvalue = GameRoot.Instance.HeroSystem.GetHeroItemStatusValue(finddata.Heroitemdata, (int)heroStatus + 1);

            value = buffvalue;
        }

        return value;
    }

    public HeroItemData GetHeroRandItem(int grade)
    {
        var tdlist   = Tables.Instance.GetTable<HeroItemInfo>().DataList.ToList();
    
        var randvalue = Random.Range(0, tdlist.Count);

        var td = tdlist[randvalue];

        return new HeroItemData()
        {
            Heroitemidx = td.item_idx,
            Grade = grade,
        };
    }



    public int GetGradeBuffTypeValue(GradeBuffType gradebufftype)
    {
        var getbuffvalue = 0;

        foreach (var heroitemdata in GameRoot.Instance.UserData.Herogroudata.Equipheroitems)
        {
            if (heroitemdata.Heroitemdata == null) continue;

            var td = Tables.Instance.GetTable<HeroItemInfo>().GetData(heroitemdata.Heroitemdata.Heroitemidx);

            if (td != null && heroitemdata.Heroitemdata != null)
            {
                if (td.item_grade_ability_type[heroitemdata.Heroitemdata.Grade - 1] == (int)gradebufftype)
                {
                    getbuffvalue += td.item_grade_ability_value[heroitemdata.Heroitemdata.Grade - 1];
                }
            }
        }

        return getbuffvalue;
    }


    public int GetCurSetEquipCheck()
    {
        int equipsettype = -1;

        List<int> setTypes = new List<int>();

        foreach (var equipheroitem in GameRoot.Instance.UserData.Herogroudata.Equipheroitems)
        {
            if (equipheroitem.Heroitemdata != null)
            {
                var td = Tables.Instance.GetTable<HeroItemInfo>().GetData(equipheroitem.Heroitemdata.Heroitemidx);

                if (td != null)
                {
                    if (td.item_set_type > 0)
                    {
                        setTypes.Add(td.item_set_type);
                    }
                }
            }
        }

        // 4개의 아이템이 모두 같은 set_type을 가지고 있는지 확인
        if (setTypes.Count == 4 && setTypes.Distinct().Count() == 1)
        {
            equipsettype = setTypes[0];
        }

        return equipsettype;
    }

    /// <summary>
    /// 세트가 적용된 경우 해당 세트 타입의 버프 수치를 반환합니다.
    /// </summary>
    public int GetSetBuffValue(HeroItemSetType setType)
    {
        if (GetCurSetEquipCheck() != (int)setType)
            return 0;

        
        var td = Tables.Instance.GetTable<HeroItemSet>().GetData((int)setType);
        if(td != null)
        {
            return td.set_ability_value;
        }


        return 0;
    }
}


