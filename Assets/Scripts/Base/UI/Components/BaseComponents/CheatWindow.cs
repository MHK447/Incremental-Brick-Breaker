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

    public void StartChoiceLevelUpReward()
    {
        GameRoot.Instance.UISystem.OpenUI<PopupLevelUpReward>(popup => popup.Init(true));
    }

    public void AddTileWeapon()
    {
        if (string.IsNullOrEmpty(inputField.text))
        {
            BpLog.LogError("input field empty!");
            return;
        }

        string[] parts = inputField.text.Split(';');
        if (parts.Length != 2)
        {
            BpLog.LogError("input format should be 'tileId;quantity' (e.g., 1;1)");
            return;
        }

        int tileId;
        if (!int.TryParse(parts[0], out tileId))
        {
            BpLog.LogError("tile id is not a valid number!");
            return;
        }

        int quantity;
        if (!int.TryParse(parts[1], out quantity))
        {
            BpLog.LogError("quantity is not a valid number!");
            return;
        }
        var td = Tables.Instance.GetTable<EquipInfo>().GetData(tileId);
        if (td != null)
        {
            if (td.item_type == 3)
            {
                GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.TileWeaponGroup.AddPlusTile(tileId);
            }
        }
        else
        {
            GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.TileWeaponGroup.AddTileWeapon(tileId, quantity);
        }
    }

    public void AddEquipment()
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
        GameRoot.Instance.UserData.Herogroudata.AddHeroItem((int)convert, 1);

    }


    public void ALlAddEquipment()
    {
        var tdlist = Tables.Instance.GetTable<HeroItemInfo>().DataList.ToList();


        foreach(var td in tdlist)
        {
            GameRoot.Instance.UserData.Herogroudata.AddHeroItem(td.item_idx, 1);
        }

    }

    public void AddTenItem()
    {
        var tdlist = Tables.Instance.GetTable<ItemInfo>().DataList.ToList();

        for(int i = 0; i < tdlist.Count; i++)
        {
            GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Item, tdlist[i].idx, 10);
        }

    }

    public void CheatMedeationCheck()
    {
        if (!MaxSdk.IsInitialized())
        {
            BpLog.LogWarning("MAX SDK is not initialized. Initialize before opening debugger.");
            return;
        }

        // Open the AppLovin MAX mediation debugger to verify adapter connectivity.
        MaxSdk.ShowMediationDebugger();
        BpLog.Log("Opened MAX mediation debugger.");
    }



    public void AllAddTileWeapon()
    {
        var tdlist = Tables.Instance.GetTable<EquipInfo>().DataList.ToList();

        foreach (var td in tdlist)
        {
            if (td.item_type == 3)
            {
                GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.TileWeaponGroup.AddPlusTile(td.idx);
            }
            else
            {
                GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.TileWeaponGroup.AddTileWeapon(td.idx, 1);
            }
        }
    }

    public void OnClick_Hide()
    {
        GameRoot.Instance.SetCheatWindow(false);
    }

    public void SetStageMove()
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

        GameRoot.Instance.UserData.Stageidx.Value = (int)convert;
    }


    public void AddHeroUnit()
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

    public void StageClearOpenCheat()
    {
        GameRoot.Instance.UISystem.OpenUI<PopupStageResult>(popup => popup.Set(true));
    }

    public void ThreeInGameUpgradeOpen()
    {
        GameRoot.Instance.UISystem.OpenUI<PopupLevelUpReward>(popup => popup.Init());
    }


    public void SetStageClear()
    {
        //GameRoot.Instance.UISystem.OpenUI<PopupStageResult>(popup => popup.Init(true));
    }

    public void ShowSelectChoice()
    {
        GameRoot.Instance.UISystem.OpenUI<PopupLevelUpReward>(popup => popup.Init(true));
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


        GameRoot.Instance.UserData.Stagerewardboxgroup.AddBox((int)convert);
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
