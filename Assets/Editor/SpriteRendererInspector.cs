using System.Linq;
using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpriteRenderer))]
[CanEditMultipleObjects]
public class SpriteRendererInspector : Editor
{


    #region Sprite Option Settings
    public enum E_SPRITE_PIVOT
    {
        Center = 0,
        Top_Left,
        Top,
        Top_Right,
        Left,
        Right,
        Bottom_Left,
        Bottom,
        Bottom_Right,
        Custom,
    }

    public enum E_SPRITE_MESHTYPE
    {
        FullRect = 0,
        Tight = 1,
    }

    Sprite _sprite;
    Editor _editor;

    bool isOpenSpriteWindow = false;
    EditorWindow _spriteWindow = null;

    bool _isChange = false;

    E_SPRITE_PIVOT _pivot = E_SPRITE_PIVOT.Center;
    Vector2 _pivotValue = Vector2.one * 0.5f;
    E_SPRITE_MESHTYPE _meshType = E_SPRITE_MESHTYPE.Tight;

    private void OnEnable()
    {
        // 기존 유니티가 사용하는 SpriteRendererEditor를 받아서 사용 ( 요게 없으면 인스펙터가 더러워짐 )
        var type = typeof(EditorApplication).Assembly.GetType("UnityEditor.SpriteRendererEditor");
        _editor = CreateEditor(targets, type);

        if (_sprite == null)
        {
            Init();
        }


    }
    private void OnDisable()
    {
        DestroyImmediate(_editor);
    }

    private void Init()
    {
        _sprite = (target as SpriteRenderer).sprite;
        if (_sprite == null) { return; }
        InitData();
    }


    private void InitData()
    {
        if (_sprite == null) { return; }

        // 스프라이트 옵션 값 동기화

        TextureImporter textureImporter = GetTextureImporter(_sprite);
        if (!textureImporter)
            return;

        _pivotValue = textureImporter.spritePivot;
        _pivot = GetSpritePivot(_pivotValue);

        TextureImporterSettings settings = new TextureImporterSettings();
        textureImporter.ReadTextureSettings(settings);
        _meshType = (E_SPRITE_MESHTYPE)settings.spriteMeshType;
        _isChange = false;
    }



    private void OnClick_Apply()
    {
        _isChange = false;

        // 선택한 옵션 적용
        var selectSprites = targets.Cast<SpriteRenderer>().ToList();

        // Hierarchy 에서 다중으로 선택된 SpriteRenderer가 있을 경우 전체 적용
        for (int i = 0; i < selectSprites.Count; i++)
        {

            if (selectSprites[i].sprite == null) { continue; }

            TextureImporter textureImporter = GetTextureImporter(selectSprites[i].sprite);
            TextureImporterSettings settings = new TextureImporterSettings();
            textureImporter.ReadTextureSettings(settings);

            settings.spriteMeshType = (SpriteMeshType)_meshType;
            settings.spritePivot = _pivotValue;
            settings.spriteAlignment = (int)_pivot;

            textureImporter.SetTextureSettings(settings);
            textureImporter.SaveAndReimport();

        }





    }
    private void OnClick_Revert()
    {
        // 원래 값으로 동기화
        _isChange = false;
        InitData();
    }
    private void OnClick_SpriteEditor()
    {
        // 스프라이트 에디터창 열기
        Type type = Type.GetType("UnityEditor.U2D.Sprites.SpriteEditorWindow,Unity.2D.Sprite.Editor");
        _spriteWindow = EditorWindow.GetWindow(type);
        isOpenSpriteWindow = true;
    }


    public override void OnInspectorGUI()
    {

        // 스프라이트 에디터 창을 닫았을때 동기화
        if (isOpenSpriteWindow)
        {
            if (_spriteWindow == null)
            {
                isOpenSpriteWindow = false;
                InitData();
            }
        }

        _editor.OnInspectorGUI(); // 기존 SpriteRendererEditor 인스펙터 그려주기


        if (_sprite == null) { return; }

        // 스프라이트 교체여부 확인
        if (_sprite != (target as SpriteRenderer).sprite)
        {
            Init();
        }

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();

        var styleText = new GUIStyle(GUI.skin.label);
        styleText.normal.textColor = Color.green;
        styleText.alignment = TextAnchor.MiddleCenter;
        EditorGUILayout.LabelField("--------------- EDITOR ONLY ---------------", styleText);

        SetMeshType((E_SPRITE_MESHTYPE)EditorGUILayout.EnumPopup("Mesh Type", _meshType));
        SetSpritePivot((E_SPRITE_PIVOT)EditorGUILayout.EnumPopup("Pivot", _pivot));

        if (_pivot == E_SPRITE_PIVOT.Custom)
        {
            EditorGUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
            _pivotValue = EditorGUILayout.Vector2Field("", _pivotValue);
            EditorGUILayout.EndHorizontal();
        }


        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
        if (GUILayout.Button("Sprite Editor", GUILayout.Width(80))) { OnClick_SpriteEditor(); }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.Space();

        EditorGUILayout.BeginHorizontal(); GUILayout.FlexibleSpace();
        EditorGUI.BeginDisabledGroup(_isChange == false);
        if (GUILayout.Button("Revert", GUILayout.Width(50))) { OnClick_Revert(); }
        if (GUILayout.Button("Apply", GUILayout.Width(50))) { OnClick_Apply(); }
        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndHorizontal();


    }


    // Sprite의 Importer 정보 가져오기
    private TextureImporter GetTextureImporter(Sprite sprite)
    {
        return AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(sprite)) as TextureImporter;
    }

    private void SetMeshType(E_SPRITE_MESHTYPE type)
    {
        if (_meshType != type) { _isChange = true; }
        _meshType = type;
    }
    private void SetSpritePivot(E_SPRITE_PIVOT pv)
    {

        if (_pivot != pv) { _isChange = true; }

        _pivot = pv;
        switch (_pivot)
        {
            case E_SPRITE_PIVOT.Center: _pivotValue = Vector2.one * 0.5f; break;
            case E_SPRITE_PIVOT.Top_Left: _pivotValue = new Vector2(0, 1); break;
            case E_SPRITE_PIVOT.Top: _pivotValue = new Vector2(0.5f, 1); break;
            case E_SPRITE_PIVOT.Top_Right: _pivotValue = new Vector2(1, 1); break;
            case E_SPRITE_PIVOT.Left: _pivotValue = new Vector2(0, 0.5f); break;
            case E_SPRITE_PIVOT.Right: _pivotValue = new Vector2(1, 0.5f); break;
            case E_SPRITE_PIVOT.Bottom_Left: _pivotValue = new Vector2(0, 0); break;
            case E_SPRITE_PIVOT.Bottom: _pivotValue = new Vector2(0.5f, 0); break;
            case E_SPRITE_PIVOT.Bottom_Right: _pivotValue = new Vector2(1, 0); break;
        }
    }

    private E_SPRITE_PIVOT GetSpritePivot(Vector2 pivot)
    {

        if (pivot.Equals(Vector2.one * 0.5f)) { return E_SPRITE_PIVOT.Center; }
        else if (pivot.Equals(Vector2.up)) { return E_SPRITE_PIVOT.Top_Left; }
        else if (pivot.Equals(new Vector2(0.5f, 1))) { return E_SPRITE_PIVOT.Top; }
        else if (pivot.Equals(Vector2.one)) { return E_SPRITE_PIVOT.Top_Right; }
        else if (pivot.Equals(new Vector2(0, 0.5f))) { return E_SPRITE_PIVOT.Left; }
        else if (pivot.Equals(new Vector2(1, 0.5f))) { return E_SPRITE_PIVOT.Right; }
        else if (pivot.Equals(Vector2.zero)) { return E_SPRITE_PIVOT.Bottom_Left; }
        else if (pivot.Equals(new Vector2(0.5f, 0))) { return E_SPRITE_PIVOT.Bottom; }
        else if (pivot.Equals(new Vector2(1, 0))) { return E_SPRITE_PIVOT.Bottom_Right; }

        return E_SPRITE_PIVOT.Center;
    }



    #endregion


    #region Sprite Preview
    public override bool HasPreviewGUI() { return _sprite != null; }
    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        GUI.DrawTexture(r, AssetPreview.GetAssetPreview(_sprite), ScaleMode.ScaleToFit, true);
    }

    #endregion







}














