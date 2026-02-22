using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UniRx;


public class LobbyBattleBoxComponent : MonoBehaviour
{
    [SerializeField]
    private List<StageRewardBoxComponent> StageRewardBoxComponents = new List<StageRewardBoxComponent>();

    private CompositeDisposable disposables = new CompositeDisposable();



    public void Init()
    {
        disposables.Clear();

        foreach (var stagerewardboxcomponent in StageRewardBoxComponents)
        {
            stagerewardboxcomponent.Clear();
        }

        SetRewardBox();

        GameRoot.Instance.UserData.Stagerewardboxgroup.Stagerewardboxdatas.ObserveAdd().Subscribe(x =>
        {
            SetRewardBox();
        }).AddTo(disposables);

        GameRoot.Instance.UserData.Stagerewardboxgroup.Stagerewardboxdatas.ObserveRemove().Subscribe(x =>
       {
           SetRewardBox();
       }).AddTo(disposables);

    }


    public void SetRewardBox()
    {
        foreach (var stagerewardboxcomponent in StageRewardBoxComponents)
        {
            stagerewardboxcomponent.Clear();
        }

        
        for (int i = 0; i < GameRoot.Instance.UserData.Stagerewardboxgroup.Stagerewardboxdatas.Count; ++i)
        {
            var boxData = GameRoot.Instance.UserData.Stagerewardboxgroup.Stagerewardboxdatas[i];
            // BoxOrder는 1부터 시작하므로 -1을 해서 0 기반 인덱스로 변환
            int slotIndex = boxData.BoxOrder - 1;
            
            // 유효한 슬롯 범위 체크
            if (slotIndex >= 0 && slotIndex < StageRewardBoxComponents.Count)
            {
                StageRewardBoxComponents[slotIndex].Set(boxData.Boxidx, i);
            }
        }
    }
}

