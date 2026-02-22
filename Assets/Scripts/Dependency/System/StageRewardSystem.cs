using UnityEngine;
using BanpoFri;
public class StageRewardSystem
{

    public double StageRewardpercent(int stageidx)
    {

        var stagetdlist = Tables.Instance.GetTable<StageInfo>().DataList.FindAll(x => x.stage_idx == stageidx);


        var unitcount = 0;



        // foreach (var stagetd in stagetdlist)
        // {
        //     unitcount += stagetd.enemy_idx.Count;
        // }


        return ProjectUtility.PercentGet(unitcount, GameRoot.Instance.UserData.Playerdata.KillCountProperty.Value);
    }


    public System.Numerics.BigInteger GetStageRewardMoney(int count = 1)
    {
        System.Numerics.BigInteger value = 0;

        var curstageidx = GameRoot.Instance.UserData.Stageidx.Value;


        for (int i = 0; i < count; ++i)
        {
            var rewardtd = Tables.Instance.GetTable<StageRewardInfo>().GetData((int)Config.CurrencyID.Money);
        }
        

        return curstageidx == 1 ? 50 : value / 100;
    }




    public System.Numerics.BigInteger GetFailedReward(int rewaridx)
    {
        var reward = GetStageRewardMoney();

        return reward / GameRoot.Instance.UserData.Playerdata.KillCountProperty.Value;
    }



    public int GetStageRewardMoney(int stageidx , bool isclear)
    {
        var stagetd = Tables.Instance.GetTable<StageInfo>().GetData(stageidx);

        if(isclear)
        {
            return stagetd.stage_clear_gold_value;
        }
        else
        {
            // 패배 시 - 킬 카운트에 따라 골드를 퍼센트로 계산
            var killcount = GameRoot.Instance.UserData.Playerdata.KillCountProperty.Value;

            // 해당 스테이지의 모든 웨이브에서 총 적의 수 계산
            var waveList = Tables.Instance.GetTable<WaveInfo>().DataList.FindAll(x => x.stage == stageidx);
            int totalEnemyCount = 0;

            foreach (var wave in waveList)
            {
                for (int i = 0; i < wave.unit_count.Count; i++)
                {
                    totalEnemyCount += wave.unit_count[i];
                }
            }

            // (킬 카운트 / 총 적 수) * 클리어 골드
            float percent = (float)killcount / totalEnemyCount;
            int rewardGold = Mathf.FloorToInt(stagetd.stage_clear_gold_value * percent);
            

            return rewardGold < 10 ? 10 : rewardGold;
        }
    }



    public int GetStageRewardMaterial(int stageidx , bool isclear)
    {
        var stagetd = Tables.Instance.GetTable<StageInfo>().GetData(stageidx);

        if(isclear)
        {
            return 20;
        }
        else
        {
            // 패배 시 - 킬 카운트에 따라 골드를 퍼센트로 계산
            var killcount = GameRoot.Instance.UserData.Playerdata.KillCountProperty.Value;

            // 해당 스테이지의 모든 웨이브에서 총 적의 수 계산
            var waveList = Tables.Instance.GetTable<WaveInfo>().DataList.FindAll(x => x.stage == stageidx);
            int totalEnemyCount = 0;

            foreach (var wave in waveList)
            {
                for (int i = 0; i < wave.unit_count.Count; i++)
                {
                    totalEnemyCount += wave.unit_count[i];
                }
            }

            // (킬 카운트 / 총 적 수) * 클리어 골드
            float percent = (float)killcount / totalEnemyCount;
            int rewardMaterial = Mathf.FloorToInt(20 * percent);
            

            return rewardMaterial;
        }


        
    }
}
