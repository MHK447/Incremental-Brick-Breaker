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

    /// <summary> 인크리멘탈 공격력 추가 보너스 </summary>
    public int IncreaDamageBonus = 0;

    /// <summary> Fall 무기 속도 배율 (높을수록 빠름) </summary>
    public float FallWeaponSpeedMultiplier = 1f;

    /// <summary> 낙하 탄환 관통 횟수 </summary>
    public int FallPenetrationCount = 0;

    /// <summary> 잼 추가 드랍 확률 </summary>
    public float BonusGemDropRate = 0f;

    /// <summary> 코인 추가 드랍 개수 </summary>
    public int BonusCoinDropCount = 0;

    /// <summary> 코인 얻는 벨류 추가 보너스 </summary>
    public int CoinValueBonus = 0;

    /// <summary> 무기 쿨타임 감소 배율 (높을수록 쿨타임 짧음) </summary>
    public float WeaponCooldownMultiplier = 1f;

    /// <summary> 체력 추가 보너스 </summary>
    public int BonusHealth = 0;

    /// <summary> 체력 회복 추가 보너스 </summary>
    public int HealthRegenBonus = 0;

    public const int FallWeaponIdx_Default = 101;
    public const int FallWeaponIdx_Bomb = 102;

    /// <summary> 인크리멘탈 보너스 적용 </summary>
    public void ApplyIncreaMentalBonuses()
    {
        var system = GameRoot.Instance.IncreaMentalSystem;

        IncreaDamageBonus = system.GetAttackDamageBonus();
        FallWeaponSpeedMultiplier = system.GetFallWeaponSpeedMultiplier();
        WeaponCooldownMultiplier = system.GetWeaponCooldownMultiplier();

        int bonusFallCount = system.GetBonusFallCount();
        int prevStartCount = WeaponFallStartCount;
        WeaponFallStartCount = 1 + bonusFallCount;

        int diff = WeaponFallStartCount - prevStartCount;
        if (diff > 0)
        {
            WeaponFallCountProperty.Value += diff;
        }

        FallPenetrationCount = system.GetFallPenetrationCount();
        BonusGemDropRate = system.GetGemDropBonusRate();
        BonusCoinDropCount = system.GetBonusCoinDropCount();
        CoinValueBonus = system.GetCoinValueBonus();
        BonusHealth = system.GetBonusHealth();
        HealthRegenBonus = system.GetHealthRegenBonus();

        FallWeaponIdxProperty.Value = system.IsBombUpgraded() ? FallWeaponIdx_Bomb : FallWeaponIdx_Default;
    }

    /// <summary> 초기 세팅용 (스테이지 시작 시) </summary>
    public void InitIncreaMentalBonuses()
    {
        var system = GameRoot.Instance.IncreaMentalSystem;

        IncreaDamageBonus = system.GetAttackDamageBonus();
        FallWeaponSpeedMultiplier = system.GetFallWeaponSpeedMultiplier();
        WeaponCooldownMultiplier = system.GetWeaponCooldownMultiplier();

        int bonusFallCount = system.GetBonusFallCount();
        WeaponFallStartCount = 1 + bonusFallCount;
        WeaponFallCountProperty.Value = WeaponFallStartCount;

        FallPenetrationCount = system.GetFallPenetrationCount();
        BonusGemDropRate = system.GetGemDropBonusRate();
        BonusCoinDropCount = system.GetBonusCoinDropCount();
        CoinValueBonus = system.GetCoinValueBonus();
        BonusHealth = system.GetBonusHealth();
        HealthRegenBonus = system.GetHealthRegenBonus();

        FallWeaponIdxProperty.Value = system.IsBombUpgraded() ? FallWeaponIdx_Bomb : FallWeaponIdx_Default;
    }
}

