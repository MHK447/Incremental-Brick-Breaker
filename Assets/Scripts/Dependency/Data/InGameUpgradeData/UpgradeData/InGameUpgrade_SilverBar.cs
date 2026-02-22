using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class InGameUpgrade_SilverBar : InGameUpgrade
{

    protected override void PreInitalize()
    {
        base.PreInitalize();
        upgradeChoiceIndex = (int)Config.InGameUpgradeChoice.SilverBar;
    }
    public override void Apply()
    {
        base.Apply();

        var td = Tables.Instance.GetTable<InGameUpgradeChoice>().GetData((int)upgradeChoiceIndex);


        if(td == null) return;

        GameRoot.Instance.UserData.Ingamesilvercoin.Value += td.upgrade_value_1[0];
    }


    public override string GetDesc()
    {
        var ingamelevel = GameRoot.Instance.InGameUpgradeSystem.GetUpgradeLevel(upgradeChoiceIndex);

        return Tables.Instance.GetTable<Localize>().GetFormat(UpgradeChoiceData.desc_1, UpgradeChoiceData.upgrade_value_1[ingamelevel]);
    }
}

