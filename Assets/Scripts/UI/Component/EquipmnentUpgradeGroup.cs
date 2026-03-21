using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using Unity.VisualScripting;
using TMPro;
using UniRx;

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
    private SelectItemComponent EquipInfoItemComponent;

    [SerializeField]
    private ColliderAction ColliderAction;

    private CanvasGroup EquipInfoCanvasGroup;

    private CompositeDisposable disposables = new CompositeDisposable();


    void Awake()
    {
        UpgradeBtn.onClick.AddListener(OnUpgradeBtnClick);

        EquipInfoCanvasGroup = EquipInfoItemComponent.GetComponent<CanvasGroup>();
        if (EquipInfoCanvasGroup == null)
        {
            EquipInfoCanvasGroup = EquipInfoItemComponent.gameObject.AddComponent<CanvasGroup>();
        }

        // Hover tooltip should not intercept pointer raycasts (prevents enter/exit flicker).
        EquipInfoCanvasGroup.blocksRaycasts = false;
        EquipInfoCanvasGroup.interactable = false;

    }

    private bool IsStartUpgrade = false;

    void OnUpgradeBtnClick()
    {
        if (IsStartUpgrade) return;

        if (GameRoot.Instance.UserData.Material.Value >= 20)
        {
            IsStartUpgrade = true;
            GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency,
            (int)Config.CurrencyID.Material, -20);


            UpgradeAnim.Play("Upgrade", 0, 0f);
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

        ProjectUtility.SetActiveCheck(EquipInfoItemComponent.gameObject, false);


        ColliderAction.AttackAction = GachaStart;


        disposables.Clear();

        UpgradeCostText.text = "20";

        GameRoot.Instance.UserData.Material.Subscribe(x =>
        {
            UpgradeCostText.color = x >= 20 ? Color.white : Color.red;
        }).AddTo(disposables);
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



    public void EquipItemSet(EquipItemData equipitemdata, bool active)
    {
        ProjectUtility.SetActiveCheck(EquipInfoItemComponent.gameObject, active);

        if (!active || equipitemdata == null) return;

        EquipInfoItemComponent.EquipItemSet(equipitemdata);
    }

    public void EquipItemSet(EquipItemData equipitemdata, bool active, Vector3 worldPos)
    {
        ProjectUtility.SetActiveCheck(EquipInfoItemComponent.gameObject, active);

        if (!active || equipitemdata == null) return;

        EquipInfoItemComponent.transform.position = worldPos;
        EquipInfoItemComponent.EquipItemSet(equipitemdata);
    }


    void OnDestroy()
    {
        disposables.Clear();
    }

    void OnDisable()
    {
        disposables.Clear();
    }
}

