using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace BanpoFri
{
    public interface ILocalizeRefresh
    {
        void RefreshText();
    }
    [Serializable]
    public class UISystem
    {
        public const string STR_SORTINGLAYER_PAGE = "Page";
        public const string STR_SORTINGLAYER_POPUP = "Popup";
        public const string STR_SORTINGLAYER_TOP = "Top";
        public const string STR_SORTINGLAYER_LAST = "Last";
        public const int START_PAGE_SORTING_NUMBER = 100;
        public const int START_POPUP_SORTING_NUMBER = 10000;
        public const int START_TOP_SORTING_NUMBER = 20000;
        public const int START_LAST_SORTING_NUMBER = 30000;
        public const int START_EFFECT_SORTING_NUMBER = 30000;

        public static Dictionary<UIBaseType, int> BaseSortingOrder = new()
        {
            { UIBaseType.Page           , 0 },
            { UIBaseType.HUDBottom      , 5000 },
            { UIBaseType.Ingame         , 10000 },
            { UIBaseType.Popup          , 15000 },
            { UIBaseType.HUDTop         , 20000 },
            { UIBaseType.OverlayPopup   , 25000 },
            { UIBaseType.Fullscreen     , 30000 },
        };

        public bool CheatHide = false;
        public Transform UIRootT { get; private set; }
        public Transform HUDUIRootT { get; private set; }
        public Canvas WorldCanvas { get; private set; }
        public GameObject LockScreen;
        private Dictionary<Type, UIBase> cachedUIs = new Dictionary<Type, UIBase>();
        private Dictionary<Type, GameObject> cachedIngameUITrans = new Dictionary<Type, GameObject>();
        private List<UIBase> openPopupList = new List<UIBase>();
        private List<UIBase> openLastList = new List<UIBase>();
        private Dictionary<UIBaseType, List<UIBase>> openedUIMap = new();
        private List<UIBase> lastOpenedUI = new();

        private List<IScreenAction> screenActionList = new List<IScreenAction>();
        private bool cacheMode = true;
        private Type LoadWaitUI = null;
        private Action<UIBase> OnLoadWait = null;
        private bool backButtonEnable = true;
        public List<ILocalizeRefresh> RefreshComponentList { get; private set; } = new List<ILocalizeRefresh>();


        void BackButtonEnabled()
        {
            backButtonEnable = true;
        }

        public void OpenUI<T>(Action<T> OnLoad = null, Action OnClose = null, bool caching = true, int targetEventStage = -1) where T : UIBase
        {
            Action LoadComplete = () =>
            {
                var targetUI = cachedUIs[typeof(T)] as T;
                if (targetUI)
                {
                    var baseCanvas = targetUI.GetComponent<Canvas>();
                    if (!targetUI.gameObject.activeSelf)
                        LockScreen.SetActive(true);
                    targetUI.OnUIShowBefore = () => { LockScreen.SetActive(false); };
                    targetUI.Show();
                    targetUI.gameObject.SetActive(true);
                    baseCanvas.overrideSorting = true;
                    AddToSortingOrder(targetUI);
                    AddToOpenedStack(targetUI);

                    switch (targetUI.UIType)
                    {
                        case UIBaseType.Popup:
                            {
                                if (!openPopupList.Contains(targetUI))
                                    openPopupList.Add(targetUI);

                                var uiBase = targetUI.GetComponent<UIBase>();
                                uiBase.ReOderParticleInUIBase();
                                if ((!cacheMode) || caching == false)
                                {
                                    targetUI.OnUIHideAfter += () =>
                                    {
                                        if (!Addressables.ReleaseInstance(targetUI.gameObject))
                                            GameObject.Destroy(targetUI.gameObject);
                                    };
                                }
                                backButtonEnable = false;
                                targetUI.OnUIShowAfter += BackButtonEnabled;

                                targetUI.OnUIHide = () =>
                                {
                                    if ((!cacheMode) || caching == false)
                                    {
                                        cachedUIs.Remove(typeof(T));
                                        GC.Collect();
                                    }

                                    openPopupList.Remove(targetUI);

                                    targetUI.OnUIShowAfter -= BackButtonEnabled;
                                    RemoveFromSortingOrder(targetUI);
                                    RemoveOpenedStack(targetUI);
                                    calculateHUDAction();
                                    OnClose?.Invoke();
                                    IsMainActive();
                                };
                            }
                            break;
                        case UIBaseType.Page:
                            {
                                var uiBase = targetUI.GetComponent<UIBase>();
                                if (uiBase is IScreenAction)
                                {
                                    if (!screenActionList.Contains(uiBase as IScreenAction))
                                        screenActionList.Add(uiBase as IScreenAction);
                                }

                                targetUI.OnUIHide = () =>
                                {
                                    OnClose?.Invoke();
                                    screenActionList.Remove(uiBase as IScreenAction);
                                    RemoveFromSortingOrder(targetUI);
                                    RemoveOpenedStack(targetUI);
                                    IsMainActive();
                                };
                            }
                            break;
                        case UIBaseType.OverlayPopup:
                            targetUI.OnUIHide = () =>
                            {
                                RemoveFromSortingOrder(targetUI);
                                RemoveOpenedStack(targetUI);
                                OnClose?.Invoke();
                                IsMainActive();
                            };
                            break;
                        case UIBaseType.Fullscreen:
                            {
                                baseCanvas.overrideSorting = true;
                                var uiBase = targetUI.GetComponent<UIBase>();
                                uiBase.ReOderParticleInUIBase();
                                if ((!cacheMode) || caching == false)
                                {
                                    targetUI.OnUIHideAfter += () =>
                                    {
                                        if (!Addressables.ReleaseInstance(targetUI.gameObject))
                                            GameObject.Destroy(targetUI.gameObject);
                                    };
                                }
                                backButtonEnable = false;
                                targetUI.OnUIShowAfter += BackButtonEnabled;

                                targetUI.OnUIHide = () =>
                                {
                                    if ((!cacheMode) || caching == false)
                                    {
                                        cachedUIs.Remove(typeof(T));
                                        GC.Collect();
                                    }

                                    targetUI.OnUIShowAfter -= BackButtonEnabled;
                                    RemoveFromSortingOrder(targetUI);
                                    RemoveOpenedStack(targetUI);
                                    calculateHUDAction();
                                    OnClose?.Invoke();
                                    IsMainActive();
                                };
                            }
                            break;
                        default:
                            targetUI.OnUIHide = () =>
                            {
                                RemoveFromSortingOrder(targetUI);
                                RemoveOpenedStack(targetUI);
                                OnClose?.Invoke();
                            };
                            break;
                    }
                    targetUI.CustomSortingOrder();
                    if (LoadWaitUI != null && LoadWaitUI.Equals(targetUI.GetType()))
                    {
                        OnLoadWait?.Invoke(targetUI);
                        OnLoadWait = null;
                    }
                    OnLoad?.Invoke(targetUI);
                }
                else
                    Debug.LogError($"UIBase::OpenUI don't load uiBase type: {typeof(T).Name}");
            };

            if (!cachedUIs.ContainsKey(typeof(T)))
            {
                bool findAtt = false;
                var attrs = Attribute.GetCustomAttributes(typeof(T));

                bool useEventPopup = false;
                var isExistPath = Array.Exists(attrs, x => x is UIPathAttribute);
                var isExistEvent = Array.Exists(attrs, x => x is UIEventPathAttribute);
                if (isExistEvent && targetEventStage > 10000) useEventPopup = true;

                System.Action<string> createUI = (string path) => { };
                foreach (var attr in attrs)
                {
                    if (useEventPopup && attr is UIEventPathAttribute)
                    {
                        var uiPath = (UIEventPathAttribute)attr;

                        var addrPath = string.Format(uiPath.Path, targetEventStage);

                        cachedUIs.Add(typeof(T), null);

                        Addressables.InstantiateAsync(addrPath).Completed += (obj) =>
                        {
                            //var inst = GameObject.Instantiate( obj.Result );
                            var inst = obj.Result;

                            inst.transform.SetParent(UIRootT, false);

                            var uiBase = inst.GetComponent<T>();
                            cachedUIs[typeof(T)] = uiBase;
                            inst.gameObject.SetActive(false);
                            LoadComplete.Invoke();
                        };
                        findAtt = true;
                        break;
                    }
                    else if (attr is UIPathAttribute)
                    {
                        var uiPath = (UIPathAttribute)attr;

                        cachedUIs.Add(typeof(T), null);
                        //Addressables.LoadAssetAsync<GameObject>(uiPath.Path).Completed += (obj) => {
                        Addressables.InstantiateAsync(uiPath.Path).Completed += (obj) =>
                        {
                            //var inst = GameObject.Instantiate( obj.Result );
                            var inst = obj.Result;
                            if (inst != null)
                            {
                                if (uiPath.Hud)
                                    inst.transform.SetParent(HUDUIRootT, false);
                                else if (uiPath.World)
                                    inst.transform.SetParent(WorldCanvas.transform, false);
                                else
                                    inst.transform.SetParent(UIRootT, false);

                                var uiBase = inst.GetComponent<T>();
                                if (uiBase != null)
                                {
                                    cachedUIs[typeof(T)] = uiBase;
                                }
                                inst.gameObject.SetActive(false);
                                LoadComplete?.Invoke();
                            }
                        };
                        findAtt = true;
                        break;
                    }
                }

                if (!findAtt)
                {
                    Debug.LogError("UIBase::OpenUI don't find UIPathAttribute. plz attach them");
                    return;
                }
            }
            else
            {
                if (cachedUIs[typeof(T)] != null)
                    LoadComplete.Invoke();
            }
        }


        public void ClearDeActiveUI()
        {
            var removeList = new List<Type>();
            foreach (var ui in cachedUIs)
            {
                if (!ui.Value.gameObject.activeSelf)
                {
                    if (!Addressables.ReleaseInstance(ui.Value.gameObject))
                        GameObject.Destroy(ui.Value.gameObject);

                    removeList.Add(ui.Key);
                }
            }

            foreach (var type in removeList)
                cachedUIs.Remove(type);
        }


        public void PreLoadUI(Type type, Action<UIBase> OnLoad = null)
        {
            var attrs = Attribute.GetCustomAttributes(type);
            foreach (var attr in attrs)
            {
                if (attr is UIPathAttribute)
                {
                    var uiPath = (UIPathAttribute)attr;

                    if (cachedUIs.ContainsKey(type)) continue;

                    cachedUIs.Add(type, null);
                    Addressables.InstantiateAsync(uiPath.Path).Completed += (obj) =>
                    {
                        var inst = obj.Result;
                        if (uiPath.Hud)
                            inst.transform.SetParent(HUDUIRootT, false);
                        else if (uiPath.World)
                            inst.transform.SetParent(WorldCanvas.transform, false);
                        else
                            inst.transform.SetParent(UIRootT, false);

                        var uiBase = inst.GetComponent(type) as UIBase;
                        cachedUIs[type] = uiBase;
                        inst.SetActive(false);
                        OnLoad?.Invoke(uiBase);
                    };
                    break;
                }
            }
        }

        public void SetFloatingUIActiveAll(bool value)
        {
            foreach (var uiTrans in cachedIngameUITrans)
                Utility.SetActiveCheck(uiTrans.Value, value);
        }

        public void SetFloatingUIActive<T>(bool value) where T : IFloatingUI
        {
            var TType = typeof(T);
            if (cachedIngameUITrans.ContainsKey(TType))
            {
                Utility.SetActiveCheck(cachedIngameUITrans[TType], value);
            }
        }

        public void SetFloatingUITargetAniHide<T>(int exceptchild = -1) where T : IFloatingUI
        {
            var TType = typeof(T);
            if (cachedIngameUITrans.ContainsKey(TType))
            {
                var childCount = cachedIngameUITrans[TType].transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    if (exceptchild == i) continue;
                    cachedIngameUITrans[TType].transform.GetChild(i).GetComponent<T>().Hide();
                }
            }
        }



        public void SetFloatingUITargetActive<T>(bool value, int exceptchild = -1) where T : IFloatingUI
        {
            var TType = typeof(T);
            if (cachedIngameUITrans.ContainsKey(TType))
            {
                var childCount = cachedIngameUITrans[TType].transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    if (exceptchild == i) continue;
                    Utility.SetActiveCheck(cachedIngameUITrans[TType].transform.GetChild(i).gameObject, value);
                }
            }
        }

        public GameObject GetFloatingUI<T>() where T : IFloatingUI
        {
            var TType = typeof(T);
            if (cachedIngameUITrans.ContainsKey(TType))
            {
                if (cachedIngameUITrans[TType].transform.childCount > 0)
                    return cachedIngameUITrans[TType].transform.GetChild(0).gameObject;
            }

            return null;
        }

        public GameObject GetActiveFloatingUI<T>() where T : IFloatingUI
        {
            var TType = typeof(T);
            if (cachedIngameUITrans.ContainsKey(TType))
            {
                if (cachedIngameUITrans[TType].transform.childCount > 0)
                {
                    for (int i = 0; i < cachedIngameUITrans[TType].transform.childCount; ++i)
                    {
                        if (cachedIngameUITrans[TType].transform.GetChild(i).gameObject.activeSelf)
                        {
                            return cachedIngameUITrans[TType].transform.GetChild(i).gameObject;
                        }
                    }
                }
            }

            return null;
        }

        public void LoadFloatingUI<T>(Action<T> onSuccess, bool caching = false, float scale = 1f) where T : IFloatingUI
        {
            var TType = typeof(T);
            if (caching)
            {
                if (cachedIngameUITrans.ContainsKey(TType))
                {
                    if (cachedIngameUITrans[TType].transform.childCount > 0)
                    {
                        cachedIngameUITrans[TType].transform.SetAsLastSibling();
                        onSuccess?.Invoke(cachedIngameUITrans[TType].transform.GetChild(0).GetComponent<T>());
                        return;
                    }
                }
            }
            var attrs = Attribute.GetCustomAttributes(TType);
            foreach (var attr in attrs)
            {
                if (attr is FloatUIPathAttribute)
                {
                    var uiPath = (FloatUIPathAttribute)attr;
                    var handle = Addressables.InstantiateAsync(uiPath.Path);
                    handle.Completed += (obj) =>
                    {
                        var inst = obj.Result;
                        GameObject parent;
                        if (cachedIngameUITrans.ContainsKey(TType))
                            parent = cachedIngameUITrans[TType];
                        else
                        {
                            parent = new GameObject(TType.ToString());
                            if (uiPath.World)
                                parent.transform.SetParent(WorldCanvas.transform);
                            else
                                parent.transform.SetParent(UIRootT.transform);

                            parent.transform.position = Vector3.zero;
                            parent.transform.localScale = Vector3.one * scale;
                            cachedIngameUITrans.Add(TType, parent);

                            UpdateFloatingUIDepth();
                        }
                        inst.transform.SetParent(parent.transform, false);
                        onSuccess?.Invoke(inst.GetComponent<T>());
                    };
                    break;
                }
            }
        }

        public int GetOpenPopupCount()
        {
            return openPopupList.Count;
        }


        delegate int DeleGetDepth(Type T);

        public void UpdateFloatingUIDepth()
        {
            DeleGetDepth GetDepth = (t) =>
            {
                var atts = Attribute.GetCustomAttributes(t);

                var isFloatingDepthAtt = Array.Find(atts, (x) => { return x is FloatingDepthAttribute; });

                if (isFloatingDepthAtt != null)
                {
                    return ((FloatingDepthAttribute)isFloatingDepthAtt).depth;
                }
                else
                {
                    return 0;
                }
            };

            var floatingTypeList = cachedIngameUITrans.Keys.Where(x => GetDepth(x) > 0).ToList();

            if (floatingTypeList.Count == 0)
            {
                return;
            }

            floatingTypeList.Sort((x, y) => { return GetDepth(x).CompareTo(GetDepth(y)); });

            foreach (var ft in floatingTypeList)
            {
                cachedIngameUITrans[ft].transform.SetAsLastSibling();
            }
        }

        public void FloatingUIFirstDepth<T>() where T : IFloatingUI
        {
            var TType = typeof(T);
            if (cachedIngameUITrans.ContainsKey(TType))
            {
                cachedIngameUITrans[TType].transform.SetAsFirstSibling();
            }
        }

        public void FloatingUILastDepth<T>() where T : IFloatingUI
        {
            var TType = typeof(T);
            if (cachedIngameUITrans.ContainsKey(TType))
            {
                cachedIngameUITrans[TType].transform.SetAsLastSibling();
            }
        }

        public void CloseUI<T>() where T : UIBase
        {
            var target = GetUI<T>();
            if (target)
                target.Hide();
        }

        public void ClosePopupAll()
        {
            var closeList = new List<UIBase>();
            foreach (var uibase in openPopupList)
            {
                if (uibase)
                {
                    closeList.Add(uibase);
                }
            }
            foreach (var uibase in closeList)
            {
                openPopupList.Remove(uibase);
                uibase.Hide();
            }

            SoundPlayer.Instance.SetBGMVolume();
        }

        public void ClosePopupBackBtn()
        {
            if (!backButtonEnable)
                return;

            if (openPopupList.Count < 1)
                return;

            var ui = openPopupList.LastOrDefault();
            if (ui != null)
            {
                if (!ui.DontCloseInteraction)
                    ui.Hide();
            }
        }

        public System.Action MainReturnAction;

        public bool IsMainActive()
        {
            if (openPopupList.Count == 0)
            {
                MainReturnAction?.Invoke();
                return true;
            }

            return false;
        }

        public UIBase GetOpenPopupLastUI()
        {
            if (openPopupList.Count < 1)
                return null;

            return openPopupList.LastOrDefault();
        }
        
        public bool isOpenUI<T>() where T : UIBase
        {
            foreach (var list in openedUIMap.Values)
            {
                foreach (var ui in list)
                {
                    if (ui.GetType() == typeof(T))
                        return true;
                }
            }

            return false;
        }

        public T GetUI<T>(Action<UIBase> onLoadWait = null) where T : UIBase
        {
            if (cachedUIs.ContainsKey(typeof(T)))
            {
                var returnValue = cachedUIs[typeof(T)] as T;
                if (onLoadWait != null && returnValue == null)
                {
                    LoadWaitUI = typeof(T);
                    OnLoadWait += onLoadWait;
                }
                return returnValue;
            }
            else
            {
                if (onLoadWait != null)
                {
                    LoadWaitUI = typeof(T);
                    OnLoadWait += onLoadWait;
                }
                return null;
            }

        }

        public void FrontUIbyUI<T, U>() where T : UIBase where U : UIBase
        {
            var frontUI = GetUI<T>();
            var backUI = GetUI<U>();

            if (frontUI == null || backUI == null)
                return;

            frontUI.SaveOringSortingData();
            var frontCanvas = frontUI.GetComponent<Canvas>();
            var backCanvas = backUI.GetComponent<Canvas>();
            frontCanvas.sortingLayerName = backCanvas.sortingLayerName;
            frontCanvas.sortingOrder = backCanvas.sortingOrder + 99;
        }
        public void RecoveryUIOrder<T>() where T : UIBase
        {
            var target = GetUI<T>();
            if (target != null)
            {
                target.RecoverySortingData();
            }
        }

        public void AllUIHide()
        {
            CheatHide = true;
            foreach (var ui in cachedUIs)
                ui.Value.gameObject.SetActive(false);
        }
        public void AllUIShow()
        {
            CheatHide = false;
            foreach (var ui in cachedUIs)
            {
                if (ui.Value.UIType == UIBaseType.Ingame || ui.Value.UIType == UIBaseType.Page)
                {
                    if (!ui.Value.name.Contains("Loading"))
                        ui.Value.gameObject.SetActive(true);
                }
            }
        }

        private void AddToOpenedStack(UIBase ui)
        {
            lastOpenedUI.Add(ui);
            ui.CurrencyTopShow();
        }

        private void RemoveOpenedStack(UIBase ui)
        {
            lastOpenedUI.Remove(ui);
            UIBase before;
            if (lastOpenedUI.Count > 0)
            {
                // for (int i = lastOpenedUI.Count - 1; i >= 0; i--)
                // {
                //     if(lastOpenedUI[i]!= UIBaseType.Popup)
                // }
                before = lastOpenedUI[^1];
            }
            else before = null;
            before.CurrencyTopShow();
        }

        private void AddToSortingOrder(UIBase ui)
        {
            UIBaseType type = ui.UIType;
            if (!openedUIMap.ContainsKey(type))
            {
                openedUIMap.Add(type, new());
            }
            if (openedUIMap[type].Contains(ui))
            {
                CalculateSortingOrder(type);
                return;
            }
            openedUIMap[type].Add(ui);
            CalculateSortingOrder(type);
        }

        private void RemoveFromSortingOrder(UIBase ui)
        {
            UIBaseType type = ui.UIType;
            if (!openedUIMap.ContainsKey(type)) return;
            openedUIMap[type].Remove(ui);
            CalculateSortingOrder(type);
        }

        private void CalculateSortingOrder(UIBaseType type)
        {
            if (!openedUIMap.ContainsKey(type)) return;

            int order = BaseSortingOrder[type];
            var listToUse = openedUIMap[type];

            HashSet<UIBase> toRemove = new();

            foreach (var ui in listToUse)
            {
                if (ui == null) continue;
                if (!ui.gameObject.activeSelf)
                {
                    toRemove.Add(ui);
                    continue;
                }
                if (!ui.TryGetComponent(out Canvas canvas)) continue;
                canvas.sortingOrder = order;
                order += 100;
            }

            //remove off
            foreach (var _ in toRemove)
            {
                listToUse.Remove(_);
            }
        }

        private void CalculatePopupSortingOrder()
        {
            var idx = START_POPUP_SORTING_NUMBER;
            foreach (var uibase in openPopupList)
            {
                if (uibase == null) continue;
                var baseCanvas = uibase.GetComponent<Canvas>();
                if (baseCanvas != null)
                {
                    baseCanvas.sortingOrder = idx;
                }
                idx += 100;
            }

            for (var i = openPopupList.Count - 1; i >= 0; --i)
            {
                if (!openPopupList[i].gameObject.activeSelf)
                {
                    openPopupList.RemoveAt(i);
                }
            }

            foreach (var action in screenActionList)
            {
                action.ScreenAction(openPopupList.Count == 0);
            }

        }



        private void calculateLastSortingOrder()
        {
            var idx = START_LAST_SORTING_NUMBER;
            foreach (var uibase in openLastList)
            {
                if (uibase == null) continue;
                var baseCanvas = uibase.GetComponent<Canvas>();
                if (baseCanvas != null)
                {
                    baseCanvas.sortingOrder = idx;
                }
                idx += 100;
            }

            for (var i = openLastList.Count - 1; i >= 0; --i)
            {
                if (!openLastList[i].gameObject.activeSelf)
                {
                    openLastList.RemoveAt(i);
                }
            }

            foreach (var action in screenActionList)
            {
                action.ScreenAction(openLastList.Count == 0);
            }

        }

        private void calculateHUDAction()
        {
            if (openPopupList.Count < 1)
            {
                ScreenAction(true, UIBase.HUDType.All);
                ScreenTopOn(false, UIBase.HUDType.All);
            }
        }

        public bool IsClear()
        {
            if (cachedIngameUITrans.Count > 0) return false;
            if (openPopupList.Count > 0) return false;
            if (screenActionList.Count > 0) return false;

            return true;
        }

        public void UnLoadUIAll(bool keepWorldMap = false)
        {
            foreach (var ui in cachedUIs)
            {
                if (ui.Value != null)
                {
                    if (!keepWorldMap || !ui.Value.gameObject.name.Contains("PageStageWorldMap"))
                        if (!Addressables.ReleaseInstance(ui.Value.gameObject))
                            GameObject.Destroy(ui.Value.gameObject);
                }
                //GameObject.Destroy(ui.Value.gameObject);
            }
            cachedUIs.Clear();
            foreach (var trans in cachedIngameUITrans)
            {
                foreach (Transform child in trans.Value.transform)
                {
                    if (!Addressables.ReleaseInstance(child.gameObject))
                        GameObject.Destroy(child.gameObject);
                }
                GameObject.Destroy(trans.Value);
            }
            cachedIngameUITrans.Clear();
            openPopupList.Clear();
            openLastList.Clear();
            screenActionList.Clear();

            SoundPlayer.Instance.SetBGMVolume();
        }

        public List<GameObject> GetOpendedUI()
        {
            List<GameObject> result = new List<GameObject>();
            foreach (var ui in cachedUIs)
            {
                if (ui.Value.gameObject.activeSelf)
                {
                    result.Add(ui.Value.gameObject);
                }
            }

            return result;
        }

        public void ScreenAction(bool value, UIBase.HUDType type)
        {
            foreach (var ui in screenActionList)
            {
                if (ui.HudType.Contains(type))
                    ui.ScreenAction(value);
            }
        }

        public bool IsScreenAction(UIBase.HUDType type)
        {
            foreach (var ui in screenActionList)
            {
                if (ui.HudType.Contains(type))
                    return ui.IsScreenAction;
            }

            return false;
        }

        public void ScreenTopOn(bool value, UIBase.HUDType type)
        {
            foreach (var ui in screenActionList)
            {
                if (ui.HudType.Contains(type))
                    ui.ScreenTopOn(value);
            }
        }

        public bool IsScreenTopOn(UIBase.HUDType type)
        {
            foreach (var ui in screenActionList)
            {
                if (ui.HudType.Contains(type))
                    return ui.IsScreenTopOn();
            }

            return false;
        }

        public void SetMainUI(Transform _trans)
        {
            UIRootT = _trans;
        }
        public void SetHudUI(Transform _trans)
        {
            HUDUIRootT = _trans;
        }

        public void SetWorldCanvas(Canvas _canvas)
        {
            WorldCanvas = _canvas;
        }

        public void AddLocalizeRefresh(ILocalizeRefresh refresher)
        {
            if (!RefreshComponentList.Contains(refresher))
                RefreshComponentList.Add(refresher);
        }

        public void RemoveLocalizeRefresh(ILocalizeRefresh refresher)
        {
            if (RefreshComponentList.Contains(refresher))
                RefreshComponentList.Remove(refresher);
        }

        public void SetCacheType(bool bCache)
        {
            cacheMode = bCache;
        }

        public string GetUIPath<T>() where T : UIBase
        {
            string path = string.Empty;
            var attrs = Attribute.GetCustomAttributes(typeof(T));
            foreach (var attr in attrs)
            {
                if (attr is UIPathAttribute)
                {
                    var uiPath = (UIPathAttribute)attr;
                    path = uiPath.Path;
                    break;
                }
            }

            return path;
        }
    }
}
