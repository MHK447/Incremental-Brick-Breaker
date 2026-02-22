using UnityEngine;

public class GeneralManager : MonoBehaviour
{
    public enum Chapter
    {
        Chapter1,
        Chapter2,
        Chapter3
    }
    
    [Header("Current Chapter")]
    public Chapter currentChapter = Chapter.Chapter1;
    
    [Header("Chapter Clear Status")]
    public bool[] chapterCleared = { false, false, false }; // Chapter1, Chapter2, Chapter3
    
    [Header("Audio Sources")]
    public AudioSource bgmSource; // BGM 전용 AudioSource
    public AudioSource sfxSource; // SFX 전용 AudioSource
    
    [Header("BGM Clips")]
    public AudioClip[] bgmClips = new AudioClip[3]; // Chapter1, Chapter2, Chapter3
    
    [Header("SFX Clips")]
    public AudioClip[] sfxClips = new AudioClip[10]; // 효과음들
    
    [Header("Audio Settings")]
    public float bgmVolume = 1f;
    public float sfxVolume = 1f;
    
    
    void Start()
    {
        // 초기 BGM 재생
        PlayChapterBGM(currentChapter);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.G))
        {
            CompleteChapterSequence(currentChapter);
        }
    }
    
    // 챕터 변경
    public void ChangeChapter(Chapter newChapter)
    {
        if (currentChapter == newChapter) return;
        
        // 이전 BGM 정지
        StopCurrentBGM();
        
        // 새 챕터로 변경
        currentChapter = newChapter;
        
        // 새 BGM 재생
        PlayChapterBGM(newChapter);
        
        Debug.Log($"챕터가 {newChapter}로 변경되었습니다.");
    }
    
    // 챕터 BGM 재생
    void PlayChapterBGM(Chapter chapter)
    {
        int chapterIndex = (int)chapter;
        
        if (bgmSource != null && chapterIndex >= 0 && chapterIndex < bgmClips.Length && bgmClips[chapterIndex] != null)
        {
            bgmSource.clip = bgmClips[chapterIndex];
            bgmSource.volume = bgmVolume;
            bgmSource.loop = true;
            bgmSource.Play();
            
            Debug.Log($"{chapter} BGM이 재생됩니다.");
        }
        else
        {
            Debug.LogWarning($"{chapter} BGM이 설정되지 않았습니다.");
        }
    }
    
    // 현재 BGM 정지
    void StopCurrentBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
        }
    }
    
    // 효과음 재생
    public void PlaySFX(int sfxIndex)
    {
        if (sfxSource != null && sfxIndex >= 0 && sfxIndex < sfxClips.Length && sfxClips[sfxIndex] != null)
        {
            sfxSource.clip = sfxClips[sfxIndex];
            sfxSource.volume = sfxVolume;
            sfxSource.Play();
        }
        else
        {
            Debug.LogWarning($"SFX 인덱스 {sfxIndex}가 유효하지 않습니다.");
        }
    }
    
    // BGM 볼륨 설정
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        
        if (bgmSource != null)
        {
            bgmSource.volume = bgmVolume;
        }
    }
    
    // SFX 볼륨 설정
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }
    
    // 현재 챕터 확인
    public Chapter GetCurrentChapter()
    {
        return currentChapter;
    }
    
    // 챕터 클리어 시퀀스 (순서대로 실행)
    public void CompleteChapterSequence(Chapter chapter)
    {
        StartCoroutine(ChapterCompleteCoroutine(chapter));
    }
    
    // 챕터 클리어 코루틴
    System.Collections.IEnumerator ChapterCompleteCoroutine(Chapter chapter)
    {
        Debug.Log($"=== {chapter} 클리어 시퀀스 시작 ===");
        
        // 1. 클리어 확인
        Debug.Log("1. 클리어 확인 중...");
        yield return new WaitForSeconds(0.2f);
        
        // 2. 클리어 처리
        Debug.Log("2. 클리어 처리 중...");
        ClearChapter(chapter);
        yield return new WaitForSeconds(0.3f);
        
        // 3. 클리어 효과음 재생
        Debug.Log("3. 클리어 효과음 재생");
        PlaySFX(0); // 클리어 효과음 (인덱스 0)
        yield return new WaitForSeconds(0.5f);
        
        // 4. 다음 챕터로 이동 (마지막 챕터가 아닌 경우)
        if (!IsAllChaptersCleared())
        {
            Debug.Log("4. 다음 챕터로 이동");
            Chapter nextChapter = GetNextChapter(chapter);
            ChangeChapter(nextChapter);
            yield return new WaitForSeconds(0.2f);
        }
        else
        {
            Debug.Log("4. 모든 챕터 클리어 완료!");
        }
        
        Debug.Log($"=== {chapter} 클리어 시퀀스 완료 ===");
    }
    
    // 챕터 클리어 처리
    public void ClearChapter(Chapter chapter)
    {
        int chapterIndex = (int)chapter;
        
        if (chapterIndex >= 0 && chapterIndex < chapterCleared.Length)
        {
            chapterCleared[chapterIndex] = true;
            Debug.Log($"{chapter} 클리어!");
            
            // 다음 챕터 해금 (마지막 챕터가 아닌 경우)
            if (chapterIndex < chapterCleared.Length - 1)
            {
                Debug.Log($"{GetNextChapter(chapter)} 챕터가 해금되었습니다!");
            }
            else
            {
                Debug.Log("모든 챕터를 클리어했습니다!");
            }
        }
    }
    
    // 특정 챕터가 클리어되었는지 확인
    public bool IsChapterCleared(Chapter chapter)
    {
        int chapterIndex = (int)chapter;
        return chapterIndex >= 0 && chapterIndex < chapterCleared.Length && chapterCleared[chapterIndex];
    }
    
    // 다음 챕터 가져오기
    public Chapter GetNextChapter(Chapter currentChapter)
    {
        int currentIndex = (int)currentChapter;
        int nextIndex = currentIndex + 1;
        
        if (nextIndex < System.Enum.GetValues(typeof(Chapter)).Length)
        {
            return (Chapter)nextIndex;
        }
        
        return currentChapter; // 마지막 챕터인 경우 현재 챕터 반환
    }
    
    // 클리어된 챕터 수 가져오기
    public int GetClearedChapterCount()
    {
        int count = 0;
        for (int i = 0; i < chapterCleared.Length; i++)
        {
            if (chapterCleared[i]) count++;
        }
        return count;
    }
    
    // 전체 진행률 가져오기 (0-1)
    public float GetProgressPercentage()
    {
        return (float)GetClearedChapterCount() / chapterCleared.Length;
    }
    
    // 모든 챕터 클리어 여부 확인
    public bool IsAllChaptersCleared()
    {
        return GetClearedChapterCount() == chapterCleared.Length;
    }
    
    // 챕터 클리어 상태 리셋 (테스트용)
    public void ResetChapterProgress()
    {
        for (int i = 0; i < chapterCleared.Length; i++)
        {
            chapterCleared[i] = false;
        }
        Debug.Log("챕터 진행률이 리셋되었습니다.");
    }
    
    // 편의 함수들
    public void GoToChapter1() => ChangeChapter(Chapter.Chapter1);
    public void GoToChapter2() => ChangeChapter(Chapter.Chapter2);
    public void GoToChapter3() => ChangeChapter(Chapter.Chapter3);
    
    public void ClearChapter1() => ClearChapter(Chapter.Chapter1);
    public void ClearChapter2() => ClearChapter(Chapter.Chapter2);
    public void ClearChapter3() => ClearChapter(Chapter.Chapter3);
}
