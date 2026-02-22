using System;
using System.Collections.Generic;
using UniRx;
using Google.FlatBuffers;
using System.Linq;


public partial class UserDataSystem
{
    public List<CardData> Carddatas { get; private set; } = new List<CardData>();
    private void SaveData_CardData(FlatBufferBuilder builder)
    {
        // 선언된 변수들은 모두 저장되어야함

        // Carddatas Array 저장
        Offset<BanpoFri.Data.CardData>[] carddatas_Array = null;
        VectorOffset carddatas_Vector = default;

        if(Carddatas.Count > 0){
            carddatas_Array = new Offset<BanpoFri.Data.CardData>[Carddatas.Count];
            int index = 0;
            foreach(var pair in Carddatas){
                var item = pair;
                carddatas_Array[index++] = BanpoFri.Data.CardData.CreateCardData(
                    builder,
                    item.Cardidx,
                    item.Cardlevel.Value,
                    item.Cardcount.Value,
                    item.Isequip.Value
                );
            }
            carddatas_Vector = BanpoFri.Data.UserData.CreateCarddatasVector(builder, carddatas_Array);
        }



        Action cbAddDatas = () => {
            BanpoFri.Data.UserData.AddCarddatas(builder, carddatas_Vector);
        };

        cb_SaveAddDatas += cbAddDatas;

    }
    private void LoadData_CardData()
    {
        // 로드 함수 내용

        // Carddatas 로드
        Carddatas.Clear();
        int Carddatas_length = flatBufferUserData.CarddatasLength;
        for (int i = 0; i < Carddatas_length; i++)
        {
            var Carddatas_item = flatBufferUserData.Carddatas(i);
            if (Carddatas_item.HasValue)
            {
                var carddata = new CardData
                {
                    Cardidx = Carddatas_item.Value.Cardidx,
                    Cardlevel = new ReactiveProperty<int>(Carddatas_item.Value.Cardlevel),
                    Cardcount = new ReactiveProperty<int>(Carddatas_item.Value.Cardcount),
                    Isequip = new ReactiveProperty<bool>(Carddatas_item.Value.Isequip)
                };
                Carddatas.Add(carddata);
            }
        }
    }

}

public class CardData
{
    public IReactiveProperty<bool> Isequip { get; set; } = new ReactiveProperty<bool>(false);

    public int Cardidx { get; set; } = 0;
    public IReactiveProperty<int> Cardlevel { get; set; } = new ReactiveProperty<int>(0);
    public IReactiveProperty<int> Cardcount { get; set; } = new ReactiveProperty<int>(0);

}
