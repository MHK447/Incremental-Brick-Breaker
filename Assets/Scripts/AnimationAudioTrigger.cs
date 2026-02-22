using UnityEngine;

public class AnimationAudioTrigger : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;

    // 🎬 Animation Event에서 호출할 함수
    public void PlaySound()
    {
        if (audioSource != null)
        {
            audioSource.Play();
        }
    }
}
