using System.Collections.Generic;
using UniRx;
using BanpoFri;
using UnityEngine;
using System.Linq;

[System.Serializable]
public enum GameType
{
    Main,
    Event,
    Travel,

    None = 99,
}

public class InGameSystem
{
    public InGameMode CurInGame { get; private set; } = null;

    private bool firstInit = false;
    public System.Action NextActionClear = null;
    public System.Action NextAction = null;




    public T GetInGame<T>() where T : InGameMode
    {
        return CurInGame as T;
    }

    public void RegisteInGame(InGameMode mode)
    {
        CurInGame = mode;
    }


    private void StartGame(GameType type, System.Action loadCallback = null, bool nextStage = false)
    {
        GameRoot.Instance.Loading.Show(true);
    }

    public void ChangeMode(GameType type, System.Action _action = null)
    {
        System.GC.Collect();
        firstInit = false;
        NextActionClear = null;
        StartGame(type, () =>
        {
            _action?.Invoke();
            SoundPlayer.Instance.PlayBGM("bgm");
        });
    }


    public void InitPopups()
    {
        GameRoot.Instance.InitCurrencyTop();
      
    }

    public void NextStage()
    {
        var lastidx = Tables.Instance.GetTable<StageInfo>().DataList.Last().stage_idx;

        if(GameRoot.Instance.UserData.Stageidx.Value >= lastidx) return;

        GameRoot.Instance.UserData.Stageidx.Value += 1;

        GameRoot.Instance.UserData.Save();
    }
}
