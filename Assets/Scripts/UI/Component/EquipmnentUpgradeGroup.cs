using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using Unity.VisualScripting;
using TMPro;

public class EquipmnentUpgradeGroup : MonoBehaviour
{
    [SerializeField]
    private List<EquipmentComponent> EquipmentComponentList = new List<EquipmentComponent>();

    [SerializeField]
    private Animator UpgradeAnim;

    [SerializeField]
    private Button UpgradeBtn;

    [SerializeField]
    private TextMeshProUGUI UpgradeCostText;

    [SerializeField]
    private ItemGachaSelectComponent ItemGachaSelectComponent;

    [SerializeField]
    private ColliderAction ColliderAction;
    


    void Awake()
    {
        UpgradeBtn.onClick.AddListener(OnUpgradeBtnClick);
    }

    private bool IsStartUpgrade = false;

    void OnUpgradeBtnClick()
    {
        if (IsStartUpgrade) return;

        if (GameRoot.Instance.UserData.Material.Value >= 20)
        {
            IsStartUpgrade = true;
            GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency , 
            (int)Config.CurrencyID.Material , -20);


            UpgradeAnim.Play("Upgrade", 0 , 0f);
            GameRoot.Instance.UserData.Save();
        }
    }


    public void GachaStart()
    {
        var getgachaitem = GameRoot.Instance.EquipmentSystem.GetGacahaItemData();

        var finddata = GameRoot.Instance.UserData.Playerequipdata.FindEquipItemData(getgachaitem.Equipitemtype);

        if (finddata == null) return;

        IsStartUpgrade = false;

        ItemGachaSelectComponent.Init(finddata, getgachaitem);

        ProjectUtility.SetActiveCheck(ItemGachaSelectComponent.gameObject, true);

    }


    public void Init()
    {
        foreach (var component in EquipmentComponentList)
        {
            component.Init();
        }

        ProjectUtility.SetActiveCheck(ItemGachaSelectComponent.gameObject, false);


        ColliderAction.AttackAction = GachaStart;
    }

    /// <summary>
    /// 장착/교체 직후 EquipmentComponent 화면만 즉시 갱신합니다.
    /// </summary>
    public void RefreshEquipmentComponents()
    {
        foreach (var component in EquipmentComponentList)
        {
            if (component == null) continue;
            component.Init();
        }
    }
}

