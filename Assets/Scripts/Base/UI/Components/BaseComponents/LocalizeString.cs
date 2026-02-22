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
            tmp.text = Tables.Instance.GetTable<Localize>().GetString(keyLocalize);
        }
        else
        {
            var label = GetComponent<Text>();
            if (label)
                label.text = Tables.Instance.GetTable<Localize>().GetString(keyLocalize);
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
