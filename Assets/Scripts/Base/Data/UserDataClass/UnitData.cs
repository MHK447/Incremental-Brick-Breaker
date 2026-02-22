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

}
public class UnitData
{
    public int Unitidx { get; set; } = 0;

    private int _unitlevel = 0;
    public int Unitlevel
    {
        get => _unitlevel;
        set
        {
            _unitlevel = value;
            UnitlevelProperty.Value = value;
        }
    }

    public IReactiveProperty<int> UnitlevelProperty { get; set; } = new ReactiveProperty<int>(0);

    private System.IDisposable levelSubscription;

    public UnitData()
    {
        levelSubscription = UnitlevelProperty.Subscribe(x => _unitlevel = x);
    }
}