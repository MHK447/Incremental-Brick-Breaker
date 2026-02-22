using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class MissingScriptFind : EditorWindow
{
    private GameObject targetRoot;

    [MenuItem("Tools/Missing Script Cleaner")]
    private static void Open()
    {
        GetWindow<MissingScriptFind>("Missing Script Cleaner");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Remove Missing Components", EditorStyles.boldLabel);
        targetRoot = (GameObject)EditorGUILayout.ObjectField(
            "Target GameObject",
            targetRoot,
            typeof(GameObject),
            true);

        using (new EditorGUI.DisabledScope(targetRoot == null))
        {
            if (GUILayout.Button("Remove Missing Components In Children"))
            {
                RemoveMissingComponents(targetRoot);
            }
        }
    }

    private static void RemoveMissingComponents(GameObject root)
    {
        int removedCount = 0;
        var transforms = root.GetComponentsInChildren<Transform>(true);

        foreach (var t in transforms)
        {
            var go = t.gameObject;
            int removed = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
            if (removed > 0)
            {
                removedCount += removed;
                EditorUtility.SetDirty(go);
            }
        }

        if (removedCount > 0)
        {
            EditorSceneManager.MarkAllScenesDirty();
        }

        Debug.Log($"Removed {removedCount} missing component(s) under '{root.name}'.");
    }
}