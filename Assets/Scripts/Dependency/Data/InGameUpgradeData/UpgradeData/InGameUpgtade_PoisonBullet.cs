using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class InGameUpgrade_PoisonBullet : InGameUpgrade
{
    protected override void PreInitalize()
    {
        base.PreInitalize();
        upgradeChoiceIndex = (int)Config.InGameUpgradeChoice.PoisonBullet;
    }

    public override bool CanSpawn()
    {
        var finddata = GameRoot.Instance.InGameUpgradeSystem.GetModifier(Config.InGameUpgradeChoice.FreezeBullet);

        return GameRoot.Instance.TileSystem.EquipItemList.Find(x => x.item_type == 2) != null && finddata == null;
    }

    public override string GetDesc()
    {
        var ingamelevel = GameRoot.Instance.InGameUpgradeSystem.GetUpgradeLevel(upgradeChoiceIndex);

        return Tables.Instance.GetTable<Localize>().GetFormat(UpgradeChoiceData.desc_1, UpgradeChoiceData.upgrade_value_2[ingamelevel], UpgradeChoiceData.upgrade_value_1[ingamelevel]);
    }

    public override void Apply()
    {
        var td = Tables.Instance.GetTable<InGameUpgradeChoice>().GetData((int)upgradeChoiceIndex);

        if (td == null) return;


        var getlevel = GameRoot.Instance.InGameUpgradeSystem.GetUpgradeLevel(upgradeChoiceIndex);

        PlayerUpgradeStateModifier modifier = new()
        {
            StatType = Config.InGameUpgradeChoice.PoisonBullet,
            Value_1 = td.upgrade_value_1[getlevel],
            Value_2 = td.upgrade_value_2[getlevel],
        };

        GameRoot.Instance.InGameUpgradeSystem.IncreaseUpgradeLevel(upgradeChoiceIndex);

        GameRoot.Instance.InGameUpgradeSystem.AddStatModifier(modifier);
    }
}

