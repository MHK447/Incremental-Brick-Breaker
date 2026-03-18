using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public enum IncreaMentalType
{
    AttackDamageUp = 1,
    FallWeaponSpeedUp = 2,
    TruckUnlock = 3,
    FallCountUp = 4,
    FallBulletPenetrationUp = 5,
    GemDropRateUp = 6,
    CoinDropRateUp = 7,
    CoinValueUp = 8,
    WeaponCooldownDown = 9,
    GearWeaponUnlock = 10,
    GearCountUp = 11,
    MachineGunUnlock = 12,
    TruckUpgradeUnlock = 13,
    BombUpgrade = 14,
    FlamethrowerUnlock = 15,
    DefenseGuardUnlock = 16,
    HealthUp = 17,
    HealthRegenUp = 18,
}

public class IncreaMentalSystem
{
    public List<int> UpgradeUnLockOrderList = new List<int>();

    private IncreaUpgradeOrderData FindOrderDataByIncreaseIdx(int increaseIdx)
    {
        return Tables.Instance.GetTable<IncreaUpgradeOrder>().DataList.Find(x => x.increase_idx == increaseIdx);
    }

    private int GetUpgradeValue(IncreaMentalType type)
    {
        int idx = (int)type;
        var allOrders = Tables.Instance.GetTable<IncreaUpgradeOrder>().DataList.FindAll(x => x.increase_idx == idx);
        if (allOrders == null || allOrders.Count == 0) return 0;

        int totalValue = 0;
        foreach (var td in allOrders)
        {
            if (td.upgrade_value == null || td.upgrade_value.Count == 0) continue;
            var finddata = FindData(td.order);
            int level = finddata != null ? finddata.Level.Value : 0;
            if (level <= 0) continue;

            int valueIdx = Mathf.Clamp(level - 1, 0, td.upgrade_value.Count - 1);
            totalValue += td.upgrade_value[valueIdx];
        }
        return totalValue;
    }
    
    public void Create()
    {
        UpgradeUnLockOrderList.Clear();
    
        var tdlist = Tables.Instance.GetTable<IncreaUpgradeOrder>().DataList;

        if (tdlist.Count != GameRoot.Instance.UserData.Increaseugprades.Count)
        {
            foreach (var td in tdlist)
            {
                var finddata = GameRoot.Instance.UserData.Increaseugprades.Find(x => x.Order == td.order);

                if (finddata != null) continue;

                var newdata = new InCreaseUpgradeData();
                newdata.Order = td.order;
                newdata.Level.Value = 0;
                GameRoot.Instance.UserData.Increaseugprades.Add(newdata);

                if(td.order == 2)
                {
                    UpgradeUnLockOrderList.Add(td.order);
                }
            }
        }

        foreach (var data in GameRoot.Instance.UserData.Increaseugprades)
        {
            if (data.Level.Value == 0) continue;

            var td = Tables.Instance.GetTable<IncreaUpgradeOrder>().GetData(data.Order);
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

    public void IncreaseLevelUp(int order)
    {
        var finddata = FindData(order);
        if (finddata != null)
        {
            finddata.Level.Value++;

            var td = Tables.Instance.GetTable<IncreaUpgradeOrder>().GetData(order);
            if (td != null)
            {
                if (!UpgradeUnLockOrderList.Contains(td.order))
                {
                    UpgradeUnLockOrderList.Add(td.order);
                }

                foreach (var openOrder in td.open_order)
                {
                    if (openOrder > 0 && !UpgradeUnLockOrderList.Contains(openOrder))
                    {
                        UpgradeUnLockOrderList.Add(openOrder);
                    }
                }

                ApplyImmediateEffect((IncreaMentalType)td.increase_idx);
            }

            RefreshPlayerBonuses();
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
            case IncreaMentalType.TruckUnlock:
                ApplyTruckUnlock(inGame);
                break;

            case IncreaMentalType.GearWeaponUnlock:
                if (weaponController != null && !weaponController.HasWeapon((int)WeaponSystem.WeaponType.Blade))
                    weaponController.AddWeapon((int)WeaponSystem.WeaponType.Blade);
                break;

            case IncreaMentalType.GearCountUp:
                if (weaponController != null)
                    weaponController.AddWeapon((int)WeaponSystem.WeaponType.Blade);
                break;

            case IncreaMentalType.MachineGunUnlock:
                // if (weaponController != null && !weaponController.HasWeapon((int)WeaponSystem.WeaponType.MachineGun))
                //     weaponController.AddWeapon((int)WeaponSystem.WeaponType.MachineGun);
                break;

            case IncreaMentalType.FlamethrowerUnlock:
                // if (weaponController != null && !weaponController.HasWeapon((int)WeaponSystem.WeaponType.Flamethrower))
                //     weaponController.AddWeapon((int)WeaponSystem.WeaponType.Flamethrower);
                break;
        }
    }

    private void ApplyTruckUnlock(InGameBase inGame)
    {
        var stage = inGame.Stage;

        stage.StopPersistentSpawn();

        var enemyGroup = stage.EnemyUnitGroup;
        enemyGroup.ClearActiveUnitsForResume();

        var player = stage.Player;
        if (player != null)
        {
            ProjectUtility.SetActiveCheck(player.gameObject, true);
            player.Init();
        }

        stage.StartCoroutine(stage.EnemySpawnStart());

        var popup = GameRoot.Instance.UISystem.GetUI<PopupInGame>();
        if (popup != null)
        {
            popup.RefreshTruckUI();
        }
    }

    public InCreaseUpgradeData FindData(int idx)
    {
        return GameRoot.Instance.UserData.Increaseugprades.Find(x => x.Order == idx);
    }

    public int GetUpgradeLevel(IncreaMentalType type)
    {
        int idx = (int)type;
        var allOrders = Tables.Instance.GetTable<IncreaUpgradeOrder>().DataList.FindAll(x => x.increase_idx == idx);
        if (allOrders == null || allOrders.Count == 0) return 0;

        int totalLevel = 0;
        foreach (var td in allOrders)
        {
            var data = FindData(td.order);
            if (data != null) totalLevel += data.Level.Value;
        }
        return totalLevel;
    }

    public bool IsUnlocked(IncreaMentalType type)
    {
        return GetUpgradeLevel(type) > 0;
    }

    /// <summary> 공격력 추가 보너스 (플러스 증가) </summary>
    public int GetAttackDamageBonus()
    {
        return GetUpgradeValue(IncreaMentalType.AttackDamageUp);
    }

    /// <summary> Fall 무기 속도 배율 (1.0 = 기본, 퍼센트 증가) </summary>
    public float GetFallWeaponSpeedMultiplier()
    {
        return 1f + GetUpgradeValue(IncreaMentalType.FallWeaponSpeedUp) * 0.001f;
    }

    /// <summary> 추가 낙하 무기 개수 (플러스 증가) </summary>
    public int GetBonusFallCount()
    {
        return GetUpgradeValue(IncreaMentalType.FallCountUp);
    }

    /// <summary> 낙하 탄환 관통 횟수 (플러스 증가, 0 = 관통 없음) </summary>
    public int GetFallPenetrationCount()
    {
        return GetUpgradeValue(IncreaMentalType.FallBulletPenetrationUp);
    }

    /// <summary> 잼(보석) 드랍 추가 확률 (플러스 증가, 0.0 ~ 1.0) </summary>
    public float GetGemDropBonusRate()
    {
        return GetUpgradeValue(IncreaMentalType.GemDropRateUp) * 0.001f;
    }

    /// <summary> 코인 추가 드랍 개수 (버프 값만큼 추가 드랍) </summary>
    public int GetBonusCoinDropCount()
    {
        return GetUpgradeValue(IncreaMentalType.CoinDropRateUp);
    }

    /// <summary> 코인 얻는 벨류 추가 보너스 (플러스 증가) </summary>
    public int GetCoinValueBonus()
    {
        return GetUpgradeValue(IncreaMentalType.CoinValueUp);
    }

    /// <summary> 무기 쿨타임 감소 배율 (1.0 = 기본, 높을수록 쿨타임 짧음) </summary>
    public float GetWeaponCooldownMultiplier()
    {
        return 1f + GetUpgradeValue(IncreaMentalType.WeaponCooldownDown) * 0.001f;
    }

    /// <summary> 톱니바퀴 추가 개수 (플러스 증가) </summary>
    public int GetBonusGearCount()
    {
        return GetUpgradeValue(IncreaMentalType.GearCountUp);
    }

    /// <summary> 체력 추가 보너스 (플러스 증가) </summary>
    public int GetBonusHealth()
    {
        return GetUpgradeValue(IncreaMentalType.HealthUp);
    }

    /// <summary> 체력 회복 추가 보너스 (플러스 증가) </summary>
    public int GetHealthRegenBonus()
    {
        return GetUpgradeValue(IncreaMentalType.HealthRegenUp);
    }

    public bool IsTruckUnlocked()
    {
        return IsUnlocked(IncreaMentalType.TruckUnlock);
    }

    public bool IsTruckUpgradeUnlocked()
    {
        return IsUnlocked(IncreaMentalType.TruckUpgradeUnlock);
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

    public bool IsFlamethrowerUnlocked()
    {
        return IsUnlocked(IncreaMentalType.FlamethrowerUnlock);
    }

    public bool IsDefenseGuardUnlocked()
    {
        return IsUnlocked(IncreaMentalType.DefenseGuardUnlock);
    }
}

