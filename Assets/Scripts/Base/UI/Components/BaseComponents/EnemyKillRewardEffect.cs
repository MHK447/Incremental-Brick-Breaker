using UnityEngine;
using UnityEngine.Events;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using DG.Tweening;

[EffectPath("Effect/EnemyKillRewardEffect", false, false)]
public class EnemyKillRewardEffect : Effect
{

    [SerializeField]
    private SpriteRenderer RewardSprite;

    [SerializeField] private float floatAmplitude = 0.3f;   // 위아래 이동 거리
    [SerializeField] private float floatDuration = 0.8f;    // 한 번 올라갔다 내려오는 시간

    [SerializeField] private UnityEvent onMouseHover;

    private int RewardType = 0;
    private int RewardIdx = 0;
    private int RewardValue = 0;


    private float XRewardPos = -260f;

    public void Set(int rewardtype, int rewardidx, int rewardvalue)
    {
        RewardSprite.sprite = Config.Instance.GetRewardImage(rewardtype, rewardidx);

        RewardType = rewardtype;
        RewardIdx = rewardidx;
        RewardValue = rewardvalue;

        transform.DOKill();
        float startY = transform.localPosition.y;
        transform.DOLocalMoveY(startY + floatAmplitude, floatDuration * 0.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetTarget(transform);
    }

    void LateUpdate()
    {
        var stage = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>()?.Stage;
        if (stage == null)
            return;

        Vector3 scrollDelta = stage.GetMapScrollWorldDelta();
        if (scrollDelta.sqrMagnitude > 0f)
            transform.position += scrollDelta;


        if (transform.localPosition.x <= XRewardPos)
        {
            OnRewardHover();
            onMouseHover?.Invoke();
        }
    }

    void OnMouseEnter()
    {
        OnRewardHover();
        onMouseHover?.Invoke();
    }

    protected virtual void OnRewardHover()
    {
        SetAutoRemove(true, 1f);

        SpriteThrowEffectParameters coinparameters = new()
        {
            sprite = RewardSprite.sprite,
            scale = 0.7f,
            duration = 1.2f,
        };
        Vector3 worldPos = this.transform.position;

        GameRoot.Instance.EffectSystem.MultiPlay<SpriteThrowEffect>(worldPos, (x) =>
          {
              var target = ProjectUtility.GetRewardEndTr(RewardType, RewardIdx);


              x.ShowWorldPos(worldPos, target, () =>
                               {
                                   target.DOScale(1.3f, 0.15f).SetEase(DG.Tweening.Ease.OutCubic).SetUpdate(true).SetLoops(2, DG.Tweening.LoopType.Yoyo);
                                   GameRoot.Instance.UserData.SetReward(RewardType, RewardIdx, RewardValue);
                               }, coinparameters);


              x.SetAutoRemove(true, 2f);
          });

        ProjectUtility.SetActiveCheck(this.gameObject, false);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
    }
}

