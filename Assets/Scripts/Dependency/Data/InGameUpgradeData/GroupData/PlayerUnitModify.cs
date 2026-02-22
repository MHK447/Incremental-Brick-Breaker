using UnityEngine;

public struct PlayerStatGroup
{
    public static PlayerStatGroup GetDefault()
    {
        return new()
        {
            DamageMultiplier = 1,
            CooldownMultiplier = 1,
            CritDamageMultiplier = 0,
            CritChanceBonus = 0,
            rotspeedbonus = 0,
            movoespeedbonus = 0,
        };
    }
    public float DamageMultiplier;
    public float CooldownMultiplier;
    public float CritDamageMultiplier;
    public float CritChanceBonus;
    public float rotspeedbonus;
    public float movoespeedbonus;
}



public class PlayerUpgradeStateModifier
{
    public Config.InGameUpgradeChoice StatType;
    public float Value_1;

    public float Value_2;

    public PlayerStatGroup ModifyStat(PlayerStatGroup stats)
    {
        return stats;
    }
}