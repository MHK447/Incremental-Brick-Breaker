using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class InGameUpgrade_HpFull : InGameUpgrade
{
    protected override void PreInitalize()
    {
        base.PreInitalize();
        upgradeChoiceIndex = (int)Config.InGameUpgradeChoice.HpFull;
    }

    public override void Apply()
    {
        var playerData = GameRoot.Instance.UserData.Playerdata;
        int maxHp = playerData.StartHpProperty.Value;
        playerData.CurHpProperty.Value = maxHp;

        // UI 갱신 (CastleHpProgress)
        var stage = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>()?.Stage;
        if (stage?.PlayerBlockGroup != null)
            stage.PlayerBlockGroup.SetHp(maxHp, maxHp);
    }

    public override string GetDesc()
    {
        return Tables.Instance.GetTable<Localize>().GetString(UpgradeChoiceData.desc_1);
    }

    public override bool CanSpawn()
    {
        var curhp = GameRoot.Instance.UserData.Playerdata.CurHpProperty.Value;
        var maxhp = GameRoot.Instance.UserData.Playerdata.StartHpProperty.Value;

        // HP가 30% 이하일 때만 스폰
        if (curhp > maxhp * 0.3f)
        {
            return false;
        }

        return true;
    }



}
