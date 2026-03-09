using System;
using System.Collections.Generic;
using UniRx;
using Google.FlatBuffers;

public partial class UserDataSystem
{
    public List<InCreaseUpgradeData> Increaseugprades { get; private set; } = new List<InCreaseUpgradeData>();



    private void SaveData_InCreaseUpgradeData(FlatBufferBuilder builder)
    {
        // 선언된 변수들은 모두 저장되어야함

        // Increaseugprades Array 저장
        Offset<BanpoFri.Data.InCreaseUpgradeData>[] increaseugprades_Array = null;
        VectorOffset increaseugprades_Vector = default;

        if(Increaseugprades.Count > 0){
            increaseugprades_Array = new Offset<BanpoFri.Data.InCreaseUpgradeData>[Increaseugprades.Count];
            int index = 0;
            foreach(var pair in Increaseugprades){
                var item = pair;
                increaseugprades_Array[index++] = BanpoFri.Data.InCreaseUpgradeData.CreateInCreaseUpgradeData(
                    builder,
                    item.Idx,
                    item.Level.Value
                );
            }
            increaseugprades_Vector = BanpoFri.Data.UserData.CreateIncreaseugpradesVector(builder, increaseugprades_Array);
        }



        Action cbAddDatas = () => {
            BanpoFri.Data.UserData.AddIncreaseugprades(builder, increaseugprades_Vector);
        };

        cb_SaveAddDatas += cbAddDatas;

    }
    private void LoadData_InCreaseUpgradeData()
    {
        // 로드 함수 내용

        // Increaseugprades 로드
        Increaseugprades.Clear();
        int Increaseugprades_length = flatBufferUserData.IncreaseugpradesLength;
        for (int i = 0; i < Increaseugprades_length; i++)
        {
            var Increaseugprades_item = flatBufferUserData.Increaseugprades(i);
            if (Increaseugprades_item.HasValue)
            {
                var increaseupgradedata = new InCreaseUpgradeData
                {
                    Idx = Increaseugprades_item.Value.Idx,
                    Level = new ReactiveProperty<int>(Increaseugprades_item.Value.Level)
                };
                Increaseugprades.Add(increaseupgradedata);
            }
        }
    }

}

public class InCreaseUpgradeData
{
    public int Idx { get; set; } = 0;
    public IReactiveProperty<int> Level { get; set; } = new ReactiveProperty<int>(0);

}
