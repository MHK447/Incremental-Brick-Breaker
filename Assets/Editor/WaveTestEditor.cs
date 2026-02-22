using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

#if UNITY_EDITOR

using System;
using System.Text;
using UnityEditor;
using Newtonsoft.Json;

public class WaveTestEditor : EditorWindow
{
    private string source;
    private string result;
    private float multiplier = 1;
    private int addition = 0;
    private bool replace = false;

  [MenuItem("BanpoFri/Wave Test Editor")]
    private static void ShowWindow()
    {
        var window = GetWindow<WaveTestEditor>();
        window.titleContent = new GUIContent("Wave Test Editor");
        window.Show();
    }

    private void OnGUI(){
        if(GUILayout.Button("TestStart")) Apply();
    }

    private void Apply(){
        // 에디터에서 Tables 에셋 직접 로드
        var tablesPath = "Assets/BanpoFri/TableAsset/Tables.asset";
        var tables = AssetDatabase.LoadAssetAtPath<Tables>(tablesPath);
        
        if (tables == null)
        {
            Debug.LogError($"[WaveTestEditor] Tables 에셋을 찾을 수 없습니다. 경로: {tablesPath}");
            return;
        }

        // Tables 로드
        tables.Load();

        var stageInfoTable = tables.GetTable<WaveInfo>();
        if (stageInfoTable == null)
        {
            Debug.LogError("[WaveTestEditor] StageInfo 테이블을 찾을 수 없습니다.");
            return;
        }

        if (stageInfoTable.DataList == null)
        {
            Debug.LogError("[WaveTestEditor] StageInfo.DataList가 null입니다.");
            return;
        }

        var tdlist = stageInfoTable.DataList.ToList();
        
        if (tdlist.Count == 0)
        {
            Debug.LogWarning("[WaveTestEditor] StageInfo 테이블에 데이터가 없습니다.");
            return;
        }

        int errorCount = 0;

        foreach (var td in tdlist)
        {
            var unitIdxCount = td.unit_idx?.Count ?? 0;
            var unitDmgCount = td.unit_dmg?.Count ?? 0;
            var unitCountCount = td.unit_count?.Count ?? 0;
            var unitAppearTimeCount = td.unit_appear_time?.Count ?? 0;

            var errors = new List<string>();

            if (unitDmgCount != unitIdxCount)
                errors.Add($"  unit_dmg: {unitDmgCount}개 (expected {unitIdxCount})");
            if (unitCountCount != unitIdxCount)
                errors.Add($"  unit_count: {unitCountCount}개 (expected {unitIdxCount})");
            if (unitAppearTimeCount != unitIdxCount)
                errors.Add($"  unit_appear_time: {unitAppearTimeCount}개 (expected {unitIdxCount})");

            if (errors.Count > 0)
            {
                errorCount++;
                var errorMsg = $"[WaveInfo Error] Stage: {td.stage}, Wave: {td.wave}\n" +
                               $"unit_idx 개수({unitIdxCount}개)와 맞지 않는 필드:\n" +
                               string.Join("\n", errors);
                Debug.LogError(errorMsg);
            }
        }

        if (errorCount == 0)
            Debug.Log("=== Wave Test Editor 검증 완료: 모든 Wave 데이터 정상 ===");
        else
            Debug.LogWarning($"=== Wave Test Editor 검증 완료: {errorCount}건의 오류 발견 ===");
    }
}
#endif
