using System;
using System.Collections.Generic;
using UniRx;
using Google.FlatBuffers;

public partial class UserDataSystem
{
    public List<ItemData> Itemdatas { get; private set; } = new List<ItemData>();
    private void SaveData_ItemData(FlatBufferBuilder builder)
    {
        // 선언된 변수들은 모두 저장되어야함

        // Itemdatas Array 저장
        Offset<BanpoFri.Data.ItemData>[] itemdatas_Array = null;
        VectorOffset itemdatas_Vector = default;

        if(Itemdatas.Count > 0){
            itemdatas_Array = new Offset<BanpoFri.Data.ItemData>[Itemdatas.Count];
            int index = 0;
            foreach(var pair in Itemdatas){
                var item = pair;
                itemdatas_Array[index++] = BanpoFri.Data.ItemData.CreateItemData(
                    builder,
                    item.Itemtype,
                    item.Itemidx,
                    item.Itemcnt.Value
                );
            }
            itemdatas_Vector = BanpoFri.Data.UserData.CreateItemdatasVector(builder, itemdatas_Array);
        }



        Action cbAddDatas = () => {
            BanpoFri.Data.UserData.AddItemdatas(builder, itemdatas_Vector);
        };

        cb_SaveAddDatas += cbAddDatas;

    }
    private void LoadData_ItemData()
    {
        // 로드 함수 내용

        // Itemdatas 로드
        Itemdatas.Clear();
        int Itemdatas_length = flatBufferUserData.ItemdatasLength;
        for (int i = 0; i < Itemdatas_length; i++)
        {
            var Itemdatas_item = flatBufferUserData.Itemdatas(i);
            if (Itemdatas_item.HasValue)
            {
                var itemdata = new ItemData
                {
                    Itemtype = Itemdatas_item.Value.Itemtype,
                    Itemidx = Itemdatas_item.Value.Itemidx,
                    Itemcnt = new ReactiveProperty<int>(Itemdatas_item.Value.Itemcnt)
                };
                Itemdatas.Add(itemdata);
            }
        }
    }

}

public class ItemData
{
    public int Itemtype { get; set; } = 0;
    public int Itemidx { get; set; } = 0;
    public IReactiveProperty<int> Itemcnt { get; set; } = new ReactiveProperty<int>(0);

}
