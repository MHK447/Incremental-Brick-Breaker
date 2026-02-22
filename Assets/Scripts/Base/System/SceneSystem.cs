using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class SceneSystem
{
    private SceneInstance curLoadInstance;
    private Dictionary<GameType, string> dicScenePath = new Dictionary<GameType, string>()
    {
        {GameType.Main, "InGameTycoon"},
        {GameType.Event, "InGameTycoon" }
    };

    public void ChangeScene(GameType type, Action callback = null)
    {
        if(!dicScenePath.ContainsKey(type))
        {
            BpLog.LogError($"SceneSystem::ChangeScene type: {type.ToString()} is not found key");
            return;
        }        

        Addressables.LoadSceneAsync(dicScenePath[type]).Completed += (handle) => {
            curLoadInstance = handle.Result;
            callback?.Invoke();
            handle.Destroyed += (d) => {                                
                LocalizeString.Localizelist.Clear();
            };
        };
    }

    public void UnLoadScene(Action callback = null)
    {
        //single mode는 필요 없다
        if (!curLoadInstance.Equals(default(SceneInstance)))
            Addressables.UnloadSceneAsync(curLoadInstance).Completed += (handle) =>
            {
                callback?.Invoke();
            };
        else
            callback?.Invoke();
    }
}
