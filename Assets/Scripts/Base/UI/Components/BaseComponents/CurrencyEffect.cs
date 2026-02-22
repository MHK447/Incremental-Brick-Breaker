using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.AddressableAssets;
using UniRx;
using Coffee.UIExtensions;
using TMPro;

public class CurrencyEffect : MonoBehaviour
{
    [SerializeField]
    private GameObject TargetObj;

    [SerializeField]
    private List<ParticleSystem> ParticleSystems = new List<ParticleSystem>();

    [SerializeField]
    private TextMeshProUGUI CurrencyText;


    public void Set(
        int rewardidx,
        double currencyCount,
        Vector3 worldStartPos,
        Vector3 worldEndPos,
        Action OnEnd = null,
        float delay = 0f,
        bool iscurrenytext = true,
        int rewardtype = (int)Config.RewardType.Currency)
    {

        TargetObj.transform.position = worldStartPos;

        if (TargetObj == null)
            return;


        var rewardsprite = Config.Instance.GetRewardImage(rewardtype, rewardidx);

        CurrencyText.text = ProjectUtility.CalculateMoneyToString((System.Numerics.BigInteger)currencyCount);

        // CurrencyText 초기 설정
        CurrencyText.gameObject.SetActive(true);
        CurrencyText.alpha = 1f;
        CurrencyText.transform.position = worldStartPos;

        foreach (var particle in ParticleSystems)
        {
            if (particle == null || rewardsprite == null) continue;

            // 파티클 시스템을 정지하고 스프라이트 설정
            particle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

            var textureSheetAnimation = particle.textureSheetAnimation;

            // Texture Sheet Animation 활성화 및 모드 설정
            textureSheetAnimation.enabled = true;
            textureSheetAnimation.mode = ParticleSystemAnimationMode.Sprites;

            // 새로운 텍스처(스프라이트) 설정
            textureSheetAnimation.SetSprite(0, rewardsprite); // 첫 번째 스프라이트 변경
        }

        float scale = 1f;


        foreach (var effect in ParticleSystems)
        {
            effect.transform.localScale = new Vector3(scale, scale, scale);
        }

        Transform trans = TargetObj.transform;
        var size = trans.childCount - 1;
        if (currencyCount >= size)
        {
            for (var i = 0; i < size; ++i)
            {
                var child = trans.GetChild(i);
                if (child != null)
                {
                    ProjectUtility.SetActiveCheck(child.gameObject, true);
                }
            }
        }
        else
        {
            List<int> listNumbers = new List<int>();
            int random;
            for (var i = 0; i < currencyCount; ++i)
            {
                do
                {
                    random = UnityEngine.Random.Range(0, size);
                } while (listNumbers.Contains(random));
                listNumbers.Add(random);
            }

            // if (rewardidx != (int)Config.ItemType.Energy && rewardtype != (int)Config.RewardType.Item)
            // {
            //     for (var i = 0; i < size; ++i)
            //     {
            //         var child = trans.GetChild(i);
            //         if (child != null)
            //         {
            //             TpUtility.SetActiveCheck(child.gameObject, listNumbers.Contains(i));
            //         }
            //     }
            // }
        }

        InitPos(TargetObj);


        var sequence = DOTween.Sequence();
        sequence.SetUpdate(true); // 시퀀스가 Time.timeScale의 영향을 받지 않도록 설정
        sequence.AppendInterval(delay);
        sequence.AppendCallback(() =>
        {
            ProjectUtility.SetActiveCheck(TargetObj, true);
            TargetObj.GetComponent<UIParticle>().Play();
        });

        // 기존 대로 부모를 움직임
        var mainMove = DOTween.To(() => TargetObj.transform.position, x =>
        {
            TargetObj.transform.position = x;
        }, worldEndPos, 1.6f).SetEase(Ease.InExpo).SetUpdate(true);

        // CurrencyText도 함께 날아가도록 애니메이션 추가
        var textMove = CurrencyText.transform.DOMove(worldEndPos, 1.6f).SetEase(Ease.InExpo).SetUpdate(true);

        // 텍스트가 날아가면서 크기도 조금씩 커지도록
        var textScale = CurrencyText.transform.DOScale(1.2f, 0.8f).SetEase(Ease.OutQuad).SetUpdate(true)
            .OnComplete(() =>
            {
                // 크기가 커진 후 다시 작아지면서 페이드아웃
                CurrencyText.transform.DOScale(0.8f, 0.8f).SetEase(Ease.InQuad).SetUpdate(true);
                CurrencyText.DOFade(0f, 0.8f).SetEase(Ease.InQuad).SetUpdate(true);
            });

        sequence.Append(mainMove);

        // 자식들이 뭉쳐지는 느낌을 줄이기 위해 움직임
        int moveCount = TargetObj.transform.childCount;
        float range = 50f;
        for (int i = 0; i < moveCount - 1; i++)
        {
            var cTarget = TargetObj.transform.GetChild(i);
            cTarget.DOLocalMove(Vector2.zero, 0.6f).From(
                new Vector2(UnityEngine.Random.Range(-range, range), UnityEngine.Random.Range(-range, range))
            ).SetEase(Ease.InQuart).SetDelay(1f).SetUpdate(true);
        }

        sequence.AppendCallback(() =>
        {
            SoundPlayer.Instance.PlaySound("get_coin");
            OnEnd?.Invoke();
            GameRoot.Instance.UserData.SyncHUDCurrency(rewardidx);
            GameRoot.Instance.UserData.SetReward((int)Config.RewardType.Currency, rewardidx, (System.Numerics.BigInteger)currencyCount, false);
            //SoundPlayer.Instance.PlaySound("get");
        });
        sequence.AppendInterval(3.5f);
        sequence.AppendCallback(() =>
        {
            ProjectUtility.SetActiveCheck(TargetObj, false);
            CurrencyText.gameObject.SetActive(false); // CurrencyText도 비활성화

            CompositeDisposable disposables = new CompositeDisposable();
            var startcount = GameRoot.Instance.PlayTimeSystem.CreateCountDownObservable(1f);
            startcount.Subscribe(_ => {; }, () =>
            {
                disposables.Clear();
                if (!Addressables.ReleaseInstance(this.gameObject))
                    Destroy(this.gameObject);
            }).AddTo(disposables);
        });
    }


    void InitPos(GameObject targetObj)
    {
        if (targetObj == null) { return; }

        for (int i = 0; i < targetObj.transform.childCount; i++)
        {
            targetObj.transform.GetChild(i).DOKill();
            targetObj.transform.GetChild(i).localPosition = Vector2.zero;
        }
    }
}
