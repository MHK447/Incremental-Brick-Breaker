using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using System;
using TMPro;

[UIPath("UI/Popup/PopupCardBook")]
public class PopupCardBook : CommonUIBase
{
    [SerializeField]
    private Button CloseBtn;

    [SerializeField]
    private List<GameObject> BookList = new List<GameObject>();

    [SerializeField]
    private GameObject BookPrefab;

    [SerializeField]
    private Transform BookRoot;

    [SerializeField]
    private Slider CardProgress;

    [SerializeField]
    private TextMeshProUGUI CardCountText;


    protected override void Awake()
    {
        base.Awake();
        CloseBtn.onClick.AddListener(OnClickClose);
    }



    public void Init()
    {
        var tdlist = Tables.Instance.GetTable<EquipInfo>().DataList.FindAll(x => x.item_type != 3).ToList();


        foreach (var book in BookList)
        {
            ProjectUtility.SetActiveCheck(book.gameObject, false);
        }

        var count = 0;

        foreach (var td in tdlist)
        {
            for (int i = 0; i < 4; ++i)
            {
                var book = GetCachedObject().GetComponent<LobbyBookComponent>();
                book.Set(td.idx, i + 1);
                ProjectUtility.SetActiveCheck(book.gameObject, true);

                var equipcheck = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.EQUIPWEAPONCOUNT, $"{td.idx}_{i + 1}");

                if(equipcheck > 0)
                {
                    count++;
                }

            }
        }

        var progresscount = tdlist.Count * 4;

        CardProgress.value =   (float)count  /  (float)progresscount;
        CardCountText.text = $"{count}/{progresscount}";

        // Sort BookList: Present -> Default -> Lock
        var activeBooks = BookList.Where(x => x.gameObject.activeSelf).ToList();
        var sortedBooks = activeBooks.OrderBy(x => 
        {
            var component = x.GetComponent<LobbyBookComponent>();
            switch (component.GetBookState)
            {
                case LobbyBookComponent.LobbyBookState.Present:
                    return 0;
                case LobbyBookComponent.LobbyBookState.Default:
                    return 1;
                case LobbyBookComponent.LobbyBookState.Lock:
                    return 2;
                default:
                    return 3;
            }
        }).ToList();

        // Reorder in hierarchy
        for (int i = 0; i < sortedBooks.Count; i++)
        {
            sortedBooks[i].transform.SetSiblingIndex(i);
        }

    }

    public void OnClickClose()
    {
        Hide();
    }



    private GameObject GetCachedObject()
    {
        var inst = BookList.Find(x => !x.gameObject.activeSelf);
        if (inst == null)
        {
            inst = GameObject.Instantiate(BookPrefab);
            inst.transform.SetParent(BookRoot);
            inst.transform.localScale = UnityEngine.Vector3.one;
            BookList.Add(inst);
        }

        return inst;
    }

}

