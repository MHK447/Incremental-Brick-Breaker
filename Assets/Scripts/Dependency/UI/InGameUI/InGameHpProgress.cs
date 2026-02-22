using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanpoFri;

[FloatUIPath("UI/InGame/InGameHpProgress")]
[FloatingDepth((int)Config.FloatingUIDepth.HpProgress)]
public class InGameHpProgress : InGameFloatingUI
{

    [SerializeField]
    private Slider HpSlider;

    [SerializeField]
    private Slider DelayHealthBar;

    public float updatespeed = 1f;


    private double CurHp;

    private double MaxHp;

    private Coroutine Col;

    private void Start()
    {
        updatespeed = 1f;
    }

    private void OnEnable()
    {
        // 활성화될 때마다 초기화
        HpSlider.value = DelayHealthBar.value = 1f;
    }

    public void SetHpText(double curhp, double maxhp)
    {
        CurHp = curhp;

        MaxHp = maxhp;

        var curhpvalue = (float)curhp / (float)maxhp;

        // DelayHealthBar는 즉시 변경 (흰색 바)
        DelayHealthBar.value = (float)curhp / (float)maxhp;

        // HpSlider는 천천히 감소 (빨간색 바)
        if (Col != null)
            GameRoot.Instance.StopCoroutine(Col);

        Col = GameRoot.Instance.StartCoroutine(UpdateDelayedHealthBar(curhpvalue));
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
    }
}
