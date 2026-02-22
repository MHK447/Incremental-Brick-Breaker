using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class NewRecycleScrollRect : ScrollRect
{
    public enum ScrollEase
    {
        Linear,
        EaseOut,
        EaseIn,
        EaseInOut,
    }

    public bool reversed = false;
    public int paddingCount = 0;
    [HideInInspector] public int totalCount = 0;

    private readonly Vector2[] viewportMinMax = new Vector2[2];
    private GameObject archetype;
    private RectTransform[] items;
    private Rect cachedItemRect;

    private int headIndex = 0;
    private int headPosition = 0;

    private Coroutine scrollCoroutine;
    private Action<int, GameObject> onScrollListener;
    private bool isScrollLocked = false;


    /// <param name="_prefab">리싸이클 프리펩</param>
    /// <param name="_totalCount">총 갯수</param>
    /// <param name="scrollCallback">리싸이클시 불리는 콜백</param>
    /// <param name="startPosition">init 시 오픈하는 위치 (인덱스값 기준)</param>
    public void Init(GameObject _prefab, int _totalCount, Action<int, GameObject> scrollCallback = null, float startPosition = 0)
    {
        Clear();
        archetype = _prefab;
        totalCount = _totalCount;
        onScrollListener = scrollCallback;
        isScrollLocked = false;
        velocity = Vector2.zero;

        SpawnObjects();
        CalculateAdjustedViewportMinMax();
        SetContentRect();
        JumpToPosition(startPosition);
    }

    public void LockScroll(bool isLocked)
    {
        isScrollLocked = isLocked;
        if (isScrollLocked)
        {
            velocity = Vector2.zero;
            if (scrollCoroutine != null)
            {
                StopCoroutine(scrollCoroutine);
                scrollCoroutine = null;
            }
        }
    }

    public GameObject GetItemAt(int indexPosition)
    {
        if (indexPosition < headPosition || indexPosition > headPosition + items.Length) return null;
        return items[GetModIndex(headIndex + (indexPosition - headPosition), items.Length)].gameObject;
    }
    public T GetItemAt<T>(int indexPosition) where T : MonoBehaviour
    {
        var item = GetItemAt(indexPosition);
        if (item == null) return null;
        return item.GetComponent<T>();
    }

    public List<GameObject> GetAllItems() => items.Select(x => x.gameObject).ToList();
    public List<T> GetAllItems<T>() where T : MonoBehaviour
    {
        return items.Select(x => x.gameObject.GetComponent<T>()).ToList();
    }

    [Obsolete("Init() 에서 통합해서 사용")]
    public void SetSlotObject(GameObject _prefab, int _totalCount)
    {
        Clear();
        archetype = _prefab;
        totalCount = _totalCount;

        SpawnObjects();
        CalculateAdjustedViewportMinMax();
        SetContentRect();
        UpdatePositions();
    }

    [Obsolete("Init()에서 통합해서 사용")]
    public void AddListener(Action<int, GameObject> action)
    {
        onScrollListener = action;
    }

    public void Clear()
    {
        onScrollListener = null;
        archetype = null;
        headPosition = 0;
        headIndex = 0;
        totalCount = 0;

        if (items == null) return;
        foreach (var item in items)
        {
            Destroy(item.gameObject);
        }
        items = null;
    }

    public void JumpToPosition(float indexPosition, bool clamped = true)
    {
        if (isScrollLocked) return;
        if (clamped) indexPosition = Mathf.Clamp(indexPosition, 0, totalCount - items.Length);
        velocity = Vector2.zero;
        indexPosition = Mathf.Clamp(indexPosition, 0, totalCount - 1);
        SetContentPosition(indexPosition);
        UpdatePositions();
    }

    public void ScrollToPosition(float indexPosition, float duration, ScrollEase ease = ScrollEase.EaseInOut, bool isRealtime = true, bool overrideExisting = false, bool updateAlways = false)
    {
        Func<float, float> easeFunction = ease switch
        {
            ScrollEase.Linear => EaseLinear,
            ScrollEase.EaseOut => EaseOut,
            ScrollEase.EaseIn => EaseIn,
            ScrollEase.EaseInOut => EaseInOut,
            _ => throw new Exception("no ease function assigned for scroll"),
        };
        ScrollToPosition(indexPosition, duration, easeFunction, isRealtime, overrideExisting, updateAlways);
    }

    public void ScrollToPosition(float indexPosition, float duration, Func<float, float> customEaseFunction, bool isRealtime = true, bool overrideExisting = false, bool updateAlways = false)
    {
        if (isScrollLocked) return;
        if (overrideExisting && scrollCoroutine != null)
        {
            StopCoroutine(scrollCoroutine);
            scrollCoroutine = null;
        }
        if (scrollCoroutine != null) return;
        scrollCoroutine = StartCoroutine(ScrollToPosition_Coroutine(indexPosition, duration, customEaseFunction, isRealtime, updateAlways));
    }

    private IEnumerator ScrollToPosition_Coroutine(float indexPosition, float duration, Func<float, float> easeFunction, bool isRealtime = true, bool updateAlways = false)
    {
        velocity = Vector2.zero;
        indexPosition = Mathf.Clamp(indexPosition, 0, totalCount - 1);
        float t = 0;
        float current = GetCurrentIndexPosition();
        float target = indexPosition;
        float durationInverse = 1 / duration;
        while (t <= 1)
        {
            if (isScrollLocked) yield break;
            t += (isRealtime ? Time.unscaledDeltaTime : Time.deltaTime) * durationInverse;
            float position = Mathf.LerpUnclamped(current, target, easeFunction(t));
            SetContentPosition(position);
            if (updateAlways) UpdatePositions();
            else CyclePositions();
            yield return null;
        }
        velocity = Vector2.zero;
        scrollCoroutine = null;
    }

    public void SetContentPosition(float indexPosition)
    {
        int sign = reversed ? 1 : -1;
        Vector3 position = content.anchoredPosition;
        if (horizontal)
        {
            position.x = indexPosition * cachedItemRect.width * sign;
        }
        else
        {
            position.y = indexPosition * cachedItemRect.height * sign;
        }
        if (float.IsNaN(position.x) || float.IsNaN(position.y)) return;
        content.anchoredPosition = position;
    }


    private float EaseLinear(float value) => GetEasedValue(ScrollEase.Linear, value);
    private float EaseIn(float value) => GetEasedValue(ScrollEase.EaseIn, value);
    private float EaseOut(float value) => GetEasedValue(ScrollEase.EaseOut, value);
    private float EaseInOut(float value) => GetEasedValue(ScrollEase.EaseInOut, value);
    private float GetEasedValue(ScrollEase ease, float value)
    {
        switch (ease)
        {
            case ScrollEase.Linear:
                return value;
            case ScrollEase.EaseOut:
                value = 1 - Mathf.Pow(1 - value, 3);
                return value;
            case ScrollEase.EaseIn:
                value = Mathf.Pow(value, 3);
                return value;
            case ScrollEase.EaseInOut:
                value = value < 0.5f ?
                    Mathf.Pow(value * 2, 3) * 0.5f :
                    1 - Mathf.Pow(2 - (2 * value), 3) * 0.5f;
                return value;
        }
        return value;
    }

    //force update all items
    public void CalculatePosition() => UpdatePositions();
    public void UpdatePositions()
    {
        if (isScrollLocked) return;
        int targetHeadPosition = Mathf.Clamp(Mathf.FloorToInt(GetCurrentIndexPosition()) - paddingCount, 0, totalCount - items.Length + 1);
        for (int i = 0; i < items.Length; i++)
        {
            var rect = items[i];
            rect.SetAsLastSibling();
            SetItemPosition(i, targetHeadPosition + i);
        }
        headPosition = targetHeadPosition;
    }

    //only update items out of viewport
    public void CyclePositions()
    {
        if (isScrollLocked) return;
        if (items == null || items.Length == 0) return;
        bool recalculated = false;
        while (!IsItemInViewport(headIndex))
        {
            if (headPosition >= totalCount - items.Length) break;
            SetAsTail(headIndex);
            recalculated = true;
        }
        if (recalculated) return;
        while (!IsItemInViewport(GetModIndex(headIndex - 1, items.Length)))
        {
            if (headPosition <= 0) break;
            SetAsHead(GetModIndex(headIndex - 1, items.Length));
        }
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();
        if (isScrollLocked)
        {
            velocity = Vector2.zero;
            return;
        }
        if (totalCount == 0) return;
        float velocityToUse = horizontal ? velocity.x : velocity.y;
        if (Mathf.Abs(velocityToUse) > 0.001f) CyclePositions();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (scrollCoroutine != null)
        {
            StopCoroutine(scrollCoroutine);
            scrollCoroutine = null;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (scrollCoroutine != null) StopCoroutine(scrollCoroutine);
    }

    private void SetContentRect()
    {
        Vector2 size = content.sizeDelta;
        float anchor = reversed ? 1 : 0;
        if (horizontal)
        {
            size.x = cachedItemRect.width * totalCount;
            content.anchorMin = new(anchor, content.anchorMin.y);
            content.anchorMax = new(anchor, content.anchorMax.y);
            content.pivot = new(anchor, content.pivot.y);
        }
        else
        {
            size.y = cachedItemRect.height * totalCount;
            content.anchorMin = new(content.anchorMin.x, anchor);
            content.anchorMax = new(content.anchorMax.x, anchor);
            content.pivot = new(content.pivot.x, anchor);
        }

        content.sizeDelta = size;
    }

    public void CalculateAdjustedViewportMinMax()
    {
        Vector2 size = viewport.rect.size;

        size.x = cachedItemRect.width * (items.Length - 1f);
        size.y = cachedItemRect.height * (items.Length - 1f);

        viewportMinMax[0] = -size * 0.5f;
        viewportMinMax[1] = size * 0.5f;
    }

    private bool IsItemInViewport(int index)
    {
        RectTransform rect = items[index];
        float rectMin, rectMax;
        float length = horizontal ? cachedItemRect.width : cachedItemRect.height;
        Vector2 rectPos = content.anchoredPosition + rect.anchoredPosition;
        rectPos += (reversed ? 1 : -1) * 0.5f * viewport.rect.size;

        if (horizontal)
        {
            rectMin = rectPos.x - length * rect.pivot.x;
            rectMax = rectPos.x + length * (1 - rect.pivot.x);
            return rectMax > viewportMinMax[0].x && rectMin < viewportMinMax[1].x;
        }
        else
        {
            rectMin = rectPos.y - length * rect.pivot.y;
            rectMax = rectPos.y + length * (1 - rect.pivot.y);
            return rectMax > viewportMinMax[0].y && rectMin < viewportMinMax[1].y;
        }
    }

    private void SetAsHead(int index)
    {
        if (index == headIndex) return;
        RectTransform item = items[index];
        headIndex = index;
        headPosition--;
        item.SetAsFirstSibling();
        SetItemPosition(index, headPosition);
    }

    private void SetAsTail(int index)
    {
        if (GetModIndex(index + 1, items.Length) == headIndex) return;
        RectTransform item = items[index];
        headIndex = GetModIndex(index + 1, items.Length);
        headPosition++;
        item.SetAsLastSibling();
        SetItemPosition(index, headPosition + items.Length - 1);
    }

    private void SetItemPosition(int itemIndex, int positionIndex)
    {
        RectTransform item = items[itemIndex];
        Vector2 originPosition = Vector2.zero;
        if (horizontal)
        {
            originPosition.x += positionIndex * cachedItemRect.width;
            if (reversed) originPosition.x *= -1;
        }
        else
        {
            originPosition.y += positionIndex * cachedItemRect.height;
            if (reversed) originPosition.y *= -1;
        }

        onScrollListener?.Invoke(positionIndex, items[itemIndex].gameObject);
        item.anchoredPosition = originPosition;
    }

    private void SpawnObjects()
    {
        RectTransform firstItem = GetItemInstance();
        firstItem.gameObject.name += "_0";
        cachedItemRect = firstItem.rect;
        //get count
        float itemLength;
        float viewportLength;
        if (horizontal)
        {
            viewportLength = viewport.rect.width;
            itemLength = cachedItemRect.width;
        }
        else
        {
            viewportLength = viewport.rect.height;
            itemLength = cachedItemRect.height;
        }
        int itemCount = Mathf.CeilToInt(viewportLength / itemLength) + paddingCount * 2 + 1;

        //spawn objects
        items = new RectTransform[itemCount];
        items[0] = firstItem;
        for (int i = 1; i < itemCount; i++)
        {
            RectTransform instance = GetItemInstance();
            instance.gameObject.name += $"_{i}";
            items[i] = instance;
        }
        headIndex = 0;
        headPosition = 0;
    }

    private RectTransform GetItemInstance()
    {
        RectTransform instance = Instantiate(archetype, content).transform as RectTransform;
        float anchor = reversed ? 1 : 0;
        if (horizontal)
        {
            instance.anchorMin = new(anchor, instance.anchorMin.y);
            instance.anchorMax = new(anchor, instance.anchorMax.y);
        }
        else
        {
            instance.anchorMin = new(instance.anchorMin.x, anchor);
            instance.anchorMax = new(instance.anchorMax.x, anchor);
        }
        return instance;
    }

    private int GetModIndex(int index, int count)
    {
        return (index + count * 2) % count;
    }

    public float IndexToNormalizedPosition(float indexPosition)
    {
        if (reversed) indexPosition = totalCount - indexPosition - 1;
        return (float)indexPosition / Mathf.Max(1, (float)totalCount - 1);
    }

    public float AbsoluteToIndexPosition(float absolutePosition)
    {
        if (reversed) absolutePosition *= -1;
        if (horizontal) return absolutePosition / content.rect.width * totalCount;
        else return absolutePosition / content.rect.height * totalCount;
    }
    public float GetCurrentIndexPosition()
    {
        return -AbsoluteToIndexPosition(horizontal ? content.anchoredPosition.x : content.anchoredPosition.y);
    }

    public float GetCurrentNormalizedPosition()
    {
        return IndexToNormalizedPosition(GetCurrentIndexPosition());
    }

    public float GetCurrentIndexPositionAtViewport(float normalizedViewportPosition)
    {
        return -AbsoluteToIndexPosition(horizontal ? content.anchoredPosition.x - viewport.rect.width * normalizedViewportPosition : content.anchoredPosition.y - viewport.rect.width * normalizedViewportPosition);
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(NewRecycleScrollRect))]
public class NewRecycleScrollRectEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
    }
}
#endif