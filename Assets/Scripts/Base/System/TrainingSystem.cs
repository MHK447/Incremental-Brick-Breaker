using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BanpoFri;
using UniRx;
public class TrainingSystem
{
    public enum TrainingType
    {
        BlockAttackDamage = 101,
        UnitAttackDamage = 102,
        UnitHpIncrease = 103,
        CastleHpIncrease = 104,}

    public void Create()
    {
        CheckTrainingData();
    }


    public void CheckTrainingData()
    {
        // if (GameRoot.Instance.UserData.Traininggroupdata.Trainingdatas.Count == 0) return;

        // GameRoot.Instance.UserData.Playerdata.UpdatePlayerLevelFromStageIndex();

        // GameRoot.Instance.UserData.Playerdata.Playerlevel = GameRoot.Instance.UserData.Stageidx.Value;

        //     var levelasttd = Tables.Instance.GetTable<BlockTrainingInfo>().DataList.Where(x => x.level == GameRoot.Instance.UserData.Playerdata.Playerlevel).LastOrDefault();

        //    var lasttrainingdata = GameRoot.Instance.UserData.Traininggroupdata.Trainingdatas.Where(x => x.Isupgradeproperty.Value).LastOrDefault();
        // if (lasttrainingdata != null)
        // {
        //     if (lasttrainingdata.Trainingorder > levelasttd.upgrade_order)
        //     {
        //         GameRoot.Instance.UserData.Newtrainingdatabuyorder.Value = levelasttd.upgrade_order;
        //     }
        //     else
        //     {
        //         GameRoot.Instance.UserData.Newtrainingdatabuyorder.Value = lasttrainingdata.Trainingorder;
        //     }
        // }

        // GameRoot.Instance.UserData.Traininggroupdata.Trainingdatas.Clear();


        // GameRoot.Instance.UserData.Save();
    }

    public double GetBuffValue(TrainingType type)
    {
        var buffvalue = 0;

        var curtrainingorder = GameRoot.Instance.UserData.Newtrainingdatabuyorder.Value;

        var tdlist = Tables.Instance.GetTable<BlockTrainingInfo>().DataList.Where(x => x.upgrade_order <= curtrainingorder && x.training_type == (int)type).ToList();

        foreach (var td in tdlist)
        {
            if (td.training_type == (int)type)
            {
                buffvalue += td.value;
            }

        }

        return buffvalue;
    }


    public double GetLevelBuffValue(TrainingType type, int level )
    {
        var buffvalue = 0;


        var tdlist = Tables.Instance.GetTable<BlockTrainingInfo>().DataList.Where(x => x.level <= level && x.training_type == (int)type).ToList();

        foreach (var td in tdlist)
        {
            if (td.training_type == (int)type)
            {
                buffvalue += td.value;
            }
        }

        return buffvalue;
    }


    // public TrainingData FindTrainingData(int order)
    // {
    //     return GameRoot.Instance.UserData.Trainingdatagroup.Trainingdatas.Find(x => x.Trainingorder == order);
    // }


    public void BuyTraining()
    {
        GameRoot.Instance.UserData.Newtrainingdatabuyorder.Value += 1;
    }
}
