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

    /// <summary> 인크리멘탈 공격력 배율 </summary>
    public float IncreaDamageMultiplier = 1f;

    /// <summary> 인크리멘탈 공격속도 배율 (높을수록 빠름) </summary>
    public float IncreaAttackSpeedMultiplier = 1f;

    /// <summary> 낙하 탄환 관통 횟수 </summary>
    public int FallPenetrationCount = 0;

    /// <summary> 잼 추가 드랍 확률 </summary>
    public float BonusGemDropRate = 0f;

    /// <summary> 코인 추가 드랍 확률 </summary>
    public float BonusCoinDropRate = 0f;

    /// <summary> 코인 등장 추가 확률 </summary>
    public float BonusCoinSpawnRate = 0f;

    public const int FallWeaponIdx_Default = 101;
    public const int FallWeaponIdx_Bomb = 102;

    /// <summary> 인크리멘탈 보너스 적용 </summary>
    public void ApplyIncreaMentalBonuses()
    {
        var system = GameRoot.Instance.IncreaMentalSystem;

        IncreaDamageMultiplier = system.GetAttackDamageMultiplier();
        IncreaAttackSpeedMultiplier = system.GetAttackSpeedMultiplier();

        int bonusFallCount = system.GetBonusFallCount();
        int prevStartCount = WeaponFallStartCount;
        WeaponFallStartCount = 2 + bonusFallCount;

        int diff = WeaponFallStartCount - prevStartCount;
        if (diff > 0)
        {
            WeaponFallCountProperty.Value += diff;
        }

        FallPenetrationCount = system.GetFallPenetrationCount();
        BonusGemDropRate = system.GetGemDropBonusRate();
        BonusCoinDropRate = system.GetCoinDropBonusRate();
        BonusCoinSpawnRate = system.GetCoinSpawnBonusRate();

        FallWeaponIdxProperty.Value = system.IsBombUpgraded() ? FallWeaponIdx_Bomb : FallWeaponIdx_Default;
    }

    /// <summary> 초기 세팅용 (스테이지 시작 시) </summary>
    public void InitIncreaMentalBonuses()
    {
        var system = GameRoot.Instance.IncreaMentalSystem;

        IncreaDamageMultiplier = system.GetAttackDamageMultiplier();
        IncreaAttackSpeedMultiplier = system.GetAttackSpeedMultiplier();

        int bonusFallCount = system.GetBonusFallCount();
        WeaponFallStartCount = 2 + bonusFallCount;
        WeaponFallCountProperty.Value = WeaponFallStartCount;

        FallPenetrationCount = system.GetFallPenetrationCount();
        BonusGemDropRate = system.GetGemDropBonusRate();
        BonusCoinDropRate = system.GetCoinDropBonusRate();
        BonusCoinSpawnRate = system.GetCoinSpawnBonusRate();

        FallWeaponIdxProperty.Value = system.IsBombUpgraded() ? FallWeaponIdx_Bomb : FallWeaponIdx_Default;
    }
}

