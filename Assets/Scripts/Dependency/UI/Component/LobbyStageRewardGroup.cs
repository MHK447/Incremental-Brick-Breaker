using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using TMPro;
using System;
public class LobbyStageRewardGroup : MonoBehaviour
{
    [SerializeField]
    private List<LobbyStageRewardComponent> LobbyStageRewardComponents = new List<LobbyStageRewardComponent>();

    [SerializeField]
    private Slider Progress;


    public void Init()
    {
        var curstageidx = GameRoot.Instance.UserData.Stageidx.Value;

        var stagetdlist = Tables.Instance.GetTable<StageInfo>().DataList.ToList();

        // 현재 스테이지를 기준으로 6개 단위 그룹의 시작 인덱스 계산
        // 예: 스테이지 3 -> 1, 스테이지 7 -> 7, 스테이지 13 -> 13
        int startStageIdx = ((curstageidx - 1) / 6) * 6 + 1;

        // 6개의 스테이지 정보를 각 컴포넌트에 설정
        for (int i = 0; i < LobbyStageRewardComponents.Count && i < 6; i++)
        {
            int stageIdx = startStageIdx + i;
            LobbyStageRewardComponents[i].Set(stageIdx);
        }

        // 현재 스테이지가 6개 그룹 내에서 몇 번째인지 계산 (0~5)
        int progressIdx = (curstageidx - 1) % 6;

        Progress.value = (float)progressIdx * 0.2f;
    }
}

