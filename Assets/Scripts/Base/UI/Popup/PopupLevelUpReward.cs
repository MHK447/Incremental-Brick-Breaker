using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using DG.Tweening;


[UIPath("UI/Popup/PopupLevelUpReward")]
public class PopupLevelUpReward : UIBase
{
    [SerializeField]
    private List<LevelupRewardComponent> LevelUpChoiceComponents = new List<LevelupRewardComponent>();


    [SerializeField]
    private AdsButton RerollBtn;

    [SerializeField]
    private TextMeshProUGUI RerollText;


    private UpgradeTier CurrentUpgradeTier;

    private bool UpgradeLock = false;

    private bool IsReroll = false;


    public bool ShowLuckyChoice = false;



    protected override void Awake()
    {
        base.Awake();

        RerollBtn.AddListener(TpMaxProp.AdRewardType.LevelUpReroll, OnClickReroll);

    }


    public void Init(bool isfirst = false)
    {
        //SoundPlayer.Instance.PlaySound("popuplevelupshow");

        UpgradeLock = false;

        // 이전 애니메이션 정리
        foreach (var component in LevelUpChoiceComponents)
        {
            component.transform.DOKill();
            ProjectUtility.SetActiveCheck(component.gameObject, false);
        }
        RerollBtn.transform.DOKill();

        if (!IsReroll)
        {
            //LuckyChoiceStatusCheck();
            // 재활용이 아닐 때만 게임 속도를 멈춤 (재활용 시에는 이미 멈춰있음)
            GameRoot.Instance.GameSpeedSystem.StopGameSpeed(true, true);
        }

        int stageidx = GameRoot.Instance.UserData.Stageidx.Value;

        var tier = IsReroll ? UpgradeTier.Legendary : UpgradeTier.Rare;

        //get upgrades
        List<InGameUpgrade> upgrades = GameRoot.Instance.InGameUpgradeSystem.GetUpgrades(tier);
        if (upgrades == null || upgrades.Count == 0)
        {
            Hide();
            return;
        }


        for (int i = 0; i < upgrades.Count; ++i)
        {
            LevelUpChoiceComponents[i].Set(upgrades[i], OnSelect);

            ProjectUtility.SetActiveCheck(LevelUpChoiceComponents[i].gameObject, true);

        }

        // var rerollopen = GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.RerollOpen);

        ProjectUtility.SetActiveCheck(RerollBtn.gameObject, true && !isfirst);

        ProjectUtility.SetActiveCheck(RerollText.gameObject, RerollBtn.gameObject.activeSelf);

    
    }


    private void OnSelect(InGameUpgrade selected)
    {
        if (UpgradeLock) return;
        UpgradeLock = true;

        // 모든 애니메이션 정리
        foreach (var component in LevelUpChoiceComponents)
        {
            component.transform.DOKill();
        }
        RerollBtn.transform.DOKill();

        selected?.CallApply();
        
        // 게임 속도를 먼저 재개하고 팝업을 닫음
        GameRoot.Instance.GameSpeedSystem.StopGameSpeed(false, false);
        Hide();
    }

    private void OnClickReroll()
    {
        IsReroll = true;
        Init();
    }

    public void OnClickEyes()
    {

    }

    public override void Hide()
    {
        // 모든 애니메이션 정리
        foreach (var component in LevelUpChoiceComponents)
        {
            component.transform.DOKill();
        }
        RerollBtn.transform.DOKill();

        base.Hide();

        // 안전장치: OnSelect에서 이미 게임 속도를 재개했지만, 다른 경로로 Hide가 호출될 수 있으므로 확인
        if (Time.timeScale <= 0f)
        {
            GameRoot.Instance.GameSpeedSystem.StopGameSpeed(false, false);
        }

        // 파티클 효과를 비동기로 처리하여 팝업 닫힘을 지연시키지 않음
        if (GameRoot.Instance.InGameSystem?.GetInGame<InGameBase>()?.Stage?.PlayerBlockGroup != null)
        {
            GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.PlayerBlockGroup.ShowStatUpgradeParticle();
        }

        IsReroll = false;
        UpgradeLock = false;

        LuckyChoiceCheck();
    }

    public void LuckyChoiceCheck()
    {
        if(GameRoot.Instance.ContentsOpenSystem.ContentsOpenCheck(ContentsOpenSystem.ContentsOpenType.TileWeaponAd)
        && ProjectUtility.IsPercentSuccess(50))
        {
            GameRoot.Instance.UISystem.OpenUI<PopupLuckyChoice>(popup => popup.Set());
        }
    }


}
