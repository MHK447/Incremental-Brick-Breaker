using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimActionComponent : MonoBehaviour
{
    public System.Action AnimAction_1;
    public System.Action<float> AnimAction_2;
    public System.Action<int> AnimAction_Start;
    public System.Action AnimAction_End;


    public void StartAction()
    {
        AnimAction_1?.Invoke();
    }

    public void StartAction_2()
    {
        AnimAction_2?.Invoke(1f);
    }

    public void StartAction_Start(int action)
    {
        AnimAction_2?.Invoke(action);
    }

    public void StartAction_End()
    {
        AnimAction_End?.Invoke();
    }
}
