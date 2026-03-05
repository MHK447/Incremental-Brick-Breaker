using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class WeaponController : MonoBehaviour
{
    public List<WeaponData> WeaponDatList = new List<WeaponData>();

    [SerializeField]
    private Transform ShooterTr;

    [SerializeField]
    private List<Transform> WeaponEquipTrList = new List<Transform>();

    public void Init()
    {
        WeaponDatList.Clear();
    }



    public void AddWeapon(WeaponData weaponData)
    {
        WeaponDatList.Add(weaponData);
    }

    public void RemoveWeapon(WeaponData weaponData)
    {
        WeaponDatList.Remove(weaponData);
    }


}

