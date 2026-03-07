using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UniRx;



public class InGamePlayerData
{
    public IReactiveProperty<bool> IsDeadProperty { get; private set; } = new ReactiveProperty<bool>(false);

    public IReactiveProperty<int> StartHpProperty { get; private set; } = new ReactiveProperty<int>(0);

    public IReactiveProperty<int> CurHppProperty { get; private set; } = new ReactiveProperty<int>(0);

    public IReactiveProperty<int> CriticalChanceProperty { get; private set; } = new ReactiveProperty<int>(0);

    public IReactiveProperty<int> CriticalDamageProperty { get; private set; } = new ReactiveProperty<int>(0);



}

