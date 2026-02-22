using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlaySoundDelay : MonoBehaviour
{
    public float delay;
    public string soundKey;

    public void PlaySound()
    {
        Sequence seq = DOTween.Sequence();
        seq.SetDelay(delay);
        seq.AppendCallback(() =>
       {
           SoundPlayer.Instance.PlaySound(soundKey);
       });
    }

}
