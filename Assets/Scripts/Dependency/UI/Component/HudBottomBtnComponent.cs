using BanpoFri;
using UnityEngine;
using UnityEngine.UI;

public class HudBottomBtnComponent : MonoBehaviour
{
    [SerializeField]
    private Button CloseBtn;

    public Button GetCloseBtn { get { return CloseBtn; } }

    public HudBottomBtnType CurBtnType;

    [SerializeField]
    private GameObject CloseObj;

    [SerializeField]
    private Button SelectBtn;

    public Button GetSelectBtn { get { return SelectBtn; } }

    [SerializeField]
    private Animator Anim;

    [SerializeField]
    private GameObject LockObj;

    [SerializeField]
    private GameObject RedDotObj;

    public GameObject GetLockObj { get { return LockObj; } }

    public bool IsSelect = false;



    private System.Action<HudBottomBtnType, bool> ClickAction;

    void Awake()
    {
        SelectBtn.onClick.AddListener(OnClick);
    }


    public void Set(System.Action<HudBottomBtnType, bool> onclickaction)
    {
        ClickAction = onclickaction;
        IsSelect = false;
    }


    public void SetLocked(bool isLocked)
    {
        ProjectUtility.SetActiveCheck(LockObj, isLocked);
        ProjectUtility.SetActiveCheck(RedDotObj, !isLocked);
    }

    public void SetActive(bool isActive)
    {
        ProjectUtility.SetActiveCheck(CloseObj, isActive);

        if (isActive)
        {
            Anim.Play("Normal");
        }
        else
        {
            Anim.Play("Selected");
        }
    }

    public void OnClick()
    {
        if (LockObj.activeSelf) return;
        //IsSelect = !IsSelect;
        ClickAction?.Invoke(CurBtnType, IsSelect);
    }
}
