using System;
using System.Collections.Generic;
using UniRx;
using Google.FlatBuffers;

public partial class UserDataSystem
{



}

public class HeroEquipItemData
{
    public HeroItemData Heroitemdata = new HeroItemData();

    public event Action<HeroItemData> OnHeroItemDataChanged;

    public int Heroitemtype { get; set; } = 0;

    public int Heroitemidx { get; set; } = 0;
    public IReactiveProperty<bool> Isequip { get; set; } = new ReactiveProperty<bool>(false);
    public IReactiveProperty<int> Level { get; set; } = new ReactiveProperty<int>(0);
    

}
