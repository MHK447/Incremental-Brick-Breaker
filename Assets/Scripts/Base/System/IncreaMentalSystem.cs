using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public enum IncreaMentalType
{
    AttackDamageUp = 1,
    AttackSpeedUp = 2,
    TruckUnlock1 = 3,
    FallCountUp = 4,
    FallBulletPenetrationUp = 5,
    GemDropRateUp = 6,
    CoinDropRateUp = 7,
    TruckUnlock2 = 8,
    CoinSpawnRateUp = 9,
    TruckUpgradeUnlock = 10,
    AttackSpeedUp2 = 11,
    GearWeaponUnlock = 12,
    GearCountUp = 13,
    MachineGunUnlock = 14,
    TruckUpgradeUnlock2 = 15,
    BombUpgrade = 16,
}

public class IncreaMentalSystem
{
    public List<int> UpgradeUnLockOrderList = new List<int>();

    private int GetUpgradeValue(IncreaMentalType type)
    {
        int idx = (int)type;
        var td = Tables.Instance.GetTable<IncreaseUpgradeOrder>().GetData(idx);
        if (td == null || td.upgrade_value == null || td.upgrade_value.Count == 0) return 0;

        var finddata = FindData(idx);
        int level = finddata != null ? finddata.Level.Value : 0;
        if (level <= 0) return 0;

        int valueIdx = Mathf.Clamp(level - 1, 0, td.upgrade_value.Count - 1);
        return td.upgrade_value[valueIdx];
    }

    public void Create()
    {
        var tdlist = Tables.Instance.GetTable<IncreaseUpgradeOrder>().DataList;

        if (tdlist.Count != GameRoot.Instance.UserData.Increaseugprades.Count)
        {
            foreach (var td in tdlist)
            {
                var finddata = GameRoot.Instance.UserData.Increaseugprades.Find(x => x.Idx == td.increase_idx);

                if (finddata != null) continue;

                var newdata = new InCreaseUpgradeData();
                newdata.Idx = td.increase_idx;
                newdata.Level.Value = 0;
                GameRoot.Instance.UserData.Increaseugprades.Add(newdata);
            }
        }

        UpgradeUnLockOrderList.Clear();

        foreach (var data in GameRoot.Instance.UserData.Increaseugprades)
        {
            if (data.Level.Value == 0) continue;

            var td = Tables.Instance.GetTable<IncreaseUpgradeOrder>().GetData(data.Idx);
            if (td != null)
            {
                foreach (var order in td.open_order)
                {
                    if (order > 0)
                    {
                        UpgradeUnLockOrderList.Add(order);
                    }
                }
            }
        }

        RefreshPlayerBonuses();
    }

    public void IncreaseLevelUp(int idx)
    {
        var finddata = FindData(idx);
        if (finddata != null)
        {
            finddata.Level.Value++;

            if (!UpgradeUnLockOrderList.Contains(idx))
            {
                UpgradeUnLockOrderList.Add(idx);
            }

            var td = Tables.Instance.GetTable<IncreaseUpgradeOrder>().GetData(idx);
            if (td != null)
            {
                foreach (var order in td.open_order)
                {
                    if (order > 0 && !UpgradeUnLockOrderList.Contains(order))
                    {
                        UpgradeUnLockOrderList.Add(order);
                    }
                }
            }

            RefreshPlayerBonuses();
            ApplyImmediateEffect((IncreaMentalType)idx);
        }
    }

    public void RefreshPlayerBonuses()
    {
        if (GameRoot.Instance == null || GameRoot.Instance.UserData == null) return;
        GameRoot.Instance.UserData.InGamePlayerData.ApplyIncreaMentalBonuses();
    }

    private void ApplyImmediateEffect(IncreaMentalType type)
    {
        var inGame = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>();
        if (inGame == null || inGame.Stage == null) return;

        var weaponController = inGame.Stage.Player?.GetWeaponController;

        switch (type)
        {
            case IncreaMentalType.GearWeaponUnlock:
                if (weaponController != null && !weaponController.HasWeapon((int)WeaponSystem.WeaponType.GearWeapon))
                    weaponController.AddWeapon((int)WeaponSystem.WeaponType.GearWeapon);
                break;

            case IncreaMentalType.GearCountUp:
                if (weaponController != null)
                    weaponController.AddWeapon((int)WeaponSystem.WeaponType.GearWeapon);
                break;

            case IncreaMentalType.MachineGunUnlock:
                if (weaponController != null && !weaponController.HasWeapon((int)WeaponSystem.WeaponType.MachineGun))
                    weaponController.AddWeapon((int)WeaponSystem.WeaponType.MachineGun);
                break;
        }
    }

    public InCreaseUpgradeData FindData(int idx)
    {
        return GameRoot.Instance.UserData.Increaseugprades.Find(x => x.Idx == idx);
    }

    public int GetUpgradeLevel(IncreaMentalType type)
    {
        var data = FindData((int)type);
        return data != null ? data.Level.Value : 0;
    }

    public bool IsUnlocked(IncreaMentalType type)
    {
        return GetUpgradeLevel(type) > 0;
    }

    /// <summary> 공격력 배율 (1.0 = 기본, 테이블 값은 퍼밀(‰) 단위) </summary>
    public float GetAttackDamageMultiplier()
    {
        return 1f + GetUpgradeValue(IncreaMentalType.AttackDamageUp) * 0.001f;
    }

    /// <summary> 공격속도 배율 (1.0 = 기본). 두 종류의 공격속도 증가를 합산 </summary>
    public float GetAttackSpeedMultiplier()
    {
        int val1 = GetUpgradeValue(IncreaMentalType.AttackSpeedUp);
        int val2 = GetUpgradeValue(IncreaMentalType.AttackSpeedUp2);
        return 1f + (val1 + val2) * 0.001f;
    }

    /// <summary> 추가 낙하 무기 개수 </summary>
    public int GetBonusFallCount()
    {
        return GetUpgradeValue(IncreaMentalType.FallCountUp);
    }

    /// <summary> 낙하 탄환 관통 횟수 (0 = 관통 없음) </summary>
    public int GetFallPenetrationCount()
    {
        return GetUpgradeValue(IncreaMentalType.FallBulletPenetrationUp);
    }

    /// <summary> 잼(보석) 드랍 추가 확률 (0.0 ~ 1.0) </summary>
    public float GetGemDropBonusRate()
    {
        return GetUpgradeValue(IncreaMentalType.GemDropRateUp) * 0.001f;
    }

    /// <summary> 코인 드랍 추가 확률 (0.0 ~ 1.0) </summary>
    public float GetCoinDropBonusRate()
    {
        return GetUpgradeValue(IncreaMentalType.CoinDropRateUp) * 0.001f;
    }

    /// <summary> 코인 등장 추가 확률 (0.0 ~ 1.0) </summary>
    public float GetCoinSpawnBonusRate()
    {
        return GetUpgradeValue(IncreaMentalType.CoinSpawnRateUp) * 0.001f;
    }

    /// <summary> 톱니바퀴 추가 개수 </summary>
    public int GetBonusGearCount()
    {
        return GetUpgradeValue(IncreaMentalType.GearCountUp);
    }

    public bool IsTruckUnlocked()
    {
        return IsUnlocked(IncreaMentalType.TruckUnlock1) || IsUnlocked(IncreaMentalType.TruckUnlock2);
    }

    public bool IsTruckUpgradeUnlocked()
    {
        return IsUnlocked(IncreaMentalType.TruckUpgradeUnlock) || IsUnlocked(IncreaMentalType.TruckUpgradeUnlock2);
    }

    public bool IsGearWeaponUnlocked()
    {
        return IsUnlocked(IncreaMentalType.GearWeaponUnlock);
    }

    public bool IsMachineGunUnlocked()
    {
        return IsUnlocked(IncreaMentalType.MachineGunUnlock);
    }

    public bool IsBombUpgraded()
    {
        return IsUnlocked(IncreaMentalType.BombUpgrade);
    }
}

