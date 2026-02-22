using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

[UIPath("UI/Popup/PopupNewRecord")]
public class PopupNewRecord : UIBase
{
    [SerializeField]
    private TextMeshProUGUI NewRecordDesc;

    [SerializeField]
    private TextMeshProUGUI RecrdCountText;

    public override void OnShowBefore()
    {
        base.OnShowBefore();

        // 초기 상태 설정
        NewRecordDesc.alpha = 0f;
        NewRecordDesc.transform.localScale = Vector3.zero;
        RecrdCountText.alpha = 0f;
        ProjectUtility.SetActiveCheck(NewRecordDesc.gameObject , false);
        ProjectUtility.SetActiveCheck(RecrdCountText.gameObject , false);
        
        GameRoot.Instance.WaitTimeAndCallback(3f , Hide);
    }
    
    public override void OnShowAfter()
    {
        base.OnShowAfter();

        ProjectUtility.SetActiveCheck(NewRecordDesc.gameObject , true);
        ProjectUtility.SetActiveCheck(RecrdCountText.gameObject , true);
        RecrdCountText.text = "0m";
        PlayNewRecordAnimation();
    }


    private void PlayNewRecordAnimation()
    {
        var targetScore = GameRoot.Instance.UserData.Highscorevalue;
        
        // 애니메이션 시퀀스 생성
        var sequence = DOTween.Sequence();
        
        // 1. NewRecordDesc 페이드인 & 극적인 스케일 애니메이션
        sequence.Append(NewRecordDesc.DOFade(1f, 0.3f).SetEase(Ease.OutQuart))
                .Join(NewRecordDesc.transform.DOScale(Vector3.one * 1.3f, 0.4f).SetEase(Ease.OutBack, 1.8f))
                .AppendInterval(0.1f);
        
        // 2. RecrdCountText 빠른 페이드인
        sequence.Append(RecrdCountText.DOFade(1f, 0.2f).SetEase(Ease.OutQuart));
        
        // 3. 빠른 숫자 카운트업 애니메이션 (더 극적인 이징)
        sequence.Append(DOTween.To(() => 0f, x => {
            RecrdCountText.text = $"{Mathf.RoundToInt(x)}m";
        }, targetScore, 0.8f).SetEase(Ease.OutExpo));
        
        // 4. 완료 후 강조 효과 (더 크고 극적으로)
        sequence.AppendCallback(() => {
            // 숫자 텍스트 큰 펄스 효과
            RecrdCountText.transform.DOPunchScale(Vector3.one * 2f, 0.6f, 8, 0.8f);
            
            // 설명 텍스트 더 강한 흔들기
            NewRecordDesc.transform.DOShakePosition(0.4f, new Vector3(10f, 5f, 0f), 15, 90f, false, true);
        });
        
        // 시퀀스 시작
        sequence.Play();
    }

}
