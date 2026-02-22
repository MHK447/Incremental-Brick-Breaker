using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BanpoFri;

public class TutorialEntityWaitSpeed : TutorialEntity
{
    public override void StartEntity()
    {
        base.StartEntity();

        GameRoot.Instance.StartCoroutine(SwapCheck());
    }


    public IEnumerator SwapCheck()
    {
        yield return new WaitUntil(() => Time.timeScale >= 1f);

        Done();
    }
}

