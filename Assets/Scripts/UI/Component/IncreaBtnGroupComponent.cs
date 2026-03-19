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

    [SerializeField]
    private List<GameObject> GroupComponentList = new List<GameObject>();

    [SerializeField]
    private EquipmnentUpgradeGroup EquipmentUpgradeGroup;

    [SerializeField]
    private IncreaseUpgradeGroup IncreaseUpgradeGroup;


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

        // 처음 시작할 때 IncreaseUpgrade 탭을 기본으로 열어 준다.
        // 1. 버튼 비주얼 상태 세팅
        for (int i = 0; i < HudBottomBtnList.Count; ++i)
        {
            HudBottomBtnList[i].SetActive(HudBottomBtnList[i].CurBtnType != HudBottomBtnType.IncreaseUpgrade);
        }

        // 2. 내부 상태 갱신 및 실제 페이지 오픈
        CurrentlyOpenPage = HudBottomBtnType.Done; // 강제로 다른 상태로 만들어서 OpenPage가 항상 실행되게
        OpenPage(HudBottomBtnType.IncreaseUpgrade, true); // 첫 진입은 무조건 강제 오픈

        // 3. Animator / 레이아웃 초기화 타이밍 문제 대비용으로 한 프레임 뒤에 한 번 더 보정
        StartCoroutine(OpenPageNextFrame(HudBottomBtnType.IncreaseUpgrade));
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

        foreach(var obj in GroupComponentList)
        {
            obj.SetActive(false);
        }

        // PreLoadUI로 로드된 UI를 활성화 (OnEnable에서 Show 애니메이션 자동 재생)
        switch (type)
        {
            case HudBottomBtnType.IncreaseUpgrade:
                {
                    GroupComponentList[(int)type].SetActive(true);
                    IncreaseUpgradeGroup.Init();
                }
                break;
            case HudBottomBtnType.EquipmentItemUpgrade:
                {
                    GroupComponentList[(int)type].SetActive(true);
                    EquipmentUpgradeGroup.Init();
                }
                break;
        }

        CurrentlyOpenPage = type;
    }



}

