using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class LobbyBoxSystem
{
    public void Create()
    {
        if (GameRoot.Instance.UserData.Stagerewardboxgroup.Boxcount == 0)
        {
            GameRoot.Instance.UserData.Stagerewardboxgroup.Boxcount = Tables.Instance.GetTable<Define>().GetData("start_box_count").value;
        }
    }



    public void UpdateOneSecond()
    {
        for (int i = GameRoot.Instance.UserData.Stagerewardboxgroup.Stagerewardboxdatas.Count - 1; i >= 0; --i)
        {
            if(GameRoot.Instance.UserData.Stagerewardboxgroup.Stagerewardboxdatas[i].Boxtime == (default(System.DateTime))) continue;
 
            var getcurtime = TimeSystem.GetCurTime();

            GameRoot.Instance.UserData.Stagerewardboxgroup.Stagerewardboxdatas[i].CurBoxTimeProperty.Value = 
            (int)(GameRoot.Instance.UserData.Stagerewardboxgroup.Stagerewardboxdatas[i].Boxtime - getcurtime).TotalSeconds;
        }
    }


    public int GetBoxReward()
    {
        var ratioList = Tables.Instance.GetTable<StageRewardBoxRatio>().DataList;
        
        // 총 가중치 합 계산
        int totalRatio = 0;
        foreach (var data in ratioList)
        {
            totalRatio += data.ratio;
        }
        
        // 랜덤 값 생성 (0 ~ 총 가중치)
        int randomValue = Random.Range(0, totalRatio);
        
        // 누적 가중치로 박스 선택
        int cumulativeRatio = 0;
        foreach (var data in ratioList)
        {
            cumulativeRatio += data.ratio;
            if (randomValue < cumulativeRatio)
            {
                return data.reward_idx;
            }
        }
        
        // 기본값 (안전장치)
        return ratioList[1].reward_idx;
    }


}

