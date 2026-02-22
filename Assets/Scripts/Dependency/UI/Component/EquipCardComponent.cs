using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class EquipCardComponent : MonoBehaviour
{
    [SerializeField]
    private Transform EquipRoot;

    [SerializeField]
    private Transform NoneEquipRoot;

    [SerializeField]
    private LobbyCardComponent LobbyCardComponent;

    [SerializeField]
    private GameObject ClickObj;

    private int CardIdx = 0;

    public int GetCardIdx { get { return CardIdx; } }

    public void Set(int cardidx)
    {
        CardIdx = cardidx;

        ProjectUtility.SetActiveCheck(NoneEquipRoot.gameObject, cardidx == -1);
        ProjectUtility.SetActiveCheck(EquipRoot.gameObject, cardidx != -1);

        var td = Tables.Instance.GetTable<CardInfo>().GetData(cardidx);

        if (td != null)
        {
            LobbyCardComponent.Set(cardidx);
        }
    }


    public void ActiveClickObj(bool isactive)
    {
        ProjectUtility.SetActiveCheck(ClickObj, isactive);
    }
}

