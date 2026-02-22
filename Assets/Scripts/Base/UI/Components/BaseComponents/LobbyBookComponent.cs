using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using DG.Tweening;

public class LobbyBookComponent : MonoBehaviour
{
    public enum LobbyBookState
    {
        Default,
        Lock,
        Present,
    }


    [SerializeField]
    private GameObject LobbyBookLockRoot;

    [SerializeField]
    private GameObject PresentRoot;


    [SerializeField]
    private GameObject DefaultRoot;


    [SerializeField]
    private Image WeaponImg;

    [SerializeField]
    private Button BookBtn;

    private LobbyBookState BookState = LobbyBookState.Default;


    private int WeaponIdx = 0;

    private int WeaponGrade = 0;

    public LobbyBookState GetBookState { get { return BookState; } }


    void Awake()
    {
        BookBtn.onClick.AddListener(OnClickBook);
    }

    public void Set(int weaponidx, int grade)
    {
        WeaponIdx = weaponidx;
        WeaponGrade = grade;

        var td = Tables.Instance.GetTable<EquipInfo>().GetData(weaponidx);

        if (td != null)
        {
            WeaponImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_CommonWeapon, $"Common_Weapon_Type_{weaponidx}_{grade}");
            SetState();
        }
    }

    public void SetState()
    {
        var equipweaponcount = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.EQUIPWEAPONCOUNT, $"{WeaponIdx}_{WeaponGrade}");
        var rewardbookweaponcount = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.REWARDBOOKWEAPONCOUNT, $"{WeaponIdx}_{WeaponGrade}");

        if(equipweaponcount > 0 && rewardbookweaponcount > 0)
        {
            BookState = LobbyBookState.Default;
        }
        else if(equipweaponcount > 0)
        {
            BookState = LobbyBookState.Present;
        }
        else
        {
            BookState = LobbyBookState.Lock;
        }

        ProjectUtility.SetActiveCheck(LobbyBookLockRoot, BookState == LobbyBookState.Lock);
        ProjectUtility.SetActiveCheck(PresentRoot, BookState == LobbyBookState.Present);
        ProjectUtility.SetActiveCheck(DefaultRoot, BookState == LobbyBookState.Default);
        
    }


    public void OnClickBook()
    {
        if (PresentRoot.gameObject.activeSelf)
        {
            ProjectUtility.RewardGoodsEffect((int)Config.RewardType.Currency, (int)Config.CurrencyID.Material, 3, transform.position);

            GameRoot.Instance.UserData.AddRecordCount(Config.RecordCountKeys.REWARDBOOKWEAPONCOUNT, 1, $"{WeaponIdx}_{WeaponGrade}");

            Set(WeaponIdx, WeaponGrade);

            GameRoot.Instance.GameNotification.UpdateNotification(GameNotificationSystem.NotificationCategory.CardBook);
        }
    }
}

