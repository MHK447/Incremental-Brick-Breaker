using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LevelupEyeComponent : MonoBehaviour
{
    [SerializeField]
    private Button EyeButton;

    [SerializeField]
    private List<GameObject> TargetObjList = new List<GameObject>();

    private RectTransform buttonRectTransform;
    private Canvas canvas;

    private void Awake()
    {
        if (EyeButton != null)
        {
            buttonRectTransform = EyeButton.GetComponent<RectTransform>();
            canvas = EyeButton.GetComponentInParent<Canvas>();
        }
    }

    private void Update()
    {
        if (EyeButton == null || buttonRectTransform == null)
            return;

        bool isPressed = CheckIfButtonPressed();

        // 누르고 있을 때와 뗐을 때의 상태에 따라 오브젝트 활성화/비활성화
        foreach(var obj in TargetObjList)
        {
            ProjectUtility.SetActiveCheck(obj, !isPressed);
        }
    }

    private bool CheckIfButtonPressed()
    {
        // 마우스 입력 체크 (PC)
        if (Input.GetMouseButton(0))
        {
            if (RectTransformUtility.RectangleContainsScreenPoint(
                buttonRectTransform, 
                Input.mousePosition, 
                canvas?.worldCamera))
            {
                return true;
            }
        }

        // 터치 입력 체크 (모바일)
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Began)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(
                    buttonRectTransform, 
                    touch.position, 
                    canvas?.worldCamera))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
