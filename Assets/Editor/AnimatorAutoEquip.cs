using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class AnimatorAutoEquip : EditorWindow
{
    [MenuItem("Tools/Animator Auto Equip")]
    public static void ShowWindow()
    {
        GetWindow<AnimatorAutoEquip>("Animator Auto Equip");
    }

    // Project 창에서 Animator Controller 우클릭 메뉴
    [MenuItem("Assets/Animator에 애니메이션 자동 연결", false, 0)]
    private static void AutoEquipFromContextMenu()
    {
        AutoEquipAnimations();
    }

    // Animator Controller가 선택되었을 때만 메뉴 활성화
    [MenuItem("Assets/Animator에 애니메이션 자동 연결", true)]
    private static bool ValidateAutoEquipFromContextMenu()
    {
        return Selection.activeObject is AnimatorController;
    }

    private void OnGUI()
    {
        GUILayout.Label("Animator 애니메이션 자동 연결", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("선택된 Animator에 애니메이션 자동 연결", GUILayout.Height(40)))
        {
            AutoEquipAnimations();
        }

        GUILayout.Space(10);
        EditorGUILayout.HelpBox(
            "사용법:\n" +
            "1. Project 창에서 Animator Controller를 선택하세요\n" +
            "2. 버튼을 클릭하거나\n" +
            "3. Animator Controller에 우클릭 → '애니메이션 자동 연결'\n\n" +
            "같은 폴더의 AnimationClip들이 자동으로 State로 추가됩니다",
            MessageType.Info);
    }

    private static void AutoEquipAnimations()
    {
        // 선택된 오브젝트 가져오기
        Object selectedObject = Selection.activeObject;

        if (selectedObject == null)
        {
            EditorUtility.DisplayDialog("오류", "Animator Controller를 선택해주세요!", "확인");
            return;
        }

        // Animator Controller인지 확인
        AnimatorController animatorController = selectedObject as AnimatorController;
        if (animatorController == null)
        {
            EditorUtility.DisplayDialog("오류", "선택된 오브젝트가 Animator Controller가 아닙니다!", "확인");
            return;
        }

        // Animator Controller의 경로 가져오기
        string controllerPath = AssetDatabase.GetAssetPath(animatorController);
        string folderPath = Path.GetDirectoryName(controllerPath);

        // 같은 폴더에서 모든 AnimationClip 찾기
        string[] guids = AssetDatabase.FindAssets("t:AnimationClip", new[] { folderPath });
        List<AnimationClip> clips = new List<AnimationClip>();

        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
            if (clip != null)
            {
                clips.Add(clip);
            }
        }

        if (clips.Count == 0)
        {
            EditorUtility.DisplayDialog("알림", "같은 폴더에 AnimationClip이 없습니다!", "확인");
            return;
        }

        // Base Layer 가져오기
        AnimatorControllerLayer baseLayer = animatorController.layers[0];
        AnimatorStateMachine stateMachine = baseLayer.stateMachine;

        int addedCount = 0;
        int updatedCount = 0;
        int skippedCount = 0;

        Debug.Log($"=== Animator Auto Equip 시작 ===");
        Debug.Log($"폴더: {folderPath}");
        Debug.Log($"발견된 AnimationClip: {clips.Count}개");

        // 각 AnimationClip을 State로 추가 또는 업데이트
        foreach (AnimationClip clip in clips)
        {
            Debug.Log($"\n처리 중: {clip.name}");

            // 이미 같은 이름의 State가 있는지 확인
            AnimatorState existingState = stateMachine.states
                .FirstOrDefault(state => state.state.name == clip.name).state;

            if (existingState != null)
            {
                // State가 존재하는 경우
                if (existingState.motion == null)
                {
                    // Motion이 비어있으면 연결
                    existingState.motion = clip;
                    updatedCount++;
                    Debug.Log($"✓ {clip.name} State에 Motion 연결 완료");
                }
                else if (existingState.motion.name != clip.name)
                {
                    // 다른 Motion이 연결되어 있으면 업데이트
                    existingState.motion = clip;
                    updatedCount++;
                    Debug.Log($"✓ {clip.name} State의 Motion 업데이트 완료 (기존: {existingState.motion.name})");
                }
                else
                {
                    // 이미 올바른 Motion이 연결되어 있음
                    Debug.Log($"⊘ {clip.name} State는 이미 올바르게 연결되어 있습니다.");
                    skippedCount++;
                }
            }
            else
            {
                // 새로운 State 생성
                AnimatorState newState = stateMachine.AddState(clip.name);
                newState.motion = clip;
                addedCount++;
                Debug.Log($"✓ {clip.name} State 생성 및 Motion 연결 완료");
            }
        }

        Debug.Log($"\n=== 완료 ===");
        Debug.Log($"추가: {addedCount}개, 업데이트: {updatedCount}개, 건너뜀: {skippedCount}개");

        // 변경사항 저장
        if (addedCount > 0 || updatedCount > 0)
        {
            EditorUtility.SetDirty(animatorController);
            AssetDatabase.SaveAssets();
            
            string message = "";
            if (addedCount > 0) message += $"{addedCount}개의 State가 추가되었습니다!\n";
            if (updatedCount > 0) message += $"{updatedCount}개의 State에 Motion이 연결되었습니다!\n";
            if (skippedCount > 0) message += $"({skippedCount}개는 이미 올바르게 연결되어 있습니다)";
            
            EditorUtility.DisplayDialog("완료", message.Trim(), "확인");
        }
        else
        {
            EditorUtility.DisplayDialog("알림", 
                $"변경사항이 없습니다.\n모든 State가 이미 올바르게 연결되어 있습니다. (총 {skippedCount}개)", 
                "확인");
        }
    }
}
