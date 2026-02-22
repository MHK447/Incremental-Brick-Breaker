using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlaySoundEnable : MonoBehaviour
{
    [HideInInspector]
    [SerializeField]
    private string keySound = string.Empty;
    [SerializeField]
    private bool BGM = false;
    [SerializeField]
    private float delay = 0f;

    [SerializeField]
    private bool IsMain = false;

   private void OnEnable() {
       Sequence seq = DOTween.Sequence();
        seq.SetDelay(delay);
        seq.AppendCallback(() =>
       {
           if (BGM)
           {

               if ("bgm" == keySound)
               {

                //    if (towerpopup != null && towerpopup.gameObject.activeSelf)
                //    {
                //        SoundPlayer.Instance.PlayBGM("bg_tower");
                //    }
                //    else
                //    {
                       SoundPlayer.Instance.PlayBGM("bgm");
                   //}
               }
               else
                   SoundPlayer.Instance.PlayBGM(keySound);
           }
           else
               SoundPlayer.Instance.PlaySound(keySound);
       });
        
    }

    private void OnDisable(){
        if (BGM)
        {
            SoundPlayer.Instance.RecoveryBGM();

            if(IsMain)
            {
                SoundPlayer.Instance.PlayBGM("bgm");
            }
        }
    }
}
