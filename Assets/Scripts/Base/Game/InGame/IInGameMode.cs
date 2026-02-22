using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using BanpoFri;

public interface IInGameMode
{
    void Load();
    void UnLoad(bool nextStage = false);
    void SetCameraBoundMinY(float value);
}

public abstract class InGameMode : MonoBehaviour, IInGameMode
{
    [SerializeField]
    private PanAndZoom MainCam;

    public PanAndZoom GetMainCam { get { return MainCam; } }

    public float CamPixelWidth { get; private set; }
    public float CamPixelHeight { get; private set; }


    private void Awake()
    {
        CamPixelWidth = MainCam.cam.pixelWidth;
        CamPixelHeight = MainCam.cam.pixelHeight;
        if (GameRoot.IsInit())
        {
            GameRoot.Instance.InGameSystem.RegisteInGame(this);

            Load();
        }
        else
        {
            GameRoot.Load();
        }
    }

    protected virtual void Update()
    {
    }

    public virtual void Load()
    {
        LoadUI();
    }

    
    public virtual void UnLoad(bool nextStage = false)
    {
        UnLoadUI(nextStage);
    }

    protected virtual void UnLoadUI(bool nextStage = false)
    {
        GameRoot.Instance.UISystem.UnLoadUIAll(nextStage);
    }

    protected virtual void LoadUI() { }

    public virtual void SetCameraBoundMinY(float value)
    {
    }
}