using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextTooltip : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI text;

    private float lifetime;

    public TextTooltip Set(string message, float duration = 1.5f)
    {
        if (!gameObject.activeSelf) ProjectUtility.SetActiveCheck(gameObject, true);
        text.text = message;
        lifetime = duration;

        transform.DOKill();
        transform.localScale = Vector3.zero;
        transform.SetAsLastSibling();
        transform.DOScale(1, 0.1f)
            .SetUpdate(true)
            .SetEase(Ease.OutBack);

        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
        return this;
    }

    public TextTooltip Set(string message, Vector3 position, Transform transformToFollow = null, float duration = 1.5f)
    {
        if (transformToFollow != null) transform.SetParent(transformToFollow);
        transform.position = position;
        return Set(message, duration);
    }

    private void Update()
    {
        if (!gameObject.activeSelf) return;
        lifetime -= Time.unscaledDeltaTime;
        if (lifetime <= 0) Hide();
    }



    public void Hide()
    {
        lifetime = 1.5f;
        transform.DOKill();
        transform.DOScale(0,0.1f)
            .SetUpdate(true)
            .SetEase(Ease.InBack)
            .OnComplete(()=>{
                gameObject.SetActive(false);
                lifetime = 1.5f;
            });
    }

    private void OnDisable(){
        transform.localScale = Vector3.zero;
    }
}