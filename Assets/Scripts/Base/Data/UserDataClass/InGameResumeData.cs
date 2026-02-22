using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using Google.FlatBuffers;

public partial class UserDataSystem
{
    public InGameResumeData Ingameresumedata { get; private set; } = new InGameResumeData();
    private void SaveData_InGameResumeData(FlatBufferBuilder builder)
    {
        // 선언된 변수들은 모두 저장되어야함

        // Ingameresumedata 단일 저장
        // Ingameresumedata.Tiles 처리 GenerateItemSaveCode IsCustom
        Offset<BanpoFri.Data.InGameResumeTileData>[] ingameresumedata_tiles_Array = null;
        VectorOffset ingameresumedata_tiles_Vector = default;

        if(Ingameresumedata.Tiles.Count > 0){
            ingameresumedata_tiles_Array = new Offset<BanpoFri.Data.InGameResumeTileData>[Ingameresumedata.Tiles.Count];
            int ingameresumedata_tiles_idx = 0;
            foreach(var ingameresumedata_tiles_pair in Ingameresumedata.Tiles){
                var ingameresumedata_tiles_item = Ingameresumedata.Tiles[ingameresumedata_tiles_idx];
                ingameresumedata_tiles_Array[ingameresumedata_tiles_idx++] = BanpoFri.Data.InGameResumeTileData.CreateInGameResumeTileData(
                    builder,
                    ingameresumedata_tiles_item.Equipidx,
                    ingameresumedata_tiles_item.Grade,
                    ingameresumedata_tiles_item.Tileorder,
                    ingameresumedata_tiles_item.Isad
                );
            }
            ingameresumedata_tiles_Vector = BanpoFri.Data.InGameResumeData.CreateTilesVector(builder, ingameresumedata_tiles_Array);
        }

        // Ingameresumedata.Unlockedtiles 처리 GenerateItemSaveCode Array
        int[] ingameresumedata_unlockedtiles_Array = null;
        VectorOffset ingameresumedata_unlockedtiles_Vector = default;

        if(Ingameresumedata.Unlockedtiles.Count > 0){
            ingameresumedata_unlockedtiles_Array = new int[Ingameresumedata.Unlockedtiles.Count];
            int ingameresumedata_unlockedtiles_idx = 0;
            foreach(int ingameresumedata_unlockedtiles_val in Ingameresumedata.Unlockedtiles){
                ingameresumedata_unlockedtiles_Array[ingameresumedata_unlockedtiles_idx++] = ingameresumedata_unlockedtiles_val;
            }
            ingameresumedata_unlockedtiles_Vector = BanpoFri.Data.InGameResumeData.CreateUnlockedtilesVector(builder, ingameresumedata_unlockedtiles_Array);
        }

        // Ingameresumedata.Upgradeindices 처리 GenerateItemSaveCode Array
        int[] ingameresumedata_upgradeindices_Array = null;
        VectorOffset ingameresumedata_upgradeindices_Vector = default;

        if(Ingameresumedata.Upgradeindices.Count > 0){
            ingameresumedata_upgradeindices_Array = new int[Ingameresumedata.Upgradeindices.Count];
            int ingameresumedata_upgradeindices_idx = 0;
            foreach(int ingameresumedata_upgradeindices_val in Ingameresumedata.Upgradeindices){
                ingameresumedata_upgradeindices_Array[ingameresumedata_upgradeindices_idx++] = ingameresumedata_upgradeindices_val;
            }
            ingameresumedata_upgradeindices_Vector = BanpoFri.Data.InGameResumeData.CreateUpgradeindicesVector(builder, ingameresumedata_upgradeindices_Array);
        }

        // Ingameresumedata 최종 생성 및 추가
        var ingameresumedata_Offset = BanpoFri.Data.InGameResumeData.CreateInGameResumeData(
            builder,
            Ingameresumedata.Isvalid,
            Ingameresumedata.Stageidx,
            Ingameresumedata.Waveidx,
            Ingameresumedata.Silvercoin,
            Ingameresumedata.Hp,
            ingameresumedata_tiles_Vector,
            Ingameresumedata.Starthp,
            Ingameresumedata.Shield,
            Ingameresumedata.Killcount,
            Ingameresumedata.Ingameexp,
            Ingameresumedata.Upgradecount,
            Ingameresumedata.Ingamemoney,
            Ingameresumedata.Rerollcount,
            ingameresumedata_unlockedtiles_Vector,
            ingameresumedata_upgradeindices_Vector
        );


        Action cbAddDatas = () => {
            BanpoFri.Data.UserData.AddIngameresumedata(builder, ingameresumedata_Offset);
        };

        cb_SaveAddDatas += cbAddDatas;

    }
    private void LoadData_InGameResumeData()
    {
        // 로드 함수 내용

        // Ingameresumedata 로드
        var fb_Ingameresumedata = flatBufferUserData.Ingameresumedata;
        if (fb_Ingameresumedata.HasValue)
        {
            Ingameresumedata.Isvalid = fb_Ingameresumedata.Value.Isvalid;
            Ingameresumedata.Stageidx = fb_Ingameresumedata.Value.Stageidx;
            Ingameresumedata.Waveidx = fb_Ingameresumedata.Value.Waveidx;
            Ingameresumedata.Silvercoin = fb_Ingameresumedata.Value.Silvercoin;
            Ingameresumedata.Hp = fb_Ingameresumedata.Value.Hp;
            Ingameresumedata.Starthp = fb_Ingameresumedata.Value.Starthp;
            Ingameresumedata.Shield = fb_Ingameresumedata.Value.Shield;
            Ingameresumedata.Killcount = fb_Ingameresumedata.Value.Killcount;
            Ingameresumedata.Ingameexp = fb_Ingameresumedata.Value.Ingameexp;
            Ingameresumedata.Upgradecount = fb_Ingameresumedata.Value.Upgradecount;
            Ingameresumedata.Ingamemoney = fb_Ingameresumedata.Value.Ingamemoney;
            Ingameresumedata.Rerollcount = fb_Ingameresumedata.Value.Rerollcount;

            // Tiles 로드
            Ingameresumedata.Tiles.Clear();
            int tilesLength = fb_Ingameresumedata.Value.TilesLength;
            for (int j = 0; j < tilesLength; j++)
            {
                var fbTilesItem = fb_Ingameresumedata.Value.Tiles(j);
                if (fbTilesItem.HasValue)
                {
                    var nested_item = new InGameResumeTileData
                    {
                        Equipidx = fbTilesItem.Value.Equipidx,
                        Grade = fbTilesItem.Value.Grade,
                        Tileorder = fbTilesItem.Value.Tileorder,
                        Isad = fbTilesItem.Value.Isad
                    };
                    Ingameresumedata.Tiles.Add(nested_item);
                }
            }

            // Unlockedtiles 리스트 로드
            Ingameresumedata.Unlockedtiles.Clear();
            for (int j = 0; j < fb_Ingameresumedata.Value.UnlockedtilesLength; j++)
            {
                int unlockedtiles_val = fb_Ingameresumedata.Value.Unlockedtiles(j);
                Ingameresumedata.Unlockedtiles.Add(unlockedtiles_val);
            }

            // Upgradeindices 리스트 로드
            Ingameresumedata.Upgradeindices.Clear();
            for (int j = 0; j < fb_Ingameresumedata.Value.UpgradeindicesLength; j++)
            {
                int upgradeindices_val = fb_Ingameresumedata.Value.Upgradeindices(j);
                Ingameresumedata.Upgradeindices.Add(upgradeindices_val);
            }
        }
    }

}

public class InGameResumeData
{
    public bool Isvalid { get; set; } = false;
    public int Stageidx { get; set; } = 0;
    public int Waveidx { get; set; } = 0;
    public int Silvercoin { get; set; } = 0;
    public int Hp { get; set; } = 0;
    public int Starthp { get; set; } = 0;
    public int Shield { get; set; } = 0;
    public int Killcount { get; set; } = 0;
    public int Ingameexp { get; set; } = 0;
    public int Upgradecount { get; set; } = 1;
    public int Ingamemoney { get; set; } = 0;
    public int Rerollcount { get; set; } = 0;
    public List<InGameResumeTileData> Tiles = new List<InGameResumeTileData>();
    public List<int> Unlockedtiles = new List<int>();
    public List<int> Upgradeindices = new List<int>();

    public void CopyFrom(InGameResumeData other)
    {
        Isvalid = other.Isvalid;
        Stageidx = other.Stageidx;
        Waveidx = other.Waveidx;
        Silvercoin = other.Silvercoin;
        Hp = other.Hp;
        Starthp = other.Starthp;
        Shield = other.Shield;
        Killcount = other.Killcount;
        Ingameexp = other.Ingameexp;
        Upgradecount = other.Upgradecount;
        Ingamemoney = other.Ingamemoney;
        Rerollcount = other.Rerollcount;
        Tiles = other.Tiles.Select(t => new InGameResumeTileData
        {
            Equipidx = t.Equipidx,
            Grade = t.Grade,
            Tileorder = t.Tileorder,
            Isad = t.Isad
        }).ToList();
        Unlockedtiles = new List<int>(other.Unlockedtiles);
        Upgradeindices = new List<int>(other.Upgradeindices);
    }
}
