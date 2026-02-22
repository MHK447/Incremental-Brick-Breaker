using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BanpoFri;
using TMPro;
using UniRx;
[FloatUIPath("UI/InGame/CastleHpProgress")]
[FloatingDepth((int)Config.FloatingUIDepth.HpProgress)]
public class CastleHpProgress : InGameFloatingUI
{

    [SerializeField]
    private Slider HpSlider;

    [SerializeField]
    private Slider DelayHealthBar;

    [SerializeField]
    private TextMeshProUGUI HpText;

    [SerializeField]
    private TextMeshProUGUI ShiledText;

    [SerializeField]
    private Transform ShiledRoot;

    public float updatespeed = 1f;


    private double CurHp;

    private double MaxHp;

    private int CurShield = 0;

    private Coroutine Col;

    private CompositeDisposable disposables = new CompositeDisposable();

    private void Start()
    {
        updatespeed = 1f;
    }

    private void OnEnable()
    {
        // 활성화될 때마다 초기화
        HpSlider.value = DelayHealthBar.value = 1f;

        disposables.Clear();

        GameRoot.Instance.UserData.Playerdata.CurShiledProperty.Subscribe(x =>
        {
            SetShiled(x);
        }).AddTo(disposables);
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

        curhp = curhp < 0 ? 0 : curhp;

        HpText.text = $"{curhp}/{maxhp}";
    }

    public void SetShiled(int shield)
    {
        CurShield = shield;
        ProjectUtility.SetActiveCheck(ShiledRoot.gameObject, shield > 0);

        ShiledText.text = shield.ToString();
    }

    private void OnDisable()
    {
        disposables.Clear();

        if (Col != null)
            GameRoot.Instance.StopCoroutine(Col);
    }

    void OnDestroy()
    {
        disposables.Clear();
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
