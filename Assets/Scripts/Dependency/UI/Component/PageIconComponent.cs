using UnityEngine;
using DG.Tweening;

public enum PageIconType
{
    StarterPackage = 0,
    NoAds = 1,
}

public class PageIconComponent : MonoBehaviour
{
    public PageIconType IconType;
    public virtual void Init() { }
    public void OnThrowSprite(int count = 1)
    {
        transform.DORewind();
        transform.DOScale(1.5f, 0.1f)
            .SetUpdate(true)
            .SetEase(Ease.OutCubic)
            .SetLoops(2, LoopType.Yoyo);
        
    }
}