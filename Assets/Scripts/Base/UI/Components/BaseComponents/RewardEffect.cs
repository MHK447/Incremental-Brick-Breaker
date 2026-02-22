using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using DG.Tweening;

[EffectPath("Effect/RewardEffect", true, false)]
public class RewardEffect : Effect
{
    [SerializeField]
    private List<Image> RewardImages = new List<Image>();

    [SerializeField]
    private List<TrailComponent> TrailComponents = new List<TrailComponent>();



    public void Set(int rewardtype , int rewardidx , Transform endrewardtr , System.Action endaction = null)
    {
        // 모든 이미지를 정 가운데 위치에 초기화하고 스프라이트 설정
        Vector3 centerPosition = transform.position;
        
        for(int i = 0; i < RewardImages.Count; i++)
        {
            var img = RewardImages[i];
            img.sprite = Config.Instance.GetRewardImage(rewardtype , rewardidx);
            
            // 가운데 근처에서 랜덤한 위치에 배치 (겹치지 않게)
            Vector2 randomOffset = Random.insideUnitCircle * 50f; // 반경 50 픽셀 내에서 랜덤
            Vector3 randomPosition = centerPosition + new Vector3(randomOffset.x, randomOffset.y, 0f);
            img.transform.position = randomPosition;
            img.transform.localScale = Vector3.zero;
            img.gameObject.SetActive(true);
            
            // 각 이미지를 차례대로 연출 (0.1초씩 지연)
            float delay = i * 0.1f;
            
            var sequence = DOTween.Sequence();
            sequence.SetUpdate(true);
            
            // 지연 후 시작
            sequence.AppendInterval(delay);
            
            // 스케일 0에서 1로 커지는 연출 (0.3초)
            sequence.Append(img.transform.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack));
            
            // 잠시 대기
            sequence.AppendInterval(0.2f);
            
            // 목표 위치로 날아가는 연출 (0.6초)
            if (endrewardtr != null)
            {
                sequence.Append(img.transform.DOMove(endrewardtr.position, 0.6f).SetEase(Ease.InQuad));
            }
            
            int index = i;
            
            // 첫 번째 이미지가 도착하면 endaction 실행
            if (i == 0)
            {
                sequence.AppendCallback(() =>
                {
                    endaction?.Invoke();
                });
            }
            
            // 마지막 이미지가 도착하면 정리
            if (i == RewardImages.Count - 1)
            {
                sequence.OnComplete(() =>
                {
                    // 모든 이미지 비활성화
                    foreach(var image in RewardImages)
                    {
                        image.gameObject.SetActive(false);
                    }
                });
            }
        }
    }
}

