using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

[UIPath("UI/Page/PopupCardUpgrade")]
public class PopupCardUpgrade : CommonUIBase
{
    [SerializeField]
    private List<CardStatusComponent> CardStatusComponents = new List<CardStatusComponent>();

    [SerializeField]
    private List<GameObject> CardUpgradeEffectList = new List<GameObject>();

    [SerializeField]
    private CardComponent CardComponent;

    [SerializeField]
    private Button UpgradeBtn;

    [SerializeField]
    private Button EquipBtn;

    [SerializeField]
    private List<CardUpgradeAbilityComponent> CardUpgradeAbilityComponents = new List<CardUpgradeAbilityComponent>();

    [SerializeField]
    private Image CardImg;

    private CardData CardData = null;

    private int CardIdx = 0;

    private int NeedCard = 0;

    protected override void Awake()
    {
        base.Awake();

        UpgradeBtn.onClick.AddListener(OnClickUpgrade);
        EquipBtn.onClick.AddListener(OnClickEquip);
    }

    public void Set(int cardidx)
    {
        CardIdx = cardidx;

        CardComponent.Set(cardidx);

        SetStatus();

        CardData = GameRoot.Instance.UserData.Carddatas.Find(x => x.Cardidx == cardidx);

        if (CardData == null) return;

        var cardupgradleveltd = Tables.Instance.GetTable<CardUpgradeLevel>().GetData(CardData.Cardlevel.Value);

        if (cardupgradleveltd == null) return;

        NeedCard = cardupgradleveltd.need_card;

        UpgradeBtn.interactable = CardData == null ? false : CardData.Cardcount.Value >= NeedCard;

        CardImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, $"Common_Card_Icon_{CardIdx}");


        foreach (var cardupgrade in CardUpgradeAbilityComponents)
        {
            ProjectUtility.SetActiveCheck(cardupgrade.gameObject, false);
        }

        var td = Tables.Instance.GetTable<CardInfo>().GetData(cardidx);

        for (int i = 0; i < td.card_ability_type.Count; ++i)
        {
            ProjectUtility.SetActiveCheck(CardUpgradeAbilityComponents[i].gameObject, true);
            CardUpgradeAbilityComponents[i].Set(CardIdx, td.card_ability_level[i]);
        }

        foreach (var cardobj in CardUpgradeEffectList)
        {
            ProjectUtility.SetActiveCheck(cardobj, false);
        }

        ProjectUtility.SetActiveCheck(EquipBtn.gameObject, !CardData.Isequip.Value);

    }


    public void SetStatus()
    {
        var cardinfotd = Tables.Instance.GetTable<CardInfo>().GetData(CardIdx);

        if (cardinfotd != null)
        {
            for (int i = 0; i < CardStatusComponents.Count; i++)
            {
                ProjectUtility.SetActiveCheck(CardStatusComponents[i].gameObject, false);
            }


            for (int i = 0; i < cardinfotd.card_upgrade_type.Count; i++)
            {
                ProjectUtility.SetActiveCheck(CardStatusComponents[i].gameObject, true);
                CardStatusComponents[i].Set(CardIdx, cardinfotd.card_upgrade_type[i]);
            }
        }
    }


    public void OnClickUpgrade()
    {
        if (CardData == null) return;

        if (CardData.Cardcount.Value >= NeedCard)
        {
            CardData.Cardcount.Value -= NeedCard;
            CardData.Cardlevel.Value++;
            Set(CardIdx);


            foreach (var cardobj in CardUpgradeEffectList)
            {
                ProjectUtility.SetActiveCheck(cardobj, true);
            }


            GameRoot.Instance.GameNotification.UpdateNotification(GameNotificationSystem.NotificationCategory.CardUpgrade);
        }
    }


    public void OnClickEquip()
    {
        if (CardData == null) return;

        GameRoot.Instance.UISystem.GetUI<PageLobbyCard>().EquipModeCheck(CardData);
        Hide();
    }

}

