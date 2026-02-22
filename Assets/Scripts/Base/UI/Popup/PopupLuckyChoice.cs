using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using BanpoFri;

[UIPath("UI/Popup/LuckyChoice")]
public class PopupLuckyChoice : UIBase
{
    [SerializeField]
    private AdsButton AdBtn;
    [SerializeField]
    private LevelupRewardComponent[] LevelUpComponents;

    List<InGameUpgrade> SelectedUpgrades;

    private bool UpgradeLock = false;

    protected override void Awake()
    {
        base.Awake();
        AdBtn.AddListener(TpMaxProp.AdRewardType.LuckyChoice, Ad);
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

}