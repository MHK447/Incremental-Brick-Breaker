using BanpoFri;
using UnityEngine;
using UnityEngine.UI;

public enum HudBottomBtnType
{

    IncreaseUpgrade = 0,
    EquipmentItemUpgrade = 1,
    TRAINING = 2,
    CARD = 3,
    BATTLE = 4,

    Done,
}



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
        //ProjectUtility.SetActiveCheck(RedDotObj, !isLocked);
    }

    public void SetActive(bool isActive)
    {
        ProjectUtility.SetActiveCheck(CloseObj, isActive);

        if (Anim == null) return;
        // 이미 같은 상태일 때도 처음부터 재생되도록 normalizedTime 0으로 재생 (첫 진입 시 애니 안 나오는 현상 방지)
        if (isActive)
            Anim.Play("Normal", 0, 0f);
        else
            Anim.Play("Selected", 0, 0f);
    }

    public void OnClick()
    {
        if (LockObj.activeSelf) return;
        //IsSelect = !IsSelect;
        ClickAction?.Invoke(CurBtnType, IsSelect);
    }
}
