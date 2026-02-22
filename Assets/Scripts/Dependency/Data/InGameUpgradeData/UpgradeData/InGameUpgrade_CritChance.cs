using UnityEngine;
using BanpoFri;


public class InGameUpgrade_CritChance : InGameUpgrade
{

    protected override void PreInitalize()
    {
        base.PreInitalize();

        //upgradeChoiceIndex = (int)Config.InGameUpgradeChoice.CritChance;
    }

    public override bool CanSpawn()
    {
        return true;
    }

    public override void Apply()
    {
    }

}
