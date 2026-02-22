using UnityEngine;
using UnityEngine.UI;
using BanpoFri;
using TMPro;
using DG.Tweening;

[EffectPath("Effect/TextEffectMoneyUpgrade", true, false)]
public class TextEffectMoneyUpgrade : Effect
{
    [SerializeField]
    private TextMeshProUGUI DescText;



    public void Set(string text, Transform target, System.Action endaction = null)
    {
        DescText.text = text;
        
        // 텍스트 초기 설정
        DescText.alpha = 1f;
        DescText.transform.localScale = Vector3.one;
        
        // 타겟 위치로 이동하는 애니메이션 시퀀스
        var sequence = DOTween.Sequence();
        sequence.SetUpdate(true); // Time.timeScale의 영향을 받지 않도록 설정
        
        // 텍스트가 약간 위로 올라가면서 크기가 커지는 효과
        sequence.Append(DescText.transform.DOScale(1.2f, 0.15f).SetEase(Ease.OutQuad));
        sequence.Join(DescText.transform.DOMoveY(DescText.transform.position.y + 90f, 0.3f).SetEase(Ease.OutQuad));
        
        
        // 타겟으로 이동하면서 크기가 점진적으로 작아지는 효과 (2단계)
        if (target != null)
        {
            Vector3 startPos = DescText.transform.position;
            Vector3 targetPos = target.position;
            DescText.transform.localScale = Vector3.one;

            
            sequence.Append(DescText.transform.DOScale(0f, 0.5f).SetEase(Ease.InExpo));
            sequence.Join(DescText.transform.DOMove(targetPos, 0.5f).SetEase(Ease.InExpo));
        }
        
        // 애니메이션 완료 후 콜백 실행
        sequence.OnComplete(() =>
        {
            endaction?.Invoke();
            
            // 오브젝트 비활성화 또는 파괴
            gameObject.SetActive(false);
        });
    }
}
