using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;

[UIPath("UI/Page/PageGetCharacterReward")]
public class PageGetCharacterReward : CommonUIBase
{

    [SerializeField]
    private Button GetAddCharacterBtn;


    private System.Action NextAction = null;


    protected override void Awake()
    {
        base.Awake();

        GetAddCharacterBtn.onClick.AddListener(OnClickReward);
    }

    public void Set(System.Action nextaction)
    {
        NextAction = nextaction;
        GetAddCharacterBtn.interactable = true;
    }

    public void OnClickReward()
    {
        GetAddCharacterBtn.interactable = false;
        GameRoot.Instance.UserData.Herogroudata.Equipplayeridx = 1;
        Hide();

        GameRoot.Instance.UserData.Herogroudata.AddHeroItem(1, 1);

        GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency, (int)Config.CurrencyID.Money, 50);

        GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Item, 1, 2);


        GameRoot.Instance.ContentsOpenSystem.RegisterOpenWaitContentByStage(ContentsOpenSystem.ContentsOpenType.HeroUpgradeOpen, (action) =>
        {
            GameRoot.Instance.UISystem.GetUI<HUDTotal>()?.CheckContentsOpen_Hero(action);
        });

        GameRoot.Instance.TutorialSystem.OnActiveTutoEnd = NextAction;

        GameRoot.Instance.UserData.Save();
    }

    public void OnClickClose()
    {
        Hide();
        NextAction?.Invoke();
    }
}

