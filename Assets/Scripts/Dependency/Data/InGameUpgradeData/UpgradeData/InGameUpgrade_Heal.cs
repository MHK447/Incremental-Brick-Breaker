using UnityEngine;
using BanpoFri;

public class InGameUpgrade_Heal : InGameUpgrade
{

    protected override void PreInitalize()
    {
        base.PreInitalize();
        //upgradeChoiceIndex = (int)Config.InGameUpgradeChoice.HealthIncrease;
    }

    public override bool CanSpawn()
    {
        return true;
    }

    public override string GetDesc()
    {
        return "";
    }



    public override void Apply()
    {
        //GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.PlayerUnitGroup.GlobalUnitBonusHpMultiplier += TierValue * 0.01f;
    }

}
