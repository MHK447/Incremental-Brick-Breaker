using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UniRx;



public class InGamePlayerData
{
    public IReactiveProperty<bool> IsDeadProperty { get; private set; } = new ReactiveProperty<bool>(false);
}

