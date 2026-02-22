using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class AutoInjectUsings : AssetModificationProcessor
{
    static readonly string[] DesiredUsings =
    {
        "using UnityEngine;",
        "using BanpoFri;",
        "using System.Collections.Generic;",
        "using System.Linq;",
        "using UnityEngine.UI;"
    };

    static void OnWillCreateAsset(string path)
    {
        path = path.Replace(".meta", "");
        if (!path.EndsWith(".cs")) return;

        // 경로를 정규화 (백슬래시를 슬래시로, 대소문자 무시)
        string normalizedPath = path.Replace("\\", "/").ToLowerInvariant();

        // FlatBuffers 관련 경로는 모두 자동화 기능 건너뛰기
        if (normalizedPath.Contains("flatbuffers"))
        {
            return;
        }

        // UserDataClass 폴더의 파일도 자동화 기능 건너뛰기 (FlatBuffersConvertor에서 관리)
        if (normalizedPath.Contains("userdataclass"))
        {
            return;
        }

        string fullPath = Path.GetFullPath(path);

        EditorApplication.delayCall += () =>
        {
            if (!File.Exists(fullPath)) return;

            string original = File.ReadAllText(fullPath, Encoding.UTF8);

            // 이미 BanpoFri 들어가있으면 중복 처리 방지
            if (original.Contains("using BanpoFri;")) return;

            string modified = InjectUsings(original);
            modified = RemoveStartAndUpdate(modified);

            File.WriteAllText(fullPath, modified, Encoding.UTF8);
            AssetDatabase.ImportAsset(path);
        };
    }

    static string InjectUsings(string text)
    {
        var lines = text.Split('\n').ToList();
        var usingPattern = new Regex(@"^\s*using\s+[A-Za-z0-9_.<>]+;\s*$");

        int idx = 0;
        while (idx < lines.Count && (usingPattern.IsMatch(lines[idx]) || string.IsNullOrWhiteSpace(lines[idx])))
            idx++;

        var existing = new HashSet<string>(lines
            .Take(idx)
            .Where(l => usingPattern.IsMatch(l))
            .Select(l => l.Trim()));

        var toAdd = DesiredUsings.Where(u => !existing.Contains(u)).ToList();
        if (toAdd.Count == 0) return text;

        var header = lines.Take(idx).Where(l => usingPattern.IsMatch(l)).ToList();
        header.AddRange(toAdd);
        header.Add("");

        var body = lines.Skip(idx).ToList();
        var sb = new StringBuilder();
        foreach (var l in header) sb.AppendLine(l);
        foreach (var l in body) sb.AppendLine(l);

        return sb.ToString();
    }

    static string RemoveStartAndUpdate(string text)
    {
        // Start/Update 함수 전체 제거 (중괄호 매칭 고려)
        var pattern = new Regex(
            @"\s*(public\s+|private\s+|protected\s+)?void\s+(Start|Update)\s*\([^)]*\)\s*\{",
            RegexOptions.Multiline);

        string cleaned = text;
        var matches = pattern.Matches(cleaned);

        // 뒤에서부터 처리 (인덱스 변경 방지)
        for (int i = matches.Count - 1; i >= 0; i--)
        {
            var match = matches[i];
            int startPos = match.Index;
            int bracePos = match.Index + match.Length - 1; // '{' 위치
            
            // 중괄호 짝 찾기
            int braceCount = 1;
            int endPos = bracePos + 1;
            
            while (endPos < cleaned.Length && braceCount > 0)
            {
                if (cleaned[endPos] == '{') braceCount++;
                else if (cleaned[endPos] == '}') braceCount--;
                endPos++;
            }
            
            // 메소드 전체 제거
            if (braceCount == 0)
            {
                cleaned = cleaned.Remove(startPos, endPos - startPos);
            }
        }

        // 연속된 빈 라인 정리 (3개 이상의 빈 라인을 2개로)
        cleaned = Regex.Replace(cleaned, @"(\r?\n\s*){3,}", "\n\n", RegexOptions.Multiline);

        return cleaned;
    }
}
