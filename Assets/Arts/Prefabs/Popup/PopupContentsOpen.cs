using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using BanpoFri;
using UnityEngine;
using UnityEngine.UI;

[UIPath("UI/Popup/PopupContentsOpen")]
public class PopupContentsOpen : CommonUIBase
{
    [SerializeField]
    private Image contentsIcon;
    [SerializeField]
    private Animator ani;
    private Action endCallback;
    private Vector3 targetWorldPos;
    private RectTransform targetRect;

    public void Set(ContentsOpenSystem.ContentsOpenType contentsOpenIdx, RectTransform _targetRect, Action onCallback)
    {
        SoundPlayer.Instance.PlaySound("effect_contents_open");
        targetRect = _targetRect;
        //targetWorldPos = _targetWorldPos;
        endCallback = onCallback;
        var td = Tables.Instance.GetTable<ContentsOpenCheck>().GetData((int)contentsOpenIdx);
        if(td != null)
        {
            contentsIcon.sprite  = AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, td.image);
        }
        var center = new UnityEngine.Vector3(
        GameRoot.Instance.InGameSystem.CurInGame.CamPixelWidth / 2,
        GameRoot.Instance.InGameSystem.CurInGame.CamPixelHeight / 2, 0);
        contentsIcon.transform.position = center;
    }



    public void MoveTarget()
    {
        ani.Play("Hide", 0, 0f);
        // var center = new UnityEngine.Vector3(
        // GameRoot.Instance.InGameSystem.CurInGame.CamPixelWidth / 2,
        // GameRoot.Instance.InGameSystem.CurInGame.CamPixelHeight / 2, 0);
        contentsIcon.transform.DOMove(targetRect.position, 1f ).SetEase(Ease.InExpo).onComplete = () => {
            endCallback?.Invoke();
            endCallback = null;
            Hide();
        };
    }

    // IEnumerator WaitFrameforAni()
    // {
    //     yield return new WaitForEndOfFrame();
    //     yield return new WaitForEndOfFrame();
    //     if(ani != null)
    //     {
    //         ani.Play("Show");
    //     }
    // }
}
