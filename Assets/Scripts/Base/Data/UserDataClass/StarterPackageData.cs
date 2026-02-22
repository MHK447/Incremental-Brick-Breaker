using System;
using System.Collections.Generic;
using UniRx;
using Google.FlatBuffers;

public partial class UserDataSystem
{
    public StarterPackageData Starterpackdata { get; private set; } = new StarterPackageData();
    private void SaveData_StarterPackageData(FlatBufferBuilder builder)
    {
        // 선언된 변수들은 모두 저장되어야함

        // Starterpackdata 단일 저장
        // Starterpackdata 최종 생성 및 추가
        var starterpackdata_Offset = BanpoFri.Data.StarterPackageData.CreateStarterPackageData(
            builder,
            Starterpackdata.Isbuy.Value,
            Starterpackdata.Starterpackbuytime.Ticks
        );


        Action cbAddDatas = () => {
            BanpoFri.Data.UserData.AddStarterpackdata(builder, starterpackdata_Offset);
        };

        cb_SaveAddDatas += cbAddDatas;

    }
    private void LoadData_StarterPackageData()
    {
        // 로드 함수 내용

        // Starterpackdata 로드
        var fb_Starterpackdata = flatBufferUserData.Starterpackdata;
        if (fb_Starterpackdata.HasValue)
        {
            Starterpackdata.Isbuy.Value = fb_Starterpackdata.Value.Isbuy;
            Starterpackdata.Starterpackbuytime = new DateTime(fb_Starterpackdata.Value.Starterpackbuytime);

            // Starterpackbuytime DateTime 로드
            Starterpackdata.Starterpackbuytime = new DateTime(fb_Starterpackdata.Value.Starterpackbuytime);
        }
    }

}

public class StarterPackageData
{
    public IReactiveProperty<bool> Isbuy { get; set; } = new ReactiveProperty<bool>(false);
    public DateTime Starterpackbuytime { get; set; } = new DateTime();

    public IReactiveProperty<int> PackageTimeProperty { get; set; } = new ReactiveProperty<int>(0);

}
