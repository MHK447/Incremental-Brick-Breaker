using UnityEngine;
using BanpoFri;

[EffectPath("Effect/ZombieBloodHit_Effect", false, false)]
public class ZombieBloodHit_Effect : Effect
{
    private void OnEnable()
    {
        // Prefab setup misses particle references in some cases.
        // Populate once from children so Play() can always run.
        if (particles.Count == 0)
        {
            particles.AddRange(GetComponentsInChildren<ParticleSystem>(true));
        }

        Play();
    }
}

