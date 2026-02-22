using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySoundAnimationKey : MonoBehaviour
{
	[SerializeField]
	private bool mute = false;

    public void PlaySound(string key)
	{
		if(!mute)
			SoundPlayer.Instance.PlaySound(key);
	}
}
