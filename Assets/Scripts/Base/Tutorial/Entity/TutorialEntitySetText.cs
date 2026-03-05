using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanpoFri;
public class TutorialEntitySetText : TutorialEntity
{
    [SerializeField]
    private Text Text;
    [SerializeField]
    private string TextStr;

    public override void StartEntity()
    {
        base.StartEntity();

        var localize = Tables.Instance?.GetTable<Localize>();
        Text.text = localize != null ? localize.GetString(TextStr) : TextStr;
        Done();
    }
}
