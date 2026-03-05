using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class EnemyBlockSpawner : MonoBehaviour
{

    public int Hp { get; private set; } = 0;

    public bool IsDead { get { return Hp <= 0; } }

    public virtual void Damage(int damage)
    {
        
    }


    public void ClearData()
    {
        
    }
}

