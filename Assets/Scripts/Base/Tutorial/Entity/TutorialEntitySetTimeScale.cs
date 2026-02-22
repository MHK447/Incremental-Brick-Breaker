using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class TutorialEntitySetTimeScale : TutorialEntity
{
    [SerializeField]
    private float timeScale = 1f;

    public override void StartEntity()
    {
        base.StartEntity();

        Time.timeScale = timeScale;

        Done();
    }
}

