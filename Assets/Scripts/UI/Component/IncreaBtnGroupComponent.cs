using UnityEngine;
using BanpoFri;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class IncreaBtnGroupComponent : MonoBehaviour
{

    [SerializeField]
    private List<HudBottomBtnComponent> HudBottomBtnList = new List<HudBottomBtnComponent>();

    public HudBottomBtnType CurrentlyOpenPage = HudBottomBtnType.Done;


    public void UpdateButtonLock()
    {
    }

    public void OnClickHudBottomBtn(HudBottomBtnType type, bool isopen)
    {
        OpenPage(type);
    }

    void OnEnable()
    {
        foreach (var item in HudBottomBtnList)
        {   
            item.Set(OnClickHudBottomBtn);
        }

        // 처음 시작할 때 IncreaseUpgrade 활성화
        for (int i = 0; i < HudBottomBtnList.Count; ++i)
        {
            HudBottomBtnList[i].SetActive(HudBottomBtnList[i].CurBtnType != HudBottomBtnType.IncreaseUpgrade);
        }
        CurrentlyOpenPage = HudBottomBtnType.IncreaseUpgrade;

        // 첫 킬 때는 Animator가 아직 갱신 전이라 Play가 먹지 않을 수 있음 → 한 프레임 뒤에 OpenPage
        GameRoot.Instance.StartCoroutine(OpenPageNextFrame(HudBottomBtnType.IncreaseUpgrade));
    }

    IEnumerator OpenPageNextFrame(HudBottomBtnType type)
    {
        yield return null; // 한 프레임 대기
        OpenPage(type); 
    }


    public void OpenPage(HudBottomBtnType type, bool forceOpen = false)
    {
        if (type == CurrentlyOpenPage && !forceOpen) return;


        for (int i = 0; i < HudBottomBtnList.Count; ++i)
        {
            HudBottomBtnList[i].SetActive(HudBottomBtnList[i].CurBtnType != type);
        }

        // PreLoadUI로 로드된 UI를 활성화 (OnEnable에서 Show 애니메이션 자동 재생)
        switch (type)
        {   
            case HudBottomBtnType.IncreaseUpgrade:
            {
                
            }
            break;
        }

        CurrentlyOpenPage = type;
    }



}

