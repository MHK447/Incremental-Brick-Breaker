using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine;
using UniRx;
using DG.Tweening;
using System.Linq;
using BanpoFri;
using UnityEngine.UI;
using System.Collections;
public partial class InGameBaseStage : MonoBehaviour
{
    

    public void Init()
    {
        //InitStage();
    }


    public void UnLoad()
    {
        SoundPlayer.Instance.Init();
    }




}
