using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;



[UIPath("UI/Page/PagePurchaseConfirm")]
public class PagePurchaseConfirm : CommonUIBase
{
    [SerializeField]
    private List<PurchaseConfirmItemComponent> EquipUpgradeComponents = new List<PurchaseConfirmItemComponent>();

    [SerializeField]
    private GameObject EquipItemPrefab;

    [SerializeField]
    private Transform EquipItemRoot;


    public void Set(List<RewardData> rewarddatas)
    {
        SoundPlayer.Instance.PlaySound("effect_levelup_reward");

        foreach (var equipupgradecomponent in EquipUpgradeComponents)
        {
            ProjectUtility.SetActiveCheck(equipupgradecomponent.gameObject, false);
        }

        for (int i = 0; i < rewarddatas.Count; i++)
        {
            var obj = GetCachedObject();

            if (obj != null)
            {
                if (rewarddatas[i].RewardType == (int)Config.RewardType.RandHeroItem)
                {
                    var heroitemdata = GameRoot.Instance.HeroSystem.GetHeroRandItem((int)rewarddatas[i].RewardIdx);

                    obj.GetComponent<PurchaseConfirmItemComponent>().Set((int)Config.RewardType.HeroEquipment, heroitemdata.Heroitemidx, heroitemdata.Grade);

                    ProjectUtility.SetActiveCheck(obj, true);
                }
                else
                {
                    obj.GetComponent<PurchaseConfirmItemComponent>().Set(rewarddatas[i].RewardType, rewarddatas[i].RewardIdx, (int)rewarddatas[i].RewardValue);
                    ProjectUtility.SetActiveCheck(obj, true);
                }
            }
        }
    }




    private GameObject GetCachedObject()
    {
        var inst = EquipUpgradeComponents.Find(x => !x.gameObject.activeSelf);

        if (inst == null)
        {
            inst = GameObject.Instantiate(EquipItemPrefab).GetComponent<PurchaseConfirmItemComponent>();
        }

        inst.transform.SetParent(EquipItemRoot);

        inst.transform.localScale = UnityEngine.Vector3.one;


        if (!EquipUpgradeComponents.Contains(inst))
        {
            EquipUpgradeComponents.Add(inst);
        }

        return inst.gameObject;
    }
}

