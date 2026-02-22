using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;

public class TileUnlockChecker : MonoBehaviour
{
    private TileWeaponGroup TileWeaponGroup;

    private InGameBtnGroup InGameBtnGroup;

    private TileComponent TargetTileComponent;

    private TileAddComponent DraggingTileAddComponent;

    private bool IsDragging = false;

    private bool CanUnlock = false;

    private bool IsDraggingTempUnlockedTile = false; // 임시 잠금 해제된 타일을 드래그 중인지 여부

    private List<TileComponent> TemporarilyActivatedTiles = new List<TileComponent>();

    private List<TileComponent> CurrentAdjacentTiles = new List<TileComponent>();

    private TileMergeChecker TileMergeChecker;

    // 다른 스크립트가 드래깅 중인지 확인할 수 있는 public 프로퍼티
    public bool IsCurrentlyDragging => IsDragging;

    // 임시 잠금 해제 상태 관리
    // 여러 임시 잠금 해제 상태를 TileAddComponent별로 관리
    private readonly Dictionary<TileAddComponent, List<TileComponent>> ActiveTempUnlocks = new Dictionary<TileAddComponent, List<TileComponent>>();
    private List<TileWeaponComponent> GrayedWeapons = new List<TileWeaponComponent>(); // 회색으로 변경된 무기들

    // 임시 잠금 해제 상태 확인 프로퍼티
    public bool IsTempUnlockActive => ActiveTempUnlocks.Count > 0;

    private IEnumerable<TileComponent> GetAllTempUnlockedTiles()
    {
        return ActiveTempUnlocks.Values.SelectMany(x => x).Where(x => x != null);
    }

    private bool TryGetTempUnlockOwner(TileComponent tileComponent, out TileAddComponent owner)
    {
        foreach (var pair in ActiveTempUnlocks)
        {
            if (pair.Value.Contains(tileComponent))
            {
                owner = pair.Key;
                return true;
            }
        }

        owner = null;
        return false;
    }

    public void Init()
    {
        TileWeaponGroup = GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.TileWeaponGroup;
        InGameBtnGroup = GameRoot.Instance.UISystem.GetUI<PopupInGame>()?.GetBtnGroup;
        TileMergeChecker = GetComponent<TileMergeChecker>();

        DraggingTileAddComponent = null;
        TargetTileComponent = null;
        IsDragging = false;
        CanUnlock = false;
        IsDraggingTempUnlockedTile = false;
        TemporarilyActivatedTiles.Clear();
        CurrentAdjacentTiles.Clear();

        ActiveTempUnlocks.Clear();
        GrayedWeapons.Clear();
    }

    private void Update()
    {
        // TileMergeChecker가 드래깅 중이면 이 스크립트는 작동하지 않음
        if (TileMergeChecker != null && TileMergeChecker.IsCurrentlyDragging)
        {
            return;
        }

        // 1. 클릭/터치 시작
        if (Input.GetMouseButtonDown(0))
        {
            DraggingTileAddComponent = null;
            CheckInitialTileAdd();
        }

        // 2. 클릭/터치 유지 중 - DraggingTileAddComponent가 있을 때만 TileComponent 감지
        if (Input.GetMouseButton(0) && DraggingTileAddComponent != null)
        {
            CheckTileComponentAtPosition();
        }

        // 3. 클릭/터치 종료
        if (Input.GetMouseButtonUp(0))
        {
            EndDrag();
        }

        MoveDragToTouchPos();
    }

    private void CheckInitialTileAdd()
    {
        if (!GameRoot.Instance.UserData.Playerdata.IsWaveRestProperty.Value) return;

        // UI EventSystem으로 감지 (UI Only)
        if (EventSystem.current != null)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            pointerData.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            foreach (var result in results)
            {
                // TileComponent 먼저 체크 (임시로 열린 타일을 드래그하는 경우)
                var tileComponent = result.gameObject.GetComponent<TileComponent>();
                if (tileComponent == null) tileComponent = result.gameObject.GetComponentInParent<TileComponent>();

                if (tileComponent != null && TryGetTempUnlockOwner(tileComponent, out var tempOwner))
                {
                    // 임시로 잠금 해제된 타일을 드래그하는 경우
                    ProjectUtility.SetActiveCheck(tempOwner.gameObject, true);

                    DraggingTileAddComponent = tempOwner;
                    IsDragging = true;
                    IsDraggingTempUnlockedTile = true; // 임시 잠금 해제된 타일 드래그 중

                    // 드래그 시작 시 맨 위로 올리기
                    DraggingTileAddComponent.transform.SetAsLastSibling();

                    SoundPlayer.Instance.PlaySound("sfx_get_equip_weapon");

                    // 해당 타일의 임시 잠금 해제 상태만 되돌림
                    RevertTempUnlock(tempOwner);

                    // 임시 장착 상태 시작 - 버튼 상태 변경
                    InGameBtnGroup?.TileTmepEquipStatus(true);

                    // 장착되지 않은 무기들을 회색으로 변경
                    GrayNonEquippedWeapons();

                    // 잠긴 타일들을 임시로 활성화
                    ShowLockedTiles();

                    TileWeaponGroup.SortQueueTileWeapon();
                    break;
                }

                // TileAddComponent 찾기
                var tileAdd = result.gameObject.GetComponent<TileAddComponent>();
                if (tileAdd == null) tileAdd = result.gameObject.GetComponentInParent<TileAddComponent>();

                if (tileAdd != null)
                {
                    // 동일 TileAddComponent가 이미 임시 잠금 해제 상태라면 우선 취소
                    CancelTempUnlock(tileAdd);

                    DraggingTileAddComponent = tileAdd;
                    IsDragging = true;

                    // 드래그 시작 시 맨 위로 올리기
                    DraggingTileAddComponent.transform.SetAsLastSibling();

                    SoundPlayer.Instance.PlaySound("sfx_get_equip_weapon");

                    // 임시 장착 상태 시작 - 버튼 상태 변경
                    InGameBtnGroup?.TileTmepEquipStatus(true);

                    // 장착되지 않은 무기들을 회색으로 변경
                    GrayNonEquippedWeapons();

                    // 잠긴 타일들을 임시로 활성화
                    ShowLockedTiles();

                    TileWeaponGroup.SortQueueTileWeapon();
                    break;
                }
            }
        }

        if (DraggingTileAddComponent == null)
        {
            BpLog.Log("TileAddComponent를 찾을 수 없습니다");
        }
    }

    private void CheckTileComponentAtPosition()
    {
        if (!GameRoot.Instance.UserData.Playerdata.IsWaveRestProperty.Value) return;

        // UI EventSystem으로 감지 (UI Only)
        if (EventSystem.current != null)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            // 터치 위치에 weapon_offset을 뺀 값을 기준점으로 사용
            Vector2 checkPosition = Input.mousePosition;
            if (DraggingTileAddComponent != null)
            {
                var td = Tables.Instance.GetTable<EquipInfo>().GetData(DraggingTileAddComponent.EquipIdx);
                if (td != null)
                {
                    if (td.weapon_offset != null && td.weapon_offset.Count >= 2)
                    {
                        Vector3 offsetPos = new Vector3(td.weapon_offset[0], td.weapon_offset[1], 0f);
                        checkPosition -= (Vector2)offsetPos;
                    }
                    // noneequip_ypos를 터치 위치에 적용 (임시 장착 중인 타일은 제외)
                    if (!IsDraggingTempUnlockedTile)
                    {
                        checkPosition.y += td.noneequip_ypos + 20;
                    }
                }
            }
            pointerData.position = checkPosition;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            bool foundTarget = false;

            foreach (var result in results)
            {
                // TileComponent 찾기 (잠겨있는 타일과 열린 타일 모두)
                var tileComponent = result.gameObject.GetComponent<TileComponent>();
                if (tileComponent == null) tileComponent = result.gameObject.GetComponentInParent<TileComponent>();

                if (tileComponent != null)
                {
                    ResetTileColor();
                    TargetTileComponent = tileComponent;

                    // 모든 타일에 대해 인접 타일 체크 (열려있는 타일도 포함)
                    bool canUnlockTiles = CheckAdjacentTiles(tileComponent);

                    // 검사한 인접 타일들 가져오기
                    CurrentAdjacentTiles = GetAdjacentTiles(tileComponent);

                    if (canUnlockTiles)
                    {
                        // 초록색으로 변경 (인접 타일 중 하나라도 잠금 해제 가능)
                        // 현재 타일이 열려있으면 색상 표시 안함
                        if (!tileComponent.IsUnLock)
                        {
                            tileComponent.TileColorChange(Config.Instance.GetImageColor("TileGreen_Color"));
                        }

                        // 인접 타일들도 초록색으로 변경 (잠긴 타일만)
                        foreach (var adjacentTile in CurrentAdjacentTiles)
                        {
                            if (!adjacentTile.IsUnLock)
                            {
                                adjacentTile.TileColorChange(Config.Instance.GetImageColor("TileGreen_Color"));
                            }
                        }

                        CanUnlock = true;
                        BpLog.Log($"TileComponent 감지 (해제 가능): {tileComponent.name}");
                    }
                    else
                    {
                        // 빨강색으로 변경 (잠금 해제 불가)
                        // 현재 타일이 열려있으면 색상 표시 안함
                        if (!tileComponent.IsUnLock)
                        {
                            tileComponent.TileColorChange(Config.Instance.GetImageColor("TileRed_Color"));
                        }

                        // 인접 타일들도 빨강색으로 변경 (잠긴 타일만)
                        foreach (var adjacentTile in CurrentAdjacentTiles)
                        {
                            if (!adjacentTile.IsUnLock)
                            {
                                adjacentTile.TileColorChange(Config.Instance.GetImageColor("TileRed_Color"));
                            }
                        }

                        CanUnlock = false;
                        BpLog.Log($"TileComponent 감지 (해제 불가): {tileComponent.name}");
                    }

                    foundTarget = true;
                    break;
                }
            }

            if (!foundTarget)
            {
                ResetTileColor();
            }
        }
    }

    private bool CheckAdjacentTiles(TileComponent baseTileComponent)
    {
        if (baseTileComponent == null || TileWeaponGroup == null) return false;
        if (!TryGetUnlockCheckPositions(DraggingTileAddComponent, out var checkPositions)) return false;
        int equipIdx = DraggingTileAddComponent.EquipIdx;

        // 1칸짜리(1004)인 경우 이미 열려 있는 타일에는 장착 불가
        if (equipIdx == 1004 && baseTileComponent.IsUnLock)
        {
            return false;
        }

        // 인접 타일들을 체크 (해당 위치에 타일만 있으면 됨, 이미 열려있는 타일이 있어도 배치 가능)
        foreach (Vector2 offset in checkPositions)
        {
            Vector2 targetPosition = baseTileComponent.TileOrderVec + offset;
            TileComponent adjacentTile = TileWeaponGroup.GetTileComponent(targetPosition, false);

            // 인접 타일이 없으면 실패
            if (adjacentTile == null)
            {
                return false;
            }
        }

        return true; // 필요한 위치에 타일이 모두 있으면 배치 가능
    }

    private List<TileComponent> GetAdjacentTiles(TileComponent baseTileComponent)
    {
        List<TileComponent> adjacentTiles = new List<TileComponent>();

        if (baseTileComponent == null || TileWeaponGroup == null)
        {
            return adjacentTiles;
        }
        if (!TryGetUnlockCheckPositions(DraggingTileAddComponent, out var checkPositions)) return adjacentTiles;

        // 인접 타일들 수집 (잠긴 타일과 열린 타일 모두 포함)
        foreach (Vector2 offset in checkPositions)
        {
            Vector2 targetPosition = baseTileComponent.TileOrderVec + offset;
            TileComponent adjacentTile = TileWeaponGroup.GetTileComponent(targetPosition, false);

            if (adjacentTile != null && adjacentTile != baseTileComponent)
            {
                adjacentTiles.Add(adjacentTile);
            }
        }

        return adjacentTiles;
    }

    private void MoveDragToTouchPos()
    {
        if (DraggingTileAddComponent == null || !IsDragging) return;

        // 타겟의 부모 RectTransform 기준 (로컬 좌표 변환용)
        RectTransform parentRect = DraggingTileAddComponent.transform.parent as RectTransform;
        if (parentRect == null) return;

        // 캔버스의 렌더 모드 확인 (Overlay vs Camera)
        Canvas canvas = DraggingTileAddComponent.GetComponentInParent<Canvas>();
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
            Vector3 offsetPos = Vector3.zero;
            var td = Tables.Instance.GetTable<EquipInfo>().GetData(DraggingTileAddComponent.EquipIdx);
            if (td != null)
            {
                if (td.weapon_offset != null && td.weapon_offset.Count >= 2)
                {
                    offsetPos = new Vector3(td.weapon_offset[0], td.weapon_offset[1], 0f);
                }
                // noneequip_ypos를 Y축에 적용 (임시 장착 중인 타일은 제외)
                if (!IsDraggingTempUnlockedTile)
                {
                    float yOffset = td.noneequip_ypos * (parentRect != null ? parentRect.lossyScale.y : 1f);
                    offsetPos += new Vector3(0, -yOffset, 0);
                }
            }
            DraggingTileAddComponent.transform.position = globalMousePos - offsetPos;
        }
    }

    public void EndDrag()
    {
        if (DraggingTileAddComponent == null) return;

        // 드래그 중 참조를 캡처해 비동기 콜백에서 안전하게 사용
        var draggingTile = DraggingTileAddComponent;
        var targetTile = TargetTileComponent;

        // 드랍 시점에 한 번 더 유효성 검증 (마지막 프레임에서 감지가 안 되었을 수도 있음)
        bool canUnlockNow = CanUnlock;
        if (!canUnlockNow && targetTile != null)
        {
            canUnlockNow = CheckAdjacentTiles(targetTile);
        }

        // 타일 잠금 해제 (임시 상태로)
        if (canUnlockNow && targetTile != null)
        {
            if (draggingTile.IsAd)
            {
                GameRoot.Instance.PluginSystem.ADProp.ShowRewardAD(TpMaxProp.AdRewardType.AdTile, (success) =>
                {
                    // 콜백 시점에 드래그 대상이 사라졌으면 실패 처리
                    if (draggingTile == null || targetTile == null)
                    {
                        HandleUnlockFail();
                        TileWeaponGroup.SortQueueTileWeapon();
                        Clear();
                        return;
                    }

                    // 콜백 시점에도 유효한 위치인지 한 번 더 확인
                    bool stillCanUnlock = CheckAdjacentTiles(targetTile);

                    if (success && stillCanUnlock)
                    {
                        draggingTile.AdCheck(false);
                        CompleteTempUnlock(draggingTile, targetTile);
                    }
                    else
                    {
                        HandleUnlockFail();
                    }

                    TileWeaponGroup.SortQueueTileWeapon();
                    Clear();
                });
                return;
            }

            CompleteTempUnlock(draggingTile, targetTile);
        }
        else
        {
            HandleUnlockFail();
        }

        TileWeaponGroup.SortQueueTileWeapon();

        Clear();
    }

    private void CompleteTempUnlock(TileAddComponent tileAddComponent, TileComponent targetTileComponent)
    {
        if (tileAddComponent == null || targetTileComponent == null) return;

        // 동일 TileAddComponent의 기존 임시 잠금 해제를 정리
        CancelTempUnlock(tileAddComponent, false);

        // 인접 타일들도 모두 임시 잠금 해제
        var tempTiles = TempUnlockAdjacentTiles(targetTileComponent, tileAddComponent);

        // 인접 타일 중 잠금 해제할 타일이 하나도 없으면 장착 실패 처리, 제자리로 복구
        if (tempTiles == null || tempTiles.Count == 0)
        {
            HandleUnlockFail();
            return;
        }

        // 임시 잠금 해제 정보 저장
        ActiveTempUnlocks[tileAddComponent] = tempTiles;

        // TileAddComponent를 완전히 숨김 (비활성화)
        ProjectUtility.SetActiveCheck(tileAddComponent.gameObject, false);

        // 임시 잠금 해제가 실제로 완료된 경우에만 confirm equip 버튼 표시
        InGameBtnGroup?.TileTmepEquipStatus(true);
    }

    private void HandleUnlockFail()
    {
        // 색상/잠금 UI 복구
        ResetTileColor();
        HideLockedTiles();

        // 드래그 상태 해제
        IsDragging = false;
        IsDraggingTempUnlockedTile = false;
        TargetTileComponent = null;
        CurrentAdjacentTiles.Clear();

        // 실패 시 큐로 복귀하도록 타일 애드 활성 유지
        if (DraggingTileAddComponent != null)
        {
            ProjectUtility.SetActiveCheck(DraggingTileAddComponent.gameObject, true);
        }

        // 잠금 해제 실패 시 임시 장착 상태 종료
        if (ActiveTempUnlocks.Count == 0)
        {
            InGameBtnGroup?.TileTmepEquipStatus(false);

            // 회색으로 변경된 무기들을 흰색으로 복원
            RestoreWeaponColors();
        }
    }

    private List<TileComponent> TempUnlockAdjacentTiles(TileComponent baseTileComponent, TileAddComponent tileAddComponent)
    {
        List<TileComponent> tempTiles = new List<TileComponent>();

        if (baseTileComponent == null || TileWeaponGroup == null)
        {
            return tempTiles;
        }
        if (!TryGetUnlockCheckPositions(tileAddComponent, out var checkPositions)) return tempTiles;

        // 모든 인접 타일 임시 잠금 해제
        foreach (Vector2 offset in checkPositions)
        {
            Vector2 targetPosition = baseTileComponent.TileOrderVec + offset;
            TileComponent adjacentTile = TileWeaponGroup.TileComponentList.FirstOrDefault(x => x.TileOrderVec == targetPosition);

            if (adjacentTile != null && !adjacentTile.IsUnLock)
            {
                TempUnlockTile(adjacentTile);
                tempTiles.Add(adjacentTile);
            }
        }

        return tempTiles;
    }

    private void TempUnlockTile(TileComponent tileComponent)
    {
        tileComponent.IsUnLock = true; // 임시로 잠금 해제 상태로 변경
        tileComponent.IsTempUnlocked = true;
        ProjectUtility.SetActiveCheck(tileComponent.gameObject, true);
        ProjectUtility.SetActiveCheck(tileComponent.TileUnLockObj, false);

        // 임시 상태를 나타내기 위해 노란색 적용
        tileComponent.TileColorChange(Config.Instance.GetImageColor("temp_unlock_color"));
    }

    private void UnlockAdjacentTiles(TileComponent baseTileComponent)
    {
        if (baseTileComponent == null || TileWeaponGroup == null) return;
        if (!TryGetUnlockCheckPositions(DraggingTileAddComponent, out var checkPositions)) return;

        // 모든 인접 타일 잠금 해제
        foreach (Vector2 offset in checkPositions)
        {
            Vector2 targetPosition = baseTileComponent.TileOrderVec + offset;
            TileComponent adjacentTile = TileWeaponGroup.TileComponentList.FirstOrDefault(x => x.TileOrderVec == targetPosition);

            if (adjacentTile != null && !adjacentTile.IsUnLock)
            {
                UnlockTile(adjacentTile);
            
            }
        }
    }

    private bool TryGetUnlockCheckPositions(TileAddComponent tileAddComponent, out List<Vector2> checkPositions)
    {
        checkPositions = null;
        if (tileAddComponent == null) return false;

        var gameRoot = GameRoot.Instance;
        var tileSystem = gameRoot?.TileSystem;
        var unlockCheckList = tileSystem?.TileUnLockCheckList;
        if (unlockCheckList == null) return false;

        int listIndex = tileAddComponent.EquipIdx - 1001;
        if (listIndex < 0 || listIndex >= unlockCheckList.Count) return false;

        checkPositions = unlockCheckList[listIndex];
        return checkPositions != null;
    }

    private void UnlockTile(TileComponent tileComponent)
    {
        tileComponent.IsTempUnlocked = false;
        tileComponent.IsUnLock = true;
        ProjectUtility.SetActiveCheck(tileComponent.gameObject, true);
        // 잠금 해제된 타일은 TileUnLockObj 비활성화
        ProjectUtility.SetActiveCheck(tileComponent.TileUnLockObj, false);

        tileComponent.TileColorChange(Config.Instance.GetImageColor("TileBase_Color"));
    }

    public void Clear()
    {
        ResetTileColor();

        // 임시로 활성화했던 타일들을 다시 비활성화
        HideLockedTiles();

        DraggingTileAddComponent = null;
        TargetTileComponent = null;
        IsDragging = false;
        CanUnlock = false;
        IsDraggingTempUnlockedTile = false;
    }

    private void ResetTileColor()
    {
        var tempUnlockedSet = new HashSet<TileComponent>(GetAllTempUnlockedTiles());

        if (TargetTileComponent != null)
        {
            // 임시로 잠금 해제된 타일은 노란색 유지
            if (!tempUnlockedSet.Contains(TargetTileComponent))
            {
                // 타겟 타일을 원래 색상으로 리셋
                if (TargetTileComponent.IsUnLock)
                {
                    // 이미 열려있는 타일은 TileBase_Color로
                    TargetTileComponent.TileColorChange(Config.Instance.GetImageColor("TileBase_Color"));
                }
                else
                {
                    // 잠긴 타일은 AddTileEquip_Color로
                    TargetTileComponent.TileColorChange(Config.Instance.GetImageColor("AddTileEquip_Color"));
                }
            }
        }

        // 인접 타일들도 원래 색상으로 리셋
        foreach (var adjacentTile in CurrentAdjacentTiles)
        {
            // 임시로 잠금 해제된 타일은 노란색 유지
            if (tempUnlockedSet.Contains(adjacentTile))
            {
                continue;
            }

            if (adjacentTile.IsUnLock)
            {
                // 이미 열려있는 타일은 TileBase_Color로
                adjacentTile.TileColorChange(Config.Instance.GetImageColor("TileBase_Color"));
            }
            else
            {
                // 잠긴 타일은 AddTileEquip_Color로
                adjacentTile.TileColorChange(Config.Instance.GetImageColor("AddTileEquip_Color"));
            }
        }

        CurrentAdjacentTiles.Clear();
        TargetTileComponent = null;
        CanUnlock = false;
    }

    private void ShowLockedTiles()
    {
        TemporarilyActivatedTiles.Clear();

        foreach (var tileComponent in TileWeaponGroup.TileComponentList)
        {
            if (!tileComponent.IsUnLock)
            {
                // 잠긴 타일을 활성화하고 리스트에 추가
                ProjectUtility.SetActiveCheck(tileComponent.gameObject, true);
                ProjectUtility.SetActiveCheck(tileComponent.TileUnLockObj, false);
                tileComponent.TileColorChange(Config.Instance.GetImageColor("AddTileEquip_Color"));
                TemporarilyActivatedTiles.Add(tileComponent);
            }
            else
            {
                // 잠금 해제된 타일은 모두 TileUnLockObj 비활성화
                ProjectUtility.SetActiveCheck(tileComponent.TileUnLockObj, false);
            }
        }
    }

    private void HideLockedTiles()
    {
        foreach (var tileComponent in TemporarilyActivatedTiles)
        {
            if (!tileComponent.IsUnLock)
            {
                // 타일을 다시 비활성화
                ProjectUtility.SetActiveCheck(tileComponent.gameObject, false);
                ProjectUtility.SetActiveCheck(tileComponent.TileUnLockObj, false);
            }
        }

        TemporarilyActivatedTiles.Clear();
    }

    // 임시 잠금 해제 취소
    private void CancelTempUnlock(TileAddComponent target, bool restoreUi = true)
    {
        if (target == null) return;
        if (!ActiveTempUnlocks.TryGetValue(target, out var tiles)) return;

        foreach (var tile in tiles)
        {
            if (tile != null)
            {
                tile.IsUnLock = false; // 잠금 상태로 되돌림
                tile.IsTempUnlocked = false;
                ProjectUtility.SetActiveCheck(tile.gameObject, false);
                ProjectUtility.SetActiveCheck(tile.TileUnLockObj, false);
            }
        }

        // TileAddComponent 다시 활성화
        ProjectUtility.SetActiveCheck(target.gameObject, true);

        ActiveTempUnlocks.Remove(target);

        // 임시 장착 상태 종료 - 버튼 상태 변경
        if (restoreUi && ActiveTempUnlocks.Count == 0)
        {
            InGameBtnGroup?.TileTmepEquipStatus(false);
            RestoreWeaponColors();
        }
    }

    // 임시 잠금 해제된 타일들만 되돌림 (TileAddComponent는 이미 처리된 상태)
    private void RevertTempUnlock(TileAddComponent target)
    {
        if (target == null) return;
        if (!ActiveTempUnlocks.TryGetValue(target, out var tiles)) return;

        foreach (var tile in tiles)
        {
            if (tile != null)
            {
                tile.IsUnLock = false; // 잠금 상태로 되돌림
                tile.IsTempUnlocked = false;
                ProjectUtility.SetActiveCheck(tile.gameObject, false);
                ProjectUtility.SetActiveCheck(tile.TileUnLockObj, false);
            }
        }

        // 버튼 상태와 무기 색상은 드래그 시작 시 다시 설정되므로 여기서는 처리하지 않음
        ActiveTempUnlocks.Remove(target);
    }

    // 임시 잠금 해제 확정 (버튼 클릭 시 호출)
    public void ConfirmTempUnlock()
    {
        if (ActiveTempUnlocks.Count == 0) return;

        // 임시로 잠금 해제된 타일들을 정식으로 잠금 해제
        foreach (var pair in ActiveTempUnlocks.ToList())
        {
            foreach (var tile in pair.Value)
            {
                if (tile != null)
                {
                    UnlockTile(tile);
                }
            }
            // TileAddComponent 비활성화
            ProjectUtility.SetActiveCheck(pair.Key.gameObject, false);
        }

        // 회색으로 변경된 무기들을 흰색으로 복원
        RestoreWeaponColors();
        InGameBtnGroup?.TileTmepEquipStatus(false);

        ActiveTempUnlocks.Clear();
    }

    // 장착되지 않은 무기들을 회색으로 변경
    private void GrayNonEquippedWeapons()
    {
        GrayedWeapons.Clear();

        if (TileWeaponGroup == null) return;

        foreach (var weapon in TileWeaponGroup.GetTileWeaponComponentList)
        {
            if (weapon != null && weapon.gameObject.activeSelf && !weapon.IsEquip)
            {
                // 무기 이미지를 회색으로 변경
                weapon.WeaponImg.color = Color.gray;
                GrayedWeapons.Add(weapon);
            }
        }
    }

    // 회색으로 변경된 무기들을 흰색으로 복원
    private void RestoreWeaponColors()
    {
        foreach (var weapon in GrayedWeapons)
        {
            if (weapon != null && weapon.WeaponImg != null)
            {
                weapon.WeaponImg.color = Color.white;
            }
        }

        GrayedWeapons.Clear();
    }
}

