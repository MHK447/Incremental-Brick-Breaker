using UnityEngine;
using BanpoFri;


public enum UpgradeTier
{
    Rare = 1,
    Epic = 2,
    Legendary = 3,
}


public class InGameUpgrade
{
    public InGameUpgradeChoiceData UpgradeChoiceData;
    public UpgradeTier Tier;
    public int TierValue;
    public bool IsRecommend = false;

    protected int upgradeChoiceIndex = 0;

    public InGameUpgrade()
    {
        PreInitalize();

        var upgrade = Tables.Instance.GetTable<InGameUpgradeChoice>().GetData(upgradeChoiceIndex);

        if (upgrade != null)
        {
            UpgradeChoiceData = upgrade;
        }
    }

    protected virtual void PreInitalize()
    {

    }

    public void SetRecommend(bool isRecommend)
    {
        IsRecommend = isRecommend;
    }

    public bool SetTierAndCheckSpawn(UpgradeTier tier)
    {
        IsRecommend = false;
        //SetTier(tier);
        return CanSpawn();
    }

    public virtual bool CanSpawn()
    {
        int upgradelevel = GameRoot.Instance.InGameUpgradeSystem.GetUpgradeLevel(upgradeChoiceIndex);

        return UpgradeChoiceData.upgrade_count > upgradelevel;
    }

    public void CallApply(bool isLuckyChoice = false)
    {
        //GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.Battle.PlayerBlockGroup.ShowStatUpgradeParticle();
        Apply();
        GameRoot.Instance.InGameUpgradeSystem.UpgradeCount++;
        GameRoot.Instance.InGameUpgradeSystem.ChoiceUpgradeTiers.Add(Tier);
        GameRoot.Instance.InGameUpgradeSystem.ChoiceUpgrades.Add(this);
    }

    public virtual string GetDesc()
    {

        return "";
    }

    public virtual void Apply()
    {
        //이 업그레이드를 선택했을때 효과 적용 (터렛스폰, 스탯 강화 등)
    }



}
