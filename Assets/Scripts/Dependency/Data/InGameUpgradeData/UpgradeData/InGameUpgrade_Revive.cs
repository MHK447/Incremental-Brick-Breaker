using UnityEngine;
using BanpoFri;
using System.Linq;

public class InGameUpgrade_Revive : InGameUpgrade
{

    protected override void PreInitalize()
    {
        base.PreInitalize();
        //upgradeChoiceIndex = (int)Config.InGameUpgradeChoice.Revive;
    }

    public override bool CanSpawn()
    {
        // PopupLuckyChoice에서만 스폰 가능하며, 죽은 유닛이 있을 때만 스폰 가능
        if (!GameRoot.Instance.InGameUpgradeSystem.IsLuckyChoiceContext)
            return false;
            
        return true;
    }

    public override string GetDesc()
    {
        return "";
    }

    public override void Apply()
    {

    }

}

