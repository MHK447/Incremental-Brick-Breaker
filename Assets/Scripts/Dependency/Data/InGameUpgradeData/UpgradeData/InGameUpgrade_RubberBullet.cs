using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class InGameUpgrade_RubberBullet : InGameUpgrade
{
    protected override void PreInitalize()
    {
        base.PreInitalize();
        upgradeChoiceIndex = (int)Config.InGameUpgradeChoice.RubberBullet;
    }

    public override bool CanSpawn()
    {
        return false;
    }

    public override string GetDesc()
    {
        var ingamelevel = GameRoot.Instance.InGameUpgradeSystem.GetUpgradeLevel(upgradeChoiceIndex);

        return Tables.Instance.GetTable<Localize>().GetFormat(UpgradeChoiceData.desc_1, UpgradeChoiceData.upgrade_value_1[ingamelevel]);
    }

    public override void Apply()
    {
        var td = Tables.Instance.GetTable<InGameUpgradeChoice>().GetData((int)upgradeChoiceIndex);

        if (td == null) return;

        GameRoot.Instance.InGameUpgradeSystem.IncreaseUpgradeLevel(upgradeChoiceIndex);

        var getlevel = GameRoot.Instance.InGameUpgradeSystem.GetUpgradeLevel(upgradeChoiceIndex);
        
        PlayerUpgradeStateModifier modifier = new()
        {
            StatType = Config.InGameUpgradeChoice.RubberBullet,
            Value_1 = td.upgrade_value_1[getlevel],
            Value_2 = td.upgrade_value_2[getlevel],
        };

        GameRoot.Instance.InGameUpgradeSystem.AddStatModifier(modifier);
    }
}

