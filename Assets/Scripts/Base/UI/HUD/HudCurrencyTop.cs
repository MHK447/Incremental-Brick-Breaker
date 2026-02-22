using DG.Tweening;
using UnityEngine.Serialization;
using UnityEngine;
using BanpoFri;
using UnityEngine.UI;
using UniRx;
using TMPro;


[UIPath("HudCurrencyTop")]
public class HudCurrencyTop : UIBase
{
    [SerializeField]
    private bool IsBuffValueCheck;


    [Header("Cash")]
    [SerializeField]
    private TextMeshProUGUI CashText;
    [SerializeField]
    private Transform CashIconTr;

    public Transform GetCashIconTr { get { return CashIconTr; } }

    [Header("MoneyCoin")]
    [SerializeField]
    private TextMeshProUGUI MoneyText;

    [SerializeField]
    private Transform SilverCoinIconTr;

    [Header("Material")]
    [SerializeField]
    private TextMeshProUGUI MaterialText;

    [SerializeField]
    private Transform MaterialIconTr;



    public Transform GetSilverCoinIconTr { get { return SilverCoinIconTr; } }





    [Header("Reminder")]

    [SerializeField]
    private Button OptionBtn;
    [SerializeField]
    private Button ProfileBtn;


    [SerializeField]
    private GameObject MoneyRoot;

    public Transform GetMoneyRoot { get { return MoneyRoot.transform; } }

    [FormerlySerializedAs("StoneRoot")]
    [SerializeField]
    private GameObject CashRoot;

    [SerializeField]
    private GameObject MaterialRoot;

    public Transform GetMaterialRoot { get { return MaterialRoot.transform; } }


    private readonly bool[] RootStates = new bool[] { true, true, true };
    private readonly System.Numerics.BigInteger[] CurrencyValues = new System.Numerics.BigInteger[3];
    private Tweener[] Tweeners = new Tweener[3];


    private int MaxEnergy = 0;


    protected override void Awake()
    {
        base.Awake();
        SetDataHook();

        if (OptionBtn != null)
            OptionBtn.onClick.AddListener(OnClickOption);

        //if (BuyCoinBtn != null) BuyCoinBtn.onClick.AddListener(OnClickCoin);


    }

    private void Update()
    {
        SetTexts();
    }

    private void SetTexts()
    {
        if (MoneyText) MoneyText.text = ProjectUtility.CalculateMoneyToString((System.Numerics.BigInteger)CurrencyValues[0]);
        if (CashText) CashText.text = ProjectUtility.CalculateMoneyToString((System.Numerics.BigInteger)CurrencyValues[1]);
        if (MaterialText) MaterialText.text = ProjectUtility.CalculateMoneyToString((System.Numerics.BigInteger)CurrencyValues[2]);
    }

    public void RewardCheck()
    {

    }

    public void SyncReward()
    {
        CurrencyValues[0] = (int)GameRoot.Instance.UserData.Money.Value;
        CurrencyValues[1] = (int)GameRoot.Instance.UserData.Cash.Value;
        if (MoneyText) MoneyText.text = ProjectUtility.CalculateMoneyToString((System.Numerics.BigInteger)CurrencyValues[0]);
        if (CashText) CashText.text = ProjectUtility.CalculateMoneyToString((System.Numerics.BigInteger)CurrencyValues[1]);
        if (MaterialText) MaterialText.text = ProjectUtility.CalculateMoneyToString((System.Numerics.BigInteger)CurrencyValues[2]);
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
                    CurrencyValues[0] = (int)x;
                    return;
                }

                if (Tweeners[0] != null)
                {
                    Tweeners[0].Kill();
                    Tweeners[0] = null;
                }

                var startValue = CurrencyValues[0];
                var endValue = (System.Numerics.BigInteger)x;

                Tweeners[0] = DOVirtual.Float(0f, 1f, 0.5f, t =>
                {
                    CurrencyValues[0] = (System.Numerics.BigInteger)((double)startValue + (double)(endValue - startValue) * t);
                })
      .SetEase(Ease.Linear)
      .SetUpdate(true)
      .OnComplete(() =>
      {
          CurrencyValues[0] = endValue;
          Tweeners[0] = null;
      });

            }).AddTo(this);
        }

        if (CashText != null)
        {
            CashText.text = GameRoot.Instance.UserData.Cash.Value.ToString();

            GameRoot.Instance.UserData.Cash.Subscribe(x =>
            {
                if (!gameObject.activeInHierarchy)
                {
                    CurrencyValues[1] = (int)x;
                    return;
                }

                if (Tweeners[1] != null)
                {
                    Tweeners[1].Kill();
                    Tweeners[1] = null;
                }

                var startValue = CurrencyValues[1];
                var endValue = (System.Numerics.BigInteger)x;

                Tweeners[1] = DOVirtual.Float(0f, 1f, 0.2f, t =>
                {
                    CurrencyValues[1] = (System.Numerics.BigInteger)((double)startValue + (double)(endValue - startValue) * t);
                })
      .SetEase(Ease.Linear)
      .SetUpdate(true)
      .OnComplete(() =>
      {
          CurrencyValues[1] = endValue;
          Tweeners[1] = null;
      });
            }).AddTo(this);
        }


        if (MaterialText != null)
        {
            MaterialText.text = GameRoot.Instance.UserData.Material.Value.ToString();

            GameRoot.Instance.UserData.Material.Subscribe(x =>
            {
                if (!gameObject.activeInHierarchy)
                {
                    CurrencyValues[2] = (int)x;
                    return;
                }

                if (Tweeners[2] != null)
                {
                    Tweeners[2].Kill();
                    Tweeners[2] = null;
                }

                var startValue = CurrencyValues[1];
                var endValue = (System.Numerics.BigInteger)x;

                Tweeners[2] = DOVirtual.Float(0f, 1f, 0.2f, t =>
                {
                    CurrencyValues[2] = (System.Numerics.BigInteger)((double)startValue + (double)(endValue - startValue) * t);
                })
      .SetEase(Ease.Linear)
      .SetUpdate(true)
      .OnComplete(() =>
      {
          CurrencyValues[2] = endValue;
          Tweeners[2] = null;
      });
            }).AddTo(this);
        }
    }


    private void OnClickOption()
    {
        GameRoot.Instance.UISystem.OpenUI<PopupOption>(popup => popup.Set(false));
    }

    private void OnClickCoin()
    {
        //BuyCoinBtn.interactable = false;
        // GameRoot.Instance.UISystem.OpenUI<PopupCoinInsufficent>(x =>
        // {
        //     BuyCoinBtn.interactable = true;
        // });
    }


    public void SetCurrencyState(ShowFlag currencyShow)
    {
        RootStates[0] = ShowRoot(MoneyRoot.transform, RootStates[0], currencyShow.HasFlag(ShowFlag.Coin));
        RootStates[1] = ShowRoot(CashRoot.transform, RootStates[1], currencyShow.HasFlag(ShowFlag.Cash));
        RootStates[2] = ShowRoot(MaterialRoot.transform, RootStates[2], currencyShow.HasFlag(ShowFlag.Material));

        if (OptionBtn != null)
            OptionBtn.gameObject.SetActive(currencyShow.HasFlag(ShowFlag.Setting));
    }

    private bool ShowRoot(Transform root, bool currentState, bool value)
    {
        if (currentState == value) return currentState;
        root.DOKill();
        if (value)
        {
            root.DOScale(1, 0.2f)
                .SetEase(Ease.OutBack)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    ProjectUtility.SetActiveCheck(root.gameObject, true);
                });
            return true;
        }
        else
        {
            root.DOScale(0, 0.2f)
                .SetEase(Ease.InBack)
                .SetUpdate(true)
                .OnComplete(() =>
                {
                    ProjectUtility.SetActiveCheck(root.gameObject, false);
                });
            return false;
        }
    }

    public Transform GetRewardEndTr(int rewardType, int rewardIdx)
    {
        switch ((Config.RewardType)rewardType)
        {
            case Config.RewardType.Currency:
                {
                    switch ((Config.CurrencyID)rewardIdx)
                    {
                        case Config.CurrencyID.Money:
                            return SilverCoinIconTr;
                        case Config.CurrencyID.Cash:
                            return CashIconTr;
                        case Config.CurrencyID.Material:
                            return MaterialIconTr;
                    }
                    break;
                }
        }


        return null;
    }


}
