using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class InGameUpgrade_LightningStatue : InGameUpgrade
{
    protected override void PreInitalize()
    {
        base.PreInitalize();
        upgradeChoiceIndex = (int)Config.InGameUpgradeChoice.LightningStatue;
    }

    public override bool CanSpawn()
    {
        var upgradelevel = GameRoot.Instance.InGameUpgradeSystem.GetUpgradeLevel(upgradeChoiceIndex);
        return GameRoot.Instance.TileSystem.EquipItemList.Find(x => x.item_type == 2) != null && UpgradeChoiceData.upgrade_count > upgradelevel;
    }


    public override void Apply()
    {
        var playergroup = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.PlayerUnitGroup;

        var getlevel = GameRoot.Instance.InGameUpgradeSystem.GetUpgradeLevel(upgradeChoiceIndex);

        PlayerUpgradeStateModifier modifier = new()
        {
            StatType = Config.InGameUpgradeChoice.LightningStatue,
            Value_1 = UpgradeChoiceData.upgrade_value_1[getlevel],
            Value_2 = UpgradeChoiceData.upgrade_value_2[getlevel],
        };

        GameRoot.Instance.InGameUpgradeSystem.IncreaseUpgradeLevel(upgradeChoiceIndex);

        GameRoot.Instance.InGameUpgradeSystem.AddStatModifier(modifier);
    }

    public override string GetDesc()
    {
        var ingamelevel = GameRoot.Instance.InGameUpgradeSystem.GetUpgradeLevel(upgradeChoiceIndex);

        return Tables.Instance.GetTable<Localize>().GetFormat(UpgradeChoiceData.desc_1, UpgradeChoiceData.upgrade_value_2[ingamelevel],UpgradeChoiceData.upgrade_value_1[ingamelevel]);
    }
}

