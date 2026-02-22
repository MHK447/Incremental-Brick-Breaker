using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using TMPro;
using UniRx;
using DG.Tweening;
public class HudTopCurrency : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI MoneyText;

    [SerializeField]
    private GameObject MoneyRoot;

    [SerializeField]
    private TextMeshProUGUI CashText;

    [SerializeField]
    private Button CashBtn;


    private CompositeDisposable disposables = new CompositeDisposable();
    private readonly BigInteger[] CurrencyValues = new BigInteger[3];
    private Tweener[] Tweeners = new Tweener[3];


    void Awake()
    {
        SetDataHook();
    }


    void OnEnable()
    {
        disposables.Clear();

        GameRoot.Instance.UserData.Money.Subscribe(x =>
        {
            MoneyText.text = ProjectUtility.CalculateMoneyToString(x);
        }).AddTo(disposables);
    }

    void OnDestroy()
    {
        disposables.Clear();
    }

    void Update()
    {
        SetTexts();
    }


    private void SetTexts()
    {
        if (MoneyText) MoneyText.text = ProjectUtility.CalculateMoneyToString(CurrencyValues[0]);
    }
    public void SyncReward()
    {
        CurrencyValues[0] = GameRoot.Instance.UserData.Money.Value;
        CurrencyValues[1] = GameRoot.Instance.UserData.Cash.Value;
        SetTexts();
    }

    private void SetDataHook()
    {
        if (MoneyText != null)
        {
            MoneyText.text = ProjectUtility.CalculateMoneyToString(GameRoot.Instance.UserData.Money.Value);

            GameRoot.Instance.UserData.Money.Subscribe(x =>
            {
                if (!gameObject.activeInHierarchy)
                {
                    CurrencyValues[0] = x;
                    return;
                }

                if (Tweeners[0] != null)
                {
                    Tweeners[0].Kill();
                    Tweeners[0] = null;
                }

                // BigInteger는 DOTween에서 직접 지원하지 않으므로 float로 변환하여 애니메이션
                var startValue = (float)CurrencyValues[0];
                var endValue = (float)x;
                
                Tweeners[0] = DOTween.To(() => startValue,
                v => {
                    CurrencyValues[0] = new BigInteger(v);
                    startValue = v;
                },
                endValue,
                 0.5f)
      .SetEase(Ease.Linear)
      .SetUpdate(true)
      .OnComplete(() =>
      {
          CurrencyValues[0] = x; // 정확한 최종값 설정
          Tweeners[0] = null;
      });

            }).AddTo(this);
        }
    }
}
