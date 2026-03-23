using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;

public class WeaponController : MonoBehaviour
{
    protected List<Weapon_Base> EquipWeaponList = new List<Weapon_Base>();

    [SerializeField]
    private List<Transform> WeaponEquipTrList = new List<Transform>();

    public void Init()
    {
        EquipWeaponList.Clear();
    }



    public void AddWeapon(int weaponIdx)
    {
        var td = Tables.Instance.GetTable<WeaponInfo>().GetData(weaponIdx);

        if (td != null)
        {
            Addressables.InstantiateAsync(td.prefab).Completed += (handle) =>
            {
                var weaponData = new WeaponData();
                weaponData.WeaponIdx = weaponIdx;
                weaponData.WeaponDamage = td.base_dmg;
                weaponData.WeaponCoolTime = td.attack_cooltime / 100f;
                weaponData.WeaponRange = td.attack_range / 100f;

                var weapon = handle.Result.GetComponent<Weapon_Base>();

                weapon.transform.SetParent(transform);

                weapon.Set(weaponData);
                EquipWeaponList.Add(weapon);

                weapon.transform.position = WeaponEquipTrList[weaponData.WeaponIdx - 1].position;
            };
        }
    }

    protected virtual void OnBulletHit(BulletBase bullet)
    {
        ProjectUtility.SetActiveCheck(bullet.gameObject, false);
    }


    public bool HasWeapon(int weaponIdx)
    {
        return EquipWeaponList.Any(w => w != null && w.GetWeaponData != null && w.GetWeaponData.WeaponIdx == weaponIdx);
    }

    public void RemoveWeapon(Weapon_Base weapon)
    {
        EquipWeaponList.Remove(weapon);
    }


}

