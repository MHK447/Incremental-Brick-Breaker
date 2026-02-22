using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleNoneTimeScaleComponent : MonoBehaviour
{
    [SerializeField]
    private List<ParticleSystem> particlelist = new List<ParticleSystem>();





    private void OnEnable()
    {
        foreach(var particle in particlelist)
        {
            var main = particle.main;
            main.simulationSpeed = 1 / Time.timeScale;
        }
    }
}
