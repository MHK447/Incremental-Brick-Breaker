using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class TargetSoftMask : MonoBehaviour
{
    [SerializeField]
    private GameObject TargetTr;
    
    [SerializeField]
    private RectTransform maskTrans;
    
    [SerializeField]
    private float paddingValue = 1.5f;
    
    [SerializeField]
    private float paddingAddYValue = 0f;
    
    [SerializeField]
    private Vector2 ClickableOffsetMin;
    
    [SerializeField]
    private Vector2 ClickableOffsetMax;
    
    
    private void OnEnable()
    {
        if (TargetTr != null && maskTrans != null)
        {
            SetupMask();
        }
    }
    

    public void SetupMask()
    {
        if (TargetTr == null || maskTrans == null)
            return;
            
        maskTrans.gameObject.SetActive(false);
        
        var viewportPointMin = Vector2.zero;
        var viewportPointMax = Vector2.zero;
        
        TargetTr.gameObject.SetActive(true);
        var rect = TargetTr.GetComponent<RectTransform>();
        
        if (rect != null)
        {
            var canvas = TargetTr.gameObject.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                switch (canvas.renderMode)
                {
                    case RenderMode.ScreenSpaceOverlay:
                        {
                            Vector2 convert = rect.position;
                            var pivot = new Vector2(0.5f - rect.pivot.x, 0.5f - rect.pivot.y);
                            var size = new Vector2(rect.rect.width, rect.rect.height);
                            convert = convert + (size * rect.lossyScale * pivot);
                            viewportPointMin = new Vector2((convert.x + ClickableOffsetMin.x - (rect.rect.width * rect.lossyScale.x * paddingValue / 2f)) / Screen.width, (convert.y + ClickableOffsetMin.y - (rect.rect.height * rect.lossyScale.y * (paddingValue + paddingAddYValue) / 2f)) / Screen.height);
                            viewportPointMax = new Vector2((convert.x + ClickableOffsetMax.x + (rect.rect.width * rect.lossyScale.x * paddingValue / 2f)) / Screen.width, (convert.y + ClickableOffsetMax.y + (rect.rect.height * rect.lossyScale.y * (paddingValue + paddingAddYValue) / 2f)) / Screen.height);
                        }
                        break;
                    case RenderMode.WorldSpace:
                        {
                            Camera cam = GameRoot.Instance.InGameSystem.CurInGame.GetMainCam.cam;
                            if (cam == null)
                            {
                                return;
                            }
                            var worldMin = rect.TransformPoint(new Vector2(rect.rect.xMin + ClickableOffsetMin.x, rect.rect.yMin + ClickableOffsetMin.y) * paddingValue);
                            viewportPointMin = cam.WorldToViewportPoint(worldMin);
                            var worldMax = rect.TransformPoint(new Vector2(rect.rect.xMax + ClickableOffsetMax.x, rect.rect.yMax + ClickableOffsetMax.y) * paddingValue);
                            viewportPointMax = cam.WorldToViewportPoint(worldMax);
                        }
                        break;
                }
            }
            
            maskTrans.gameObject.SetActive(true);
            maskTrans.anchorMin = viewportPointMin;
            maskTrans.anchorMax = viewportPointMax;
            maskTrans.offsetMin = Vector2.zero;
            maskTrans.offsetMax = Vector2.zero;
        }
        else
        {
            // BoxCollider2D 처리
            Camera cam = GameRoot.Instance.InGameSystem.CurInGame.GetMainCam.cam;
            if (cam == null)
            {
                return;
            }
            
            var collider = TargetTr.GetComponent<BoxCollider2D>();
            if (collider != null)
            {
                float halfX = collider.size.x / 2f * paddingValue;
                float halfY = collider.size.y / 2f * (paddingValue + paddingAddYValue);
                var worldPosMin = new Vector3(TargetTr.transform.position.x + ((collider.offset.x - halfX) * Mathf.Abs(TargetTr.transform.lossyScale.x)),
                        TargetTr.transform.position.y + ((collider.offset.y - halfY) * TargetTr.transform.lossyScale.y), TargetTr.transform.position.z * TargetTr.transform.lossyScale.z);
                viewportPointMin = cam.WorldToViewportPoint(worldPosMin);
                var worldPosMax = new Vector3(TargetTr.transform.position.x + ((collider.offset.x + halfX) * Mathf.Abs(TargetTr.transform.lossyScale.x)),
                        TargetTr.transform.position.y + ((collider.offset.y + halfY) * TargetTr.transform.lossyScale.y), TargetTr.transform.position.z * TargetTr.transform.lossyScale.z);
                viewportPointMax = cam.WorldToViewportPoint(worldPosMax);
                
                maskTrans.gameObject.SetActive(true);
                maskTrans.anchorMin = viewportPointMin;
                maskTrans.anchorMax = viewportPointMax;
                maskTrans.offsetMin = Vector2.zero;
                maskTrans.offsetMax = Vector2.zero;
            }
        }
    }
}

