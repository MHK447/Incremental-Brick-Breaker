using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

[EffectPath("Effect/DoubleShotEffect", false, true)]
public class DoubleShotEffect : Effect
{

    [SerializeField]
    private Animator Anim;


    public void Init()
    {
        Anim.Play("DoubleShot", 0, 0f);
    }
}

