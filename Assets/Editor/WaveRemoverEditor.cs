#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public class WaveRemoverEditor : EditorWindow
{
    private string source;
    private string result;
    private Vector2 sourceScroll;
    private Vector2 resultScroll;

    private const int COL_UNIT_IDX = 2;
    private const int COL_UNIT_DMG = 3;
    private const int COL_UNIT_HP = 4;
    private const int COL_UNIT_COUNT = 5;
    private const int COL_UNIT_APPEAR_TIME = 7;

    private static readonly int[] ListColumns = {
        COL_UNIT_IDX, COL_UNIT_DMG, COL_UNIT_HP, COL_UNIT_COUNT, COL_UNIT_APPEAR_TIME
    };

    private static readonly int[] AdjustTargetColumns = {
        COL_UNIT_DMG, COL_UNIT_HP, COL_UNIT_COUNT, COL_UNIT_APPEAR_TIME
    };

    [MenuItem("BanpoFri/Wave Remover Editor")]
    private static void ShowWindow()
    {
        var window = GetWindow<WaveRemoverEditor>();
        window.titleContent = new GUIContent("Wave Remover Editor");
        window.Show();
    }

    private void OnGUI()
    {
        GUILayout.Label("WaveInfo 테이블 데이터를 붙여넣기 (탭 구분, 리스트는 ; 구분)", EditorStyles.boldLabel);
        GUILayout.Label("unit_idx 개수에 맞게 unit_dmg, unit_hp, unit_count, unit_appear_time 을 조정합니다.");
        GUILayout.Space(5);

        GUILayout.Label("Input");
        sourceScroll = EditorGUILayout.BeginScrollView(sourceScroll, GUILayout.Height(250));
        source = EditorGUILayout.TextArea(source, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        if (GUILayout.Button("Apply", GUILayout.Height(30)))
            Apply();

        GUILayout.Label("Result");
        resultScroll = EditorGUILayout.BeginScrollView(resultScroll, GUILayout.Height(250));
        result = EditorGUILayout.TextArea(result, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();

        if (!string.IsNullOrEmpty(result) && GUILayout.Button("Copy Result"))
        {
            EditorGUIUtility.systemCopyBuffer = result;
            Debug.Log("[WaveRemoverEditor] Result copied to clipboard.");
        }
    }

    private void Apply()
    {
        if (string.IsNullOrWhiteSpace(source))
        {
            result = "";
            return;
        }

        var rows = source.Split('\n');
        var sb = new StringBuilder();
        int fixedCount = 0;

        for (int r = 0; r < rows.Length; r++)
        {
            var row = rows[r].TrimEnd('\r');
            if (string.IsNullOrWhiteSpace(row))
            {
                if (r < rows.Length - 1) sb.Append('\n');
                continue;
            }

            var cols = row.Split('\t');

            if (cols.Length <= COL_UNIT_APPEAR_TIME)
            {
                sb.Append(row);
                if (r < rows.Length - 1) sb.Append('\n');
                continue;
            }

            var unitIdxValues = ParseList(cols[COL_UNIT_IDX]);
            int targetCount = unitIdxValues.Count;

            if (targetCount == 0)
            {
                sb.Append(row);
                if (r < rows.Length - 1) sb.Append('\n');
                continue;
            }

            foreach (int colIdx in AdjustTargetColumns)
            {
                var values = ParseList(cols[colIdx]);
                if (values.Count != targetCount)
                {
                    string colName = GetColumnName(colIdx);
                    int oldCount = values.Count;
                    values = AdjustList(values, targetCount);
                    cols[colIdx] = string.Join(";", values);
                    fixedCount++;
                    Debug.Log($"[WaveRemoverEditor] Row {r + 1} (stage={cols[0]}, wave={cols[1]}): {colName} {oldCount} -> {targetCount}");
                }
            }

            sb.Append(string.Join("\t", cols));
            if (r < rows.Length - 1) sb.Append('\n');
        }

        result = sb.ToString();

        if (fixedCount > 0)
            Debug.Log($"[WaveRemoverEditor] 총 {fixedCount}건 수정 완료.");
        else
            Debug.Log("[WaveRemoverEditor] 모든 행이 정상입니다. 수정 없음.");
    }

    private List<string> ParseList(string cell)
    {
        if (string.IsNullOrWhiteSpace(cell)) return new List<string>();
        return cell.Trim(';').Split(';').Where(s => !string.IsNullOrWhiteSpace(s)).ToList();
    }

    private List<string> AdjustList(List<string> values, int targetCount)
    {
        if (values.Count == 0) return values;

        if (values.Count > targetCount)
        {
            return values.Take(targetCount).ToList();
        }

        string lastValue = values[values.Count - 1];
        while (values.Count < targetCount)
        {
            values.Add(lastValue);
        }
        return values;
    }

    private string GetColumnName(int colIdx)
    {
        return colIdx switch
        {
            COL_UNIT_DMG => "unit_dmg",
            COL_UNIT_HP => "unit_hp",
            COL_UNIT_COUNT => "unit_count",
            COL_UNIT_APPEAR_TIME => "unit_appear_time",
            _ => $"col_{colIdx}"
        };
    }
}
#endif
