using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class Weapon_Base : MonoBehaviour
{  
    protected WeaponData WeaponData;

    public virtual void Set(WeaponData weaponData)
    {
        WeaponData = weaponData;

    }
}

