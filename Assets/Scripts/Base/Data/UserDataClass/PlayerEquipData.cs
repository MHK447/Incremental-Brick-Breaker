using System;
using System.Collections.Generic;
using UniRx;
using Google.FlatBuffers;

public partial class UserDataSystem
{
    public PlayerEquipData Playerequipdata { get; private set; } = new PlayerEquipData();



    private void SaveData_PlayerEquipData(FlatBufferBuilder builder)
    {
        // 선언된 변수들은 모두 저장되어야함

        // Playerequipdata 단일 저장
        // Playerequipdata.Equipitemdatas 처리 GenerateItemSaveCode IsCustom
        Offset<BanpoFri.Data.EquipItemData>[] playerequipdata_equipitemdatas_Array = null;
        VectorOffset playerequipdata_equipitemdatas_Vector = default;

        if(Playerequipdata.Equipitemdatas.Count > 0){
            playerequipdata_equipitemdatas_Array = new Offset<BanpoFri.Data.EquipItemData>[Playerequipdata.Equipitemdatas.Count];
            int playerequipdata_equipitemdatas_idx = 0;
            foreach(var playerequipdata_equipitemdatas_pair in Playerequipdata.Equipitemdatas){
                var playerequipdata_equipitemdatas_item = Playerequipdata.Equipitemdatas[playerequipdata_equipitemdatas_idx];
                playerequipdata_equipitemdatas_Array[playerequipdata_equipitemdatas_idx++] = BanpoFri.Data.EquipItemData.CreateEquipItemData(
                    builder,
                    playerequipdata_equipitemdatas_item.Equipitemidx,
                    playerequipdata_equipitemdatas_item.Level,
                    playerequipdata_equipitemdatas_item.Grade
                );
            }
            playerequipdata_equipitemdatas_Vector = BanpoFri.Data.PlayerEquipData.CreateEquipitemdatasVector(builder, playerequipdata_equipitemdatas_Array);
        }

        // Playerequipdata 최종 생성 및 추가
        var playerequipdata_Offset = BanpoFri.Data.PlayerEquipData.CreatePlayerEquipData(
            builder,
            playerequipdata_equipitemdatas_Vector
        );


        Action cbAddDatas = () => {
            BanpoFri.Data.UserData.AddPlayerequipdata(builder, playerequipdata_Offset);
        };

        cb_SaveAddDatas += cbAddDatas;

    }
    private void LoadData_PlayerEquipData()
    {
        // 로드 함수 내용

        // Playerequipdata 로드
        var fb_Playerequipdata = flatBufferUserData.Playerequipdata;
        if (fb_Playerequipdata.HasValue)
        {

            // Equipitemdatas 로드
            Playerequipdata.Equipitemdatas.Clear();
            int equipitemdatasLength = fb_Playerequipdata.Value.EquipitemdatasLength;
            for (int j = 0; j < equipitemdatasLength; j++)
            {
                var fbEquipitemdatasItem = fb_Playerequipdata.Value.Equipitemdatas(j);
                if (fbEquipitemdatasItem.HasValue)
                {
                    var nested_item = new EquipItemData
                    {
                        Equipitemidx = fbEquipitemdatasItem.Value.Equipitemidx,
                        Level = fbEquipitemdatasItem.Value.Level,
                        Grade = fbEquipitemdatasItem.Value.Grade
                    };
                    Playerequipdata.Equipitemdatas.Add(nested_item);
                }
            }
        }
    }

}

public class PlayerEquipData
{
    public List<EquipItemData> Equipitemdatas = new List<EquipItemData>();

}
