using UnityEngine;
using BanpoFri;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using DG.Tweening;
public class UnitBase : MonoBehaviour
{
    public enum StateType
    {
        Idle,
        Move,
        Attack,
        Dead,
        KnockBack,
        Sturn,
        Taunt,
    }


    [SerializeField]
    protected Animator Anim;

    [SerializeField]
    protected Transform ShadowTr;

    [SerializeField]
    protected List<SpriteRenderer> UnitSpriteList;

    [SerializeField]
    protected ColliderAction UnitColliderAction;


    protected InGameBaseStage BaseStage;

    protected InGameHpProgress InGameHpProgress;

    protected UnitInfoData InfoData = new UnitInfoData();

    public StateType CurState = StateType.Idle;

    public StateType GetState { get { return CurState; } }


    protected Vector3 InitShadowPos;

    private Coroutine fadeOutCoroutine;


    public bool IsDead { get { return CurState == StateType.Dead; } }

    protected string StartAnimName { get; set; } = "";



    //쿨다운과 애니매이션 속도 일치하도록 배속 맞춤 (Attacking 애니메이션만 적용)
    protected void SyncAnimationSpeed()
    {
        // Avoid accessing destroyed components or missing data during spawn/despawn transitions
        if (this == null || Anim == null) return;

        var controller = Anim.runtimeAnimatorController;
        if (controller == null || InfoData == null) return;

        var clips = controller.animationClips;
        if (clips == null || clips.Length == 0) return;

        var clip = clips.FirstOrDefault(x => x != null && x.name == "Attacking");
        if (clip == null || clip.length <= 0f) return;

        Anim.SetFloat("AttackSpeed", InfoData.AttackSpeed / clip.length);
    }

    public virtual void SetState(StateType state)
    {
        if (CurState == state) return;

        CurState = state;

        SetAnim();
    }


    public virtual void SetAnim()
    {
        if (Anim == null) return;

        switch (CurState)
        {
            case StateType.Idle:
            case StateType.KnockBack:
            case StateType.Sturn:
                if (StartAnimName == "Idle") return;
                StartAnimName = "Idle";
                Anim.Play("Idle");
                break;
            case StateType.Move:
                if (StartAnimName == "Walking") return;
                StartAnimName = "Walking";
                Anim.Play("Walking");
                break;
            case StateType.Attack:
                if (StartAnimName == "Attacking") return;
                StartAnimName = "Attacking";
                Anim.Play("Attacking", 0, UnityEngine.Random.Range(0f, 0.3f));
                break;
            case StateType.Dead:
                //Anim.Play("Dead");
                break;
            case StateType.Taunt:
                if (StartAnimName == "Taunt") return;
                StartAnimName = "Taunt";
                Anim.Play("Taunt");
                break;
        }


    }

    public virtual void SetInfo() { }

    public virtual void Attack() { }

    public virtual void Dead(bool isdirection = false)
    {
        if (IsDead) return;

        SetState(StateType.Dead);

        GameRoot.Instance.InGameUpgradeSystem.StateRefreshEvent -= SetInfo;

        if (InGameHpProgress != null)
        {
            ProjectUtility.SetActiveCheck(InGameHpProgress.gameObject, false);
        }

        // 그림자 바로 끄기
        if (ShadowTr != null)
        {
            ProjectUtility.SetActiveCheck(ShadowTr.gameObject, false);
        }

        // 이미 페이드 아웃 중이면 중복 실행 방지
        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
        }
        if (isdirection)
        {
            PowerDirection();
        }
        else
        {
            fadeOutCoroutine = StartCoroutine(FadeOutAndDelete());
        }

    }


    public void PowerDirection()
    {
        // 죽은 상태로 설정
        SetState(StateType.Dead);

        // HP바 숨기기
        HideHpProgress();

        // 그림자 끄기
        if (ShadowTr != null)
        {
            ProjectUtility.SetActiveCheck(ShadowTr.gameObject, false);
        }

        // 왼쪽으로 날아가는 목표 위치 (현재 위치에서 상대적으로 왼쪽으로 이동)
        Vector3 targetPosition = transform.position;
        targetPosition.x += -10f; // 현재 위치에서 왼쪽으로 10만큼
        targetPosition.y += Random.Range(2f, 4f); // 현재 위치에서 위로 2~4만큼

        // 랜덤 회전 속도 (빠르게 회전: 5~8바퀴)
        float randomRotation = Random.Range(1800f, 2880f);

        // 랜덤 날아가는 시간 (0.5~0.8초)
        float randomDuration = Random.Range(1f, 1.5f);

        // 애니메이션 시퀀스 생성
        Sequence knockbackSequence = DOTween.Sequence();

        // z축 회전하면서 왼쪽으로 날아가기 (x, y축은 고정)
        Vector3 currentRotation = transform.eulerAngles;
        knockbackSequence.Append(transform.DOMove(targetPosition, randomDuration).SetEase(Ease.OutQuad));
        knockbackSequence.Join(transform.DORotate(new Vector3(currentRotation.x, currentRotation.y, randomRotation), randomDuration, RotateMode.FastBeyond360));

        // 애니메이션 완료 후 유닛 삭제
        knockbackSequence.OnComplete(() =>
        {
            if (this != null && gameObject != null)
            {
                DeleteUnit();
            }
        });
    }

    public virtual void SetHp(double hp)
    {
        // 유닛이 이미 파괴되었거나 비활성화된 경우 안전하게 무시
        if (this == null || gameObject == null || !isActiveAndEnabled) return;

        // transform 사용 전 로컬 참조 확보 (파괴된 오브젝트 접근 방지)
        var targetTransform = transform;
        if (targetTransform == null) return;

        if (InGameHpProgress == null)
        {
            GameRoot.Instance.UISystem.LoadFloatingUI<InGameHpProgress>(hpprogress =>
                    {
                        // 로드 완료 시점에 유닛이 사라졌으면 바로 반환
                        if (this == null || gameObject == null || !isActiveAndEnabled)
                        {
                            return;
                        }

                        // transform가 유효한지 다시 확인
                        if (targetTransform == null) return;
                        if (hpprogress == null || hpprogress.gameObject == null) return;

                        InGameHpProgress = hpprogress;
                        // 먼저 비활성화하여 잘못된 위치에서 보이지 않도록 함
                        ProjectUtility.SetActiveCheck(hpprogress.gameObject, false);
                        hpprogress.Init(targetTransform);
                        hpprogress.SetHpText(hp, InfoData.StartHp);
                    });
        }
        else
        {
            if (InGameHpProgress != null && InGameHpProgress.gameObject != null)
            {
                InGameHpProgress.SetHpText(hp, InfoData.StartHp);
                ProjectUtility.SetActiveCheck(InGameHpProgress.gameObject, false);
            }
        }

    }

    public virtual void HideHpProgress()
    {
        if (InGameHpProgress != null && InGameHpProgress.gameObject.activeSelf)
        {
            ProjectUtility.SetActiveCheck(InGameHpProgress.gameObject, false);
        }
    }

    private bool IsDamageDirect = false;

    public virtual void DamageColorEffect()
    {
        if (!IsDamageDirect)
        {
            IsDamageDirect = true;

            // 피격 효과 적용
            foreach (var sprite in UnitSpriteList)
            {
                if (sprite != null)
                {
                    sprite.EnableHitEffect();
                }
            }

            GameRoot.Instance.WaitTimeAndCallback(0.15f, () =>
            {
                if (this != null)
                {
                    // 효과 종료 후 원래 머티리얼로 복귀
                    foreach (var sprite in UnitSpriteList)
                    {
                        if (sprite != null)
                        {
                            sprite.DisableHitEffect();
                        }
                    }

                    IsDamageDirect = false;
                }
            });
        }
    }

    protected virtual IEnumerator FadeOutAndDelete()
    {
        // DOTween을 사용하여 0.3초간 스케일을 0으로 만들기
        transform.DOScale(Vector3.zero, 0.3f)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                DeleteUnit();
                fadeOutCoroutine = null;
            });

        yield return null;
    }



    public virtual void DeleteUnit()
    {
        // 모든 DOTween 애니메이션 정리 (스케일 애니메이션 포함)
        transform.DOKill();

        // 페이드 아웃 코루틴 정리
        if (fadeOutCoroutine != null)
        {

            if (InGameHpProgress != null)
            {
                ProjectUtility.SetActiveCheck(InGameHpProgress.gameObject, false);
            }

            StopCoroutine(fadeOutCoroutine);
            fadeOutCoroutine = null;
        }
    }

    // 재활용을 위한 초기화 메서드
    protected virtual void ResetUnit()
    {
        // 진행 중인 모든 DOTween 애니메이션 중지 (스케일 애니메이션 포함)
        transform.DOKill();

        // 코루틴 정리
        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
            fadeOutCoroutine = null;
        }

        // 피격 효과 플래그 리셋
        IsDamageDirect = false;

        // 스프라이트 머티리얼 효과 리셋
        foreach (var sprite in UnitSpriteList)
        {
            if (sprite != null)
            {
                sprite.DisableHitEffect();
            }
        }

        // 애니메이터 속도 리셋
        if (Anim != null)
        {
            Anim.speed = 1f;
        }

        // 상태 리셋
        CurState = StateType.Idle;

        // 애니메이션 이름 리셋 (중요: 재활용 시 애니메이션이 올바르게 설정되도록)
        StartAnimName = "";

        // HP Progress 비활성화
        if (InGameHpProgress != null && InGameHpProgress.gameObject.activeSelf)
        {
            ProjectUtility.SetActiveCheck(InGameHpProgress.gameObject, false);
        }

        // 스케일 리셋 (DOTween 정리 후 명시적으로 설정)
        transform.localScale = Vector3.one;

        // 회전 리셋 (보스 등장 시 KillEnemyWithFallAnimation에서 적용된 회전 초기화)
        transform.rotation = Quaternion.identity;

        // 그림자 다시 활성화
        if (ShadowTr != null)
        {
            ProjectUtility.SetActiveCheck(ShadowTr.gameObject, true);
        }
    }

    protected virtual void OnDestroy()
    {
        transform.DOKill();
        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
            fadeOutCoroutine = null;
        }
        if (GameRoot.Instance != null && GameRoot.Instance.InGameUpgradeSystem != null)
            GameRoot.Instance.InGameUpgradeSystem.StateRefreshEvent -= SetInfo;
    }

    void OnEnable()
    {
        foreach (var sprite in UnitSpriteList)
        {
            if (sprite != null)
            {
                sprite.DisableHitEffect();
            }
        }
    }

    public virtual void DataInit()
    {
    }

    // 스프라이트 리스트에 접근할 수 있는 메서드
    public List<SpriteRenderer> GetUnitSpriteList()
    {
        return UnitSpriteList;
    }

    // 배틀 포기 시 유닛 비활성화
    public virtual void DeactivateUnit()
    {
        // 진행 중인 모든 DOTween 애니메이션 중지 (스케일 애니메이션 포함)
        transform.DOKill();

        // 코루틴 정리
        if (fadeOutCoroutine != null)
        {
            StopCoroutine(fadeOutCoroutine);
            fadeOutCoroutine = null;
        }

        // 스케일 리셋
        transform.localScale = Vector3.one;

        // HP Progress 비활성화
        HideHpProgress();

        // 이벤트 구독 해제
        GameRoot.Instance.InGameUpgradeSystem.StateRefreshEvent -= SetInfo;

        // 게임 오브젝트 비활성화
        ProjectUtility.SetActiveCheck(gameObject, false);
    }

}

