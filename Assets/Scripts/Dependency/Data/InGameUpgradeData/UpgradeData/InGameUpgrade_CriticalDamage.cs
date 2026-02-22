


using System.Diagnostics;
using UnityEngine;
using BanpoFri;
using System.Linq;
using System.Collections.Generic;

public class InGameUpgrade_CritDamage : InGameUpgrade
{

    protected override void PreInitalize()
    {
        base.PreInitalize();
        //upgradeChoiceIndex = (int)Config.InGameUpgradeChoice.CritDamage;
    }

    public override bool CanSpawn()
    {
        return true;
    }

    public override void Apply()
    {

    }
}