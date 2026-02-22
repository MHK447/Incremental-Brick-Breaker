using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class BulletInfo
{

    public double AttackDamage = 0;
    public double AttackSpeed = 0;
    public double AttackRange = 0;
    public double AttackDelay = 0;
    public double AttackAngle = 0;
    public double AttackForce = 0;
    public double AttackRadius = 0;
    public double AttackDuration = 0;
    public double AttackCooldown = 0;
    public int PenetrationCount = 0; // 관통 횟수
    public int BounceCount = 0; // 튕김 횟수
    public float BounceSpeedMultiplier = 1.0f; // 튕김 시 속도 배율
}

