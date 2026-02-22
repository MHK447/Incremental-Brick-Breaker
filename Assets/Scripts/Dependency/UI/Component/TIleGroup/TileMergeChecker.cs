using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor;
using UnityEngine.Rendering; // UI 감지를 위해 추가
using DG.Tweening;
public class TileMergeChecker : MonoBehaviour
{
    private TileWeaponGroup TileWeaponGroup;

    private TileComponent TargetTileComponent;

    private TileWeaponComponent TargetTileWeaponComponent;

    private TileWeaponComponent HoverTileWeaponComponent; // 드래그 중에 겹쳐진 무기

    // 드래그 시작 시점에 장착돼 있던 타일 저장 (광고 실패 시 원위치 복구용)
    private TileComponent InitialTileComponent;

    private bool IsMergeStart = false;
    private float HoldTime = 0.08f; // 홀드해야 하는 시간
    private float CurrentHoldTime = 0f; // 현재 홀드한 시간
    private TileWeaponComponent InitialHoverWeapon; // 처음 클릭한 무기

    private bool IsCheckOn = false;

    // 일반 터치 감지를 위한 변수들
    private Vector2 TouchStartPosition; // 터치 시작 위치
    private float TouchStartTime; // 터치 시작 시간
    private const float MaxTouchMoveDistance = 10f; // 일반 터치로 인정할 최대 이동 거리

    private List<TileComponent> CheckTileComponentList = new List<TileComponent>();

    // 포지션 부드럽게 따라가기 위한 변수
    private Vector3 targetPosition; // 목표 포지션
    private Vector3 currentVelocity; // SmoothDamp를 위한 속도 변수
    private const float followDuration = 0f; // 따라가는 시간 (초)

    private PlayerBlockGroup PlayerBlockGroup;

    private TileUnlockChecker TileUnlockChecker;

    // 최초 선택 시점의 장착 상태 저장
    private bool InitialIsEquip = false;

    // 광고 보상 대기 여부
    private bool IsAdRequested = false;

    // 다른 스크립트가 드래깅 중인지 확인할 수 있는 public 프로퍼티
    public bool IsCurrentlyDragging => TargetTileWeaponComponent != null;

    public void Init()
    {
        IsCheckOn = true;

        IsMergeStart = false;
        CurrentHoldTime = 0f;

        TileWeaponGroup = GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.TileWeaponGroup;
        PlayerBlockGroup = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>()?.Stage?.PlayerBlockGroup;
        TileUnlockChecker = GetComponent<TileUnlockChecker>();

        CheckTileComponentList.Clear();

        TargetTileComponent = null;
        TargetTileWeaponComponent = null;
        HoverTileWeaponComponent = null;
        InitialHoverWeapon = null;
        InitialIsEquip = false;
        InitialTileComponent = null;
    }

    private void Update()
    {
        if (GameRoot.Instance.UISystem.GetOpenPopupCount() > 0) return;

        // TileUnlockChecker가 드래깅 중이면 이 스크립트는 작동하지 않음
        if (TileUnlockChecker != null && TileUnlockChecker.IsCurrentlyDragging)
        {
            return;
        }

        // 1. 클릭/터치 시작
        if (Input.GetMouseButtonDown(0))
        {
            CurrentHoldTime = 0f; // 홀드 타이머 초기화
            TargetTileWeaponComponent = null; // 클릭 시 초기화
            TouchStartPosition = Input.mousePosition; // 터치 시작 위치 저장
            TouchStartTime = Time.time; // 터치 시작 시간 저장
        }

        // 2. 클릭/터치 유지 중
        if (Input.GetMouseButton(0))
        {
            // 0.2초 홀드 체크
            if (CurrentHoldTime < HoldTime)
            {
                CurrentHoldTime += Time.deltaTime;

                // 0.2초를 넘으면 CheckInitialTileWeapon 호출
                if (CurrentHoldTime >= HoldTime && TargetTileWeaponComponent == null)
                {
                    CheckInitialTileWeapon();
                }
            }

            // TargetTileWeaponComponent가 설정된 후에만 TileComponent 감지
            if (TargetTileWeaponComponent != null)
            {
                CheckTileComponentAtPosition();
            }
        }

        // 3. 클릭/터치 종료
        if (Input.GetMouseButtonUp(0))
        {
            // 일반 터치 체크 (홀딩하지 않고 빠르게 터치한 경우)
            if (TargetTileWeaponComponent == null)
            {
                float touchDuration = Time.time - TouchStartTime;
                float touchMoveDistance = Vector2.Distance(TouchStartPosition, Input.mousePosition);

                // 홀딩 시간보다 짧고, 이동 거리가 적으면 일반 터치로 간주
                if (touchDuration < HoldTime && touchMoveDistance < MaxTouchMoveDistance)
                {
                    HandleQuickTouch();
                }
            }
            EndDrag();
        }

        MoveTargetToTouchPos();
    }

    private void HandleQuickTouch()
    {
        // UI EventSystem으로 감지 (UI Only)
        if (EventSystem.current != null)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Input.mousePosition; // 터치 혹은 마우스 좌표
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                // 본인 혹은 부모에서 컴포넌트 찾기
                var weapon = result.gameObject.GetComponent<TileWeaponComponent>();
                var tilecompnent = result.gameObject.GetComponent<TileComponent>();

                if (tilecompnent != null && tilecompnent.TargetTileWeaponComponent != null)
                {
                    weapon = tilecompnent.TargetTileWeaponComponent;
                }
                else if (weapon == null)
                {
                    weapon = result.gameObject.GetComponentInParent<TileWeaponComponent>();
                }

                if (weapon != null)
                {
                    // 일반 터치 이벤트 발생 (InfoBtn 클릭 이벤트 호출)
                    weapon.OnClickInfo();
                    BpLog.Log($"Quick Touch Event: {weapon.name}");
                    break;
                }
            }
        }
    }

    private void CheckInitialTileWeapon()
    {
        if (!GameRoot.Instance.UserData.Playerdata.IsWaveRestProperty.Value) return;

        // 임시 장착 중에는 무기를 움직이지 못하게 막음
        if (TileUnlockChecker != null && TileUnlockChecker.IsTempUnlockActive)
        {
            return;
        }

        // UI EventSystem으로 감지 (UI Only)
        if (EventSystem.current != null)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Input.mousePosition; // 터치 혹은 마우스 좌표
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                // 본인 혹은 부모에서 컴포넌트 찾기
                var weapon = result.gameObject.GetComponent<TileWeaponComponent>();
                var tilecompnent = result.gameObject.GetComponent<TileComponent>();


                if (tilecompnent != null && tilecompnent.TargetTileWeaponComponent != null)
                {
                    weapon = tilecompnent.TargetTileWeaponComponent;
                }
                else if (weapon == null)
                {
                    weapon = result.gameObject.GetComponentInParent<TileWeaponComponent>();
                }


                if (weapon != null)
                {
                    CheckTileComponentList.Clear();
                    TargetTileWeaponComponent = weapon;
                    // 최초 선택 시점의 장착 상태 저장
                    InitialIsEquip = TargetTileWeaponComponent.IsEquip;
                    InitialTileComponent = weapon.GetComponentInParent<TileComponent>();
                    TargetTileWeaponComponent.WeaponImg.raycastTarget = true;

                    // 홀딩 시작 표시
                    TargetTileWeaponComponent.IsHolding = true;

                    // 머지 가능한 무기들을 흔들기
                    TileWeaponGroup.CheckMergeEquipTile(TargetTileWeaponComponent);

                    // 드래그 시작 시 장착된 블록 제거
                    if (TargetTileWeaponComponent.IsEquip && TargetTileWeaponComponent.LinkedPlayerBlock != null)
                    {
                        RemovePlayerBlock(TargetTileWeaponComponent);
                    }

                    TileWeaponGroup.WeaponDragStart(TargetTileWeaponComponent);
                    TileWeaponGroup.SortQueueTileWeapon(TargetTileWeaponComponent);

                    // SortQueueTileWeapon 이후에 SetAsLastSibling 호출해야 맨 위로 올라옴
                    TargetTileWeaponComponent.transform.SetAsLastSibling();


                    TargetTileComponent = weapon.GetComponentInParent<TileComponent>();
                    BpLog.Log($"UI EventSystem Hit: {weapon.name}");
                    break;
                }
            }
        }

        if (TargetTileWeaponComponent == null)
        {
            BpLog.Log("No Target Found");
        }
    }

    private void CheckTileComponentAtPosition()
    {
        if (!GameRoot.Instance.UserData.Playerdata.IsWaveRestProperty.Value) return;

        // UI EventSystem으로 감지 (UI Only)
        if (EventSystem.current != null)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            // 터치 위치에 weapon_offset을 더한 값을 기준점으로 사용
            Vector2 checkPosition = Input.mousePosition;

            // 최초 선택 시점에 장착되지 않았던 무기인 경우 noneequip_ypos를 터치 위치에 적용
            if (!InitialIsEquip && TargetTileWeaponComponent != null)
            {
                var equipInfo = Tables.Instance.GetTable<EquipInfo>().GetData(TargetTileWeaponComponent.EquipIdx);
                if (equipInfo != null)
                {
                    // noneequip_ypos를 Y축에 적용 (스크린 좌표이므로 직접 더함)
                    checkPosition.y += equipInfo.noneequip_ypos;
                }
            }

            // if (TargetTileWeaponComponent != null)
            // {
            //     checkPosition -= (Vector2)TargetTileWeaponComponent.OffsetPos;
            // }
            pointerData.position = checkPosition;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            bool foundTarget = false;

            foreach (var result in results)
            {
                // 먼저 TileWeaponComponent 찾기 (드래그 중인 무기는 제외, 장착되지 않은 무기만)
                var weaponComponent = result.gameObject.GetComponent<TileWeaponComponent>();
                if (weaponComponent == null) weaponComponent = result.gameObject.GetComponentInParent<TileWeaponComponent>();

                if (weaponComponent != null && weaponComponent != TargetTileWeaponComponent && !weaponComponent.IsEquip)
                {
                    CheckTileReset();
                    HoverTileWeaponComponent = weaponComponent;
                    foundTarget = true;
                    BpLog.Log($"TileWeaponComponent Detected: {weaponComponent.name}");
                    break;
                }

                // TileComponent 찾기
                var tileComponent = result.gameObject.GetComponent<TileComponent>();
                if (tileComponent == null) tileComponent = result.gameObject.GetComponentInParent<TileComponent>();

                if (tileComponent != null)
                {
                    CheckTileReset();
                    TargetTileComponent = tileComponent;
                    TileColorSet();
                    foundTarget = true;
                    BpLog.Log($"TileComponent Detected: {tileComponent.name}");
                    break;
                }
            }

            if (!foundTarget)
            {
                CheckTileReset();
            }
        }
    }

    private void MoveTargetToTouchPos()
    {
        if (TargetTileWeaponComponent == null) return;

        // 타겟의 부모 RectTransform 기준 (로컬 좌표 변환용)
        RectTransform parentRect = TargetTileWeaponComponent.transform.parent as RectTransform;
        if (parentRect == null) return;

        // 캔버스의 렌더 모드 확인 (Overlay vs Camera)
        Canvas canvas = TargetTileWeaponComponent.GetComponentInParent<Canvas>();
        Camera uiCamera = null;

        // Screen Space - Camera 혹은 World Space일 경우 카메라 필요
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera != null ? canvas.worldCamera : Camera.main;
        }

        // 화면 좌표(마우스/터치)를 월드 좌표로 변환하여 UI 이동
        Vector3 globalMousePos;
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(parentRect, Input.mousePosition, uiCamera, out globalMousePos))
        {
            // 목표 포지션 업데이트
            targetPosition = globalMousePos - TargetTileWeaponComponent.OffsetPos;

            // 최초 선택 시점에 장착 중이지 않았던 무기인 경우 noneequip_ypos 적용
            if (!InitialIsEquip)
            {
                var equipInfo = Tables.Instance.GetTable<EquipInfo>().GetData(TargetTileWeaponComponent.EquipIdx);
                if (equipInfo != null)
                {
                    // noneequip_ypos를 Y축에 적용 (부모의 로컬 스케일 고려)
                    float yOffset = equipInfo.noneequip_ypos * (parentRect != null ? parentRect.lossyScale.y : 1f);
                    targetPosition += new Vector3(0, yOffset, 0);
                }
            }

            // followDuration이 0이므로 즉시 마우스 위치로 이동
            TargetTileWeaponComponent.transform.position = targetPosition;
        }
    }


    public void EndDrag()
    {
        // 무기끼리 머지 체크 (큐에 있는 무기들끼리 머지)
        if (TargetTileWeaponComponent != null && HoverTileWeaponComponent != null)
        {
            SoundPlayer.Instance.PlaySound("sfx_get_equip_weapon");
            bool canMergeWeapons = HoverTileWeaponComponent.Grade < 4
                                && HoverTileWeaponComponent.Grade == TargetTileWeaponComponent.Grade
                                && HoverTileWeaponComponent.EquipIdx == TargetTileWeaponComponent.EquipIdx;

            if (canMergeWeapons)
            {
                // IsAD가 true인 무기가 있으면 광고 띄우기
                if (TargetTileWeaponComponent.IsAD || HoverTileWeaponComponent.IsAD)
                {
                    IsAdRequested = true;
                    GameRoot.Instance.PluginSystem.ADProp.ShowRewardAD(TpMaxProp.AdRewardType.AdWeapon, (success) =>
                    {
                        IsAdRequested = false;
                        if (success)
                        {
                            if (TargetTileWeaponComponent == null || HoverTileWeaponComponent == null)
                            {
                                Clear();
                                return;
                            }
                            TargetTileWeaponComponent.AdCheck(false);
                            PerformWeaponMerge();
                        }
                        else
                        {
                            HandleAdFail();
                        }
                    });
                    return;
                }
                else
                {
                    PerformWeaponMerge();
                }
            }

            Clear();
            return;
        }

        // 기존 타일 장착 로직
        if (IsCheckOn)
        {
            TileColorSet();
            EquipTile();
        }
        else
        {
            // TargetTileComponent에 무기가 있고, 같은 등급, 같은 Idx라면 등급업
            if (TargetTileComponent != null && TargetTileWeaponComponent != null)
            {
                var existingWeapon = TargetTileComponent.TargetTileWeaponComponent;
                bool canMerge = existingWeapon != null
                             && existingWeapon.Grade < 4
                             && existingWeapon.Grade == TargetTileWeaponComponent.Grade
                             && existingWeapon.EquipIdx == TargetTileWeaponComponent.EquipIdx;


                if (canMerge)
                {
                    // IsAD가 true인 무기가 있으면 광고 띄우기
                    if (TargetTileWeaponComponent.IsAD || existingWeapon.IsAD)
                    {
                        IsAdRequested = true;
                        GameRoot.Instance.PluginSystem.ADProp.ShowRewardAD(TpMaxProp.AdRewardType.AdWeapon, (success) =>
                        {
                            IsAdRequested = false;
                        if (success)
                        {
                            if (TargetTileWeaponComponent == null || existingWeapon == null)
                            {
                                Clear();
                                return;
                            }
                            TargetTileWeaponComponent.AdCheck(false);
                            PerformTileMerge(existingWeapon);
                            Clear();
                        }
                        else
                        {
                            HandleAdFail();
                        }
                    });
                    return;
                }
                    else
                    {
                        PerformTileMerge(existingWeapon);
                    }
                }
                else
                {
                    TargetTileWeaponComponent.IsEquip = false;
                }
            }
            else
            {
                if (TargetTileWeaponComponent != null)
                {
                    TargetTileWeaponComponent.IsEquip = false;
                }
            }
        }

        if (IsAdRequested) return;

        Clear();
    }

    private void PerformWeaponMerge()
    {
        if (TargetTileWeaponComponent == null || HoverTileWeaponComponent == null) return;

        bool wasEquipped = HoverTileWeaponComponent.IsEquip;

        // 머지 성공 직후 모든 흔들림 애니메이션 중지
        TileWeaponGroup.StopAllShakeAnimations();

        // 머지 시: 호버된 무기가 장착되어 있다면 블록 제거
        if (HoverTileWeaponComponent.LinkedPlayerBlock != null)
        {
            RemovePlayerBlock(HoverTileWeaponComponent);
        }

        // 드래그한 무기를 비활성화하고, 호버된 무기의 등급을 올림
        ProjectUtility.SetActiveCheck(TargetTileWeaponComponent.gameObject, false);
        UnitMergeOn(HoverTileWeaponComponent);

        // 등급업 후 해당 무기의 애니메이션 확실히 중지 및 로테이션 초기화
        HoverTileWeaponComponent.transform.DOKill();
        HoverTileWeaponComponent.transform.localEulerAngles = Vector3.zero;

        Debug.Log($"무기 머지 성공: {HoverTileWeaponComponent.EquipIdx}, 등급: {HoverTileWeaponComponent.Grade}");

        SoundPlayer.Instance.PlaySound("effect_get_reward");
        ProjectUtility.Vibrate();

        // 머지된 무기가 장착되어 있었다면 새로운 등급의 블록 추가
        if (wasEquipped)
        {
            AddPlayerBlock(HoverTileWeaponComponent);
        }

        Clear();
    }




    public void EquipTile()
    {
        if (!IsCheckOn || CheckTileComponentList.Count == 0 || TargetTileComponent == null || TargetTileWeaponComponent == null) return;

        //타일이 같은등급, 같은 Idx인 경우 합치기 (SetTileComponent 호출 전에 체크해야 함)
        var existingWeapon = TargetTileComponent.TargetTileWeaponComponent;
        bool canMerge = existingWeapon != null
                     && existingWeapon.Grade < 4
                     && existingWeapon.Grade == TargetTileWeaponComponent.Grade
                     && existingWeapon.EquipIdx == TargetTileWeaponComponent.EquipIdx
                     && existingWeapon.EquipTargetTileComponent == TargetTileComponent;



        if (canMerge)
        {
            // IsAD가 true인 무기가 있으면 광고 띄우기
            if (TargetTileWeaponComponent.IsAD || existingWeapon.IsAD)
            {
                IsAdRequested = true;
                GameRoot.Instance.PluginSystem.ADProp.ShowRewardAD(TpMaxProp.AdRewardType.AdWeapon, (success) =>
                {
                    IsAdRequested = false;
                    if (success)
                    {
                        if (TargetTileWeaponComponent == null || TargetTileComponent == null)
                        {
                            Clear();
                            return;
                        }
                        TargetTileWeaponComponent.AdCheck(false);
                        PerformTileMerge(existingWeapon);
                        Clear();
                    }
                    else
                    {
                        HandleAdFail();
                    }
                });
                return;
            }
            else
            {
                PerformTileMerge(existingWeapon);
            }
        }
        else
        {
            // IsAD가 true인 무기가 있으면 광고 띄우기
            if (TargetTileWeaponComponent.IsAD)
            {
                IsAdRequested = true;
                GameRoot.Instance.PluginSystem.ADProp.ShowRewardAD(TpMaxProp.AdRewardType.AdWeapon, (success) =>
                {
                    IsAdRequested = false;
                    if (success)
                    {
                        if (TargetTileWeaponComponent == null || TargetTileComponent == null)
                        {
                            Clear();
                            return;
                        }
                        TargetTileWeaponComponent.AdCheck(false);
                        PerformTileEquip();
                        Clear();
                    }
                    else
                    {
                        HandleAdFail();
                    }
                });
                return;
            }
            else
            {
                PerformTileEquip();
            }
        }
    }

    private void PerformTileMerge(TileWeaponComponent existingWeapon)
    {
        if (TargetTileWeaponComponent == null || existingWeapon == null) return;

        // 머지 성공 직후 모든 흔들림 애니메이션 중지
        TileWeaponGroup.StopAllShakeAnimations();

        // 머지 시: 기존 무기의 블록 제거
        if (existingWeapon.LinkedPlayerBlock != null)
        {
            RemovePlayerBlock(existingWeapon);
        }

        ProjectUtility.SetActiveCheck(TargetTileWeaponComponent.gameObject, false);
        UnitMergeOn(existingWeapon);

        // 등급업 후 해당 무기의 애니메이션 확실히 중지 및 로테이션 초기화
        existingWeapon.transform.DOKill();
        existingWeapon.transform.localEulerAngles = Vector3.zero;

        SoundPlayer.Instance.PlaySound("effect_get_reward");
        // 머지 후: 업그레이드된 무기에 블록 추가
        AddPlayerBlock(existingWeapon);

        RefreshAllWeaponsMergeCheck();
    }

    private void PerformTileEquip()
    {
        if (TargetTileWeaponComponent == null) return;

        // 머지 가능한 무기 찾기: 같은 등급, 같은 무기 인덱스
        TileWeaponComponent mergeableWeapon = null;
        foreach (var tilecomponent in CheckTileComponentList)
        {
            if (tilecomponent.TargetTileWeaponComponent != null &&
                tilecomponent.TargetTileWeaponComponent != TargetTileWeaponComponent &&
                tilecomponent.TargetTileWeaponComponent.Grade == TargetTileWeaponComponent.Grade &&
                tilecomponent.TargetTileWeaponComponent.EquipIdx == TargetTileWeaponComponent.EquipIdx &&
                tilecomponent.TargetTileWeaponComponent.Grade < 4)
            {
                mergeableWeapon = tilecomponent.TargetTileWeaponComponent;
                break;
            }
        }

        // 머지 가능한 무기가 있으면 머지 수행
        if (mergeableWeapon != null)
        {
            bool wasEquipped = mergeableWeapon.IsEquip;

            // 머지 성공 직후 모든 흔들림 애니메이션 중지
            TileWeaponGroup.StopAllShakeAnimations();

            // 머지 시: 기존 무기의 블록 제거
            if (mergeableWeapon.LinkedPlayerBlock != null)
            {
                RemovePlayerBlock(mergeableWeapon);
            }

            // 드래그한 무기를 비활성화하고, 기존 무기의 등급을 올림
            ProjectUtility.SetActiveCheck(TargetTileWeaponComponent.gameObject, false);
            UnitMergeOn(mergeableWeapon);

            // 등급업 후 해당 무기의 애니메이션 확실히 중지 및 로테이션 초기화
            mergeableWeapon.transform.DOKill();
            mergeableWeapon.transform.localEulerAngles = Vector3.zero;

            // 머지 후: 업그레이드된 무기에 블록 추가
            if (wasEquipped)
            {
                AddPlayerBlock(mergeableWeapon);
            }

            RefreshAllWeaponsMergeCheck();
            return;
        }

        // 머지 불가능한 경우 기존 장착 로직 수행
        // 새 무기를 장착하기 전에, 겹치는 타일들에 있는 기존 무기들의 블록 제거
        HashSet<TileWeaponComponent> weaponsToRemove = new HashSet<TileWeaponComponent>();

        foreach (var tilecomponent in CheckTileComponentList)
        {
            if (tilecomponent.TargetTileWeaponComponent != null &&
                tilecomponent.TargetTileWeaponComponent != TargetTileWeaponComponent)
            {
                weaponsToRemove.Add(tilecomponent.TargetTileWeaponComponent);
            }
        }

        // 중복 제거된 무기들의 블록 제거
        foreach (var weaponToRemove in weaponsToRemove)
        {
            if (weaponToRemove.LinkedPlayerBlock != null)
            {
                RemovePlayerBlock(weaponToRemove);
            }
        }

        TargetTileWeaponComponent.EquipTile(TargetTileComponent);

        foreach (var tilecomponent in CheckTileComponentList)
        {
            tilecomponent.SetTileComponent(TargetTileWeaponComponent);
        }

        // 장착 시: 블록 추가
        AddPlayerBlock(TargetTileWeaponComponent);
    }



    public void UnitMergeOn(TileWeaponComponent tileweapon)
    {
        tileweapon.Grade++;
        tileweapon.Set(tileweapon.EquipIdx, tileweapon.Grade);

        GameRoot.Instance.EffectSystem.MultiPlay<ItemMergeEffect>(tileweapon.transform.position, (x) =>
        {
            x.SetAutoRemove(true, 2f);
        });

        // 머지 후 모든 활성화된 무기들의 MergeOnCheck 호출
        RefreshAllWeaponsMergeCheck();
    }

    private void RefreshAllWeaponsMergeCheck()
    {
        if (TileWeaponGroup == null) return;

        foreach (var weapon in TileWeaponGroup.GetTileWeaponComponentList)
        {
            if (weapon != null && weapon.gameObject.activeSelf)
            {
                weapon.MergeOnCheck();
            }
        }
    }



    public void Clear()
    {
        IsAdRequested = false;
        // 홀딩 상태 해제
        if (TargetTileWeaponComponent != null)
        {
            TargetTileWeaponComponent.IsHolding = false;
        }

        TargetTileComponent = null;
        TargetTileWeaponComponent = null;
        HoverTileWeaponComponent = null;
        InitialIsEquip = false;
        InitialTileComponent = null;

        CheckTileComponentList.Clear();

        // 속도 초기화
        currentVelocity = Vector3.zero;

        IsMergeStart = false;

        // 흔들림 애니메이션 중지
        TileWeaponGroup.StopAllShakeAnimations();
        TileWeaponGroup.SortQueueTileWeapon();
        TileWeaponGroup.DragEnd();
    }

    private void HandleAdFail()
    {
        IsAdRequested = false;
        var targetWeapon = TargetTileWeaponComponent;
        var originTile = InitialTileComponent;
        var wasEquipped = InitialIsEquip;

        // 드래그 중 색상 변경된 타일 복구
        CheckTileReset();

        if (targetWeapon != null)
        {
            targetWeapon.IsHolding = false;

            // 광고 실패 시에는 원래 위치로 돌려놓기
            if (wasEquipped && originTile != null)
            {
                targetWeapon.EquipTile(originTile);
                originTile.SetTileComponent(targetWeapon);
                AddPlayerBlock(targetWeapon);
            }
            else
            {
                targetWeapon.IsEquip = false;
                targetWeapon.EquipTargetTileComponent = null;
            }
        }

        Clear();
    }


    public void TileColorSet()
    {
        if (TargetTileComponent == null || TargetTileWeaponComponent == null) return;



        var td = Tables.Instance.GetTable<EquipInfo>().GetData(TargetTileWeaponComponent.EquipIdx);

        if (td != null)
        {

            var tilechecklist = GameRoot.Instance.TileSystem.TileTypeList[td.tilecheck_type - 1];

            IsCheckOn = true;

            foreach (var tilecheck in tilechecklist)
            {
                var tilepos = TargetTileComponent.TileOrderVec + tilecheck;

                var gettile = TileWeaponGroup.GetTileComponent(tilepos);
                if (gettile == null)
                {
                    IsCheckOn = false;
                }
                else
                {
                    // 임시 잠금 해제된 타일은 임시 장착이 활성화된 경우에만 유효
                    bool canUseTile = gettile.IsUnLock && !CheckTileComponentList.Contains(gettile);
                    if (gettile.IsTempUnlocked && (TileUnlockChecker == null || !TileUnlockChecker.IsTempUnlockActive))
                        canUseTile = false;
                    if (canUseTile)
                    {
                        CheckTileComponentList.Add(gettile);
                    }
                }
            }

            // 단 한 곳이라도 언락된 타일이 없으면 장착 불가
            if (IsCheckOn && CheckTileComponentList.Count == 0)
            {
                IsCheckOn = false;
            }

            var gettilecolor = IsCheckOn ? Config.Instance.GetImageColor("TileGreen_Color") : Config.Instance.GetImageColor("TileRed_Color");

            foreach (var tilecomponent in CheckTileComponentList)
            {
                if (tilecomponent.IsTempUnlocked) continue;
                tilecomponent.TileColorChange(gettilecolor);
            }


        }

    }


    public void CheckTileReset()
    {
        IsCheckOn = false;
        foreach (var tilecomponent in CheckTileComponentList)
        {
            if (tilecomponent.IsTempUnlocked) continue;
            tilecomponent.TileColorChange(Config.Instance.GetImageColor("TileBase_Color"));
        }

        if (TargetTileWeaponComponent != null)
        {
            TargetTileWeaponComponent.EquipTargetTileComponent = null;
            TargetTileWeaponComponent.IsEquip = false;
        }

        TargetTileComponent = null;
        HoverTileWeaponComponent = null;

        CheckTileComponentList.Clear();
    }


    private void AddPlayerBlock(TileWeaponComponent weapon)
    {
        if (weapon == null || PlayerBlockGroup == null) return;


        var td = Tables.Instance.GetTable<EquipInfo>().GetData(weapon.EquipIdx);

        if(td == null || td.block_check_type == -1) return;

        // 이미 블록이 연결되어 있다면 제거
        if (weapon.LinkedPlayerBlock != null)
        {
            RemovePlayerBlock(weapon);
        }

        // 새 블록 추가
        PlayerBlockGroup.AddBlock(weapon.EquipIdx, weapon.Grade);

        SoundPlayer.Instance.PlaySound("effect_add_block");

        // 가장 최근에 추가된 블록을 찾아서 연결
        // PlayerBlockGroup의 ActiveBlocks에서 같은 EquipIdx를 가진 블록 찾기
        foreach (var block in PlayerBlockGroup.ActiveBlocks)
        {
            if (block.BlockIdx == weapon.EquipIdx && block != PlayerBlockGroup.CastleTop)
            {
                // LinkedPlayerBlock이 null인 블록을 찾아서 연결 (새로 추가된 블록)
                bool isAlreadyLinked = false;
                foreach (var tileWeapon in TileWeaponGroup.GetComponentsInChildren<TileWeaponComponent>())
                {
                    if (tileWeapon.LinkedPlayerBlock == block)
                    {
                        isAlreadyLinked = true;
                        break;
                    }
                }

                if (!isAlreadyLinked)
                {
                    weapon.LinkedPlayerBlock = block;
                    Debug.Log($"블록 추가: {weapon.EquipIdx}");
                    
                    // 무기 장착 기록 추가 (처음 장착할 때만)
                    string recordKey = $"{weapon.EquipIdx}_{weapon.Grade}";
                    int currentCount = GameRoot.Instance.UserData.GetRecordCount(Config.RecordCountKeys.EQUIPWEAPONCOUNT, recordKey);
                    if (currentCount == 0)
                    {
                        GameRoot.Instance.UserData.AddRecordCount(Config.RecordCountKeys.EQUIPWEAPONCOUNT, 1, recordKey);
                    }
                    
                    break;
                }
            }
        }
    }

    private void RemovePlayerBlock(TileWeaponComponent weapon)
    {
        if (weapon == null || weapon.LinkedPlayerBlock == null || PlayerBlockGroup == null) return;

        PlayerBlock blockToRemove = weapon.LinkedPlayerBlock;
        weapon.LinkedPlayerBlock = null;

        // 블록 제거
        PlayerBlockGroup.OnBlockRemove(blockToRemove);

        // 블록 오브젝트 파괴
        if (blockToRemove != null && blockToRemove.gameObject != null)
        {
            UnityEngine.Object.Destroy(blockToRemove.gameObject);
        }

        weapon.WeaponImg.raycastTarget = true;

        Debug.Log($"블록 제거: {weapon.EquipIdx}");
    }

}
