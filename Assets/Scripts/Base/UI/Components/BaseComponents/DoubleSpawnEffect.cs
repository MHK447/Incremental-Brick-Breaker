using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

[EffectPath("Effect/DoubleSpawnEffect", false, true)]
public class DoubleSpawnEffect : Effect
{
    [SerializeField]
    private Animator DoubleSpawnAnim;


    public void Init()
    {
        DoubleSpawnAnim.Play("MultiShot", 0, 0f);
    }
}

