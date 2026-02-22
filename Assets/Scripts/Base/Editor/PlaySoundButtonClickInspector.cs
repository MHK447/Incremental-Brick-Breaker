using System.Collections.Generic;
using RotaryHeart.Lib.AutoComplete;
using UnityEditor;

[CustomEditor(typeof(PlaySoundButtonClick))]
public class PlaySoundButtonClickInspector : Editor
{
    private SerializedProperty stringPorperty; 
    private List<string> keyList = new List<string>();
    private SoundPlayer loadData = null;

    private void OnEnable()
    {
        keyList.Clear();
        loadData = AssetDatabase.LoadAssetAtPath<SoundPlayer>("Assets/Arts/Config/SoundPlayer.asset");
        if(loadData != null)
        {
            foreach(var data in loadData.SoundDataList)
                keyList.Add(data.soundKey);
        }
        stringPorperty = serializedObject.FindProperty("keySound");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        serializedObject.Update();
        var prevStrValue = stringPorperty.stringValue;
        stringPorperty.stringValue = AutoCompleteTextField.EditorGUILayout.AutoCompleteTextField("Sound Key", stringPorperty.stringValue, keyList.ToArray(), "Type something");
        serializedObject.ApplyModifiedProperties();        
    }
}
