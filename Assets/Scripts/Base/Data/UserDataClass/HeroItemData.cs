using System;
using System.Collections.Generic;
using UniRx;
using Google.FlatBuffers;

public partial class UserDataSystem
{



}

public class HeroItemData
{
    public IReactiveProperty<bool> Isequip { get; set; } = new ReactiveProperty<bool>(false);

    public int Heroitemidx { get; set; } = 0;
    public int Grade { get; set; } = 0;

}
