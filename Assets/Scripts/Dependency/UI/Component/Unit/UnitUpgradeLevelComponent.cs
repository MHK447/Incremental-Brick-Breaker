using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Unity.VisualScripting;
public class UnitUpgradeLevelComponent : MonoBehaviour
{
    [SerializeField]
    private Image FxImg;

    [SerializeField]
    private Image InnerImg;


    public void Activecheck(bool isactive)
    {
        // DOTween 애니메이션 중지 및 초기화
        FxImg.DOKill();
        InnerImg.DOKill();

        var fxColor = FxImg.color;
        fxColor.a = 1f;
        FxImg.color = fxColor;

        var innerColor = InnerImg.color;
        innerColor.a = 1f;
        InnerImg.color = innerColor;
        

        ProjectUtility.SetActiveCheck(FxImg.gameObject, isactive);
        ProjectUtility.SetActiveCheck(InnerImg.gameObject, isactive);
    }
public void AlpahScaleCheck()
{
    Activecheck(true);

    // 먼저 알파 초기화 (중요!)
    var fxColor = FxImg.color;
    fxColor.a = 0f;
    FxImg.color = fxColor;

    var innerColor = InnerImg.color;
    innerColor.a = 0f;
    InnerImg.color = innerColor;

    // FxImg 깜빡이기
    FxImg.DOFade(1f, 0.5f)
        .SetLoops(-1, LoopType.Yoyo)
        .SetUpdate(true)
        .SetEase(Ease.InOutSine)
        .From(0f);  // ← 이걸 붙이면 비활성→활성 후에도 잘 동작

    // InnerImg 깜빡이기
    InnerImg.DOFade(1f, 0.5f)
        .SetLoops(-1, LoopType.Yoyo)
        .SetUpdate(true)
        .SetEase(Ease.InOutSine)
        .From(0f);
}


}
