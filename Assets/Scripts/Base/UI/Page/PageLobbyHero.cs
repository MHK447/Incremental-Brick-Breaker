using UnityEngine;
using BanpoFri;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using UniRx;


[UIPath("UI/Page/PageLobbyHero")]
public class PageLobbyHero : CommonUIBase
{
    public enum State
    {
        Set = 0,
        ALL = 1,
        HELMAT = 2,
        AROMOR = 3,
        SHOOSE = 4,
        RING = 5,
    }


    [SerializeField]
    private Image UnitImg;

    [SerializeField]
    private List<TextMeshProUGUI> HeroAbilityValueTextList = new List<TextMeshProUGUI>();

    [SerializeField]
    private Button MergeBtn;

    [SerializeField]
    private Button UnEquipAllBtn;

    [SerializeField]
    private List<EquipItemComponent> EquipItemComponentList = new List<EquipItemComponent>();

    [SerializeField]
    private List<Toggle> Tabs = new List<Toggle>();

    [SerializeField]
    private ScrollRect ScrollRect;

    [SerializeField]
    private List<HeroEquipSetComponent> HeroEquipSetComponentList = new List<HeroEquipSetComponent>();

    [SerializeField]
    private List<Transform> HeroEquipHeaderList = new List<Transform>();

    [SerializeField]
    private EquipmentMaterialGroup EquipmentMaterialGroup;

    [SerializeField]
    private TextMeshProUGUI HeroNameText;

    [SerializeField]
    private GameObject MergeReddotObj;

    private int CurTab = 1;

    private CompositeDisposable disposables = new CompositeDisposable();


    override protected void Awake()
    {
        base.Awake();
        MergeBtn.onClick.AddListener(OnClickMerge);
        UnEquipAllBtn.onClick.AddListener(OnClickUnEquipAll);

        var idx = 0;
        foreach (var btn in Tabs)
        {
            var tabIdx = idx;
            btn.onValueChanged.AddListener((on) => { OnChangeTab(tabIdx, on); });
            // 명시적으로 탭 상태 초기화 (첫 번째만 true, 나머지는 false)
            btn.isOn = (tabIdx == 0);
            idx++;
        }
    }

    public void Init()
    {
        var heroidx = GameRoot.Instance.UserData.Herogroudata.Equipplayeridx;

        var td = Tables.Instance.GetTable<HeroInfo>().GetData(heroidx);
        UnitImg.sprite = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_EquipItem, td.image);
        HeroAbilitySet();

        foreach (var item in EquipItemComponentList)
        {
            item.Init();
        }

        GameRoot.Instance.StartCoroutine(RebuildLayoutDelayed());

        EquipmentMaterialGroup.Init();

        HeroNameText.text = Tables.Instance.GetTable<Localize>().GetString($"str_hero_name_{heroidx}");


        disposables.Clear();

        foreach (var herodata in GameRoot.Instance.UserData.Herogroudata.Equipheroitems)
        {
            herodata.Isequip.Subscribe(on =>
            {
                HeroAbilitySet();
            }).AddTo(disposables);
        }

        MergeReddotCheck();
    }

    private IEnumerator RebuildLayoutDelayed()
    {
        // Canvas 업데이트 강제
        Canvas.ForceUpdateCanvases();

        // 한 프레임 대기
        yield return null;

        // 레이아웃 재구성
        LayoutRebuilder.ForceRebuildLayoutImmediate(ScrollRect.content);

        // 추가로 한 프레임 더 대기 후 다시 재구성
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(ScrollRect.content);
    }


    public override void OnShowBefore()
    {
        base.OnShowBefore();

        if (!Tabs[1].isOn)
        {
            Tabs[1].isOn = true;
        }
        else
        {
            var ani = Tabs[1].gameObject.GetComponent<Animator>();
            if (ani != null)
            {
                ani.SetTrigger("Selected");
            }
            OnChangeTab(1, true);
        }

        // 스크롤 뷰 레이아웃 재구성
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


    private void OnChangeTab(int tabIdx, bool on)
    {
        if (on)
        {
            CurTab = tabIdx;
        }

        var ani = Tabs[tabIdx].gameObject.GetComponent<Animator>();
        if (ani != null)
        {
            if (on)
            {
                ChangeSetState((State)tabIdx);

                if (!Tabs[(int)tabIdx].isOn)
                    Tabs[(int)tabIdx].isOn = true;
                ani.SetTrigger("Selected");
            }
            else
                ani.SetTrigger("Normal");
        }
    }

    public void HeroAbilitySet()
    {
        HeroAbilityValueTextList[0].text = GameRoot.Instance.HeroSystem.GetHeroStatusValue(HeroStatus.Atttack).ToString();
        HeroAbilityValueTextList[1].text = GameRoot.Instance.HeroSystem.GetHeroStatusValue(HeroStatus.Hp).ToString();
        HeroAbilityValueTextList[2].text = GameRoot.Instance.HeroSystem.GetHeroStatusValue(HeroStatus.AtkSpeed).ToString();
    }


    public void ChangeSetState(State state)
    {
        foreach (var setcomponent in HeroEquipSetComponentList)
        {
            ProjectUtility.SetActiveCheck(setcomponent.gameObject, false);
        }

        foreach (var header in HeroEquipHeaderList)
        {
            ProjectUtility.SetActiveCheck(header.gameObject, false);
        }

        CurTab = (int)state;

        switch (state)
        {
            case State.Set:
                {
                    var tdlist = Tables.Instance.GetTable<HeroItemSet>().DataList.ToList();

                    for (int i = 0; i < tdlist.Count; i++)
                    {
                        ProjectUtility.SetActiveCheck(HeroEquipHeaderList[i].gameObject, true);
                        ProjectUtility.SetActiveCheck(HeroEquipSetComponentList[i].gameObject, true);
                        var heroequipsetcomponent = HeroEquipSetComponentList[i];
                        heroequipsetcomponent.Set(tdlist[i].set_idx, HeroEquipSetComponent.EquipState.Set);
                    }
                }
                break;
            case State.ALL:
                {
                    var tdlist = Tables.Instance.GetTable<HeroItemSet>().DataList.ToList();

                    for (int i = 0; i < tdlist.Count; i++)
                    {
                        var finddata = GameRoot.Instance.UserData.Herogroudata.Heroitemdatas.Find(x =>
                        Tables.Instance.GetTable<HeroItemInfo>().GetData(x.Heroitemidx).item_set_type == tdlist[i].set_idx);

                        if (finddata != null)
                        {
                            var heroequipsetcomponent = HeroEquipSetComponentList[i];
                            ProjectUtility.SetActiveCheck(HeroEquipHeaderList[i].gameObject, true);
                            ProjectUtility.SetActiveCheck(HeroEquipSetComponentList[i].gameObject, true);
                            heroequipsetcomponent.Set(tdlist[i].set_idx, HeroEquipSetComponent.EquipState.SetEquipItem);
                        }
                    }
                }
                break;
            case State.HELMAT:
                {
                    ProjectUtility.SetActiveCheck(HeroEquipSetComponentList[0].gameObject, true);
                    HeroEquipSetComponentList[0].Set(-1, HeroEquipSetComponent.EquipState.Helmet);
                }
                break;
            case State.AROMOR:
                {
                    ProjectUtility.SetActiveCheck(HeroEquipSetComponentList[0].gameObject, true);
                    HeroEquipSetComponentList[0].Set(-1, HeroEquipSetComponent.EquipState.Armor);
                }
                break;
            case State.SHOOSE:
                {
                    ProjectUtility.SetActiveCheck(HeroEquipSetComponentList[0].gameObject, true);
                    HeroEquipSetComponentList[0].Set(-1, HeroEquipSetComponent.EquipState.Shoose);
                }
                break;
            case State.RING:
                {
                    ProjectUtility.SetActiveCheck(HeroEquipSetComponentList[0].gameObject, true);
                    HeroEquipSetComponentList[0].Set(-1, HeroEquipSetComponent.EquipState.Ring);
                }
                break;
        }

        // 레이아웃 재구성
        GameRoot.Instance.StartCoroutine(RebuildLayoutDelayed());
    }


    public void SetItem(int itemidx)
    {
        var td = Tables.Instance.GetTable<HeroItemInfo>().GetData(itemidx);
        if (td != null)
        {
            EquipItemComponentList[td.item_equip_type].Init();
        }
    }

    public void MergeReddotCheck()
    {
        bool isreddot = false;
        // merge available with 3+ same idx/grade
        var heroItems = GameRoot.Instance.UserData.Herogroudata.Heroitemdatas;
        if (heroItems != null)
        {
            var canMerge = heroItems
                .GroupBy(item => new { item.Heroitemidx, item.Grade })
                .Any(group => group.Count() >= 3);

            if (canMerge)
            {
                isreddot = true;
            }
        }
        ProjectUtility.SetActiveCheck(MergeReddotObj, isreddot);
    }


    public void OnClickMerge()
    {
        var heroItems = GameRoot.Instance.UserData.Herogroudata.Heroitemdatas;

        List<HeroItemData> equipmentheroitems = new List<HeroItemData>();

        // 같은 idx와 grade를 가진 아이템들을 그룹화 (3개 이상 필요)
        var groupedItems = heroItems
            .GroupBy(item => new { item.Heroitemidx, item.Grade })
            .Where(group => group.Count() >= 3)
            .ToList();

        if (groupedItems.Count == 0)
        {
            return;
        }

        // 각 그룹에서 3개씩 병합
        foreach (var group in groupedItems)
        {
            var itemsList = group.ToList();

            // 장착된 아이템이 있으면 그것을 업그레이드 대상으로, 없으면 첫 번째 아이템을 선택
            var itemToUpgrade = itemsList.FirstOrDefault(item => item.Isequip.Value) ?? itemsList[0];

            // 업그레이드할 아이템을 제외한 나머지 2개를 제거 대상으로 선택
            var itemsToRemove = itemsList.Where(item => item != itemToUpgrade).Take(2).ToList();

            // 2개의 아이템을 제거
            foreach (var itemToRemove in itemsToRemove)
            {
                // 장착된 아이템인지 확인하고 장착 해제
                if (itemToRemove.Isequip.Value)
                {
                    var equipSlot = GameRoot.Instance.UserData.Herogroudata.Equipheroitems
                        .Find(x => x.Heroitemdata == itemToRemove);
                    if (equipSlot != null)
                    {
                        equipSlot.Isequip.Value = false;
                        equipSlot.Heroitemdata = null;
                    }
                }

                heroItems.Remove(itemToRemove);
            }

            itemToUpgrade.Grade = group.Key.Grade + 1;
            equipmentheroitems.Add(itemToUpgrade);
        }

        GameRoot.Instance.UISystem.OpenUI<PageEquipmentUpgrade>(popup => popup.Set(equipmentheroitems), () =>
        {

            Init();
            ChangeSetState((State)CurTab);
        });

    }

    public void OnClickUnEquipAll()
    {
        foreach (var item in GameRoot.Instance.UserData.Herogroudata.Equipheroitems)
        {
            item.Isequip.Value = false;

            if (item.Heroitemdata != null)
            {
                item.Heroitemdata.Isequip.Value = false;
                item.Heroitemdata = null;
            }
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

