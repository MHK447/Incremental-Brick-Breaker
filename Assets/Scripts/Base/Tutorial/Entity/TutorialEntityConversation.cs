using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using BanpoFri;
using TMPro;

public class TutorialEntityConversation : TutorialEntity
{  
    [SerializeField]
    private Animator Anim;

    public string AnimKey;

    public TextMeshProUGUI context;
    public string descKey;
    protected bool isNext = false;
    private string contextOriginal;

    public override void StartEntity()
    {
        base.StartEntity();

        isNext = false;

        var localize = Tables.Instance?.GetTable<Localize>();
        contextOriginal = localize != null ? localize.GetString(descKey) : descKey;

        Anim.Play(AnimKey , 0 ,0f);
        
        TextAllPrint();
    }

    protected virtual void Update()
    {

        if (Input.GetMouseButtonUp(0))
        {

            if (isNext)
            {
                Done();
                isNext = false;
            }
        }
    }

    protected void TextAllPrint()
    {
        context.text = contextOriginal;
        isNext = true;
    }

}
