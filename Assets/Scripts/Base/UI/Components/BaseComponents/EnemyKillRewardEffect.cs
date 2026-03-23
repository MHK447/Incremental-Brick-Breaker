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
    private bool IsRewardHovered = false;
    private float AutoCollectEnableTime = 0f;
    [SerializeField] private float autoCollectDelay = 0.2f;


    private float XRewardPos = -260f;
    private Collider2D cachedCollider;
    private Camera cachedCamera;

    public void Set(int rewardtype, int rewardidx, int rewardvalue)
    {
        RewardSprite.sprite = Config.Instance.GetRewardImage(rewardtype, rewardidx);

        var ypos = rewardidx == (int)Config.CurrencyID.Material ? 1.8f : 1f;

        RewardSprite.transform.localScale = new Vector3(ypos, ypos, 1f);

        RewardType = rewardtype;
        RewardIdx = rewardidx;
        RewardValue = rewardvalue;
        IsRewardHovered = false;
        AutoCollectEnableTime = Time.time + autoCollectDelay;
        cachedCollider = GetComponent<Collider2D>();
        cachedCamera = Camera.main;

        RewardSprite.DOKill();
        float startY = transform.localPosition.y;
        RewardSprite.transform.DOLocalMoveY(startY + floatAmplitude, floatDuration * 0.5f)
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
            TryRewardHover(false);
        }

        if (!IsRewardHovered && cachedCollider != null && cachedCamera != null)
        {
            Vector3 mouseWorld = cachedCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;
            if (cachedCollider.OverlapPoint(mouseWorld))
            {
                TryRewardHover(true);
            }
        }
    }

    private void TryRewardHover(bool ignoreAutoCollectDelay)
    {
        if (!ignoreAutoCollectDelay && Time.time < AutoCollectEnableTime)
            return;

        if (IsRewardHovered)
            return;

        IsRewardHovered = true;
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
        if (collision.GetComponentInParent<EnemyBlockSpawner>() != null)
            TryRewardHover(false);
    }
}

