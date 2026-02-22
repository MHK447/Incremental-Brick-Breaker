using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

public class CreateChangeImage : EditorWindow
{
    private enum PositionMode
    {
        PreserveLocal,
        PreserveWorld
    }

    private GameObject sourcePrefab;
    private bool convertChildren = true;
    private PositionMode positionMode = PositionMode.PreserveLocal;
    private bool isCreatingPrefab = false;

    [MenuItem("Tools/UI/Create RectTransform Prefab")]
    private static void OpenWindow()
    {
        GetWindow<CreateChangeImage>("RectTransform Prefab");
    }

    private void OnEnable()
    {
        SyncSelection();
    }

    private void OnSelectionChange()
    {
        // 프리팹 생성 중에는 Selection 변경을 무시
        if (!isCreatingPrefab)
        {
            SyncSelection();
            Repaint();
        }
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Source", EditorStyles.boldLabel);
        sourcePrefab = (GameObject)EditorGUILayout.ObjectField(
            "Prefab or Scene Object",
            sourcePrefab,
            typeof(GameObject),
            true);

        EditorGUILayout.Space(6);
        convertChildren = EditorGUILayout.Toggle("Convert Children", convertChildren);
        positionMode = (PositionMode)EditorGUILayout.EnumPopup("Position Mode", positionMode);

        EditorGUILayout.Space(6);
        string outputPath = GetOutputPath(sourcePrefab);
        EditorGUILayout.LabelField("Output Prefab", outputPath);

        EditorGUILayout.Space(8);
        using (new EditorGUI.DisabledScope(sourcePrefab == null))
        {
            if (GUILayout.Button("Create Prefab"))
            {
                CreatePrefab();
            }
        }
    }

    private void SyncSelection()
    {
        if (Selection.activeObject is GameObject selected)
        {
            sourcePrefab = selected;
        }
    }

    private void CreatePrefab()
    {
        if (sourcePrefab == null)
        {
            return;
        }

        isCreatingPrefab = true;
        try
        {
            string outputPath = GetOutputPath(sourcePrefab);
            if (string.IsNullOrEmpty(outputPath))
            {
                ShowNotification(new GUIContent("Output path must be under Assets/"));
                return;
            }

            EnsureFolderExists(Path.GetDirectoryName(outputPath));

            GameObject instance = InstantiateSource(sourcePrefab);
            if (instance == null)
            {
                ShowNotification(new GUIContent("Failed to instantiate source."));
                return;
            }

            try
            {
                GameObject convertedRoot = ConvertToRectTransform(instance, convertChildren, positionMode == PositionMode.PreserveWorld);

                bool success;
                GameObject createdPrefab = PrefabUtility.SaveAsPrefabAsset(convertedRoot, outputPath, out success);
                if (success && createdPrefab != null)
                {
                    Selection.activeObject = createdPrefab;
                    ShowNotification(new GUIContent($"Prefab created: {createdPrefab.name}"));
                    Debug.Log($"RectTransform Prefab created: {outputPath}");
                }
                else
                {
                    ShowNotification(new GUIContent("Prefab creation failed."));
                    Debug.LogError("Failed to create RectTransform prefab");
                }
            }
            finally
            {
                DestroyImmediate(instance);
            }
        }
        finally
        {
            isCreatingPrefab = false;
        }
    }

    private static GameObject InstantiateSource(GameObject source)
    {
        GameObject instance = null;
        if (PrefabUtility.IsPartOfPrefabAsset(source))
        {
            instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
        }

        if (instance == null)
        {
            instance = Instantiate(source);
        }

        instance.name = source.name;
        return instance;
    }

    private static GameObject ConvertToRectTransform(GameObject root, bool includeChildren, bool preserveWorld)
    {
        List<TransformSnapshot> snapshots = new List<TransformSnapshot>();
        Transform[] transforms = includeChildren
            ? root.GetComponentsInChildren<Transform>(true)
            : new[] { root.transform };

        foreach (Transform transform in transforms)
        {
            snapshots.Add(new TransformSnapshot(transform));
        }

        // 역순으로 처리 (자식부터, leaf부터)
        snapshots.Reverse();

        Dictionary<GameObject, GameObject> replacements = new Dictionary<GameObject, GameObject>();

        foreach (TransformSnapshot snapshot in snapshots)
        {
            GameObject go = snapshot.GameObject;
            
            // 이미 파괴된 GameObject는 스킵
            if (go == null || !go)
            {
                continue;
            }

            // 이미 교체된 GameObject는 스킵
            if (replacements.ContainsKey(go))
            {
                continue;
            }

            RectTransform rectTransform = go.GetComponent<RectTransform>();
            if (rectTransform == null)
            {
                rectTransform = go.AddComponent<RectTransform>();
                if (rectTransform == null)
                {
                    rectTransform = ReplaceWithRectTransform(go, snapshot, preserveWorld, replacements, ref root);
                }
            }

            if (rectTransform != null)
            {
                snapshot.Apply(rectTransform, preserveWorld);
            }
        }

        return root;
    }

    private static RectTransform ReplaceWithRectTransform(
        GameObject original,
        TransformSnapshot snapshot,
        bool preserveWorld,
        Dictionary<GameObject, GameObject> replacements,
        ref GameObject root)
    {
        Transform originalTransform = original.transform;
        GameObject replacement = new GameObject(original.name, typeof(RectTransform));
        RectTransform rectTransform = replacement.GetComponent<RectTransform>();

        // 부모 설정 (world position 유지)
        replacement.transform.SetParent(originalTransform.parent, true);
        replacement.transform.SetSiblingIndex(originalTransform.GetSiblingIndex());

        // 자식들 이동
        while (originalTransform.childCount > 0)
        {
            Transform child = originalTransform.GetChild(0);
            child.SetParent(replacement.transform, true);
        }

        // 컴포넌트 복사
        Component[] components = original.GetComponents<Component>();
        foreach (Component component in components)
        {
            if (component is Transform || component is RectTransform)
            {
                continue;
            }

            if (ComponentUtility.CopyComponent(component))
            {
                ComponentUtility.PasteComponentAsNew(replacement);
            }
        }

        replacements[original] = replacement;
        if (root == original)
        {
            root = replacement;
        }

        snapshot.Apply(rectTransform, preserveWorld);
        DestroyImmediate(original);
        return rectTransform;
    }

    private static string GetOutputPath(GameObject source)
    {
        string folder = "Assets/RectTransformPrefabs";
        if (source != null)
        {
            string assetPath = AssetDatabase.GetAssetPath(source);
            if (!string.IsNullOrEmpty(assetPath))
            {
                folder = Path.GetDirectoryName(assetPath);
            }
        }

        if (string.IsNullOrEmpty(folder) || !folder.StartsWith("Assets"))
        {
            return null;
        }

        string fileName = source != null ? $"{source.name}_Rect" : "RectPrefab";
        string fullPath = $"{folder}/{fileName}.prefab";
        return AssetDatabase.GenerateUniqueAssetPath(fullPath.Replace("\\", "/"));
    }

    private static void EnsureFolderExists(string folder)
    {
        if (string.IsNullOrEmpty(folder))
        {
            return;
        }

        folder = folder.Replace("\\", "/");
        if (AssetDatabase.IsValidFolder(folder))
        {
            return;
        }

        string[] parts = folder.Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{current}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[i]);
            }
            current = next;
        }
    }

    private sealed class TransformSnapshot
    {
        public readonly GameObject GameObject;
        private readonly Vector3 localPosition;
        private readonly Quaternion localRotation;
        private readonly Vector3 localScale;
        private readonly Vector3 worldPosition;
        private readonly Quaternion worldRotation;
        private readonly Vector3 worldScale;
        private readonly Vector2 size;

        public TransformSnapshot(Transform transform)
        {
            GameObject = transform.gameObject;
            localPosition = transform.localPosition;
            localRotation = transform.localRotation;
            localScale = transform.localScale;
            worldPosition = transform.position;
            worldRotation = transform.rotation;
            worldScale = transform.lossyScale;

            // Renderer나 Collider에서 크기 정보 추출
            size = GetObjectSize(transform);
        }

        private static Vector2 GetObjectSize(Transform transform)
        {
            // SpriteRenderer가 있으면 sprite 크기 사용
            SpriteRenderer spriteRenderer = transform.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null)
            {
                Sprite sprite = spriteRenderer.sprite;
                // sprite의 픽셀 크기 사용
                Rect rect = sprite.rect;
                return new Vector2(rect.width, rect.height);
            }

            // Renderer가 있으면 local bounds 사용
            Renderer renderer = transform.GetComponent<Renderer>();
            if (renderer != null)
            {
                Bounds bounds = renderer.localBounds;
                return new Vector2(bounds.size.x * 100f, bounds.size.y * 100f);
            }

            // Collider가 있으면 크기 사용
            Collider collider = transform.GetComponent<Collider>();
            if (collider != null)
            {
                Vector3 size = collider.bounds.size;
                return new Vector2(size.x * 100f, size.y * 100f);
            }

            // 2D Collider 확인
            Collider2D collider2D = transform.GetComponent<Collider2D>();
            if (collider2D != null)
            {
                Vector2 size = collider2D.bounds.size;
                return new Vector2(size.x * 100f, size.y * 100f);
            }

            // 기본값: 100x100 (Unity UI 기본 크기)
            return new Vector2(100f, 100f);
        }

        public void Apply(RectTransform rectTransform, bool preserveWorld)
        {
            // 먼저 Transform 값 설정
            if (preserveWorld)
            {
                // 월드 좌표 보존 모드
                rectTransform.position = worldPosition;
                rectTransform.rotation = worldRotation;
                rectTransform.localScale = CalculateLocalScale(worldScale, rectTransform.parent);
            }
            else
            {
                // 로컬 좌표 보존 모드 - 원본 Transform 값 그대로 유지
                rectTransform.localPosition = localPosition;
                rectTransform.localRotation = localRotation;
                rectTransform.localScale = localScale;
            }
        }

        private static Vector3 CalculateLocalScale(Vector3 targetWorldScale, Transform parentTransform)
        {
            if (parentTransform == null)
            {
                return targetWorldScale;
            }

            Vector3 parentScale = parentTransform.lossyScale;
            return new Vector3(
                SafeDivide(targetWorldScale.x, parentScale.x),
                SafeDivide(targetWorldScale.y, parentScale.y),
                SafeDivide(targetWorldScale.z, parentScale.z));
        }

        private static float SafeDivide(float value, float divisor)
        {
            if (Mathf.Approximately(divisor, 0f))
            {
                return value;
            }

            return value / divisor;
        }
    }
}