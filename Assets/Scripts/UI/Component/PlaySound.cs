using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySound : MonoBehaviour
{
    public string soundKey;

    public void PlaySoundDirect()
    {
        SoundPlayer.Instance.PlaySound(soundKey);
    }
}
