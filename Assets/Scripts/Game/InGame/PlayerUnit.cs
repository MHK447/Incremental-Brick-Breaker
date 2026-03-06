using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;


public class PlayerUnit : MonoBehaviour
{
    [SerializeField]
    private WeaponController WeaponController;

    public void Init()
    {
        WeaponController.Init();
    }

    public void Damage(int damage)
    {
        
    }
    
}

