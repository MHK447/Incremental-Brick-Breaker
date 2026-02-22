using System;
using System.Collections.Generic;
using UniRx;
using Google.FlatBuffers;

public partial class UserDataSystem
{
    public HeroGroupData Herogroudata { get; private set; } = new HeroGroupData();
    private void SaveData_HeroGroupData(FlatBufferBuilder builder)
    {
        // 선언된 변수들은 모두 저장되어야함

        // Herogroudata 단일 저장
        // Herogroudata.Equipheroitems 처리 GenerateItemSaveCode IsCustom
        Offset<BanpoFri.Data.HeroEquipItemData>[] herogroudata_equipheroitems_Array = null;
        VectorOffset herogroudata_equipheroitems_Vector = default;

        if(Herogroudata.Equipheroitems.Count > 0){
            herogroudata_equipheroitems_Array = new Offset<BanpoFri.Data.HeroEquipItemData>[Herogroudata.Equipheroitems.Count];
            int herogroudata_equipheroitems_idx = 0;
            foreach(var herogroudata_equipheroitems_pair in Herogroudata.Equipheroitems){
                var herogroudata_equipheroitems_item = Herogroudata.Equipheroitems[herogroudata_equipheroitems_idx];
                herogroudata_equipheroitems_Array[herogroudata_equipheroitems_idx++] = BanpoFri.Data.HeroEquipItemData.CreateHeroEquipItemData(
                    builder,
                    herogroudata_equipheroitems_item.Isequip.Value,
                    herogroudata_equipheroitems_item.Level.Value,
                    herogroudata_equipheroitems_item.Heroitemtype
                );
            }
            herogroudata_equipheroitems_Vector = BanpoFri.Data.HeroGroupData.CreateEquipheroitemsVector(builder, herogroudata_equipheroitems_Array);
        }

        // Herogroudata.Heroitemdatas 처리 GenerateItemSaveCode IsCustom
        Offset<BanpoFri.Data.HeroItemData>[] herogroudata_heroitemdatas_Array = null;
        VectorOffset herogroudata_heroitemdatas_Vector = default;

        if(Herogroudata.Heroitemdatas.Count > 0){
            herogroudata_heroitemdatas_Array = new Offset<BanpoFri.Data.HeroItemData>[Herogroudata.Heroitemdatas.Count];
            int herogroudata_heroitemdatas_idx = 0;
            foreach(var herogroudata_heroitemdatas_pair in Herogroudata.Heroitemdatas){
                var herogroudata_heroitemdatas_item = Herogroudata.Heroitemdatas[herogroudata_heroitemdatas_idx];
                herogroudata_heroitemdatas_Array[herogroudata_heroitemdatas_idx++] = BanpoFri.Data.HeroItemData.CreateHeroItemData(
                    builder,
                    herogroudata_heroitemdatas_item.Heroitemidx,
                    herogroudata_heroitemdatas_item.Grade,
                    herogroudata_heroitemdatas_item.Isequip.Value
                );
            }
            herogroudata_heroitemdatas_Vector = BanpoFri.Data.HeroGroupData.CreateHeroitemdatasVector(builder, herogroudata_heroitemdatas_Array);
        }

        // Herogroudata 최종 생성 및 추가
        var herogroudata_Offset = BanpoFri.Data.HeroGroupData.CreateHeroGroupData(
            builder,
            Herogroudata.Equipplayeridx,
            herogroudata_equipheroitems_Vector,
            herogroudata_heroitemdatas_Vector,
            Herogroudata.Herolevel
        );


        Action cbAddDatas = () => {
            BanpoFri.Data.UserData.AddHerogroudata(builder, herogroudata_Offset);
        };

        cb_SaveAddDatas += cbAddDatas;

    }
    private void LoadData_HeroGroupData()
    {
        // 로드 함수 내용

        // Herogroudata 로드
        var fb_Herogroudata = flatBufferUserData.Herogroudata;
        if (fb_Herogroudata.HasValue)
        {
            Herogroudata.Equipplayeridx = fb_Herogroudata.Value.Equipplayeridx;
            Herogroudata.Herolevel = fb_Herogroudata.Value.Herolevel;

            // Equipheroitems 로드
            Herogroudata.Equipheroitems.Clear();
            int equipheroitemsLength = fb_Herogroudata.Value.EquipheroitemsLength;
            for (int j = 0; j < equipheroitemsLength; j++)
            {
                var fbEquipheroitemsItem = fb_Herogroudata.Value.Equipheroitems(j);
                if (fbEquipheroitemsItem.HasValue)
                {
                    var nested_item = new HeroEquipItemData
                    {
                        Isequip = new ReactiveProperty<bool>(fbEquipheroitemsItem.Value.Isequip),
                        Level = new ReactiveProperty<int>(fbEquipheroitemsItem.Value.Level),
                        Heroitemtype = fbEquipheroitemsItem.Value.Heroitemtype
                    };
                    Herogroudata.Equipheroitems.Add(nested_item);
                }
            }

            // Heroitemdatas 로드
            Herogroudata.Heroitemdatas.Clear();
            int heroitemdatasLength = fb_Herogroudata.Value.HeroitemdatasLength;
            for (int j = 0; j < heroitemdatasLength; j++)
            {
                var fbHeroitemdatasItem = fb_Herogroudata.Value.Heroitemdatas(j);
                if (fbHeroitemdatasItem.HasValue)
                {
                    var nested_item = new HeroItemData
                    {
                        Heroitemidx = fbHeroitemdatasItem.Value.Heroitemidx,
                        Grade = fbHeroitemdatasItem.Value.Grade,
                        Isequip = new ReactiveProperty<bool>(fbHeroitemdatasItem.Value.Isequip)
                    };
                    Herogroudata.Heroitemdatas.Add(nested_item);
                }
            }
        }
    }

}

public class HeroGroupData
{
    public int Herolevel { get; set; } = 0;

    public List<HeroItemData> Heroitemdatas = new List<HeroItemData>();

    public int Equipplayeridx { get; set; } = 0;
    public List<HeroEquipItemData> Equipheroitems = new List<HeroEquipItemData>();


    public void AddHeroItem(int heroidx, int grade)
    {
        var addheroitem = new HeroItemData() { Heroitemidx = heroidx, Grade = grade };

        Heroitemdatas.Add(addheroitem);
    }

}
