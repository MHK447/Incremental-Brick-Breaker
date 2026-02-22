using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

[UIPath("UI/Popup/PopupWelComeBack", true)]
public class PopupWelComeBack : UIBase
{



    [SerializeField]
    private Button ReLoadBtn;

    [SerializeField]
    private TextMeshProUGUI WaveText;


    protected override void Awake()
    {
        base.Awake();

        ReLoadBtn.onClick.AddListener(OnClickReLoad);
    }

    public void Init()
    {
        var stagetd = Tables.Instance.GetTable<WaveInfo>().DataList.FindAll(x => x.stage == GameRoot.Instance.UserData.Stageidx.Value).Count;

        WaveText.text = Tables.Instance.GetTable<Localize>().GetString("str_desc_wave") +
        $"{GameRoot.Instance.UserData.Waveidx.Value}/{stagetd}";
    }

    private void OnClickReLoad()
    {
        var lobbyPage = GameRoot.Instance.UISystem.GetUI<PageLobbyBattle>();
        lobbyPage?.Hide();

        List<TpParameter> parameters = new List<TpParameter>();
        parameters.Add(new TpParameter("Stage", GameRoot.Instance.UserData.Stageidx.Value));
        GameRoot.Instance.PluginSystem.AnalyticsProp.AllEvent(IngameEventType.None, "m_stage_start", parameters);

        bool resumeStarted = GameRoot.Instance.UserData.StartBattleWithInGameResumeData();
        if (!resumeStarted)
        {
            GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.StartBattle();
        }


        if (!GameRoot.Instance.ShopSystem.NoInterstitialAds.Value)
        {
            GameRoot.Instance.PluginSystem.ShowBanner(MaxSdkBase.BannerPosition.BottomCenter);
        }

        Hide();
    }
}

