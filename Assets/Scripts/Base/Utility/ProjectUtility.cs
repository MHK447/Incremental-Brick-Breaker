using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using BanpoFri;
using System.Linq;
using UnityEngine.AddressableAssets;


public static class ProjectUtility
{

    private static string str_seconds;
    private static string str_minute;
    private static string str_hour;
    private static string str_day;

    public static void SetActiveCheck(GameObject target, bool value)
    {
        if (target != null)
        {
            if (value && !target.activeSelf)
                target.SetActive(true);
            else if (!value && target.activeSelf)
                target.SetActive(false);
        }
    }

    public static System.Numerics.BigInteger FibonacciDynamic(int n)
    {
        if (n <= 1)
            return n;

        System.Numerics.BigInteger[] fib = new System.Numerics.BigInteger[n + 1];
        fib[0] = 0;
        fib[1] = 1;

        for (int i = 2; i <= n; i++)
        {
            fib[i] = fib[i - 1] + fib[i - 2];
        }
        return fib[n];
    }
    private static char[] NumberChar =
 {
            'k','m','b','t'
        };


    public static string CalculateMoneyToString(System.Numerics.BigInteger _Long)
    {
        var targetString = _Long.ToString();
        var targetLen = targetString.Length;
        var front = targetLen;

        string top = "";
        string top_back = "";
        char frontChar;

        if (targetLen < 6)
        {
            return targetString;
        }
        else
        {
            var remindLen = targetLen - 6;
            if (remindLen < 1)
            {
                front = 0;
            }
            else
            {
                front = remindLen / 3;
            }

            if (NumberChar.Length > front)
            {
                frontChar = NumberChar[front];
                var unit = (front + 1) * 3;
                var startBackIndex = targetString.Length - unit;
                top = targetString.Substring(0, startBackIndex);
                if (top.Length > 3)
                {
                    top = top.Insert(top.Length - 3, ",");
                }
                top_back = targetString.Substring(startBackIndex, 1);
            }
            else
            {
                frontChar = NumberChar[NumberChar.Length - 1];
                var unit = NumberChar.Length * 3;
                var startBackIndex = targetString.Length - unit;
                top = targetString.Substring(0, startBackIndex);
                if (top.Length > 3)
                {
                    top = top.Insert(top.Length - 3, ",");
                }
                top_back = targetString.Substring(startBackIndex, 1);
            }
        }
        if (top_back.Equals("0"))
            return string.Format("{0}{1}", top, frontChar);
        return string.Format("{0}.{1}{2}", top, top_back, frontChar);
    }

    public static T GetRandom<T>(this IList<T> list)
    {
        if (list.Count == 0) return default;
        return list[UnityEngine.Random.Range(0, list.Count)];
    }

    public static string CalculateMoneyToString(double _Long)
    {
        var targetString = _Long.ToString();
        var targetLen = targetString.Length - 1;
        if (targetLen == 0)
            targetLen = 1;
        var front = targetLen / 3;
        var back = targetLen % 3;
        if (front == 0)
        {
            return _Long.ToString();
        }
        var top = targetString.Substring(0, back + 1);
        var top_back = targetString.Substring(back + 1, 1);
        var top_back2 = targetString.Substring(back + 2, 1);

        var front_copy = front;
        if (front > 1378) // 26 + 26 * 26 + 26 * 26 + 26 * 26
        {
            front_copy = front_copy - 1378;
        }
        else if (front > 702) // 26 + 26 * 26
        {
            front_copy = front_copy - 702;
        }
        else if (front > 26)
        {
            front_copy = front_copy - 26;
        }

        var front_front = front_copy / 26;
        var front_second = front_copy % 26;

        char secondChar;
        if (front_second == 0)
        {
            secondChar = 'z';
            front_front = front_front - 1;
        }
        else if (front_second > 0 && front_second < 26)
            secondChar = NumberChar[front_second - 1];
        else
            secondChar = (char)0;

        char frontChar;
        if (front_front == 26)
            frontChar = 'z';
        else if (front_front >= 0 && front_front <= 26)
            frontChar = NumberChar[front_front];
        else
            frontChar = (char)0;

        string final_numTostr = string.Empty;

        if (front > 1378) // 26 + 26 * 26 + 26 * 26 + 26 * 26
            final_numTostr = $"{char.ToUpper(frontChar)}{char.ToUpper(secondChar)}";
        else if (front > 702) // 26 + 26 * 26 + 26 * 26 + 26 * 26
            final_numTostr = $"{char.ToUpper(frontChar)}{secondChar}";
        else if (front > 26)
            final_numTostr = $"{frontChar}{secondChar}";
        else
            final_numTostr = $"{secondChar}";

        if (top_back == "0" && top_back2 != "0")
            return string.Format("{0}.{1}{2}{3}", top, top_back, top_back2, final_numTostr);
        else if (top_back == "0" && top_back2 == "0")
            return string.Format("{0}{1}", top, final_numTostr);
        else if (top_back != "0" && top_back2 == "0")
            return string.Format("{0}.{1}{2}", top, top_back, final_numTostr);
        else
            return string.Format("{0}.{1}{2}{3}", top, top_back, top_back2, final_numTostr);

    }


    public static float PercentCalc(float value, float percent)
    {
        float returnvalue = 0f;

        returnvalue = value * percent;


        returnvalue = returnvalue / 100;

        return returnvalue;
    }

    public static int GetWeightedRandomGrade(List<int> weights)
    {
        int totalWeight = 0;

        // 가중치의 총합 계산
        foreach (int weight in weights)
        {
            totalWeight += weight;
        }

        // 1부터 총합까지의 랜덤 값을 생성
        int randomValue = UnityEngine.Random.Range(1, totalWeight + 1);
        int accumulatedWeight = 0;

        // 랜덤 값을 가중치에 따라 분류
        for (int i = 0; i < weights.Count; i++)
        {
            accumulatedWeight += weights[i];
            if (randomValue <= accumulatedWeight)
            {
                return i + 1;
            }
        }

        // 기본값
        return 1;
    }


    public static bool IsPercentSuccess(float probability)
    {
        return UnityEngine.Random.Range(0f, 100f) < probability;
    }


    public static double PercentGet(double basevalue, double percentvalue)
    {
        if (percentvalue == 0)
            return 0;

        return (percentvalue / basevalue) * 100;
    }


    public static double PercentCalc(double value, double percent)
    {
        double returnvalue = 0f;

        returnvalue = value * percent;


        returnvalue = returnvalue / 100;

        return returnvalue;
    }

    public static System.Numerics.BigInteger RandomBigRange(System.Numerics.BigInteger min, System.Numerics.BigInteger max)
    {
        if (min > max)
        {
            var temp = min;
            min = max;
            max = temp;
        }

        if (min == max)
            return min;

        // 범위 계산
        System.Numerics.BigInteger range = max - min;

        // 작은 범위는 최적화된 경로 사용
        if (range <= int.MaxValue)
        {
            return min + UnityEngine.Random.Range(0, (int)range);
        }

        // 필요한 바이트 수 계산 (비트를 바이트로 변환)
        int byteCount = range.ToByteArray().Length;
        byte[] randomBytes = new byte[byteCount];

        System.Numerics.BigInteger result;

        // 균등 분포를 위해 범위 내 값이 나올 때까지 반복
        // 무한 루프 방지를 위해 최대 시도 횟수 제한
        int maxAttempts = 100;
        int attempt = 0;

        do
        {
            // UnityEngine.Random을 사용하여 랜덤 바이트 생성
            for (int i = 0; i < byteCount; i++)
            {
                randomBytes[i] = (byte)UnityEngine.Random.Range(0, 256);
            }

            // 음수 방지를 위해 최상위 비트를 0으로 설정
            randomBytes[byteCount - 1] &= 0x7F;

            result = new System.Numerics.BigInteger(randomBytes);
            attempt++;

            // 최대 시도 횟수에 도달하면 모듈로 연산 사용
            if (attempt >= maxAttempts)
            {
                result = result % range;
                break;
            }

        } while (result >= range);

        return min + result;
    }

    public static Vector3 worldToUISpace(Canvas parentCanvas, Vector3 worldPos)
    {
        //Convert the world for screen point so that it can be used with ScreenPointToLocalPointInRectangle function
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        Vector2 movePos;

        //Convert the screenpoint to ui rectangle local point
        RectTransformUtility.ScreenPointToLocalPointInRectangle(parentCanvas.transform as RectTransform, screenPos, null, out movePos);
        //Convert the local point to world point
        return parentCanvas.transform.TransformPoint(movePos);
    }

    /// <summary>
    /// UI RectTransform의 위치를 target Canvas 좌표계로 변환
    /// </summary>
    public static Vector3 rectTransformToCanvasSpace(Canvas targetCanvas, RectTransform sourceRect)
    {
        Canvas sourceCanvas = sourceRect.GetComponentInParent<Canvas>();
        Camera sourceCam = (sourceCanvas != null && sourceCanvas.worldCamera != null) ? sourceCanvas.worldCamera : Camera.main;
        Vector3 screenPos = RectTransformUtility.WorldToScreenPoint(sourceCam, sourceRect.position);

        Camera targetCam = targetCanvas.worldCamera != null ? targetCanvas.worldCamera : null;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(targetCanvas.transform as RectTransform, screenPos, targetCam, out Vector2 localPos);
        return targetCanvas.transform.TransformPoint(localPos);
    }


    public static Color WithAlpha(this Color c, float a)
    {
        c.a = a;
        return c;
    }


    private static MaterialPropertyBlock propertyBlock = new();


    public static void EnableHitEffect(this SpriteRenderer sprite)
    {
        if (sprite.sprite == null) return;

        sprite.material = Config.Instance.SolidMat;

        sprite.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_Color", sprite.material.GetColor("_Color").WithAlpha(1f));
        propertyBlock.SetFloat("_SelfIllum", 1f);
        propertyBlock.SetFloat("_FlashAmount", 0.7f);
        sprite.SetPropertyBlock(propertyBlock);
    }

    /// <summary>
    /// disables hit effect
    /// </summary>
    public static void DisableHitEffect(this SpriteRenderer sprite)
    {
        if (sprite.sprite == null) return;

        sprite.material = Config.Instance.DefaultMat;

        sprite.GetPropertyBlock(propertyBlock);
        propertyBlock.SetColor("_Color", sprite.material.GetColor("_Color").WithAlpha(1f));
        sprite.SetPropertyBlock(propertyBlock);
    }

    public static bool IsSimilar(float a, float b, float tolerance = 0.1f)
    {
        return a > b - tolerance && a < b + tolerance;
    }


    public static System.Numerics.BigInteger PercentCalc(System.Numerics.BigInteger value, int percent)
    {
        System.Numerics.BigInteger returnvalue = 0;

        returnvalue = value * percent;


        returnvalue = returnvalue / 100;

        return returnvalue;
    }
    /// <summary>
    /// 처음 게임 로딩을 수행합니다.
    /// </summary>
    public static void FirstLoad()
    {
        if (GameRoot.Instance == null)
        {
            UnityEngine.Debug.LogError("GameRoot instance is null!");
            return;
        }

        // 로딩 UI 표시
        if (GameRoot.Instance.Loading != null)
        {
            GameRoot.Instance.Loading.Show(true);
        }

        // 초기 로딩 수행
        GameRoot.Instance.WaitTimeAndCallback(0.5f, () =>
        {
            // 유저 데이터 로드
            GameRoot.Instance.UserData.Load();

            // 각종 시스템 생성
            GameRoot.Instance.GameNotification.Create();
            GameRoot.Instance.ContentsOpenSystem.Create();
            GameRoot.Instance.CardSystem.Create();
            GameRoot.Instance.AttendanceSystem.Create();
            GameRoot.Instance.ItemSystem.Create();
            GameRoot.Instance.DailyResetSystem.Create();
            // BGM 재생
            GameRoot.Instance.BgmOn();

            // 로딩 UI 숨김
            GameRoot.Instance.WaitTimeAndCallback(0.5f, () =>
            {
                if (GameRoot.Instance.Loading != null)
                {
                    GameRoot.Instance.Loading.Hide(true);
                }
            });
        });
    }

    /// <summary>
    /// 게임을 재로딩합니다.
    /// </summary>
    /// <param name="changeQuality">품질 설정 변경 여부</param>
    public static void ReLoadGame(bool changeQuality = false)
    {
        GameRoot.Instance.UserData.Save(true);

        if (GameRoot.Instance.InGameSystem.CurInGame != null)
            GameRoot.Instance.InGameSystem.CurInGame.UnLoad();

        if (changeQuality)
        {
            if (!Addressables.ReleaseInstance(GameRoot.Instance.Loading.gameObject))
                GameObject.Destroy(GameRoot.Instance.Loading.gameObject);

            GameRoot.Instance.Loading = null;
        }
        else
        {
            GameRoot.Instance.Loading.Show(true);
        }

        GameRoot.Instance.EffectSystem.Clear();
        GameRoot.Instance.UISystem.UnLoadUIAll();
        GameRoot.Instance.ContentsOpenSystem.UnLoad();
        GameRoot.Instance.CurrencyTop = null; // 기존 참조는 파괴되므로 초기화

        // 재로딩 수행
        System.Action Load = () =>
        {
            GameRoot.Instance.WaitTimeAndCallback(1f, () =>
            {
                GameRoot.Instance.UserData.Load();

                GameRoot.Instance.GameNotification.Create();
                GameRoot.Instance.ContentsOpenSystem.Create();
                GameRoot.Instance.CardSystem.Create();
                GameRoot.Instance.AttendanceSystem.Create();
                GameRoot.Instance.ItemSystem.Create();
                GameRoot.Instance.DailyResetSystem.Create();

                // Reload stage instance so lobby can start battles after data download.
                GameRoot.Instance.InGameSystem.GetInGame<InGameBase>()?.EnsureStageLoaded();

                GameRoot.Instance.Loading.Hide(true);

                // 통화 HUD부터 다시 열어 참조를 재구성한 뒤 로비/HUD를 표시
            
            });
        };

        // Load 액션 실행
        Load.Invoke();
    }


    public static void RewardGoodsEffect(int rewardtype, int rewardidx, int rewardvalue, Vector3 startpos)
    {
        string rewardstr = "";

        Transform targetroot = null;

        switch (rewardtype)
        {
            case (int)Config.RewardType.Currency:
                {
                    switch (rewardidx)
                    {
                        case (int)Config.CurrencyID.Material:
                            {
                                rewardstr = "Common_Currency_Material";
                            }
                            break;
                        case (int)Config.CurrencyID.Money:
                            {
                                rewardstr = "Common_Currency_Money";
                            }
                            break;
                        case (int)Config.CurrencyID.Cash:
                            {
                                rewardstr = "Common_Currency_Cash";
                            }
                            break;
                    }

                }
                break;
        }



        GameRoot.Instance.EffectSystem.MultiPlay<RewardEffect>(startpos, x =>
                          {
                              //적에 전체체력에 퍼센트

                              var endtr = GameRoot.Instance.UISystem.GetUI<HudCurrencyTop>().GetRewardEndTr(rewardtype, rewardidx);


                              x.Set(rewardtype, rewardidx, endtr, () =>
                              {
                                  GameRoot.Instance.UserData.SetReward(rewardtype, rewardidx, rewardvalue);
                              });
                              x.SetAutoRemove(true, 4f);
                          });
    }



    public static string GetRecordCountText(Config.RecordCountKeys key, params object[] objs)
    {
        if (key == Config.RecordCountKeys.StartStage || key == Config.RecordCountKeys.StageFailedCount || key == Config.RecordCountKeys.StageReward || key == Config.RecordCountKeys.NewChapter
        || key == Config.RecordCountKeys.EQUIPWEAPONCOUNT || key == Config.RecordCountKeys.REWARDBOOKWEAPONCOUNT || key == Config.RecordCountKeys.EQUIPWEAPONCOUNT
        || key == Config.RecordCountKeys.StageLifeTime || key == Config.RecordCountKeys.TryStageClear)
            return $"{key.ToString()}_{objs[0]}";
        else
            return key.ToString();
    }


    public static void PurgeChildren(this Transform transform)
    {
        List<Transform> children = new();
        for (int i = 0; i < transform.childCount; i++)
        {
            children.Add(transform.GetChild(i));
        }

        foreach (var child in children)
        {
            UnityEngine.Object.Destroy(child.gameObject);
        }
    }


    public static Vector3 GetRandomPositionAroundTarget(Vector3 center, float radius)
    {
        // 원형 범위 내에서 랜덤 방향 벡터 계산
        Vector2 randomDirection = UnityEngine.Random.insideUnitCircle * radius;

        // 랜덤 방향 벡터를 기준 위치에 추가 (y는 동일한 높이로 유지)
        return new Vector3(center.x + randomDirection.x, center.y, 1f);
    }


    public static Sprite GetRewardItemIconImg(int rewardtype, int rewardidx, int grade = -1)
    {
        switch (rewardtype)
        {
            case (int)Config.RewardType.Currency:
                {
                    switch (rewardidx)
                    {
                        // case (int)Config.CurrencyID.StarCoin:
                        //     {
                        //         return AtlasManager.Instance.GetSprite(Atlas.Atlas_UI_Common, "Common_Icon_Goods_Star");
                        //     }
                        //     break;
                    }
                }
                break;
        }

        return null;
    }

    public static string GetRecordValueText(Config.RecordKeys key, params object[] objs)
    {
        if (key == Config.RecordKeys.ABTest)
            return $"{key.ToString()}_{objs[0]}";

        return key.ToString();
    }



    public static float GetPercentValue(float value, float percent)
    {
        float returnvalue = 0f;

        returnvalue = (value * percent) / 100f;


        return returnvalue;
    }
    public static string GetTimeStringFormattingShort(int seconds)
    {
        str_seconds = Tables.Instance.GetTable<Localize>().GetString("str_time_second");
        str_minute = Tables.Instance.GetTable<Localize>().GetString("str_time_minute");
        str_hour = Tables.Instance.GetTable<Localize>().GetString("str_time_hour");
        str_day = Tables.Instance.GetTable<Localize>().GetString("str_time_day");

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        var cnt = 0;
        var time = new TimeSpan(0, 0, seconds);
        if (time.Days > 0)
        {
            sb.Append(time.Days.ToString());
            sb.Append(str_day);
            ++cnt;
        }
        if (time.Hours > 0)
        {
            if (cnt > 0)
                sb.Append(" ");
            sb.Append(time.Hours.ToString());
            sb.Append(str_hour);

            ++cnt;
        }
        if (time.Minutes > 0 && cnt < 2)
        {
            if (cnt > 0)
                sb.Append(" ");
            sb.Append(time.Minutes.ToString());
            sb.Append(str_minute);
            ++cnt;
        }
        if (time.Seconds >= 0 && cnt < 2)
        {
            if (cnt > 0)
                sb.Append(" ");
            sb.Append(time.Seconds.ToString());
            sb.Append(str_seconds);
            ++cnt;
        }
        return sb.ToString();
    }

    public static void Vibrate()
    {
        if (GameRoot.Instance.UserData.Vib)
            BanpoFriNative.Vibrate();
    }

    public static void SetRewardAndEffect(int rewardType, int rewardIndex, System.Numerics.BigInteger amount, System.Action OnEnd = null)
    {
        switch ((Config.RewardType)(int)Config.RewardType.Currency)
        {
            case Config.RewardType.Currency:

                var currencyHud = GameRoot.Instance.UISystem.GetUI<HudCurrencyTop>();
                if (currencyHud != null && currencyHud.gameObject.activeInHierarchy)
                {
                    UnityEngine.Vector3 pos = currencyHud.transform.position;
                    switch ((Config.CurrencyID)rewardIndex)
                    {
                        case Config.CurrencyID.Money:
                            pos = currencyHud.GetMoneyRoot.position;
                            break;
                    }


                    PlayGoodsEffect(Vector3.zero, (int)Config.RewardType.Currency, rewardIndex, 0, amount, true, OnEnd, 0f, "", null, true, true, pos);
                }
                break;

        }
    }


    public static long GetUUID()
    {
        var now = DateTime.UtcNow;
        var zeroDate = DateTime.MinValue.AddHours(now.Hour).AddMinutes(now.Minute).AddSeconds(now.Second).AddMilliseconds(now.Millisecond);
        long uniqueId = zeroDate.Ticks * 10 + UnityEngine.Random.Range(0, 10);
        return uniqueId;
    }


    public static void PlayGoodsEffect(UnityEngine.Vector3 startPos, int rewardType, int rewardIdx, int rewardGrade, System.Numerics.BigInteger value, bool isCenterStart = true, System.Action OnEnd = null, float delay = 0f, string viewText = "", UIBase curui = null, bool reward = true, bool underOrder = false, UnityEngine.Vector3 endPos = default(UnityEngine.Vector3)
        , bool iscurrencytext = true)
    {
        if (value <= 0)
            return;

        if (GameRoot.Instance.InGameSystem == null)
        {
            return;
        }
        if (GameRoot.Instance.InGameSystem.CurInGame == null)
        {

            return;
        }

        var pWidth = GameRoot.Instance.InGameSystem.CurInGame.CamPixelWidth;
        var pHeight = GameRoot.Instance.InGameSystem.CurInGame.CamPixelHeight;
        var center = new UnityEngine.Vector3(pWidth / 2, pHeight / 2, 0);
        if (isCenterStart)
        {
            center = new UnityEngine.Vector3(pWidth / 2, pHeight / 2, 0);
            startPos = center;
        }
        else
        {
            center = startPos;
        }
        if (endPos == default(UnityEngine.Vector3))
            endPos = GameRoot.Instance.UISystem.GetUI<HudCurrencyTop>().GetRewardEndTr(rewardType, rewardIdx).position;



        ProjectUtility.GoodsGetEffect(
        startPos,
        center,
        endPos,
        rewardType,
        rewardIdx,
        rewardGrade,
        value,
        OnEnd,
        delay,
        viewText,
        reward,
        underOrder,
        "Show",
        iscurrencytext);
    }


    public static void GoodsGetEffect(
        UnityEngine.Vector3 worldStartPos,
        UnityEngine.Vector3 worldMiddlePos,
        UnityEngine.Vector3 worldEndPos,
        int goodsType,
        int goodsIdx,
        int goodsGrade,
        System.Numerics.BigInteger goodsCnt,
        System.Action OnEnd = null,
        float delay = 0f,
        string viewText = "",
        bool isreward = true,
        bool underOrder = false,
        string ani = "Show",
        bool iscurrencytext = true)
    {
        //bool isseasonal = (int)Config.RewardType.Item == (int)goodsType && (int)Config.ItemType.OneTimeValue == (int)goodsIdx;

        if (goodsType == (int)Config.RewardType.Currency)
        {
            string prefab = "";
            prefab = "UI/Component/RewardEffect";

            Addressables.InstantiateAsync(prefab).Completed += (obj) =>
            {
                if (obj.Result)
                {
                    var inst = obj.Result;
                    inst.transform.SetParent(GameRoot.Instance.UISystem.UIRootT, false);

                    var goodsEff = inst.GetComponent<CurrencyEffect>();
                    if (goodsEff != null)
                        goodsEff.Set(goodsIdx, (double)goodsCnt, worldStartPos, worldEndPos, OnEnd, delay, iscurrencytext, goodsType);


                    if (underOrder)
                    {
                        var canvas = inst.GetComponent<Canvas>();
                        if (canvas != null)
                            canvas.sortingOrder = 30000;
                    }
                }
            };
        }
        else
        {
            var prefab = "UI/Component/GoodsEffect";
            Addressables.InstantiateAsync(prefab).Completed += (obj) =>
            {
                if (obj.Result)
                {
                    var inst = obj.Result;
                    inst.transform.SetParent(GameRoot.Instance.UISystem.UIRootT, false);

                    var goodsEff = inst.GetComponent<GoodEffect>();
                    if (goodsEff != null)
                        goodsEff.Set(worldStartPos, worldMiddlePos, worldEndPos, goodsType, goodsIdx, goodsGrade, (System.Numerics.BigInteger)goodsCnt, OnEnd, delay, viewText, isreward, ani);

                    if (underOrder)
                    {
                        var canvas = inst.GetComponent<Canvas>();
                        if (canvas != null)
                            canvas.sortingOrder = 30000;
                    }
                }
            };
        }

    }

}

public static class ScrollViewFocusFunctions
{
    public static Vector2 CalculateFocusedScrollPosition(this ScrollRect scrollView, Vector2 focusPoint)
    {
        Vector2 contentSize = scrollView.content.rect.size;
        Vector2 viewportSize = ((RectTransform)scrollView.content.parent).rect.size;
        Vector2 contentScale = scrollView.content.localScale;

        contentSize.Scale(contentScale);
        focusPoint.Scale(contentScale);

        Vector2 scrollPosition = scrollView.normalizedPosition;
        if (scrollView.horizontal && contentSize.x > viewportSize.x)
            scrollPosition.x = Mathf.Clamp01((focusPoint.x - viewportSize.x * 0.5f) / (contentSize.x - viewportSize.x));
        if (scrollView.vertical && contentSize.y > viewportSize.y)
            scrollPosition.y = Mathf.Clamp01((focusPoint.y - viewportSize.y * 0.5f) / (contentSize.y - viewportSize.y));

        return scrollPosition;
    }

    public static Vector2 CalculateFocusedScrollPosition(this ScrollRect scrollView, RectTransform item)
    {
        Vector2 itemCenterPoint = scrollView.content.InverseTransformPoint(item.transform.TransformPoint(item.rect.center));

        Vector2 contentSizeOffset = scrollView.content.rect.size;
        contentSizeOffset.Scale(scrollView.content.pivot);

        return scrollView.CalculateFocusedScrollPosition(itemCenterPoint + contentSizeOffset);
    }

    public static void FocusAtPoint(this ScrollRect scrollView, Vector2 focusPoint)
    {
        scrollView.normalizedPosition = scrollView.CalculateFocusedScrollPosition(focusPoint);
    }

    public static void FocusOnItem(this ScrollRect scrollView, RectTransform item)
    {
        scrollView.normalizedPosition = scrollView.CalculateFocusedScrollPosition(item);
    }



    private static IEnumerator LerpToScrollPositionCoroutine(this ScrollRect scrollView, Vector2 targetNormalizedPos, float speed, System.Action endaction = null)
    {
        Vector2 initialNormalizedPos = scrollView.normalizedPosition;

        float t = 0f;
        while (t < 1f)
        {
            scrollView.normalizedPosition = Vector2.LerpUnclamped(initialNormalizedPos, targetNormalizedPos, 1f - (1f - t) * (1f - t));

            yield return null;
            t += speed * Time.unscaledDeltaTime;
        }

        scrollView.normalizedPosition = targetNormalizedPos;

        endaction?.Invoke();
    }

    //exponet 1 균등 , 2 뽑기좀 어려움 , +3  많이 어려움 
    public static int GetBiasedWeightRandomInt(int min, int max, float exponent = 2f)
    {
        float t = Mathf.Pow(UnityEngine.Random.value, exponent);
        float result = Mathf.Lerp(min, max + 1, t);  // max 포함을 위해 +1
        return Mathf.FloorToInt(result);
    }

    public static IEnumerator FocusAtPointCoroutine(this ScrollRect scrollView, Vector2 focusPoint, float speed)
    {
        yield return scrollView.LerpToScrollPositionCoroutine(scrollView.CalculateFocusedScrollPosition(focusPoint), speed);
    }

    public static IEnumerator FocusOnItemCoroutine(this ScrollRect scrollView, RectTransform item, float speed, System.Action endaction = null)
    {
        yield return scrollView.LerpToScrollPositionCoroutine(scrollView.CalculateFocusedScrollPosition(item), speed, endaction);
    }
    public static IEnumerator FocusOnItemCoroutine(this ScrollRect scrollView, RectTransform item, float speed, Vector2 addPos)
    {
        var pos = scrollView.CalculateFocusedScrollPosition(item);
        pos += addPos;
        yield return scrollView.LerpToScrollPositionCoroutine(pos, speed);
    }

}



