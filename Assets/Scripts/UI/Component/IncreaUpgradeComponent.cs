using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class IncreaUpgradeComponent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum UpgradeType
    {
        UnLock,
        Lock,
        Hide,
    }

    [SerializeField]
    private Image UpgradeImg;

    [SerializeField]
    private int UpgradeOrder = 0;

    public int GetOrderIdx { get { return UpgradeOrder; } }

    [SerializeField]
    private Button UpgradeBtn;

    [SerializeField]
    private GameObject GlowRootObj;

    [SerializeField]
    private List<GameObject> NextLineList = new List<GameObject>();

    private UpgradeType CurType = UpgradeType.Hide;


    private int UpgradeCost = 0;

    private System.Action<int> UnlockAction = null;

    void Awake()
    {
        UpgradeBtn.onClick.AddListener(OnClickUpgrade);
    }



    void OnEnable()
    {



    }

    public void Init(System.Action<int> unlockaction)
    {
        var td = Tables.Instance.GetTable<IncreaseUpgradeOrder>().GetData(UpgradeOrder);
        if (td != null)
        {
            UpgradeCost = td.cost;
        }

        UnlockAction = unlockaction;

        SetState();
    }


    public void SetStateCheck()
    {

    }


    public void OnClickUpgrade()
    {
        if (GameRoot.Instance.UserData.Money.Value >= UpgradeCost)
        {
            GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency, (int)Config.CurrencyID.Money, -UpgradeCost);
            GameRoot.Instance.IncreaMentalSystem.IncreaseLevelUp(UpgradeOrder);
            UnLock();
            UnlockAction?.Invoke(UpgradeOrder);
        }
    }


    public void UnLock()
    {
        GameRoot.Instance.IncreaMentalSystem.UpgradeUnLockOrderList.Add(UpgradeOrder);

        SetState();
    }



    public void OnPointerEnter(PointerEventData eventData)
    {
        var popup = GameRoot.Instance.UISystem.GetUI<PopupInGame>();
        if (popup != null)
        {
            popup.UpgradeImgHover(UpgradeOrder, this.transform.position);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        var popup = GameRoot.Instance.UISystem.GetUI<PopupInGame>();
        if (popup != null)
        {
            popup.UpgradeImgHoverExit();
        }
    }

    public void SetState()
    {
        var finddata = GameRoot.Instance.IncreaMentalSystem.FindData(UpgradeOrder);
        if (finddata != null)
        {
            foreach (var line in NextLineList)
            {
                ProjectUtility.SetActiveCheck(line, false);
            }

            if (finddata.Level.Value == 0 && !GameRoot.Instance.IncreaMentalSystem.UpgradeUnLockOrderList.Contains(UpgradeOrder))
            {
                CurType = UpgradeType.Hide;

                ProjectUtility.SetActiveCheck(this.gameObject, false);
            }
            else if (finddata.Level.Value <= 0 && GameRoot.Instance.IncreaMentalSystem.UpgradeUnLockOrderList.Contains(UpgradeOrder))
            {
                CurType = UpgradeType.Lock;


                ProjectUtility.SetActiveCheck(this.gameObject, true);
            }
            else
            {
                CurType = UpgradeType.UnLock;

                ProjectUtility.SetActiveCheck(this.gameObject, true);

                foreach (var line in NextLineList)
                {
                    ProjectUtility.SetActiveCheck(line, true);
                }
            }

            UpgradeImg.material = CurType == UpgradeType.Lock ?  Config.Instance.GrayScaleMat : null;


            ProjectUtility.SetActiveCheck(GlowRootObj, CurType == UpgradeType.Lock);
        }
    }


}

