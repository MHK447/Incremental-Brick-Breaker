using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

[EffectPath("Effect/LuckyEffect", true, false)]
public class LuckyEffect : Effect
{
    [SerializeField]
    private Animator LuckyEffectAnim;


    public void Init()
    {
        LuckyEffectAnim.Play("DoubleShot", 0, 0f);
    }
}

