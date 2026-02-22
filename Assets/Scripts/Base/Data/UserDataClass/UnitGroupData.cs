using System;
using System.Collections.Generic;
using UniRx;
using Google.FlatBuffers;
using UnityEngine;
using BanpoFri;
using System.Linq;
using UnityEngine.UI;

public partial class UserDataSystem
{
    public UnitGroupData Unitgroupdata { get; private set; } = new UnitGroupData();
    private void SaveData_UnitGroupData(FlatBufferBuilder builder)
    {
        // 선언된 변수들은 모두 저장되어야함

        // Unitgroupdata 단일 저장
        // Unitgroupdata.Equipidxs 처리 GenerateItemSaveCode Array
        int[] unitgroupdata_equipidxs_Array = null;
        VectorOffset unitgroupdata_equipidxs_Vector = default;

        if (Unitgroupdata.Equipidxs.Count > 0)
        {
            unitgroupdata_equipidxs_Array = new int[Unitgroupdata.Equipidxs.Count];
            int unitgroupdata_equipidxs_idx = 0;
            foreach (int unitgroupdata_equipidxs_val in Unitgroupdata.Equipidxs)
            {
                unitgroupdata_equipidxs_Array[unitgroupdata_equipidxs_idx++] = unitgroupdata_equipidxs_val;
            }
            unitgroupdata_equipidxs_Vector = BanpoFri.Data.UnitGroupData.CreateEquipidxsVector(builder, unitgroupdata_equipidxs_Array);
        }

        // Unitgroupdata.Unitdatas 처리 GenerateItemSaveCode IsCustom
        Offset<BanpoFri.Data.UnitData>[] unitgroupdata_unitdatas_Array = null;
        VectorOffset unitgroupdata_unitdatas_Vector = default;

        if (Unitgroupdata.Unitdatas.Count > 0)
        {
            unitgroupdata_unitdatas_Array = new Offset<BanpoFri.Data.UnitData>[Unitgroupdata.Unitdatas.Count];
            int unitgroupdata_unitdatas_idx = 0;
            foreach (var unitgroupdata_unitdatas_pair in Unitgroupdata.Unitdatas)
            {
                var unitgroupdata_unitdatas_item = Unitgroupdata.Unitdatas[unitgroupdata_unitdatas_idx];
                unitgroupdata_unitdatas_Array[unitgroupdata_unitdatas_idx++] = BanpoFri.Data.UnitData.CreateUnitData(
                    builder,
                    unitgroupdata_unitdatas_item.Unitidx,
                    unitgroupdata_unitdatas_item.Unitlevel
                );
            }
            unitgroupdata_unitdatas_Vector = BanpoFri.Data.UnitGroupData.CreateUnitdatasVector(builder, unitgroupdata_unitdatas_Array);
        }

        // Unitgroupdata 최종 생성 및 추가
        var unitgroupdata_Offset = BanpoFri.Data.UnitGroupData.CreateUnitGroupData(
            builder,
            unitgroupdata_equipidxs_Vector,
            unitgroupdata_unitdatas_Vector
        );


        Action cbAddDatas = () =>
        {
            BanpoFri.Data.UserData.AddUnitgroupdata(builder, unitgroupdata_Offset);
        };

        cb_SaveAddDatas += cbAddDatas;

    }
    private void LoadData_UnitGroupData()
    {
        // 로드 함수 내용

        // Unitgroupdata 로드
        var fb_Unitgroupdata = flatBufferUserData.Unitgroupdata;
        if (fb_Unitgroupdata.HasValue)
        {

            // Equipidxs 리스트 로드
            Unitgroupdata.Equipidxs.Clear();
            for (int j = 0; j < fb_Unitgroupdata.Value.EquipidxsLength; j++)
            {
                int equipidxs_val = fb_Unitgroupdata.Value.Equipidxs(j);
                Unitgroupdata.Equipidxs.Add(equipidxs_val);
            }

            // Unitdatas 로드
            Unitgroupdata.Unitdatas.Clear();
            int unitdatasLength = fb_Unitgroupdata.Value.UnitdatasLength;
            for (int j = 0; j < unitdatasLength; j++)
            {
                var fbUnitdatasItem = fb_Unitgroupdata.Value.Unitdatas(j);
                if (fbUnitdatasItem.HasValue)
                {
                    var nested_item = new UnitData
                    {
                        Unitidx = fbUnitdatasItem.Value.Unitidx,
                        Unitlevel = fbUnitdatasItem.Value.Unitlevel
                    };
                    Unitgroupdata.Unitdatas.Add(nested_item);
                }
            }
        }
    }

}

public class UnitGroupData
{
    public List<int> Equipidxs = new List<int>();
    public List<UnitData> Unitdatas = new List<UnitData>();



    public void AddUnit(int unitidx)
    {
        var finddata = FindUnit(unitidx);

        if (finddata == null)
        {
            Unitdatas.Add(new UnitData()
            {
                Unitidx = unitidx,
                Unitlevel = 1
            });

            if (GameRoot.Instance.UserData.Unitgroupdata.Equipidxs.Count < 4)
            {
                GameRoot.Instance.UserData.Unitgroupdata.Equipidxs.Add(unitidx);
            }


            GameRoot.Instance.UserData.Save();
        }
    }

    public UnitData FindUnit(int unitidx)
    {
        return Unitdatas.ToList().Find(x => x.Unitidx == unitidx);
    }

    public void UnitLevelUp(int unitidx)
    {
        var finddata = FindUnit(unitidx);
        if (finddata != null)
        {
            finddata.UnitlevelProperty.Value++;
        }
    }




}
