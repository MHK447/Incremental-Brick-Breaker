using UnityEngine;
using BanpoFri;
using UniRx;
using UnityEngine.UI;
using TMPro;

[UIPath("UI/Popup/PopupStageGiveup")]
public class PopupStageGiveup : UIBase
{
    [SerializeField]
    private Button GiveupBtn;

    [SerializeField]
    private Button QuiteBtn;


    protected override void Awake()
    {
        base.Awake();
        GiveupBtn.onClick.AddListener(OnClickGiveup);
        QuiteBtn.onClick.AddListener(Hide);
    }


    protected override void OnEnable()
    {
        base.OnEnable();
        GameRoot.Instance.GameSpeedSystem.StopGameSpeed(true);
    }


    public void OnClickGiveup()
    {
        Hide();
        GameRoot.Instance.UserData.SaveInGameResumeSnapshot(true, true);
        
        // 모든 유닛 비활성화
        DeactivateAllUnits();
        
        GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.ReturnMainScreen();
    }

    private void DeactivateAllUnits()
    {
        var stage = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage;
        
        if (stage == null) return;

        // PlayerUnitGroup의 모든 유닛 비활성화
        if (stage.PlayerUnitGroup != null)
        {
            foreach (var unit in stage.PlayerUnitGroup.ActiveUnits)
            {
                if (unit != null)
                {
                    unit.DeactivateUnit();
                }
            }
        }

        // EnemyUnitGroup의 모든 유닛 비활성화
        if (stage.EnemyUnitGroup != null)
        {
            foreach (var unit in stage.EnemyUnitGroup.ActiveUnits)
            {
                if (unit != null)
                {
                    unit.DeactivateUnit();
                }
            }
        }

        // PlayerBlockGroup의 모든 블록 비활성화
        if (stage.PlayerBlockGroup != null)
        {
            foreach (var block in stage.PlayerBlockGroup.ActiveBlocks)
            {
                if (block != null)
                {
                    // PlayerBlock도 비활성화
                    ProjectUtility.SetActiveCheck(block.gameObject, false);
                }
            }

            // CastleTop도 비활성화
            if (stage.PlayerBlockGroup.CastleTop != null)
            {
                ProjectUtility.SetActiveCheck(stage.PlayerBlockGroup.CastleTop.gameObject, false);
            }
        }
    }

    public override void Hide()
    {
        base.Hide();
        GameRoot.Instance.GameSpeedSystem.StopGameSpeed(false);
    }
}
