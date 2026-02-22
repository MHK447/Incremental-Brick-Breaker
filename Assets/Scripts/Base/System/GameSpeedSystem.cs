using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using BanpoFri;

public class GameSpeedSystem
{
    public IReactiveProperty<bool> IsGameSpeed = new ReactiveProperty<bool>(false);
    public IReactiveProperty<float> CurGameSpeedValue = new ReactiveProperty<float>(1);
    public IReactiveProperty<float> CurGameSpeedValue2 = new ReactiveProperty<float>(1);
    public IReactiveProperty<float> InitGameSpeedValue = new ReactiveProperty<float>(1);
    public ReactiveProperty<bool> IsGameSpeedPackageValue = new ReactiveProperty<bool>();

    CompositeDisposable disposables = new CompositeDisposable();

    public float[] GameSpeedCountList = { 1, 1.5f };

    public int AdAddSpeedTime = 600;

    public void Create()
    {
        CheckGameSpeedPackage();
        InitGameSpeedValue.Value = 1f;

        disposables.Clear();

        CurGameSpeedValue.Subscribe(x =>
        {
            Time.timeScale = x * CurGameSpeedValue2.Value;
        }).AddTo(disposables);

        CurGameSpeedValue2.Subscribe(x =>
        {
            Time.timeScale = x * CurGameSpeedValue.Value;
        }).AddTo(disposables);
        
        // if (GameRoot.Instance.UserData.CurMode.ADSpeedUpTimeProperty.Value <= 0 && !IsGameSpeedPackageValue.Value)
        // {
        //     GameRoot.Instance.UserData.CurMode.GameSpeedCount = 0;
        // }



        IsGameSpeed.Value = GameRoot.Instance.GameSpeedSystem.IsGameSpeedPackageValue.Value || GameRoot.Instance.UserData.ADSpeedUpTimeProperty.Value > 0;
    }


    public void Dispose()
    {
        disposables.Clear();
    }

    public void ResetGameSpeed()
    {
        CurGameSpeedValue.Value = InitGameSpeedValue.Value;
        CurGameSpeedValue2.Value = InitGameSpeedValue.Value;
    }


    public void CheckGameSpeedPackage()
    {
        // IsGameSpeedPackageValue.Value = GameRoot.Instance.UserData.BuyInappIds.Contains(InAppPurchaseManager.SpeedUp);

        // if (IsGameSpeedPackageValue.Value)
        // {
        //     GameRoot.Instance.GameSpeedSystem.IsGameSpeed.Value = true;
        // }
    }


    public void ClickGameSpeed()
    {
        GameRoot.Instance.UserData.GameSpeedCount += 1;

        if (GameSpeedCountList.Length <= GameRoot.Instance.UserData.GameSpeedCount)
        {
            GameRoot.Instance.UserData.GameSpeedCount = 0;
        }
        CurGameSpeedValue.Value = InitGameSpeedValue.Value = GameSpeedCountList[GameRoot.Instance.UserData.GameSpeedCount];
    }

    public void GameSpeedActiveCheck(bool isbattlestart)
    {
        if (GameRoot.Instance.UserData.GameSpeedCount <= 0 && !IsGameSpeedPackageValue.Value) 
        {
            GameRoot.Instance.UserData.GameSpeedCount = 0;
        }

        GameRoot.Instance.GameSpeedSystem.CurGameSpeedValue.Value = InitGameSpeedValue.Value = isbattlestart ? GameSpeedCountList[GameRoot.Instance.UserData.GameSpeedCount] : 1f;
    }

    public void StopGameSpeed(bool isStop, bool isSecond = false)
    {
        if (isStop)
        {
            Time.timeScale = 0f;
        }
        else
        {
            // ReactiveProperty를 통한 자동 업데이트와 일치하도록 설정
            Time.timeScale = CurGameSpeedValue.Value * CurGameSpeedValue2.Value;
        }
    }


    public void AddAdSpeedTime()
    {
        GameRoot.Instance.UserData.GameSpeedCount = AdAddSpeedTime;
        IsGameSpeed.Value = true;
    }




}
