using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Cysharp.Threading.Tasks;
using UniRx;


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
    private List<Image> GlowColorImgList = new List<Image>();

    [SerializeField]
    private List<Image> UnLockImgList = new List<Image>();

    [SerializeField]
    private GameObject LineListRoot;

    private List<GameObject> NextLineList = new List<GameObject>();

    private UpgradeType CurType = UpgradeType.Hide;

    private int UpgradeCost = 0;

    private System.Action<int> UnlockAction = null;

    private CompositeDisposable disposables = new CompositeDisposable();

    void Awake()
    {
        UpgradeBtn.onClick.AddListener(OnClickUpgrade);

        NextLineList.Clear();

        if (LineListRoot != null)
        {
            for (int i = 0; i < LineListRoot.transform.childCount; i++)
            {
                NextLineList.Add(LineListRoot.transform.GetChild(i).gameObject);
            }
        }
    }



    void OnEnable()
    {

    }

    public void Init(System.Action<int> unlockaction)
    {
        var td = Tables.Instance.GetTable<IncreaUpgradeOrder>().GetData(UpgradeOrder);
        if (td != null  && td.cost.Count > 0)
        {
            var finddata = GameRoot.Instance.IncreaMentalSystem.FindData(td.order);
            int level = finddata != null ? finddata.Level.Value : 0;
            int costIdx = Mathf.Clamp(level, 0, td.cost.Count - 1);
            UpgradeCost = td.cost[costIdx];

            var upgradetd = Tables.Instance.GetTable<IncreaUpgradeInfo>().GetData(td.increase_idx);

            if (upgradetd != null)
            {
                UpgradeImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Increa, upgradetd.icon);
            }
        }

        disposables.Clear();

        GameRoot.Instance.UserData.Money.Subscribe(x =>
        {
            SetGlowColor((int)x);
        }).AddTo(disposables);

        UnlockAction = unlockaction;

        SetState();
    }


    public void SetGlowColor(int cost)
    {
        foreach (var img in GlowColorImgList)
        {
            img.color = cost >= UpgradeCost ? Config.Instance.GetImageColor("image_color_green") : Config.Instance.GetImageColor("image_color_red");
        }
    }


    public void SetStateCheck()
    {

    }


    public void OnClickUpgrade()
    {
        var td = Tables.Instance.GetTable<IncreaUpgradeOrder>().GetData(UpgradeOrder);
        if (td == null) return;

        var finddata = GameRoot.Instance.IncreaMentalSystem.FindData(td.order);
        int level = finddata != null ? finddata.Level.Value : 0;

        if (level >= td.increase_max_lv) return;

        if (GameRoot.Instance.UserData.Money.Value >= UpgradeCost)
        {
            GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency, (int)Config.CurrencyID.Money, -UpgradeCost);
            GameRoot.Instance.IncreaMentalSystem.IncreaseLevelUp(td.order);
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
        var td = Tables.Instance.GetTable<IncreaUpgradeOrder>().GetData(UpgradeOrder);
        if (td == null) return;

        var finddata = GameRoot.Instance.IncreaMentalSystem.FindData(td.order);
        if (finddata != null)
        {
            foreach (var line in NextLineList)
            {
                ProjectUtility.SetActiveCheck(line, false);
            }
    
            if (finddata.Level.Value == 0 && !GameRoot.Instance.IncreaMentalSystem.UpgradeUnLockOrderList.Contains(UpgradeOrder)
            && UpgradeOrder != 2)
            {
                CurType = UpgradeType.Hide;

                ProjectUtility.SetActiveCheck(this.gameObject, false);
            }
            else if (finddata.Level.Value <= 0 && (GameRoot.Instance.IncreaMentalSystem.UpgradeUnLockOrderList.Contains(UpgradeOrder) || UpgradeOrder == 2))
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

            foreach(var img in UnLockImgList)
            {
                img.color = CurType == UpgradeType.UnLock ? Config.Instance.GetImageColor("increa_unlock_img") : Config.Instance.GetImageColor("increa_lock_img");
            }

            ProjectUtility.SetActiveCheck(GlowRootObj, CurType == UpgradeType.Lock);

            disposables.Clear();
            GameRoot.Instance.UserData.Money.Subscribe(x =>
            {
                SetGlowColor((int)x);
            }).AddTo(disposables);
        }
    }

    void OnDestroy()
    {
        disposables.Clear();
    }

    void OnDisable()
    {
        disposables.Clear();
    }


}

