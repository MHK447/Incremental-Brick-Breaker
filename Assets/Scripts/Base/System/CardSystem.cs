using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UniRx;


public class CardSystem
{
    public int CardEquipMax = 8;

    public CardData ChangeEquipCardData = null;

    public void Create()
    {

        //TEST

        ChangeEquipCardData = null;


        if (GameRoot.Instance.UserData.Carddatas.Count == 0)
        {
            AddCard(1);
            AddCard(109);
        }

        var carddatacheck = GameRoot.Instance.UserData.Carddatas.FindAll(x => x.Isequip.Value);

        if(carddatacheck.Count == 0)
        {
            var equipCount = Mathf.Min(CardEquipMax, GameRoot.Instance.UserData.Carddatas.Count);
            for(int i = 0; i < equipCount; ++i)
            {
                GameRoot.Instance.UserData.Carddatas[i].Isequip.Value = true;
            }

            GameRoot.Instance.UserData.Save();
        }
    }



    public double GetStatusValue(int cardidx, UnitStatusType statusType, int grade = 1)
    {
        double value = 0;
        var cardplusvalue = GetCardValue(cardidx, statusType);

        var cardtd = Tables.Instance.GetTable<CardInfo>().GetData(cardidx);

        if (cardtd == null) return value;

        var unitinfotd = Tables.Instance.GetTable<UnitInfo>().GetData(cardidx);

        var weaponinfotd = Tables.Instance.GetTable<EquipInfo>().GetData(cardidx);

        switch (statusType)
        {
            
            case UnitStatusType.AttackRange:
                return unitinfotd == null ? weaponinfotd.attack_range * 0.01f + cardplusvalue : unitinfotd.attack_range * 0.01f + cardplusvalue;
            case UnitStatusType.Shield:
                return (weaponinfotd.base_value_1 * grade) + cardplusvalue;
            case UnitStatusType.SILVERCOIN:
                return (weaponinfotd.base_value_1 * grade) + cardplusvalue;
            case UnitStatusType.HpHeal:
                return (weaponinfotd.base_value_1 * grade) + cardplusvalue;
            default:
                return 0;
        }
    }



    public double GetCardValue(int cardidx, UnitStatusType statusType)
    {
        double value = 0;

        var finddata = GameRoot.Instance.UserData.Carddatas.Find(x => x.Cardidx == cardidx);

        if (finddata != null)
        {
            var td = Tables.Instance.GetTable<CardInfo>().GetData(cardidx);

            if (td != null)
            {
                var findidx = td.card_upgrade_type.FindIndex(x => x == (int)statusType);

                if (findidx == -1) return value;

                value = td.card_upgrade_increase[findidx] * (finddata.Cardlevel.Value - 1);
            }
        }

        return value;
    }

    public bool CardEquipMaxCheck()
    {
        return GameRoot.Instance.UserData.Carddatas.FindAll(x => x.Isequip.Value).Count >= CardEquipMax;
    }

    public void AddCard(int cardidx, int count = 1)
    {
        var finddata = GameRoot.Instance.UserData.Carddatas.Find(x => x.Cardidx == cardidx);

        if (finddata == null)
        {
            bool isequipcheck = CardEquipMaxCheck() ? false : true;

            GameRoot.Instance.UserData.Carddatas.Add(new CardData()
            {
                Cardidx = cardidx,
                Cardlevel = new ReactiveProperty<int>(1),
                Cardcount = new ReactiveProperty<int>(count - 1),
                Isequip = new ReactiveProperty<bool>(isequipcheck),
            });
        }
        else
        {
            finddata.Cardcount.Value += count;
        }
    }

    public int GetUpgradableCardCount()
    {
        int count = 0;
        var skillcardtdlist = Tables.Instance.GetTable<CardInfo>().DataList.ToList();

        foreach (var skillcard in skillcardtdlist)
        {
            var finddata = FindCardData(skillcard.card_idx);
            if (finddata == null || finddata.Cardlevel.Value < 100)
            {
                count++;
                continue;
            }
        }
        return count;
    }
    public CardData FindCardData(int cardidx)
    {
        var finddata = GameRoot.Instance.UserData.Carddatas.ToList().Find(x => x.Cardidx == cardidx);

        return finddata;
    }

}

