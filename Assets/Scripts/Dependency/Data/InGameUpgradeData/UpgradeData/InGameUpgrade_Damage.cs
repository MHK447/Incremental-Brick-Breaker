


public class InGameUpgrade_Damage : InGameUpgrade
{

    protected override void PreInitalize()
    {
        base.PreInitalize();
        //upgradeChoiceIndex = (int)Config.InGameUpgradeChoice.DamageIncrease;
    }

    public override bool CanSpawn()
    {
        return true;
    }

    public override void Apply()
    {

    }

}