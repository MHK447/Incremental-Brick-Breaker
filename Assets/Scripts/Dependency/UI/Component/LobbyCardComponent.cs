using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using UniRx;
using DG.Tweening;

public class LobbyCardComponent : MonoBehaviour
{
    [SerializeField]
    private Image LockCardImg;

    [SerializeField]
    private Image BgImg;

    [SerializeField]
    private Image CardImg;

    [SerializeField]
    private TextMeshProUGUI CardCountText;

    [SerializeField]
    private TextMeshProUGUI CardUnLockText;

    [SerializeField]
    private TextMeshProUGUI CardLevelText;

    [SerializeField]
    private TextMeshProUGUI CardNameText;


    [SerializeField]
    private Slider CardProgress;

    [SerializeField]
    private GameObject LockObj;

    [SerializeField]
    private Button CardBtn;

    [SerializeField]
    private GameObject RedDotObj;

    void Awake()
    {
        CardBtn.onClick.AddListener(OnClickCard);
    }

    private int CardIdx = 0;

    private CompositeDisposable disposables = new CompositeDisposable();

    private CardData CardData = null;

    public void Set(int cardidx)
    {
        CardIdx = cardidx;

        var td = Tables.Instance.GetTable<CardInfo>().GetData(CardIdx);

        if (td != null)
        {
            LockCardImg.sprite = CardImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, td.icon);

            CardSetInfo();

            BgImg.sprite = td.card_type == 2 ? AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, "Common_Card_Frame_Weapon")
           : AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, "Common_Card_Frame_Unit");


            if (CardUnLockText != null)
                CardUnLockText.text = td.unlock_type == 2 ? "???" : $"STAGE {td.unlock_stage}";

            if (CardData != null)
            {
                disposables.Clear();

                CardData.Cardlevel.Subscribe(x => { CardSetInfo(); }).AddTo(disposables);
                CardData.Cardcount.Subscribe(x => { CardSetInfo(); }).AddTo(disposables);
            }

            var level = CardData == null ? 1 : CardData.Cardlevel.Value;

            if (CardLevelText != null)
                CardLevelText.text = $"Lv.{level}";

            if (CardNameText != null)
                CardNameText.text = Tables.Instance.GetTable<Localize>().GetString(td.card_name);
        }

    }


    public void CardSetInfo()
    {
        CardData = GameRoot.Instance.UserData.Carddatas.Find(x => x.Cardidx == CardIdx);


        CardProgress.value = 0f;

        if (CardData == null)
        {
            ProjectUtility.SetActiveCheck(LockObj, true);
            ProjectUtility.SetActiveCheck(RedDotObj, false);
            return;
        }

        ProjectUtility.SetActiveCheck(LockObj, false);

        var cardleveltd = Tables.Instance.GetTable<CardUpgradeLevel>().GetData(CardData.Cardlevel.Value);

        CardProgress.value = 0;

        if (cardleveltd != null)
        {
            CardProgress.value = (float)CardData.Cardcount.Value / (float)cardleveltd.need_card;

            CardCountText.text = $"{CardData.Cardcount.Value}/{cardleveltd.need_card}";

            CardLevelText.text = $"Lv.{CardData.Cardlevel.Value}";
        }

        ProjectUtility.SetActiveCheck(RedDotObj, CardData.Cardcount.Value >= cardleveltd.need_card);


    }


    public void OnClickCard()
    {
        if (CardData == null)
        {
            return;
        }

        if (GameRoot.Instance.CardSystem.ChangeEquipCardData != null && CardData.Isequip.Value)
        {
            var list = GameRoot.Instance.UserData.Carddatas;
            var equipCardData = GameRoot.Instance.CardSystem.ChangeEquipCardData;
            int idxClicked = list.IndexOf(CardData);
            int idxEquip = list.IndexOf(equipCardData);
            if (idxClicked >= 0 && idxEquip >= 0)
            {
                var temp = list[idxClicked];
                list[idxClicked] = list[idxEquip];
                list[idxEquip] = temp;
            }

            CardData.Isequip.Value = false;
            equipCardData.Isequip.Value = true;
            GameRoot.Instance.CardSystem.ChangeEquipCardData = null;
            EquipModeCheck();
            return;
        }
        else
        {
            GameRoot.Instance.UISystem.OpenUI<PopupCardUpgrade>(popup => popup.Set(CardIdx));
        }
    }


    public void EquipModeCheck()
    {
        GameRoot.Instance.UISystem.GetUI<PageLobbyCard>().CardInfoSet();
        GameRoot.Instance.UISystem.GetUI<PageLobbyCard>().EquipModeOff();
    }


    void OnDisable()
    {
        disposables.Clear();
    }

    void OnDestroy()
    {
        disposables.Clear();
    }

}

