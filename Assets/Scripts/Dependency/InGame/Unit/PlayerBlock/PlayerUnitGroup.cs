using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using DG.Tweening;

public class PlayerUnitGroup : MonoBehaviour
{
    [HideInInspector]
    public List<PlayerUnit> ActiveUnits = new List<PlayerUnit>();

    [HideInInspector]
    public List<PlayerUnit> DeadUnits = new List<PlayerUnit>();

    [SerializeField]
    private Transform UnitRootTr;

    [SerializeField]
    private Transform UnitHeroTr;

    private PlayerHero_Base PlayerHero;

    [HideInInspector]
    public bool IsStartInterAd = false;

    private void ReleaseInstance(GameObject target)
    {
        if (target == null)
            return;

        // Addressables.InstantiateAsync로 생성된 경우 ReleaseInstance로 해제해야 번들 참조가 정리된다.
        if (!Addressables.ReleaseInstance(target))
        {
            Destroy(target);
        }
    }

    private void ReleaseUnit(PlayerUnit unit)
    {
        if (unit == null)
            return;

        unit.transform.DOKill();
        unit.HideHpProgress();
        ProjectUtility.SetActiveCheck(unit.gameObject, false);
        ReleaseInstance(unit.gameObject);
    }

    private void DeactivateUnit(PlayerUnit unit)
    {
        if (unit == null)
            return;

        // DOTween 애니메이션 정리 및 UI 숨김
        unit.transform.DOKill();
        unit.HideHpProgress();
        ProjectUtility.SetActiveCheck(unit.gameObject, false);
    }

    // 승리 연출 진행 중 여부
    [HideInInspector]
    public bool IsWinAnimationPlaying = false;

    public void Init()
    {
        IsStartInterAd = GameRoot.Instance.ShopSystem.NoInterstitialAds.Value;
            
        // 이전 스테이지에서 남아있을 수 있는 모든 유닛/히어로 정리
        ClearData();

        GameRoot.Instance.WaitTimeAndCallback(0.3f, () =>
        {
            SetPlayerUnit(GameRoot.Instance.UserData.Herogroudata.Equipplayeridx);
        });
    }

    public void AddUnit(int unit_idx, int grade, Transform spawntr, int blockOrder = 0)
    {
        // 승리 연출 중에는 유닛 추가 불가
        if (IsWinAnimationPlaying)
            return;

        var td = Tables.Instance.GetTable<UnitInfo>().GetData(unit_idx);

        if (td != null)
        {
            SoundPlayer.Instance.PlaySound("item_get");

            var find = DeadUnits.Find(x => x.PlayerUnitIdx == unit_idx && x.PlayerGrade == grade);
            if (find != null)
            {
                // 재활용 전 이전 애니메이션 정리
                find.transform.DOKill();

                ActiveUnits.Add(find);
                DeadUnits.Remove(find);

                find.transform.localScale = Vector3.one;

                // spawntr 위치에서 시작
                find.transform.position = spawntr.position;

                // Set() 호출 - 비활성 상태에서 먼저 초기화
                find.Set(unit_idx, grade, blockOrder);

                // 모든 설정이 완료된 후 활성화
                ProjectUtility.SetActiveCheck(find.gameObject, true);
            }
            else
            {
                var unit = Addressables.InstantiateAsync(td.prefab, UnitRootTr);

                var result = unit.WaitForCompletion();

                PlayerUnit instance = result.GetComponent<PlayerUnit>();

                // 생성 직후 바로 spawntr 위치로 설정
                result.transform.position = spawntr.position;

                // 초기화 전에 비활성화 (화면에 안 보이도록)
                ProjectUtility.SetActiveCheck(instance.gameObject, false);

                ActiveUnits.Add(instance);

                // Set() 호출 - 비활성 상태에서 먼저 초기화
                instance.Set(unit_idx, grade, blockOrder);

                // 모든 설정이 완료된 후 활성화 (올바른 위치에서)
                ProjectUtility.SetActiveCheck(instance.gameObject, true);
            }
        }
    }

    public void DeleteUnit(PlayerUnit unit)
    {
        if (unit == null)
            return;

        // DOTween 애니메이션 정리
        unit.transform.DOKill();

        // HpProgress 비활성화
        unit.HideHpProgress();

        ProjectUtility.SetActiveCheck(unit.gameObject, false);

        if (ActiveUnits.Remove(unit))
        {
            DeadUnits.Add(unit);
        }
    }

    public void WaveStart()
    {
        if (PlayerHero != null)
        {
            PlayerHero.SetState(PlayerHero_Base.HeroState.Idle);
        }
    }

    public void WaveEnd()
    {
        if (PlayerHero != null)
        {
            PlayerHero.SetState(PlayerHero_Base.HeroState.Taunt);
        }

        foreach (var unit in ActiveUnits)
        {
            if (unit != null)
            {
                unit.SetState(PlayerUnit.StateType.Taunt);
            }
        }
    }


    public void ClearData()
    {
        var unitsToRelease = new HashSet<PlayerUnit>();

        foreach (var unit in ActiveUnits)
            unitsToRelease.Add(unit);

        foreach (var unit in DeadUnits)
            unitsToRelease.Add(unit);

        // 리스트 참조가 끊긴 유닛까지 포함해서 모두 해제
        if (UnitRootTr != null)
        {
            var allUnits = UnitRootTr.GetComponentsInChildren<PlayerUnit>(true);
            foreach (var unit in allUnits)
            {
                unitsToRelease.Add(unit);
            }
        }

        foreach (var unit in unitsToRelease)
        {
            ReleaseUnit(unit);
        }

        // 히어로 정리
        if (PlayerHero != null)
        {
            PlayerHero.transform.DOKill();
            ProjectUtility.SetActiveCheck(PlayerHero.gameObject, false);
            ReleaseInstance(PlayerHero.gameObject);
            PlayerHero = null;
        }

        // 리스트 클리어
        ActiveUnits.Clear();
        DeadUnits.Clear();
        IsWinAnimationPlaying = false;
    }



    public void SetPlayerUnit(int unitidx)
    {
        var td = Tables.Instance.GetTable<HeroInfo>().GetData(unitidx);

        if (td != null)
        {
            if (PlayerHero != null)
            {
                PlayerHero.transform.DOKill();
                ProjectUtility.SetActiveCheck(PlayerHero.gameObject, false);
                ReleaseInstance(PlayerHero.gameObject);
                PlayerHero = null;
            }

            var unit = Addressables.InstantiateAsync(td.prefab, UnitHeroTr);
            var result = unit.WaitForCompletion();

            if (result == null)
            {
                Debug.LogError($"[PlayerUnitGroup] Failed to instantiate hero prefab: {td.prefab}");
                return;
            }

            PlayerHero = result.GetComponent<PlayerHero_Base>();
            if (PlayerHero == null)
            {
                Debug.LogError($"[PlayerUnitGroup] PlayerHero_Base component not found on prefab: {td.prefab}");
                ReleaseInstance(result);
                return;
            }

            PlayerHero.Set(unitidx);
            ProjectUtility.SetActiveCheck(PlayerHero.gameObject, true);
        }
    }
}

