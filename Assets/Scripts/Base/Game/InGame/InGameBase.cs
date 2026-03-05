using UnityEngine;
using UnityEngine.AddressableAssets;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class InGameBase : InGameMode
{
    private InGameBaseStage stage;
    private bool stageLoading = false;
    private bool stageLoadCancelled = false;

    public InGameBaseStage Stage { get { return stage; } }

    /// <summary>
    /// Ensures the stage instance exists. Safe to call multiple times.
    /// </summary>
    public void EnsureStageLoaded()
    {
        TryLoadStage();
    }

    private void TryLoadStage()
    {
        if (stage != null || stageLoading)
            return;

        stageLoading = true;
        stageLoadCancelled = false;
        // ChapterMap_Base prefab removed - stage remains null
        stageLoading = false;
    }

    public override void Load()
    {
        base.Load();

        TryLoadStage();
    }

    protected override void LoadUI()
    {
        base.LoadUI();

        GameRoot.Instance.InGameSystem.InitPopups();
    }

    public override void UnLoad(bool nextStage = false)
    {
        base.UnLoad(nextStage);
        if (stage != null)
        {
            stage.UnLoad();

            if (!Addressables.ReleaseInstance(stage.gameObject))
                GameObject.Destroy(stage);

            stage = null;
        }
        stageLoadCancelled = true;
        stageLoading = false;
    }

private void OnDestroy()
    {
        if (stage != null)
        {
            if (!Addressables.ReleaseInstance(stage.gameObject))
                GameObject.Destroy(stage);
        }
    }
}

