using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Spriter2UnityDX;

public class UnitSpriteAutoEquip : EditorWindow
{
    [MenuItem("Tools/Unit Sprite Auto Equip")]
    public static void ShowWindow()
    {
        GetWindow<UnitSpriteAutoEquip>("Sprite Auto Equip");
    }

    private void OnGUI()
    {
        GUILayout.Label("Unity Sprite 자동 연결", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("선택된 프리팹에 Sprite 자동 연결", GUILayout.Height(40)))
        {
            AutoEquipSprites();
        }

        GUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "사용법:\n" +
            "1. Hierarchy에서 프리팹을 선택하세요\n" +
            "2. 버튼을 클릭하면 자식들의 SpriteRenderer에\n" +
            "   같은 폴더에 있는 동일한 이름의 Sprite가 자동으로 연결됩니다",
            MessageType.Info);
    }

    private void AutoEquipSprites()
    {
        GameObject selectedObject = Selection.activeGameObject;

        if (selectedObject == null)
        {
            EditorUtility.DisplayDialog("오류", "프리팹을 선택해주세요!", "확인");
            return;
        }

        // 프리팹의 경로 가져오기
        string prefabPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(selectedObject);
        if (string.IsNullOrEmpty(prefabPath))
        {
            EditorUtility.DisplayDialog("오류", "선택된 오브젝트가 프리팹이 아닙니다!", "확인");
            return;
        }

        string folderPath = Path.GetDirectoryName(prefabPath);
        int changedCount = 0;

        // 모든 자식 오브젝트 순회
        SpriteRenderer[] spriteRenderers = selectedObject.GetComponentsInChildren<SpriteRenderer>(true);

        // Order in Layer 설정 규칙
        Dictionary<string, int> orderInLayerRules = new Dictionary<string, int>
        {
            { "Head", 1 },
            { "Face", 2 },
            { "Face 01", 2 },
            { "Face 02", 2 },
            { "Face 03", 2 },
            { "Right Arm", 1 },
            { "Right Leg", -1 },
            { "Left Leg", -1 }
        };

        foreach (SpriteRenderer sr in spriteRenderers)
        {
            string objectName = sr.gameObject.name;

            // Order in Layer 설정
            if (orderInLayerRules.ContainsKey(objectName))
            {
                Undo.RecordObject(sr, "Set Order in Layer");
                sr.sortingOrder = orderInLayerRules[objectName];
                Debug.Log($"✓ {objectName}의 Order in Layer를 {orderInLayerRules[objectName]}로 설정");
            }

            // 같은 폴더에서 동일한 이름의 Sprite 찾기
            string[] guids = AssetDatabase.FindAssets($"{objectName} t:Sprite", new[] { folderPath });

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Path.GetFileNameWithoutExtension(assetPath);

                if (fileName == objectName)
                {
                    Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                    if (sprite != null)
                    {
                        Undo.RecordObject(sr, "Auto Equip Sprite");
                        sr.sprite = sprite;
                        changedCount++;
                        Debug.Log($"✓ {objectName}에 {sprite.name} Sprite 연결 완료");
                        break;
                    }
                }
            }
        }

        // TextureController 처리
        TextureController[] textureControllers = selectedObject.GetComponentsInChildren<TextureController>(true);
        
        foreach (TextureController tc in textureControllers)
        {
            // Face 01, Face 02, Face 03 스프라이트 찾기
            List<Sprite> faceSprites = new List<Sprite>();
            string[] faceNames = { "Face 01", "Face 02", "Face 03" };

            foreach (string faceName in faceNames)
            {
                string[] guids = AssetDatabase.FindAssets($"{faceName} t:Sprite", new[] { folderPath });

                foreach (string guid in guids)
                {
                    string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    string fileName = Path.GetFileNameWithoutExtension(assetPath);

                    if (fileName == faceName)
                    {
                        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                        if (sprite != null)
                        {
                            faceSprites.Add(sprite);
                            Debug.Log($"✓ {tc.gameObject.name}에 {sprite.name} Sprite 발견");
                            break;
                        }
                    }
                }
            }

            if (faceSprites.Count > 0)
            {
                Undo.RecordObject(tc, "Auto Equip Face Sprites");
                tc.Sprites = faceSprites.ToArray();
                changedCount += faceSprites.Count;
                Debug.Log($"✓ {tc.gameObject.name}의 TextureController에 {faceSprites.Count}개 Face Sprite 연결 완료");
            }
        }

        if (changedCount > 0)
        {
            EditorUtility.SetDirty(selectedObject);
            PrefabUtility.RecordPrefabInstancePropertyModifications(selectedObject);
            EditorUtility.DisplayDialog("완료", $"{changedCount}개의 Sprite가 연결되었습니다!", "확인");
        }
        else
        {
            EditorUtility.DisplayDialog("알림", "연결된 Sprite가 없습니다.\n오브젝트 이름과 Sprite 파일명이 일치하는지 확인해주세요.", "확인");
        }
    }
}

