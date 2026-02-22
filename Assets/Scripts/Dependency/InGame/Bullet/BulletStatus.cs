using System;

[Flags]
public enum AttackEffect
{
    None = 0,
    Fire = 1 << 1,
    Poison = 1 << 2,
    Ice = 1 << 3,
}


public struct BulletStatus
{

    //damage
    public double Damage;
    public double CritDamageMultiplier;
    public double CritChance; // 0~1 단위
    public AttackEffect EffectFlags;

    //bullet data
    public float ProjectileSpeed;

    public float Value1;
    public float Value2;


    public bool HasFlag(AttackEffect effect)
    {
        return (EffectFlags & effect) > 0;
    }

    public BulletStatus WithDamage(double damage)
    {
        this.Damage = damage;
        return this;
    }
}