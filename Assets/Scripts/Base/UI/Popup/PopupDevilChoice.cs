using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

[UIPath("UI/Popup/PopupDevilChoice")]
public class PopupDevilChoice : UIBase
{
    [SerializeField]
    private AdsButton AdBtn;

    [SerializeField]
    private Button DevilBtn;

    [SerializeField]
    private LevelupRewardComponent[] LevelUpComponents;

    [SerializeField]
    private TextMeshProUGUI DevilHpText;

    List<InGameUpgrade> SelectedUpgrades;

    private bool UpgradeLock = false;

    private int CurHpDecrease = 0;

    protected override void Awake()
    {
        base.Awake();
        AdBtn.AddListener(TpMaxProp.AdRewardType.LuckyChoice, Ad);
        DevilBtn.onClick.AddListener(OnClickDevil);
    }

    public void Set()
    {
        SoundPlayer.Instance.PlaySound("popuplevelupshow");
        UpgradeLock = false;

        GameRoot.Instance.GameSpeedSystem.StopGameSpeed(true, false);
        SelectedUpgrades = GameRoot.Instance.InGameUpgradeSystem.GetUpgrades();
        for (int i = 0; i < LevelUpComponents.Length; i++)
        {
            LevelUpComponents[i].Set(SelectedUpgrades[i], OnSelect);
        }

        CurHpDecrease = (int)ProjectUtility.PercentCalc((double)GameRoot.Instance.UserData.Playerdata.CurHpProperty.Value, 70);

        DevilHpText.text = Tables.Instance.GetTable<Localize>().GetFormat("str_devil_hp_decrease", CurHpDecrease.ToString("F0"));
    }

    private void Ad()
    {
        if (SelectedUpgrades == null) return;
        foreach (var upgrade in SelectedUpgrades)
        {
            upgrade?.CallApply();
        }
        Hide();
    }

    public override void Hide()
    {
        base.Hide();
        // OnSelect에서 이미 게임 속도를 재개했으므로 여기서는 중복 호출 방지
        // 단, 다른 경로로 Hide가 호출될 수 있으므로 안전장치로 유지
        if (Time.timeScale <= 0f)
        {
            GameRoot.Instance.GameSpeedSystem.StopGameSpeed(false, false);
        }
    }
    private void OnSelect(InGameUpgrade selected)
    {
        if (UpgradeLock) return;
        UpgradeLock = true;

        selected?.CallApply();

        // 게임 속도를 먼저 재개하고 팝업을 닫음
        GameRoot.Instance.GameSpeedSystem.StopGameSpeed(false, false);
        Hide();
    }



    private void OnClickDevil()
    {
        if (SelectedUpgrades == null) return;

        GameRoot.Instance.UserData.Playerdata.CurHpProperty.Value -= CurHpDecrease;

        // 체력 소모가 UI에도 즉시 반영되도록 현재 스테이지의 HP 표시를 갱신
        var stage = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>()?.Stage;
        stage?.PlayerBlockGroup?.SetHp(
            GameRoot.Instance.UserData.Playerdata.CurHpProperty.Value,
            GameRoot.Instance.UserData.Playerdata.StartHpProperty.Value);

        foreach (var upgrade in SelectedUpgrades)
        {
            upgrade?.CallApply();
        }
        Hide();
    }
}

