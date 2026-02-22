using System;
using System.Collections.Generic;
using UniRx;
using Google.FlatBuffers;
using System.Linq;
using BanpoFri;

public partial class UserDataSystem
{
    public StageRewardBoxGroupData Stagerewardboxgroup { get; private set; } = new StageRewardBoxGroupData();
    private void SaveData_StageRewardBoxGroupData(FlatBufferBuilder builder)
    {
        // 선언된 변수들은 모두 저장되어야함

        // Stagerewardboxgroup 단일 저장
        // Stagerewardboxgroup.Stagerewardboxdatas 처리 GenerateItemSaveCode IsCustom
        Offset<BanpoFri.Data.StageRewardBoxData>[] stagerewardboxgroup_stagerewardboxdatas_Array = null;
        VectorOffset stagerewardboxgroup_stagerewardboxdatas_Vector = default;

        if (Stagerewardboxgroup.Stagerewardboxdatas.Count > 0)
        {
            stagerewardboxgroup_stagerewardboxdatas_Array = new Offset<BanpoFri.Data.StageRewardBoxData>[Stagerewardboxgroup.Stagerewardboxdatas.Count];
            int stagerewardboxgroup_stagerewardboxdatas_idx = 0;
            foreach (var stagerewardboxgroup_stagerewardboxdatas_pair in Stagerewardboxgroup.Stagerewardboxdatas)
            {
                var stagerewardboxgroup_stagerewardboxdatas_item = Stagerewardboxgroup.Stagerewardboxdatas[stagerewardboxgroup_stagerewardboxdatas_idx];
                stagerewardboxgroup_stagerewardboxdatas_Array[stagerewardboxgroup_stagerewardboxdatas_idx++] = BanpoFri.Data.StageRewardBoxData.CreateStageRewardBoxData(
                    builder,
                    stagerewardboxgroup_stagerewardboxdatas_item.Boxidx,
                    stagerewardboxgroup_stagerewardboxdatas_item.Boxtime.Ticks,
                    stagerewardboxgroup_stagerewardboxdatas_item.Isopenstart,
                    stagerewardboxgroup_stagerewardboxdatas_item.BoxOrder,
                    stagerewardboxgroup_stagerewardboxdatas_item.BoxGetStageIdx
                );
            }
            stagerewardboxgroup_stagerewardboxdatas_Vector = BanpoFri.Data.StageRewardBoxGroupData.CreateStagerewardboxdatasVector(builder, stagerewardboxgroup_stagerewardboxdatas_Array);
        }

        // Stagerewardboxgroup.Equipitembox 처리 GenerateItemSaveCode Array
        int[] stagerewardboxgroup_equipitembox_Array = null;
        VectorOffset stagerewardboxgroup_equipitembox_Vector = default;

        if (Stagerewardboxgroup.Equipitembox.Count > 0)
        {
            stagerewardboxgroup_equipitembox_Array = new int[Stagerewardboxgroup.Equipitembox.Count];
            int stagerewardboxgroup_equipitembox_idx = 0;
            foreach (int stagerewardboxgroup_equipitembox_val in Stagerewardboxgroup.Equipitembox)
            {
                stagerewardboxgroup_equipitembox_Array[stagerewardboxgroup_equipitembox_idx++] = stagerewardboxgroup_equipitembox_val;
            }
            stagerewardboxgroup_equipitembox_Vector = BanpoFri.Data.StageRewardBoxGroupData.CreateEquipitemboxVector(builder, stagerewardboxgroup_equipitembox_Array);
        }

        // Stagerewardboxgroup 최종 생성 및 추가
        var stagerewardboxgroup_Offset = BanpoFri.Data.StageRewardBoxGroupData.CreateStageRewardBoxGroupData(
            builder,
            stagerewardboxgroup_stagerewardboxdatas_Vector,
            Stagerewardboxgroup.Boxcount,
            stagerewardboxgroup_equipitembox_Vector
        );


        Action cbAddDatas = () =>
        {
            BanpoFri.Data.UserData.AddStagerewardboxgroup(builder, stagerewardboxgroup_Offset);
        };

        cb_SaveAddDatas += cbAddDatas;

    }
    private void LoadData_StageRewardBoxGroupData()
    {
        // 로드 함수 내용

        // Stagerewardboxgroup 로드
        var fb_Stagerewardboxgroup = flatBufferUserData.Stagerewardboxgroup;
        if (fb_Stagerewardboxgroup.HasValue)
        {
            Stagerewardboxgroup.Boxcount = fb_Stagerewardboxgroup.Value.Boxcount;

            // Stagerewardboxdatas 로드
            Stagerewardboxgroup.Stagerewardboxdatas.Clear();
            int stagerewardboxdatasLength = fb_Stagerewardboxgroup.Value.StagerewardboxdatasLength;
            for (int j = 0; j < stagerewardboxdatasLength; j++)
            {
                var fbStagerewardboxdatasItem = fb_Stagerewardboxgroup.Value.Stagerewardboxdatas(j);
                if (fbStagerewardboxdatasItem.HasValue)
                {
                    var nested_item = new StageRewardBoxData
                    {
                        Boxidx = fbStagerewardboxdatasItem.Value.Boxidx,
                        Boxtime = new DateTime(fbStagerewardboxdatasItem.Value.Boxtime),
                        Isopenstart = fbStagerewardboxdatasItem.Value.Isopenstart,
                        BoxOrder = fbStagerewardboxdatasItem.Value.Boxorder,
                        BoxGetStageIdx = fbStagerewardboxdatasItem.Value.Boxgetstageidx
                    };

                    // Boxtime DateTime 로드
                    Stagerewardboxgroup.Stagerewardboxdatas.Add(nested_item);
                }
            }

            // Equipitembox 리스트 로드
            Stagerewardboxgroup.Equipitembox.Clear();
            for (int j = 0; j < fb_Stagerewardboxgroup.Value.EquipitemboxLength; j++)
            {
                int equipitembox_val = fb_Stagerewardboxgroup.Value.Equipitembox(j);
                Stagerewardboxgroup.Equipitembox.Add(equipitembox_val);
            }
        }
    }

}

public class StageRewardBoxGroupData
{

    public List<int> Equipitembox = new List<int>();

    public ReactiveCollection<StageRewardBoxData> Stagerewardboxdatas = new ReactiveCollection<StageRewardBoxData>();
    public int Boxcount { get; set; } = 0;


    public bool IsBoxMaxCheck()
    {
        return Stagerewardboxdatas.Count >= 4;
    }

    public bool IsBoxUnlockStart()
    {
        return Stagerewardboxdatas.ToList().Find(x => x.Isopenstart) != null;
    }


    public void AddBox(int boxidx)
    {
        if (IsBoxMaxCheck())
        {
            var finddata = Stagerewardboxdatas.ToList().Find(x => x.Boxidx < boxidx && !x.Isopenstart);

            if (finddata != null)
            {
                finddata.Boxidx = boxidx;
            }
        }
        else
        {

            // 가장 작은 사용 가능한 BoxOrder 찾기 (중복 방지)
            int newBoxOrder = 1;
            var existingOrders = Stagerewardboxdatas.Select(x => x.BoxOrder).OrderBy(x => x).ToList();
            foreach (var order in existingOrders)
            {
                if (order == newBoxOrder)
                {
                    newBoxOrder++;
                }
                else
                {
                    break;
                }
            }

            Stagerewardboxdatas.Add(new StageRewardBoxData() { Boxidx = boxidx, BoxOrder = newBoxOrder, BoxGetStageIdx = GameRoot.Instance.UserData.Stageidx.Value });
        }


        GameRoot.Instance.UserData.Save();
    }




    public void GetBoxReward(int boxidx)
    {
        var td = Tables.Instance.GetTable<RewardBoxInfo>().GetData(boxidx);

        List<RewardData> rewarddatas = new List<RewardData>();


        for (int i = 0; i < td.reward_type.Count; ++i)
        {
            System.Numerics.BigInteger rewardvalue = 0;

            if (td.reward_idx[i] == (int)Config.CurrencyID.Money)
            {
                
                rewardvalue = ProjectUtility.RandomBigRange(GameRoot.Instance.StageRewardSystem.GetStageRewardMoney(td.reward_value_min[i]),
                 GameRoot.Instance.StageRewardSystem.GetStageRewardMoney(td.reward_value_max[i]));

                 rewardvalue = rewardvalue / 2;
            }
            else
            {
                rewardvalue = UnityEngine.Random.Range(td.reward_value_min[i], td.reward_value_max[i]);
            }

            rewarddatas.Add(new RewardData(td.reward_type[i], td.reward_idx[i], rewardvalue));
        }

        GameRoot.Instance.UISystem.OpenUI<PageRewardConfirm>(page => page.Init(rewarddatas, true));
    }


    public void RemoveBox(StageRewardBoxData boxdata)
    {

        Stagerewardboxdatas.Remove(boxdata);

        GameRoot.Instance.UserData.Save();
    }

}
