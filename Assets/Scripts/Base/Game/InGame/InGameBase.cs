using UnityEngine;
using UnityEngine.AddressableAssets;

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

        Addressables.InstantiateAsync($"ChapterMap_Base").Completed += (handle) =>
        {
            stageLoading = false;

            if (stageLoadCancelled)
            {
                if (handle.Result != null)
                    Addressables.ReleaseInstance(handle.Result);
                return;
            }

            var igls = handle.Result != null ? handle.Result.GetComponent<InGameBaseStage>() : null;
            if (igls != null)
            {
                stage = igls;
                igls.Init();
            }
            else if (handle.Result != null)
            {
                // Fallback: release unexpected instance to avoid leaks
                Addressables.ReleaseInstance(handle.Result);
            }
        };
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
