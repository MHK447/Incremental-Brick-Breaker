#if UNITY_EDITOR

using System;
using System.IO;
using System.Reflection;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditor.SceneManagement;
using System.Linq;
using System.Collections.Generic;
using BanpoFri;

[InitializeOnLoad]
public class BanpoFriCustomToolbar
{
    [InitializeOnLoadMethod]
    private static void InitializeOnLoad() { EditorApplication.update -= OnUpdate; EditorApplication.update += OnUpdate; }

    static VisualElement leftZone;
    static VisualElement rightZone;
    static VisualElement playZone;
    static Slider timeSlider;
    static TextElement textSliderValue;
    static ToolbarMenu toolbar_Lang;
    static ToolbarButton btnAddressableRegister;
    static ToolbarButton btnBackground;

    static Color baseBtnColor = new Color(0.3f , 0.3f , 0.3f , 1f);

    private static void OnUpdate()
    {

        

        // 툴바 가져오기
        var toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
        var toolbars = Resources.FindObjectsOfTypeAll(toolbarType);
        var currentToolbar = toolbars.FirstOrDefault();
        if (currentToolbar == null) return;

        // 툴바 루트의 시각적 요소 얻기
        var bindingFlags = BindingFlags.NonPublic | BindingFlags.Instance;
        var fieldInfo = toolbarType.GetField("m_Root", bindingFlags);
        var toolbar = Convert.ChangeType(currentToolbar, toolbarType);
        var field = fieldInfo.GetValue(toolbar);
        var rootVisualElement = field as VisualElement;

        if (toolbar_Lang != null){
            toolbar_Lang.text = PlayerPrefs.GetString("UNITY_EDITOR_LANGS" , "en");
        }

        if (rootVisualElement != null)
        {
            if (leftZone == null) { leftZone = rootVisualElement.Q("ToolbarZoneLeftAlign"); }
            if (rightZone == null) { rightZone = rootVisualElement.Q("ToolbarZoneRightAlign"); }
            if (playZone == null) { playZone = rootVisualElement.Q("ToolbarZonePlayMode"); }
        


            if(btnBackground == null)
            {
                // Background 실행 토글 버튼
                btnBackground = new ToolbarButton(() => {
                    Application.runInBackground = !Application.runInBackground;
                    UpdateBackgroundButtonStyle();
                });
                btnBackground.style.marginLeft = 10;
                btnBackground.tooltip = "Toggle Run In Background";
                btnBackground.text = "BG";
                SetBackgroundColor_ToolbarBtn(btnBackground);
                UpdateBackgroundButtonStyle();
                
                leftZone.Add(btnBackground);
            }
            else
            {
                UpdateBackgroundButtonStyle();
            }


            if (timeSlider == null)
            {

                // 타임 스케일 타이틀
                var sliderTitle = new TextElement(){ style = { marginLeft = 5 , alignSelf = Align.Center } };
                sliderTitle.text = "TimeScale";
                playZone.Add(sliderTitle);

                // 타임 스케일 슬라이더 바
                timeSlider = new Slider(0 , 10){ style = { width = 100 } };
                timeSlider.RegisterValueChangedCallback( v => { Time.timeScale = v.newValue; textSliderValue.text = Time.timeScale.ToString();  });
                playZone.Add(timeSlider);

                // 타임 스케일 값 배경
                var sliderValueBG = new Toolbar(){ style = { width = 25 } }; 

                // 타임 스케일 값
                textSliderValue = new TextElement(){ style = { width = sliderValueBG.style.width , unityTextAlign = TextAnchor.MiddleCenter } };
                textSliderValue.text = Time.timeScale.ToString();
                sliderValueBG.Add( textSliderValue );
                playZone.Add(sliderValueBG);


                // 타임 스케일 초기화 버튼
                var btnInitSliderValue = new ToolbarButton(()=>{ 
                    timeSlider.value = 1; Time.timeScale = 1; textSliderValue.text = Time.timeScale.ToString(); 
                });
                btnInitSliderValue.style.marginLeft = 5;
                btnInitSliderValue.tooltip = "Init TimeScale 1";
                btnInitSliderValue.text = "INIT";
                SetBackgroundColor_ToolbarBtn(btnInitSliderValue);

                playZone.Add(btnInitSliderValue);
                timeSlider.value = Time.timeScale;


                // 언어 변경 툴바 
                toolbar_Lang = new ToolbarMenu(){ style = { width = btnInitSliderValue.style.width , marginLeft = 350 } };
                toolbar_Lang.text = PlayerPrefs.GetString("UNITY_EDITOR_LANGS" , "en");
                toolbar_Lang.tooltip = "Change Language";
                toolbar_Lang.style.marginLeft = 20;
                toolbar_Lang.style.paddingLeft = 6;
            

                List<string> langsList = new List<string>(Enum.GetNames(typeof(Language)));
                for (int i = 0; i < langsList.Count; i++){
                    toolbar_Lang.menu.AppendAction( langsList[i] , (drop) => {
                        toolbar_Lang.text = drop.name;
                        PlayerPrefs.SetString("UNITY_EDITOR_LANGS" , drop.name);

                        if (Application.isPlaying){

                            var editor_LangCode = drop.name;
                            Language curLang;
                            
                            System.Enum.TryParse<Language>(editor_LangCode, out curLang);
                            GameRoot.Instance.UserData.Language = curLang;
                            Config.Instance.UpdateFallbackOrder(curLang);

                            foreach (var ls in LocalizeString.Localizelist)
                            {
                                if (ls != null) {ls.RefreshText(); }
                            }

                            var list = GameRoot.Instance.UISystem.RefreshComponentList;
                            foreach (var ls in list) ls.RefreshText();

                        }

                    });
                }

                playZone.Add( toolbar_Lang );   



                // Left Zone

                // 씬 선택 메뉴
                var sceneMenu = new ToolbarMenu() { style = { width = 120, marginLeft = 350 } };
                sceneMenu.text = EditorSceneManager.GetActiveScene().name;
                sceneMenu.tooltip = "Change Select Scene";
                sceneMenu.style.marginLeft = 150;

                // Assets/Scenes 하위에 있는 씬 검색 (GUID 반환)
                var findScenesGUID = AssetDatabase.FindAssets("t:Scene", new string[] { "Assets/Scenes" });
                for (int i = 0; i < findScenesGUID.Length; i++)
                {

                    var convertPath = AssetDatabase.GUIDToAssetPath(findScenesGUID[i]);
                    var sceneName = convertPath.Replace("Assets/Scenes/", "").Replace(".unity", "");

                    sceneMenu.menu.AppendAction(sceneName, (drop) =>
                    {
                        EditorSceneManager.OpenScene(convertPath);
                        sceneMenu.text = drop.name;
                    });
                }

                leftZone.Add(sceneMenu);


                // PlayerPrefs 삭제 버튼
                var btnPrefabsClear = new ToolbarButton(() =>
                {
                    Debug.Log("DeleteAll PlayerPrefs!");
                    PlayerPrefs.DeleteAll();
                });
                btnPrefabsClear.style.marginLeft = 10;
                btnPrefabsClear.tooltip = "DeleteAll PlayerPrefs!";
                SetBackgroundColor_ToolbarBtn(btnPrefabsClear);

                // PlayerPrefs 삭제 버튼에 올라갈 이미지
                var iconPrefabsClear = new Image();
                iconPrefabsClear.image = EditorGUIUtility.IconContent("SaveFromPlay").image;
                btnPrefabsClear.Add(iconPrefabsClear);

                leftZone.Add(btnPrefabsClear);


                // Master 파일 삭제 버튼 (스낵바용)
                var btnMasterClear = new ToolbarButton(() =>
                {
                    string filePath = "Assets/Master.dat";
                    if (File.Exists(filePath))
                    {
                        Debug.Log("마스터 파일 삭제!");
                        File.Delete(filePath);
                        File.Delete(filePath + ".meta");
                        File.Delete(filePath + ".savebackup");
                        File.Delete(filePath + ".savebackup.meta");
                        AssetDatabase.Refresh();
                    }
                    else
                    {
                        Debug.Log("마스터 파일이 존재하지 않음!");
                    }
                });
                btnMasterClear.style.marginLeft = 10;
                btnMasterClear.tooltip = "DeleteAll Master File!";
                SetBackgroundColor_ToolbarBtn(btnMasterClear);

                // PlayerPrefs 삭제 버튼에 올라갈 이미지
                var iconMasterRemove2 = new Image();
                iconMasterRemove2.image = EditorGUIUtility.IconContent("TreeEditor.Trash").image;
                btnMasterClear.Add(iconMasterRemove2);

                leftZone.Add(btnMasterClear);



                // Right Zone
                var savedataMenu = new ToolbarMenu() { style = { width = 150 } };

                // 세이브 데이터 새로고침 버튼
                var btnRefreshSaveData = new ToolbarButton(() =>
                {
                    RefreshSaveDataList(savedataMenu);
                }) { style = { marginRight = 200 } };
                btnRefreshSaveData.style.marginLeft = 10;
                btnRefreshSaveData.tooltip = "Refresh .dat List";
                SetBackgroundColor_ToolbarBtn(btnRefreshSaveData);
                

                // PlayerPrefs 삭제 버튼에 올라갈 이미지
                var iconRefresh = new Image();
                iconRefresh.image = EditorGUIUtility.IconContent("TreeEditor.Refresh").image;
                btnRefreshSaveData.Add(iconRefresh);

                rightZone.Add(btnRefreshSaveData);



                // 세이브 데이터 선택 메뉴 (정렬 순서상 후 기입)
                savedataMenu.tooltip = "Change Target .dat";
                RefreshSaveDataList(savedataMenu);

                rightZone.Add(savedataMenu);
            }

        }




    }

    private static void RefreshSaveDataList(ToolbarMenu savedataMenu)
    {
        //세이브 데이터 리스트 갱신
        savedataMenu.menu.MenuItems().Clear();

        var CustomSaveData = "Master";
        var CustomSaveDataKey = "CustomSaveDataKey";
        if (EditorPrefs.HasKey(CustomSaveDataKey))
        {
            CustomSaveData = EditorPrefs.GetString(CustomSaveDataKey);
        }
        savedataMenu.text = CustomSaveData;

        // Assets 하위에 있는 .dat 검색
        var findDats = Directory.GetFiles("Assets/", "*.dat");
        for (int i = 0; i < findDats.Length; i++)
        {
            var datName = findDats[i].Replace("Assets/", "").Replace(".dat", "");
            savedataMenu.menu.AppendAction(datName, (drop) =>
            {
                EditorPrefs.SetString(CustomSaveDataKey, drop.name);
                savedataMenu.text = drop.name;
            });

            if (string.Equals(datName, CustomSaveData))
            {
                savedataMenu.text = CustomSaveData;
            }
        }
    }


    private static void SetBackgroundColor_ToolbarBtn(ToolbarButton btn){

        var overColor = new Color(baseBtnColor.r + 0.1f , baseBtnColor.g + 0.1f , baseBtnColor.b + 0.1f , baseBtnColor.a);
        btn.style.backgroundColor = baseBtnColor;
        var clickColor = new Color(0.2745098f , 0.3764706f , 0.4862745f , 1f);


        // 마우스 오버 이벤트
        btn.RegisterCallback<MouseEnterEvent>(evt => { btn.style.backgroundColor = new StyleColor(overColor); });
        // 마우스 나가기 이벤트
        btn.RegisterCallback<MouseLeaveEvent>(evt => { btn.style.backgroundColor = new StyleColor(baseBtnColor); });

        // 마우스 다운 이벤트
        btn.RegisterCallback<MouseDownEvent>(evt => {  btn.style.backgroundColor = new StyleColor(clickColor); } , TrickleDown.TrickleDown);

        // 마우스 업 이벤트
        btn.RegisterCallback<MouseUpEvent>(evt => {  btn.style.backgroundColor = new StyleColor(baseBtnColor); } , TrickleDown.TrickleDown);
    }

    private static void UpdateBackgroundButtonStyle()
    {
        if (btnBackground != null)
        {
            btnBackground.style.backgroundColor = Application.runInBackground ? 
                new Color(0.4f, 0.7f, 0.4f, 1f) : // 활성화 시 초록색
                baseBtnColor; // 비활성화 시 기본 색상
        }
    }

}

#endif

