using UnityEngine;
using BanpoFri;


[EffectPath("Effect/SparkEffect", true)]
public class SparkEffect : Effect
{
    [SerializeField]
    private ParticleSystem ParticleSystem;

    public void SetColor(Color color)
    {
        var main = ParticleSystem.main;
        main.startColor = color;
    }


}
