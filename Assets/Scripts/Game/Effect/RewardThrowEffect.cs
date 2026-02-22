using DG.Tweening;
using UnityEngine.UI;
using BanpoFri;
using UnityEngine;

[EffectPath("Effect/RewardThrowEffect", true, false)]
public class RewardThrowEffect : Effect
{

    [SerializeField]
    private GameObject chestlockobj;

    [SerializeField]
    private GameObject chestunlockobj;

    [SerializeField]
    private GameObject DevilChestLockObj;

    [SerializeField]
    private GameObject DevilChestUnLockObj;

    [SerializeField]
    private GameObject DevilChestRoot;

    [SerializeField]
    private GameObject NormalChestRoot;

    private Vector3 startPos;
    private Transform endTarget;
    private Vector3 endPos;
    private Vector3 nudgePos;
    private float move;

    private System.Action EndCallBack = null;
    private SpriteThrowEffectParameters parameters;
    private bool isRewardSet = false; // 중복 호출 방지 플래그

    public void ShowWorldPos(bool isdevil, Vector3 startWorldPos, Transform targetUI, System.Action callback, SpriteThrowEffectParameters _parameters)
    {
        ProjectUtility.SetActiveCheck(DevilChestRoot, isdevil);
        ProjectUtility.SetActiveCheck(NormalChestRoot, !isdevil);

        if (isdevil)
        {
            DevilShowUIPos(ProjectUtility.worldToUISpace(GameRoot.Instance.UISystem.WorldCanvas, startWorldPos), targetUI, callback, _parameters);
        }
        else
        {
            NormalShowUIPos(ProjectUtility.worldToUISpace(GameRoot.Instance.UISystem.WorldCanvas, startWorldPos), targetUI, callback, _parameters);
        }
    }

    public void NormalShowUIPos(Vector3 from, Transform targetUI, System.Action callback, SpriteThrowEffectParameters _parameters)
    {
        parameters = _parameters;
        EndCallBack = callback;
        isRewardSet = false; // 초기화

        ProjectUtility.SetActiveCheck(chestlockobj, true);
        ProjectUtility.SetActiveCheck(chestunlockobj, false);

        float duration = parameters.duration;
        Vector3 to = targetUI.position;

        move = 0;
        transform.localScale = Vector3.zero;

        startPos = from;
        endTarget = targetUI;
        endPos = targetUI.position; // 초기 목표 위치 설정
        nudgePos = Vector3.zero;

        Sequence sequence = DOTween.Sequence();
        Vector3 targetNudgeOffset = (Vector3)Random.insideUnitCircle * 100;

        sequence.Append(
            DOVirtual.Vector3(Vector3.zero, targetNudgeOffset, 0.4f, x =>
            {
                nudgePos = x;
            }).SetUpdate(true).SetEase(Ease.OutCubic).SetTarget(transform));

        if (parameters.scaleCurve == null)
        {
            sequence.Join(transform.DOScale(parameters.scale, 0.1f).SetEase(Ease.InOutCubic).SetUpdate(true));
        }
        else
        {
            sequence.Join(transform.DOScale(parameters.scale, duration).SetEase(parameters.scaleCurve).SetUpdate(true));
        }

        float moveDuration = duration * 0.3f;
        sequence.Join(DOVirtual.Float(0, 1, moveDuration, x =>
        {
            move = x;
        }).SetEase(Ease.InCubic).SetUpdate(true).SetTarget(transform).SetDelay(0.05f));

        float moveEndTime = 0.05f + moveDuration;
        sequence.InsertCallback(moveEndTime, () =>
        {
            ProjectUtility.SetActiveCheck(chestlockobj, false);
            ProjectUtility.SetActiveCheck(chestunlockobj, true);

            GameRoot.Instance.WaitTimeAndCallback(1.5f, () =>
            {
                SetReward();
                OnForceCollect();
            });
        });

        sequence.SetUpdate(true);
        sequence.SetDelay(parameters.delay);
    }


    public void DevilShowUIPos(Vector3 from, Transform targetUI, System.Action callback, SpriteThrowEffectParameters _parameters)
    {
        parameters = _parameters;
        EndCallBack = callback;
        isRewardSet = false; // 초기화

        ProjectUtility.SetActiveCheck(DevilChestLockObj, true);
        ProjectUtility.SetActiveCheck(DevilChestUnLockObj, false);

        float duration = parameters.duration;
        Vector3 to = targetUI.position;

        move = 0;
        transform.localScale = Vector3.zero;

        startPos = from;
        endTarget = targetUI;
        endPos = targetUI.position; // 초기 목표 위치 설정
        nudgePos = Vector3.zero;

        Sequence sequence = DOTween.Sequence();
        Vector3 targetNudgeOffset = (Vector3)Random.insideUnitCircle * 100;

        sequence.Append(
            DOVirtual.Vector3(Vector3.zero, targetNudgeOffset, 0.4f, x =>
            {
                nudgePos = x;
            }).SetUpdate(true).SetEase(Ease.OutCubic).SetTarget(transform));

        if (parameters.scaleCurve == null)
        {
            sequence.Join(transform.DOScale(parameters.scale, 0.1f).SetEase(Ease.InOutCubic).SetUpdate(true));
        }
        else
        {
            sequence.Join(transform.DOScale(parameters.scale, duration).SetEase(parameters.scaleCurve).SetUpdate(true));
        }

        float moveDuration = duration * 0.3f;
        sequence.Join(DOVirtual.Float(0, 1, moveDuration, x =>
        {
            move = x;
        }).SetEase(Ease.InCubic).SetUpdate(true).SetTarget(transform).SetDelay(0.05f));

        float moveEndTime = 0.05f + moveDuration;
        sequence.InsertCallback(moveEndTime, () =>
        {
            ProjectUtility.SetActiveCheck(DevilChestLockObj, false);
            ProjectUtility.SetActiveCheck(DevilChestUnLockObj, true);

            GameRoot.Instance.WaitTimeAndCallback(1.5f, () =>
            {
                DevilSetReward();
                OnForceCollect();
            });
        });

        sequence.SetUpdate(true);
        sequence.SetDelay(parameters.delay);
    }

    private void Update()
    {
        if (endTarget != null && move < 0.9f)
        {
            endPos = endTarget.position;
        }
        transform.position = Vector3.Lerp(startPos, endPos - nudgePos, move) + nudgePos;
    }

    private void OnDisable()
    {
        transform.DOKill();
        isRewardSet = false; // 리셋
    }

    public void DevilSetReward()
    {
        GameRoot.Instance.UISystem.OpenUI<PopupDevilChoice>(popup => popup.Set(), () =>
        {
            EndCallBack?.Invoke();
            EndCallBack = null;
        });
    }

    public void SetReward()
    {
        // 중복 호출 방지
        if (isRewardSet)
            return;

        isRewardSet = true;

        var curwaveidx = GameRoot.Instance.UserData.Waveidx.Value;

        var recordcount = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.NewEnemyBlockSpawnerReward);


        // Random.Range(1, 3)으로 수정하여 1 또는 2를 반환하도록 함
        int gettype = Random.Range(1, 3);

        if (recordcount == 0)
        {
            GameRoot.Instance.UserData.AddRecordCount(Config.RecordCountKeys.NewEnemyBlockSpawnerReward, 1);
            gettype = 1;
        }


        switch (gettype)
        {
            case 1:
                {
                    GameRoot.Instance.UISystem.OpenUI<PopupLevelUpReward>(popup => popup.Init(), () =>
                    {
                        EndCallBack?.Invoke();
                        EndCallBack = null;
                    });
                }
                break;
            case 2:
                {
                    GameRoot.Instance.EffectSystem.MultiPlay<RewardEffect>(transform.position, x =>
                     {
                         var target = GameRoot.Instance.UISystem.GetUI<PopupInGame>().SilverCoinRoot;

                         x.Set((int)Config.RewardType.Currency, (int)Config.CurrencyID.SilverCoin, target, () =>
                         {
                             EndCallBack?.Invoke();
                             EndCallBack = null;
                             GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency, (int)Config.CurrencyID.SilverCoin, curwaveidx * 10);
                         });
                         x.SetAutoRemove(true, 1.5f);
                     });
                }
                break;
        }
    }
}

