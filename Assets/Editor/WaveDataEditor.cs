#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using Newtonsoft.Json;

public class WaveDataEditor : EditorWindow
{
    private string source;
    private string result;
    private float multiplier = 1;
    private int addition = 0;
    private bool replace = false;

  [MenuItem("BanpoFri/Wave Data Editor")]
    private static void ShowWindow()
    {
        var window = GetWindow<WaveDataEditor>();
        window.titleContent = new GUIContent("Wave Data Editor");
        window.Show();
    }

    private void OnGUI(){
        GUILayout.Label("Edit");
        multiplier = EditorGUILayout.FloatField("Multiplier",multiplier);
        addition = EditorGUILayout.IntField("Addition",addition);
        replace = EditorGUILayout.Toggle("Set -1", replace);
        GUILayout.Label("Input");
        source = EditorGUILayout.TextArea(source, GUILayout.Height(200));
        if(GUILayout.Button("Apply")) Apply();
        GUILayout.Label("Result");
        result = EditorGUILayout.TextArea(result, GUILayout.Height(200));
    }

    private void Apply(){
        List<List<int>> hpList = new();
        var rawHpList = source.Split('\n');
        foreach(var raw in rawHpList){
            hpList.Add(raw.Trim(';').Split(';').Select(x=>int.Parse(x)).ToList());
        }

        StringBuilder builder = new();
        foreach(var hp in hpList){
            for (int i = 0; i < hp.Count; i++)
            {
                int num = 0;
                if (replace) num = -1;
                else num = Mathf.RoundToInt(hp[i] * multiplier) + addition;
                builder.Append(num);
                if(i != hp.Count-1) builder.Append(';');
            }
            builder.Append('\n');
        }
        result = builder.ToString();

    }
}
#endif