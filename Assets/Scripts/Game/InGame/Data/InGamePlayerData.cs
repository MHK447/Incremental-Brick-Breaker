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

    public IReactiveProperty<int> WeaponFallCountProperty { get; private set; } = new ReactiveProperty<int>(1);

    public IReactiveProperty<int> FallWeaponIdxProperty { get; private set; } = new ReactiveProperty<int>(1);

    /// <summary> 낙하 무기 쿨타임 진행도 0~1 (0=쿨 시작, 1=쿨 완료). fillAmount = 1 - 이 값 </summary>
    public IReactiveProperty<float> WeaponFallCooldownProgressProperty { get; private set; } = new ReactiveProperty<float>(1f);

    public int WeaponFallStartCount = 1;
}

