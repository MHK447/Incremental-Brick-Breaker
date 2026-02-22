using System.Collections;
using System.Numerics;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using BanpoFri;
using System;
using System.Linq;

public class CheatWindow : MonoBehaviour
{
    [SerializeField]
    private InputField inputField;

    public void SetMoney()
    {
        if (string.IsNullOrEmpty(inputField.text))
        {
            BpLog.LogError("input field empty!");
            return;
        }
        BigInteger convert;
        if (!BigInteger.TryParse(inputField.text, out convert))
        {
            BpLog.LogError("input field string don't convert number!");
            return;
        }
        inputField.text = "";
        GameRoot.Instance.UserData.Money.Value += convert;
        GameRoot.Instance.UserData.HUDMoney.Value += convert;
    }


    public void SetEnergeyMoney()
    {
        if (string.IsNullOrEmpty(inputField.text))
        {
            BpLog.LogError("input field empty!");
            return;
        }
        BigInteger convert;
        if (!BigInteger.TryParse(inputField.text, out convert))
        {
            BpLog.LogError("input field string don't convert number!");
            return;
        }
        inputField.text = "";
        GameRoot.Instance.UserData.CurMode.EnergyMoney.Value += convert;
    }

    public void SetCash()
    {
        if (string.IsNullOrEmpty(inputField.text))
        {
            BpLog.LogError("input field empty!");
            return;
        }

        BigInteger convert;
        if (!BigInteger.TryParse(inputField.text, out convert))
        {
            BpLog.LogError("input field string don't convert number!");
            return;
        }

        inputField.text = "";

        if (convert > int.MaxValue || (convert + GameRoot.Instance.UserData.Cash.Value) > int.MaxValue)
        {
            GameRoot.Instance.UserData.Cash.Value = int.MaxValue;
            GameRoot.Instance.UserData.HUDCash.Value = int.MaxValue;
        }
        else
        {
            GameRoot.Instance.UserData.Cash.Value += (int)convert;
            GameRoot.Instance.UserData.HUDCash.Value += (int)convert;
        }
    }

    public void AddAllCard()
    {
        var tdlist = Tables.Instance.GetTable<CardInfo>().DataList.ToList();


        foreach(var td in tdlist)
        {
            GameRoot.Instance.CardSystem.AddCard(td.card_idx);
        }

        GameRoot.Instance.UserData.Save();
    }


    public void SetInGameSilverCoin()
    {
        if (string.IsNullOrEmpty(inputField.text))
        {
            BpLog.LogError("input field empty!");
            return;
        }

        BigInteger convert;
        if (!BigInteger.TryParse(inputField.text, out convert))
        {
            BpLog.LogError("input field string don't convert number!");
            return;
        }

        inputField.text = "";

        if (convert > int.MaxValue || (convert + GameRoot.Instance.UserData.Material.Value) > int.MaxValue)
        {
            GameRoot.Instance.UserData.Ingamesilvercoin.Value = int.MaxValue;
        }
        else
        {
            GameRoot.Instance.UserData.Ingamesilvercoin.Value += (int)convert;
        }
    }


    public void SetMaterial()
    {
        if (string.IsNullOrEmpty(inputField.text))
        {
            BpLog.LogError("input field empty!");
            return;
        }

        BigInteger convert;
        if (!BigInteger.TryParse(inputField.text, out convert))
        {
            BpLog.LogError("input field string don't convert number!");
            return;
        }

        inputField.text = "";

        if (convert > int.MaxValue || (convert + GameRoot.Instance.UserData.Material.Value) > int.MaxValue)
        {
            GameRoot.Instance.UserData.Material.Value = int.MaxValue;
            GameRoot.Instance.UserData.HUDMaterial.Value = int.MaxValue;
        }
        else
        {
            GameRoot.Instance.UserData.Material.Value += (int)convert;
            GameRoot.Instance.UserData.HUDMaterial.Value += (int)convert;
        }
    }

    public void SetStartTutorial()
    {
        if (string.IsNullOrEmpty(inputField.text))
        {
            BpLog.LogError("input field empty!");
            return;
        }

        BigInteger convert;
        if (!BigInteger.TryParse(inputField.text, out convert))
        {
            BpLog.LogError("input field string don't convert number!");
            return;
        }

        var id = convert.ToString();
        if (GameRoot.Instance.TutorialSystem.IsClearTuto(id.ToString()))
        {
            GameRoot.Instance.UserData.Tutorial.Remove(id.ToString());
        }


        inputField.text = "";

        GameRoot.Instance.TutorialSystem.StartTutorial(convert.ToString());
    }


    public void SetTicket()
    {
        if (string.IsNullOrEmpty(inputField.text))
        {
            BpLog.LogError("input field empty!");
            return;
        }
        BigInteger convert;
        if (!BigInteger.TryParse(inputField.text, out convert))
        {
            BpLog.LogError("input field string don't convert number!");
            return;
        }
        inputField.text = "";
        GameRoot.Instance.UserData.CurMode.GachaCoin.Value += (int)convert;
    }

    public void AddAllUnit()
    {
        var tdlist = Tables.Instance.GetTable<UnitInfo>().DataList.ToList();

        foreach (var td in tdlist)
        {
            GameRoot.Instance.UserData.Unitgroupdata.AddUnit(td.idx);
        }
    }

    public void SetStarAdd()
    {
        if (string.IsNullOrEmpty(inputField.text))
        {
            BpLog.LogError("input field empty!");
            return;
        }
        BigInteger convert;
        if (!BigInteger.TryParse(inputField.text, out convert))
        {
            BpLog.LogError("input field string don't convert number!");
            return;
        }
        inputField.text = "";
        GameRoot.Instance.UserData.Starcoinvalue.Value += (int)convert;
    }

    public void AddRewardBox()
    {
        if (string.IsNullOrEmpty(inputField.text))
        {
            BpLog.LogError("input field empty!");
            return;
        }
        BigInteger convert;
        if (!BigInteger.TryParse(inputField.text, out convert))
        {
            BpLog.LogError("input field string don't convert number!");
            return;
        }
        inputField.text = "";


    }



#if UNITY_EDITOR
    [UnityEditor.MenuItem("BanpoFri/ShowCheat _F3")]
    static void ShowCheat()
    {
        if (UnityEditor.EditorApplication.isPlaying)
            GameRoot.Instance.SetCheatWindow(true);
    }
#endif
}
