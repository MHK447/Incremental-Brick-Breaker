using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class WeaponData
{

    public int WeaponIdx = 0;

    public float WeaponCoolTime = 0f;

    public int WeaponDamage = 0;

    public float WeaponRange = 0f;

    public float WeaponDeltime = 0f;



    public void SetWeaponData(int weaponIdx, float weaponCoolTime, int weaponDamage, float weaponRange)
    {
        WeaponIdx = weaponIdx;
        WeaponCoolTime = weaponCoolTime;
        WeaponDamage = weaponDamage;
        WeaponRange = weaponRange;
        WeaponDeltime = 0f;

        Init();
    } 


    public virtual void Init()
    {

    }

    
}

