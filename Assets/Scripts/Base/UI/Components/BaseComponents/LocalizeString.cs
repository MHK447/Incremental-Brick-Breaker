using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanpoFri;
using TMPro;

public class LocalizeString : MonoBehaviour
{
    public static List<LocalizeString> Localizelist { get; set; } = new List<LocalizeString>();
    [HideInInspector]
    [SerializeField]
    private string keyLocalize = "str_error";
    private void Start() {
        if(!Localizelist.Contains(this))
            Localizelist.Add(this);
        var tmp = GetComponent<Text>();
        RefreshText();
    }
    public void RefreshText()
    {
        var tmp = GetComponent<TextMeshProUGUI>();
        if (tmp)
        {
            var localize = Tables.Instance?.GetTable<Localize>();
            tmp.text = localize != null ? localize.GetString(keyLocalize) : keyLocalize;
        }
        else
        {
            var label = GetComponent<Text>();
            if (label)
            {
                var localize = Tables.Instance?.GetTable<Localize>();
                label.text = localize != null ? localize.GetString(keyLocalize) : keyLocalize;
            }
        }
    }
  
    public void SetText(string txt)
    {
        var tmp = GetComponent<Text>();
        if(tmp)
        {
            tmp.text = txt;
        }
        else
        {
            var label = GetComponent<Text>();
            if(label)
                label.text = txt;
        }
    }
}
