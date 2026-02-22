using System;
using System.Collections.Generic;
using UniRx;
using Google.FlatBuffers;

public partial class UserDataSystem
{
    public TrainingGroupData Traininggroupdata { get; private set; } = new TrainingGroupData();
    private void SaveData_TrainingGroupData(FlatBufferBuilder builder)
    {
        // 선언된 변수들은 모두 저장되어야함

        // Traininggroupdata 단일 저장
        // Traininggroupdata.Trainingdatas 처리 GenerateItemSaveCode IsCustom
        Offset<BanpoFri.Data.TrainingData>[] traininggroupdata_trainingdatas_Array = null;
        VectorOffset traininggroupdata_trainingdatas_Vector = default;

        if(Traininggroupdata.Trainingdatas.Count > 0){
            traininggroupdata_trainingdatas_Array = new Offset<BanpoFri.Data.TrainingData>[Traininggroupdata.Trainingdatas.Count];
            int traininggroupdata_trainingdatas_idx = 0;
            foreach(var traininggroupdata_trainingdatas_pair in Traininggroupdata.Trainingdatas){
                var traininggroupdata_trainingdatas_item = Traininggroupdata.Trainingdatas[traininggroupdata_trainingdatas_idx];
                traininggroupdata_trainingdatas_Array[traininggroupdata_trainingdatas_idx++] = BanpoFri.Data.TrainingData.CreateTrainingData(
                    builder,
                    traininggroupdata_trainingdatas_item.Trainingorder,
                    traininggroupdata_trainingdatas_item.Isupgradeproperty.Value,
                    traininggroupdata_trainingdatas_item.Specialisupgradeproperty.Value,
                    traininggroupdata_trainingdatas_item.Traininglevel
                );
            }
            traininggroupdata_trainingdatas_Vector = BanpoFri.Data.TrainingGroupData.CreateTrainingdatasVector(builder, traininggroupdata_trainingdatas_Array);
        }

        // Traininggroupdata 최종 생성 및 추가
        var traininggroupdata_Offset = BanpoFri.Data.TrainingGroupData.CreateTrainingGroupData(
            builder,
            traininggroupdata_trainingdatas_Vector,
            Traininggroupdata.Classlevel
        );


        Action cbAddDatas = () => {
            BanpoFri.Data.UserData.AddTraininggroupdata(builder, traininggroupdata_Offset);
        };

        cb_SaveAddDatas += cbAddDatas;

    }
    private void LoadData_TrainingGroupData()
    {
        // 로드 함수 내용

        // Traininggroupdata 로드
        var fb_Traininggroupdata = flatBufferUserData.Traininggroupdata;
        if (fb_Traininggroupdata.HasValue)
        {
            Traininggroupdata.Classlevel = fb_Traininggroupdata.Value.Classlevel;

            // Trainingdatas 로드
            Traininggroupdata.Trainingdatas.Clear();
            int trainingdatasLength = fb_Traininggroupdata.Value.TrainingdatasLength;
            for (int j = 0; j < trainingdatasLength; j++)
            {
                var fbTrainingdatasItem = fb_Traininggroupdata.Value.Trainingdatas(j);
                if (fbTrainingdatasItem.HasValue)
                {
                    var nested_item = new TrainingData
                    {
                        Trainingorder = fbTrainingdatasItem.Value.Trainingorder,
                        Isupgradeproperty = new ReactiveProperty<bool>(fbTrainingdatasItem.Value.Isupgradeproperty),
                        Specialisupgradeproperty = new ReactiveProperty<bool>(fbTrainingdatasItem.Value.Specialisupgradeproperty),
                        Traininglevel = fbTrainingdatasItem.Value.Traininglevel
                    };
                    Traininggroupdata.Trainingdatas.Add(nested_item);
                }
            }
        }
    }

}

public class TrainingGroupData
{
    public List<TrainingData> Trainingdatas = new List<TrainingData>();
    public int Classlevel { get; set; } = 0;

}
