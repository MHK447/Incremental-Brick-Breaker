using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using UniRx;
using System.Collections;

[UIPath("UI/Page/PageLobbyCard")]
public class PageLobbyCard : CommonUIBase
{
    [SerializeField]
    private List<GameObject> LobbyCardList = new List<GameObject>();


    [SerializeField]
    private Transform UnLockCardRoot;

    [SerializeField]
    private Transform LockCardRoot;

    [SerializeField]
    private Transform SelectEquipRoot;

    [SerializeField]
    private Button EquipModeOffBtn;

    [SerializeField]
    private LobbyCardComponent SelectCardSet;

    [SerializeField]
    private GameObject CardPrefab;

    [SerializeField]
    private VerticalLayoutGroup LobbyCardLayout;

    [SerializeField]
    private List<EquipCardComponent> EquipCardComponentList = new List<EquipCardComponent>();


    [SerializeField]
    private Button CardBuyBtn;

    [SerializeField]
    private TextMeshProUGUI CardButtonText;

    [SerializeField]
    private TextMeshProUGUI CardBuyText;

    [SerializeField]
    private ScrollRect ScrollRect;

    private int BuyOneCardPrice = 0;

    private CompositeDisposable disposables = new CompositeDisposable();


    protected override void Awake()
    {
        base.Awake();
        CardBuyBtn.onClick.AddListener(OnClickPurchase);
        EquipModeOffBtn.onClick.AddListener(EquipModeOff);
    }


    public void Init(bool isanim = true)
    {

        BuyOneCardPrice = 20;

        EquipModeOff();

        CardPurchaseInit();

        CardInfoSet();

        disposables.Clear();

        GameRoot.Instance.UserData.Material.Subscribe(x =>
        {
            CardPurchaseInit();
        }).AddTo(disposables);

    }

    public void CardInfoSet()
    {

        foreach (var obj in LobbyCardList)
        {
            ProjectUtility.SetActiveCheck(obj, false);
        }

        var cardlist = Tables.Instance.GetTable<CardInfo>().DataList
            .OrderBy(x => x.unlock_type == 2 ? 1 : 0)
            .ThenBy(x => x.unlock_stage)
            .ToList();

        // 먼저 모든 카드를 정상 스케일로 생성하고 활성화
        List<LobbyCardComponent> cardComponents = new List<LobbyCardComponent>();
        foreach (var card in cardlist)
        {
            var finddata = GameRoot.Instance.UserData.Carddatas.Find(x => x.Cardidx == card.card_idx);

            if (finddata != null && finddata.Isequip.Value) continue;


            var getcachedobj = GetCachedObject(finddata == null).GetComponent<LobbyCardComponent>();

            if (getcachedobj != null)
            {
                // 레이아웃 계산을 위해 반드시 활성화 상태여야 함
                ProjectUtility.SetActiveCheck(getcachedobj.gameObject, true);
                getcachedobj.Set(card.card_idx);
                getcachedobj.transform.localScale = Vector3.one; // 먼저 정상 크기로

                // CanvasGroup으로 투명하게 (레이아웃은 유지)
                var canvasGroup = getcachedobj.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = getcachedobj.gameObject.AddComponent<CanvasGroup>();
                }
                canvasGroup.alpha = 0f; // 보이지 않게

                cardComponents.Add(getcachedobj);
            }
        }

        // DOTween의 DelayedCall을 사용하여 레이아웃이 잡힌 후 애니메이션 시작
        DOVirtual.DelayedCall(0.01f, () =>
        {
            // 다시 한번 레이아웃 갱신
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(LobbyCardLayout.transform as RectTransform);

            // 이제 애니메이션 시작
            int index = 0;
            foreach (var component in cardComponents)
            {
                // 스케일 애니메이션과 알파 애니메이션 동시에
                component.transform.localScale = Vector3.zero;
                float delay = index * 0.05f;
                component.transform.DOScale(Vector3.one, 0.3f)
                    .SetDelay(delay)
                    .SetEase(Ease.OutBack);

                var canvasGroup = component.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.DOFade(1f, 0.3f)
                        .SetDelay(delay)
                        .SetEase(Ease.OutQuad);
                }

                index++;
            }
        });



        var equipcarddatas = GameRoot.Instance.UserData.Carddatas.FindAll(x => x.Isequip.Value);


        for (int i = 0; i < EquipCardComponentList.Count; ++i)
        {
            var cardidx = i < equipcarddatas.Count ? equipcarddatas[i].Cardidx : -1;
            EquipCardComponentList[i].Set(cardidx);
        }


        GameRoot.Instance.StartCoroutine(RebuildLayoutOnShow());
    }

    private IEnumerator RebuildLayoutOnShow()
    {
        // Canvas 업데이트 강제
        Canvas.ForceUpdateCanvases();

        // 한 프레임 대기
        yield return null;

        // 레이아웃 재구성
        LayoutRebuilder.ForceRebuildLayoutImmediate(ScrollRect.content);
    }

    private GameObject GetCachedObject(bool islock)
    {
        var inst = LobbyCardList.Find(x => !x.gameObject.activeSelf);

        if (inst == null)
        {
            inst = GameObject.Instantiate(CardPrefab);
        }

        if (islock)
        {
            inst.transform.SetParent(LockCardRoot);
        }
        else
        {
            inst.transform.SetParent(UnLockCardRoot);
        }

        inst.transform.SetAsLastSibling();


        inst.transform.localScale = UnityEngine.Vector3.one;


        if (!LobbyCardList.Contains(inst))
        {
            LobbyCardList.Add(inst);
        }

        return inst;
    }

    private int GetBuyCount()
    {
        int result = (int)(GameRoot.Instance.UserData.Material.Value / BuyOneCardPrice);
        result = Mathf.Clamp(result, 0, 10);
        return result;
    }


    public void OnClickPurchase()
    {
        if (GameRoot.Instance.TutorialSystem.IsActive("6"))
        {
            int buyCount = GetBuyCount();

            GameRoot.Instance.UISystem.OpenUI<PageGachaSkillCard>(page => page.ChoiceCard(103, 1), () => Init(false));

            GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency, (int)Config.CurrencyID.Material, -BuyOneCardPrice * buyCount);
        }
        else if (GameRoot.Instance.UserData.Material.Value >= BuyOneCardPrice)
        {
            int buyCount = GetBuyCount();

            GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency, (int)Config.CurrencyID.Material, -BuyOneCardPrice * buyCount);

            GameRoot.Instance.UISystem.OpenUI<PageGachaSkillCard>(page => page.Init(buyCount), () =>
            {
                Init(false);
            });
        }
        else
        {
            GameRoot.Instance.UISystem.OpenUI<PopupMaterialInsufficent>();
        }
    }



    public void CardPurchaseInit()
    {
        int buyCount = GetBuyCount();
        int showBuyCount = Mathf.Max(1, buyCount);
        int buyPrice = showBuyCount * BuyOneCardPrice;

        CardBuyText.text = Tables.Instance.GetTable<Localize>().GetFormat("str_card_puchase_1", showBuyCount);

        CardButtonText.text = buyPrice.ToString();
    }


    void OnDestroy()
    {
        disposables.Clear();
    }


    void OnDisable()
    {
        disposables.Clear();
    }


    public void EquipModeCheck(CardData carddata)
    {
        var tdlist = Tables.Instance.GetTable<CardInfo>().GetData(carddata.Cardidx);

        var finddata = EquipCardComponentList.Find(x => x.GetCardIdx == -1);

        if (finddata != null)
        {
            //즉시 장착 
            finddata.Set(carddata.Cardidx);

            carddata.Isequip.Value = true;
        }
        else
        {
            //장착 모드 

            GameRoot.Instance.CardSystem.ChangeEquipCardData = carddata;

            ProjectUtility.SetActiveCheck(SelectEquipRoot.gameObject, true);
            ProjectUtility.SetActiveCheck(UnLockCardRoot.gameObject, false);

            SelectCardSet.Set(carddata.Cardidx);

            foreach (var equipcard in EquipCardComponentList)
            {
                equipcard.ActiveClickObj(true);
            }
        }
    }


    public void EquipModeOff()
    {

        ProjectUtility.SetActiveCheck(SelectEquipRoot.gameObject, false);
        ProjectUtility.SetActiveCheck(UnLockCardRoot.gameObject, true);

        foreach (var equipcard in EquipCardComponentList)
        {
            equipcard.ActiveClickObj(false);
        }
    }

}

