using DG.Tweening;
using UnityEngine.UI;
using BanpoFri;
using UnityEngine;

public struct SpriteThrowEffectParameters
{
    public Sprite sprite;
    public float scale;
    public float duration;
    public float delay;
    public bool fixRotation;
    public float rotation;
    public AnimationCurve scaleCurve;
}

[EffectPath("Effect/SpriteThrowEffect", true)]
public class SpriteThrowEffect : Effect
{
    [SerializeField]
    private Image Img;

    private Vector3 startPos;
    private Transform endTarget;
    private Vector3 endPos;
    private Vector3 nudgePos;
    private float move;

    private SpriteThrowEffectParameters parameters;

    public void ShowWorldPos(Vector3 startWorldPos, Transform targetUI, System.Action callback, SpriteThrowEffectParameters _parameters)
    {
        ShowUIPos(ProjectUtility.worldToUISpace(GameRoot.Instance.UISystem.WorldCanvas, startWorldPos), targetUI, callback, _parameters);
    }

    public void ShowUIPos(Vector3 from, Transform targetUI, System.Action callback, SpriteThrowEffectParameters _parameters)
    {
        if (targetUI == null)
            return;

        parameters = _parameters;
        if (Img != null)
            Img.sprite = parameters.sprite;
        float duration = parameters.duration;
        Vector3 to = targetUI.position;

        move = 0;
        transform.SetPositionAndRotation(from, parameters.fixRotation ? Quaternion.Euler(0, 0, parameters.rotation) : Quaternion.Euler(0, 0, Random.Range(0, 360)));
        transform.localScale = Vector3.zero;

        startPos = from;
        endTarget = targetUI;
        nudgePos = Vector3.zero;

        Sequence sequence = DOTween.Sequence();
        Vector3 targetNudgeOffset = (Vector3)Random.insideUnitCircle * 100;

        sequence.Append(
            DOVirtual.Vector3(Vector3.zero, targetNudgeOffset, 0.4f, x =>
            {
                nudgePos = x;
            }).SetUpdate(true).SetEase(Ease.OutCubic).SetTarget(transform));

        if (parameters.scaleCurve == null) sequence.Join(transform.DOScale(parameters.scale, 0.1f).SetEase(Ease.InOutCubic).SetUpdate(true));
        else sequence.Join(transform.DOScale(parameters.scale, duration).SetEase(parameters.scaleCurve).SetUpdate(true));

        sequence.Join(DOVirtual.Float(0, 1, duration, x =>
        {
            move = x;
        }).SetEase(Ease.InCubic).SetUpdate(true).SetTarget(transform).SetDelay(0.05f));

        sequence.InsertCallback(duration, () => { callback?.Invoke(); });
        sequence.InsertCallback(duration + 0.05f, OnForceCollect);

        sequence.SetUpdate(true);
        sequence.SetDelay(parameters.delay);
    }

    private void Update()
    {
        if (endTarget != null && move < 0.9f)
            endPos = endTarget.position;
        if (transform != null)
            transform.position = Vector3.Lerp(startPos, endPos - nudgePos, move) + nudgePos;
    }

    private void OnDisable()
    {
        if (transform != null)
        {
            transform.DOKill();
            transform.localScale = Vector3.zero;
        }
    }
}
