using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanpoFri;
using System.Linq;
using TMPro;

[UIPath("UI/Popup/PageGachaSkillCard")]
public class PageGachaSkillCard : CommonUIBase
{
    [SerializeField]
    private List<GachaSkillCardComponent> CardComponents = new();
    [SerializeField]
    private GachaSkillCardComponent CardCompForOne;
    private readonly Dictionary<int, int> CardsToReward = new();

    [SerializeField]
    private TextMeshProUGUI CardInfoDescText;

    private bool RefreshPageOnClose = false;

    private int SelectCardIdx = 0;

    public GameObject GetClaimBtn()
    {
        return closeBtn.gameObject;
    }

    public void ChoiceCard(int selectcardidx, int cardCount)
    {
        SelectCardIdx = selectcardidx;

        CardsToReward.Clear();
        string animStateName = cardCount == 1 ? "Show" : "Show_10";
        activeAnimator.Play(animStateName, 0, 0);

        RefreshPageOnClose = GameRoot.Instance.TutorialSystem.IsActive(TutorialSystem.Tuto_2);

        // 선택할 카드 개수만큼 카드를 보여줌
        ProjectUtility.SetActiveCheck(CardCompForOne.gameObject, false);

        var getcarddata = Tables.Instance.GetTable<CardInfo>().GetData(selectcardidx);


        for (int i = 0; i < CardComponents.Count; i++)
        {
            if (i >= cardCount)
            {
                ProjectUtility.SetActiveCheck(CardComponents[i].gameObject, false);
                continue;
            }

            ProjectUtility.SetActiveCheck(CardComponents[i].gameObject, true);


            // selectcardidx가 유효하고 첫 번째 카드일 경우 해당 카드를 사용


            if (getcarddata != null)
            {
                var finddata = GameRoot.Instance.UserData.Carddatas.Find(x => x.Cardidx == getcarddata.card_idx);

                // 카드 설정
                CardComponents[i].Set(getcarddata.card_idx, finddata == null);

                // 보상 목록에 추가
                AddCardToReward(getcarddata.card_idx);

                CardInfoDescText.text = Tables.Instance.GetTable<Localize>().GetString($"str_card_desc_{getcarddata.card_idx}");
            }
        }
    }

    private void OnCardSelected(int selectedCardIdx)
    {
        // 선택한 카드만 보상으로 추가
        CardsToReward.Clear();
        AddCardToReward(selectedCardIdx);

        // 선택 효과음
        SoundPlayer.Instance.PlaySound("button");

        // 페이지 닫기
        Hide();
    }

    public void Init(int openCount)
    {
        CardsToReward.Clear();
        string animStateName = openCount == 1 ? "Show" : "Show_10";
        activeAnimator.Play(animStateName, 0, 0);

        RefreshPageOnClose = GameRoot.Instance.TutorialSystem.IsActive(TutorialSystem.Tuto_2);

        if (openCount == 1)
        {
            foreach (var comp in CardComponents) ProjectUtility.SetActiveCheck(comp.gameObject, false);
            ProjectUtility.SetActiveCheck(CardCompForOne.gameObject, true);

            var piccard = PickCard(CardCompForOne);
            SetTexts(piccard);
            AddCardToReward(piccard.card_idx);
            CardCompForOne.Set(piccard.card_idx, false);
            CardInfoDescText.text = Tables.Instance.GetTable<Localize>().GetString($"str_card_desc_{piccard.card_idx}");
        }
        else
        {
            for (int i = 0; i < CardComponents.Count; i++)
            {
                if (i >= openCount)
                {
                    ProjectUtility.SetActiveCheck(CardComponents[i].gameObject, false);
                    continue;
                }
                ProjectUtility.SetActiveCheck(CardComponents[i].gameObject, true);

                var getcarddata = PickCard(CardComponents[i]);
                if (getcarddata != null)
                {
                    var finddata = GameRoot.Instance.UserData.Carddatas.Find(x => x.Cardidx == getcarddata.card_idx);

                    CardComponents[i].Set(getcarddata.card_idx, finddata == null);

                    AddCardToReward(getcarddata.card_idx);

                    CardInfoDescText.text = Tables.Instance.GetTable<Localize>().GetString($"str_card_desc_{getcarddata.card_idx}");
                }
            }
        }
    }

    private void SetTexts(CardInfoData cardToShow)
    {
        // int cardIndex = cardToShow.idx;
        // var cardData = GameRoot.Instance.CardSystem.FindCardData(cardIndex);
        // int cardLevel = cardData == null ? 1 : cardData.Level;
        // var cardRatio = GameRoot.Instance.CardSystem.GetCardRatioValue(cardIndex, cardLevel);
        // var cardValue = GameRoot.Instance.CardSystem.GetCardValue(cardIndex, cardLevel);

        // if (cardRatio >= 100) DescText.text = Tables.Instance.GetTable<Localize>().GetFormat(cardToShow.desc, cardValue);
        // else DescText.text = Tables.Instance.GetTable<Localize>().GetFormat(cardToShow.desc, cardRatio, cardValue);
    }

    private CardInfoData PickCard(GachaSkillCardComponent card)
    {
        var tdlist = Tables.Instance.GetTable<CardInfo>().DataList.ToList();

        var stageidx = GameRoot.Instance.UserData.Stageidx.Value;

        var carddatas = GameRoot.Instance.UserData.Carddatas;

        // 현재 스테이지로 언락된 카드들만 필터링
        var unlockedCards = tdlist.Where(x => x.unlock_stage <= stageidx && x.unlock_type == 1).ToList();

        if (unlockedCards.Count == 0)
            return null;

        // 사용자가 가지고 있는 카드의 인덱스 목록
        var ownedCardIndices = carddatas.Select(x => x.Cardidx).ToHashSet();

        // 우선순위: 아직 가지지 않은 언락된 카드들
        var notOwnedCards = unlockedCards.Where(x => !ownedCardIndices.Contains(x.card_idx)).ToList();

        // 가지지 않은 카드와 모든 언락된 카드를 합쳐서 뽑기 (가지지 않은 카드는 2배 확률)
        var cardPool = new List<CardInfoData>(unlockedCards);
        cardPool.AddRange(notOwnedCards);

        CardInfoData selectedCard = cardPool[Random.Range(0, cardPool.Count)];

        return selectedCard;
    }

    public void AddCardToReward(int cardidx)
    {
        if (CardsToReward.ContainsKey(cardidx))
        {
            CardsToReward[cardidx] += 1;
        }
        else
        {
            CardsToReward.Add(cardidx, 1);
        }
    }

    public override void OnShowAfter()
    {
        base.OnShowAfter();
    }

    public override void Hide()
    {
        RewardCard();
        base.Hide();


        if (SelectCardIdx == 103 && !GameRoot.Instance.TutorialSystem.IsClearTuto("6"))
        {
            GameRoot.Instance.UISystem.GetUI<HUDTotal>()?.CheckContentsOpen_Card(true);
        }



        if (RefreshPageOnClose)
        {
            GameRoot.Instance.WaitRealTimeAndCallback(0.1f, () =>
            {
                var page = GameRoot.Instance.UISystem.GetUI<PageLobbyCard>();
                page.Init();
            });
        }
    }

    public override void OnHideAfter()
    {
        base.OnHideAfter();

        GameRoot.Instance.GameNotification.UpdateNotification(GameNotificationSystem.NotificationCategory.CardUpgrade);
    }

    private void RewardCard()
    {
        foreach (var card in CardsToReward)
            GameRoot.Instance.CardSystem.AddCard(card.Key, card.Value);

        CardsToReward.Clear();
    }

    public CardInfoData GetRandomCardData()
    {
        return null;
    }

}

