using UnityEngine;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;
using System;
using System.IO;
using System.Reflection;
using System.Collections;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class CaptureRecorderWindow : EditorWindow
{
    private string savePath = "";
    private bool showDebugConsole = true;
    private bool isRecording = false;
    private bool isRecorderAvailable = false;
    private GameObject debugConsoleObj = null;
    private bool isInitialRecordingDone = false; // 첫 녹화 초기화 여부
    
    // EditorPrefs 키
    private const string SAVE_PATH_PREF_KEY = "TreepllaCaptureRecorderSavePath";
    private const string INITIAL_RECORDING_DONE_KEY = "TreepllaCaptureRecorderInitialDone";
    
    // 캡처 관련 변수
    private enum ImageFormat { JPG, PNG }
    private ImageFormat selectedImageFormat = ImageFormat.PNG;
    
    // 녹화 관련 변수
    private bool useHighFrameRate = true; // true = 60fps, false = 30fps
    private enum VideoFormat { MP4, AVI, WebM }
    private VideoFormat selectedVideoFormat = VideoFormat.MP4;
    private RecorderController recorderController;
    
    // 성능 개선 옵션
    private bool optimizeForPerformance = true; // 성능 최적화 여부
    private float captureEveryNthFrame = 1; // 매 몇 프레임마다 캡처할지 (기본: 매 프레임)
    
    [MenuItem("BanpoFri/캡쳐-녹화")]
    public static void ShowWindow()
    {
        CaptureRecorderWindow window = GetWindow<CaptureRecorderWindow>("캡쳐-녹화");
        window.minSize = new Vector2(400, 500);
        window.maxSize = new Vector2(600, 700);
    }
    
    private void OnEnable()
    {
        // 저장된 경로 불러오기
        savePath = EditorPrefs.GetString(SAVE_PATH_PREF_KEY, "");
        
        // 경로가 없으면 기본 경로 설정
        if (string.IsNullOrEmpty(savePath))
        {
            savePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "UnityCaptures");
        }
        
        // 첫 녹화 초기화 여부 확인
        isInitialRecordingDone = EditorPrefs.GetBool(INITIAL_RECORDING_DONE_KEY, false);
        
        // Unity Recorder 패키지 존재 여부 확인
        CheckRecorderAvailability();
        
        // DebugConsole 찾기
        FindDebugConsole();
        
        // 프레임 드랍 방지를 위한 초기화 처리
        if (!isInitialRecordingDone && isRecorderAvailable)
        {
            PreInitializeRecorder();
        }
    }
    
    // 첫 녹화 시 프레임 드랍 방지를 위한 초기화
    private void PreInitializeRecorder()
    {
        EditorApplication.delayCall += () => {
            try {
                // 최소 해상도, 최소 시간으로 녹화 준비 (실제로 녹화하지는 않음)
                var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
                var videoRecorder = ScriptableObject.CreateInstance<MovieRecorderSettings>();
                videoRecorder.name = "Initializer";
                videoRecorder.Enabled = true;
                videoRecorder.ImageInputSettings = new GameViewInputSettings
                {
                    OutputWidth = 320,
                    OutputHeight = 240
                };
                videoRecorder.OutputFile = Path.Combine(Application.temporaryCachePath, "temp");
                videoRecorder.FrameRate = 30;
                
                controllerSettings.AddRecorderSettings(videoRecorder);
                controllerSettings.SetRecordModeToManual();
                
                var initRecorder = new RecorderController(controllerSettings);
                initRecorder.PrepareRecording();
                
                // 메모리 정리
                EditorUtility.UnloadUnusedAssetsImmediate(true);
                GC.Collect();
                
                // 초기화 완료 기록
                isInitialRecordingDone = true;
                EditorPrefs.SetBool(INITIAL_RECORDING_DONE_KEY, true);
                
                UnityEngine.Object.DestroyImmediate(videoRecorder);
                UnityEngine.Object.DestroyImmediate(controllerSettings);
            }
            catch (Exception e) {
                Debug.LogError($"레코더 초기화 중 오류 발생: {e.Message}");
            }
        };
    }
    
    private void FindDebugConsole()
    {
        // 먼저 IngameDebugConsole 직접 찾기
        debugConsoleObj = GameObject.Find("IngameDebugConsole");
        if (debugConsoleObj != null)
        {
            Debug.Log("IngameDebugConsole 게임 오브젝트를 찾았습니다.");
            return;
        }
        
        // 모든 루트 게임 오브젝트 검색
        foreach (GameObject obj in UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects())
        {
            // 정확한 이름 검색
            if (obj.name == "IngameDebugConsole")
            {
                debugConsoleObj = obj;
                Debug.Log("IngameDebugConsole 게임 오브젝트를 찾았습니다.");
                return;
            }
            
            // 자식 오브젝트 중에서도 찾기
            Transform child = obj.transform.Find("IngameDebugConsole");
            if (child != null)
            {
                debugConsoleObj = child.gameObject;
                Debug.Log("IngameDebugConsole 자식 게임 오브젝트를 찾았습니다.");
                return;
            }
        }
        
        Debug.LogWarning("IngameDebugConsole 게임 오브젝트를 찾을 수 없습니다.");
    }
    
    private void CheckRecorderAvailability()
    {
        try
        {
            // Unity Recorder API 접근 시도
            var settings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            isRecorderAvailable = true;
            UnityEngine.Object.DestroyImmediate(settings);
        }
        catch (System.Exception)
        {
            isRecorderAvailable = false;
        }
    }
    
    private void OnGUI()
    {
        // 공통 설정 영역
        GUILayout.Space(10);
        EditorGUILayout.LabelField("공통 설정", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("iOS 패키지 심사 이미지를 하기 위해서는 Game씬을 아이폰 해상도로 해주세요", MessageType.Info);
        
        GUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("저장 위치:", GUILayout.Width(70));
        string newSavePath = EditorGUILayout.TextField(savePath);
        if (newSavePath != savePath)
        {
            savePath = newSavePath;
            // 경로 변경 시 저장
            EditorPrefs.SetString(SAVE_PATH_PREF_KEY, savePath);
        }
        
        if (GUILayout.Button("찾아보기", GUILayout.Width(80)))
        {
            string path = EditorUtility.OpenFolderPanel("저장 위치 선택", savePath, "");
            if (!string.IsNullOrEmpty(path))
            {
                savePath = path;
                // 경로 변경 시 저장
                EditorPrefs.SetString(SAVE_PATH_PREF_KEY, savePath);
            }
        }
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(5);
        bool newDebugConsole = EditorGUILayout.Toggle("Debug Console 표시", showDebugConsole);
        if (newDebugConsole != showDebugConsole)
        {
            showDebugConsole = newDebugConsole;
            ToggleDebugConsole(showDebugConsole);
        }
        
        // 구분선
        GUILayout.Space(10);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Space(10);
        
        // 캡처 설정 영역
        EditorGUILayout.LabelField("캡처 설정", EditorStyles.boldLabel);
        
        GUILayout.Space(5);
        selectedImageFormat = (ImageFormat)EditorGUILayout.EnumPopup("이미지 포맷:", selectedImageFormat);
        
        GUILayout.Space(5);
        if (GUILayout.Button("캡처하기", GUILayout.Height(30)))
        {
            CaptureScreenshot();
        }
        
        // 구분선
        GUILayout.Space(10);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Space(10);
        
        // 녹화 설정 영역
        EditorGUILayout.LabelField("녹화 설정", EditorStyles.boldLabel);
        
        if (!isRecorderAvailable)
        {
            EditorGUILayout.HelpBox("Unity Recorder 패키지가 설치되어 있지 않습니다. Package Manager에서 'Unity Recorder'를 설치해주세요.", MessageType.Warning);
            if (GUILayout.Button("패키지 매니저 열기"))
            {
                OpenPackageManager();
            }
            return;
        }
        
        GUILayout.Space(5);
        EditorGUILayout.HelpBox("녹화 시 소리도 자동으로 녹음되며 녹음 중에는 소리가 나지않습니다", MessageType.Info);
        
        GUILayout.Space(5);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("프레임 설정:");
        if (GUILayout.Toggle(useHighFrameRate, "60 FPS", EditorStyles.radioButton))
            useHighFrameRate = true;
        if (GUILayout.Toggle(!useHighFrameRate, "30 FPS", EditorStyles.radioButton))
            useHighFrameRate = false;
        EditorGUILayout.EndHorizontal();
        
        GUILayout.Space(5);
        selectedVideoFormat = (VideoFormat)EditorGUILayout.EnumPopup("비디오 포맷:", selectedVideoFormat);
        
        // 성능 최적화 옵션 추가
        GUILayout.Space(5);
        optimizeForPerformance = EditorGUILayout.Toggle("성능 최적화", optimizeForPerformance);
        
        if (optimizeForPerformance)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.HelpBox("성능 최적화 기능이 활성화되면 화질은 약간 저하될 수 있으나 녹화 중 프레임 드랍이 줄어듭니다.", MessageType.Info);
            EditorGUI.indentLevel--;
        }
        
        if (!isInitialRecordingDone)
        {
            GUILayout.Space(5);
            EditorGUILayout.HelpBox("첫 번째 녹화 시작 시 초기화 과정이 필요합니다.", MessageType.Info);
        }
        
        GUILayout.Space(10);
        
        EditorGUILayout.BeginHorizontal();
        GUI.enabled = !isRecording;
        if (GUILayout.Button(isRecording ? "녹화 중..." : "녹화 시작", GUILayout.Height(30)))
        {
            ToggleRecording();
        }
        GUI.enabled = isRecording;
        if (GUILayout.Button("녹화 중지", GUILayout.Height(30)))
        {
            StopRecording();
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();
    }
    
    private void CaptureScreenshot()
    {
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);
            
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string extension = selectedImageFormat == ImageFormat.PNG ? "png" : "jpg";
        string filename = $"Screenshot_{timestamp}.{extension}";
        string fullPath = Path.Combine(savePath, filename);
        
        ScreenCapture.CaptureScreenshot(fullPath);
        Debug.Log($"스크린샷 저장 완료: {fullPath}");
        
        // 폴더 열기
        OpenSaveFolder();
    }
    
    private void ToggleRecording()
    {
        if (isRecording)
            StopRecording();
        else
            StartRecording();
    }
    
    private void StartRecording()
    {
        if (!isRecorderAvailable) return;
        
        if (!Directory.Exists(savePath))
            Directory.CreateDirectory(savePath);
        
        try
        {
            // 녹화 전 메모리 정리
            EditorUtility.UnloadUnusedAssetsImmediate(true);
            GC.Collect();
            
            var controllerSettings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
            var mediaOutputFolder = savePath;
    
            // 레코딩 설정
            var videoRecorder = ScriptableObject.CreateInstance<MovieRecorderSettings>();
            videoRecorder.name = "Video Recorder";
            videoRecorder.Enabled = true;
    
            // 출력 설정
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string extension = GetVideoFileExtension();
            videoRecorder.OutputFile = Path.Combine(mediaOutputFolder, $"Recording_{timestamp}");
            
            // 인코딩 설정 - 성능 최적화 시 비트레이트 낮춤
            if (typeof(MovieRecorderSettings).GetProperty("VideoBitRateMode") != null)
            {
                int bitrateMode = optimizeForPerformance ? 0 : 1; // 성능 최적화 시 Low, 아니면 Medium
                videoRecorder.GetType().GetProperty("VideoBitRateMode").SetValue(videoRecorder, bitrateMode);
            }
            
            // 현재 게임 뷰 해상도 가져오기
            Vector2 gameViewSize = GetMainGameViewSize();
            
            // 원본 해상도 사용 (스케일링 없음)
            int width = (int)gameViewSize.x;
            int height = (int)gameViewSize.y;
            
            // 성능 최적화 시 해상도 감소
            if (optimizeForPerformance)
            {
                // 해상도를 유지하되 captureEveryNthFrame 설정
                captureEveryNthFrame = 2; // 2프레임마다 캡처
                
                // 해상도는 약간만 줄임 (95% 유지)
                width = (int)(width * 0.95f);
                height = (int)(height * 0.95f);
            }
            else
            {
                captureEveryNthFrame = 1; // 모든 프레임 캡처
            }
            
            // 홀수 해상도 검사 및 조정 (짝수로 만들기)
            if (selectedVideoFormat == VideoFormat.MP4)
            {
                if (width % 2 != 0) width++;
                if (height % 2 != 0) height++;
            }
            
            // 캡처 소스 설정 (게임 뷰)
            videoRecorder.ImageInputSettings = new GameViewInputSettings
            {
                OutputWidth = width,
                OutputHeight = height
            };
            
            // 프레임 레이트 설정 (30 또는 60)
            int frameRate = useHighFrameRate ? 60 : 30;
            videoRecorder.FrameRate = frameRate;
            
            // 성능 최적화를 위한 캡처 프레임 설정
            if (optimizeForPerformance && typeof(RecorderSettings).GetProperty("CaptureEveryNthFrame") != null)
            {
                try
                {
                    videoRecorder.GetType().GetProperty("CaptureEveryNthFrame").SetValue(
                        videoRecorder, (int)captureEveryNthFrame);
                }
                catch (Exception)
                {
                    // 속성이 없거나 설정 실패 시 무시
                }
            }
            
            // 코덱 설정 - 직접 VideoCodec 열거형을 사용하지 않고 문자열 속성 사용
            SetVideoFormat(videoRecorder, selectedVideoFormat);
            
            // 오디오 활성화
            videoRecorder.CaptureAudio = true;
            
            // 녹화 설정 적용
            controllerSettings.AddRecorderSettings(videoRecorder);
            controllerSettings.SetRecordModeToManual();
            controllerSettings.FrameRate = frameRate;
            
            // 초기화 여부 기록
            if (!isInitialRecordingDone)
            {
                isInitialRecordingDone = true;
                EditorPrefs.SetBool(INITIAL_RECORDING_DONE_KEY, true);
                Debug.Log("녹화 초기화가 완료되었습니다. 이제부터 프레임 드랍이 줄어들 것입니다.");
            }
            
            // 레코더 컨트롤러 생성 및 시작
            recorderController = new RecorderController(controllerSettings);
            recorderController.PrepareRecording();
            recorderController.StartRecording();
            
            isRecording = true;
            
            // 게임 오디오 활성화
            SetGameAudio(true);
            
            string performanceNote = optimizeForPerformance ? " (성능 최적화 모드)" : "";
            Debug.Log($"녹화 시작됨: {videoRecorder.OutputFile}.{extension} (해상도: {width}x{height}, 프레임: {frameRate}fps){performanceNote}");
        }
        catch (Exception e)
        {
            Debug.LogError($"녹화 시작 중 오류 발생: {e.Message}\n{e.StackTrace}");
        }
    }
    
    private void SetVideoFormat(MovieRecorderSettings recorder, VideoFormat format)
    {
        // Unity Recorder 버전에 따라 API가 다를 수 있으므로 리플렉션 사용
        try
        {
            var codecProperty = recorder.GetType().GetProperty("OutputFormat");
            if (codecProperty != null)
            {
                // 최신 버전 API
                int formatValue;
                switch (format)
                {
                    case VideoFormat.MP4:
                        formatValue = 0; // MP4
                        break;
                    case VideoFormat.WebM:
                        formatValue = 1; // WebM
                        break;
                    default:
                        formatValue = 0; // 기본값 MP4
                        break;
                }
                codecProperty.SetValue(recorder, formatValue);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"비디오 포맷 설정 오류: {e.Message}");
        }
    }
    
    private void StopRecording()
    {
        if (recorderController != null && isRecording)
        {
            try
            {
                recorderController.StopRecording();
                isRecording = false;
                Debug.Log("녹화 완료. 파일이 저장되었습니다.");
                
                // 녹화 후 메모리 정리
                EditorUtility.UnloadUnusedAssetsImmediate(true);
                GC.Collect();
                
                // 폴더 열기
                OpenSaveFolder();
            }
            catch (Exception e)
            {
                Debug.LogError($"녹화 중지 중 오류 발생: {e.Message}");
                isRecording = false;
            }
        }
    }
    
    private string GetVideoFileExtension()
    {
        switch (selectedVideoFormat)
        {
            case VideoFormat.MP4: return "mp4";
            case VideoFormat.AVI: return "avi";
            case VideoFormat.WebM: return "webm";
            default: return "mp4";
        }
    }
    
    private void OpenPackageManager()
    {
        EditorApplication.ExecuteMenuItem("Window/Package Manager");
    }
    
    private void ToggleDebugConsole(bool show)
    {
        // DebugConsole이 없으면 다시 찾기 시도
        if (debugConsoleObj == null)
        {
            FindDebugConsole();
        }
        
        // 이제 DebugConsole 활성화/비활성화
        if (debugConsoleObj != null)
        {
            debugConsoleObj.SetActive(show);
            Debug.Log($"DebugConsole 게임 오브젝트 {(show ? "활성화" : "비활성화")}됨");
        }
        else
        {
            Debug.LogWarning("DebugConsole 게임 오브젝트를 찾을 수 없습니다.");
        }
    }
    
    private void SetGameAudio(bool enabled)
    {
        // 모든 AudioListener 컴포넌트 찾기
        AudioListener[] audioListeners = FindObjectsOfType<AudioListener>();
        foreach (var listener in audioListeners)
        {
            listener.enabled = enabled;
        }
    }
    
    private void OnDestroy()
    {
        // 윈도우가 닫힐 때 녹화 중지
        if (isRecording)
            StopRecording();
    }
    
    // 현재 게임 뷰 해상도를 가져오는 함수
    private Vector2 GetMainGameViewSize()
    {
        try
        {
            // 에디터에서 게임 뷰 해상도 가져오기
            Type gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
            if (gameViewType != null)
            {
                EditorWindow gameView = EditorWindow.GetWindow(gameViewType);
                if (gameView != null)
                {
                    PropertyInfo targetSizeProperty = gameViewType.GetProperty("targetSize", 
                        BindingFlags.Instance | BindingFlags.NonPublic);
                    
                    if (targetSizeProperty != null)
                    {
                        Vector2 size = (Vector2)targetSizeProperty.GetValue(gameView);
                        if (size.x > 0 && size.y > 0)
                        {
                            return size;
                        }
                    }
                }
            }
            
            // 게임 뷰 크기를 가져올 수 없는 경우 현재 화면 해상도 반환
            return new Vector2(Screen.width, Screen.height);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"게임 뷰 해상도를 가져오는 중 오류 발생: {e.Message}. 기본 해상도(1920x1080)를 사용합니다.");
            return new Vector2(1920, 1080);
        }
    }
    
    // 저장 폴더 열기
    private void OpenSaveFolder()
    {
        if (Directory.Exists(savePath))
        {
            try
            {
                // 시스템 파일 탐색기로 폴더 열기
                System.Diagnostics.Process.Start(savePath);
            }
            catch (Exception e)
            {
                Debug.LogError($"폴더를 열지 못했습니다: {e.Message}");
            }
        }
    }
} 
