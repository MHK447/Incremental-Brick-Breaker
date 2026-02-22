using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanpoFri;
using TMPro;
using System;


public class BossHpComponent : MonoBehaviour
{

    [SerializeField]
    private Slider HpSlider;

    [SerializeField]
    private Slider DelayHealthBar;

    [SerializeField]
    private TextMeshProUGUI BossNameText;

    [SerializeField]
    private TextMeshProUGUI BossHpText;


    public float updatespeed = 1f;


    private double CurHp;

    private double MaxHp;

    private int EnemyIdx = 0;

    private Coroutine Col;
    
    private Action OnHpAnimationComplete;

    private void Start()
    {
        updatespeed = 1f;
    }

    public void Set(int enemyidx, double curhp, double maxhp, Action onComplete = null)
    {

        var td = Tables.Instance.GetTable<EnemyInfo>().GetData(enemyidx);

        if (td != null)
        {
            BossNameText.text = Tables.Instance.GetTable<Localize>().GetString(td.name);
        }

        // 처음 등장 시 0부터 시작하도록 초기화
        HpSlider.value = 0f;
        DelayHealthBar.value = 0f;

        OnHpAnimationComplete = onComplete;
        SetHp(enemyidx, curhp, maxhp);
    }



    public void SetHp(int enemyidx, double curhp, double maxhp)
    {
        EnemyIdx = enemyidx;

        CurHp = curhp;

        MaxHp = maxhp;

        var curhpvalue = (float)curhp / (float)maxhp;

        // DelayHealthBar는 즉시 변경 (흰색 바)
        DelayHealthBar.value = (float)curhp / (float)maxhp;

        // HpSlider는 천천히 감소 (빨간색 바)
        if (Col != null)
            GameRoot.Instance.StopCoroutine(Col);

        Col = GameRoot.Instance.StartCoroutine(UpdateDelayedHealthBar(curhpvalue));


        BossHpText.text = $"{(long)curhp}/{(long)maxhp}";


    }

    public void SetHpText(double curhp, double maxhp)
    {
        BossHpText.text = $"{(long)curhp}/{(long)maxhp}";
        HpSlider.value = (float)curhp / (float)maxhp;
    }

    private void OnDisable()
    {
        if (Col != null)
            GameRoot.Instance.StopCoroutine(Col);
    }


    private IEnumerator UpdateDelayedHealthBar(double hp)
    {
        float preChangePct = HpSlider.value;
        float elapsed = 0f;

        while (elapsed < updatespeed)
        {
            elapsed += Time.deltaTime;
            HpSlider.value = Mathf.Lerp(preChangePct, (float)hp, elapsed / updatespeed);
            yield return null;
        }

        HpSlider.value = (float)hp;

        // 애니메이션 완료 시 콜백 호출 (한 번만 호출 후 해제)
        var callback = OnHpAnimationComplete;
        OnHpAnimationComplete = null;
        try
        {
            callback?.Invoke();
        }
        catch (NullReferenceException)
        {
            // 콜백 실행 시 구독 대상(보스 등)이 이미 파괴된 경우 NRE 방지
        }
    }
}

