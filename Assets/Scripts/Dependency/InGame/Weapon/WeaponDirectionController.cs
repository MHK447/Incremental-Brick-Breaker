using UnityEngine;
using System.Collections.Generic;
using BanpoFri;
using UnityEngine.AddressableAssets;
using System.Linq;

public class WeaponDirectionController : MonoBehaviour
{
    [HideInInspector]
    public EnemyUnit TargetEnemy;

    private EnemyUnitGroup EnemyUnitGroup;

    [SerializeField]
    private Transform WeaponTransform; // 회전시킬 무기 Transform
    
    public Transform GetWeaponTransform => WeaponTransform;
    
    [SerializeField]
    private float rotationSpeed = 10f; // 회전 속도

    [SerializeField]
    private float rotationOffset = 90f; // 스프라이트 방향 보정값

    [Header("Rotation Limits")]
    [SerializeField]
    private float minRotationZ = -55f;
    [SerializeField]
    private float maxRotationZ = 55f;

    private bool isManualControl = false;
    private PanAndZoom Cam;

    public void Init()
    {
        EnemyUnitGroup = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.EnemyUnitGroup;

        if (WeaponTransform == null)
            WeaponTransform = transform;
            
        Cam = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().GetMainCam;
    }



    void Update()
    {
        CheckInput();

        if (isManualControl)
        {
            RotateToInputPosition();
        }
        else
        {
            // 자동 조작: 가장 가까운 적을 찾아서 타겟팅
            FindTargetEnemy();
            RotateToTarget();
        }
    }

    private void CheckInput()
    {
        // 모바일 터치 또는 마우스 입력 체크
        if (Input.touchCount > 0)
        {
            isManualControl = Input.GetTouch(0).phase == UnityEngine.TouchPhase.Began || 
                            Input.GetTouch(0).phase == UnityEngine.TouchPhase.Moved ||
                            Input.GetTouch(0).phase == UnityEngine.TouchPhase.Stationary;
        }
        else if (Input.GetMouseButton(0))
        {
            isManualControl = true;
        }
        else
        {
            isManualControl = false;
        }
    }

    private void RotateToInputPosition()
    {
        Vector3 inputPosition;
        
        // 터치 또는 마우스 위치 가져오기
        if (Input.touchCount > 0)
        {
            inputPosition = Cam.cam.ScreenToWorldPoint(Input.GetTouch(0).position);
        }
        else
        {
            inputPosition = Cam.cam.ScreenToWorldPoint(Input.mousePosition);
        }
        
        inputPosition.z = 0f;
        
        // 무기에서 입력 위치로의 방향 계산
        Vector2 direction = (inputPosition - WeaponTransform.position).normalized;
        
        // 방향을 각도로 변환
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle += rotationOffset;
    
        // 각도 제한 (-180 ~ 180 범위로 정규화 후 제한)
        if (angle > 180) angle -= 360;
        else if (angle < -180) angle += 360;
        
        angle = Mathf.Clamp(angle, minRotationZ, maxRotationZ);
        
        // 무기 회전
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        WeaponTransform.rotation = Quaternion.Lerp(WeaponTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }


    private void RotateToTarget()
    {
        if (TargetEnemy == null) return;
        
        // 타겟 방향으로 회전
        Vector2 direction = (TargetEnemy.transform.position - WeaponTransform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        angle += rotationOffset;

        // 각도 제한 (-180 ~ 180 범위로 정규화 후 제한)
        if (angle > 180) angle -= 360;
        else if (angle < -180) angle += 360;
        
        angle = Mathf.Clamp(angle, minRotationZ, maxRotationZ);
        
        Quaternion targetRotation = Quaternion.Euler(0, 0, angle);
        WeaponTransform.rotation = Quaternion.Lerp(WeaponTransform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    public void FindTargetEnemy()
    {
        if(EnemyUnitGroup == null) return;
        if(EnemyUnitGroup.ActiveUnits.Count == 0)
        {
            TargetEnemy = null;
            return;
        }

        // 가장 가까운 적 찾기
        EnemyUnit closestEnemy = null;
        float closestDistanceSqr = float.MaxValue;
        
        foreach (var enemy in EnemyUnitGroup.ActiveUnits)
        {
            if (enemy == null || !enemy.gameObject.activeSelf) continue;
            
            float distanceSqr = (enemy.transform.position - WeaponTransform.position).sqrMagnitude;
            
            if (distanceSqr < closestDistanceSqr)
            {
                closestDistanceSqr = distanceSqr;
                closestEnemy = enemy;
            }
        }
        
        TargetEnemy = closestEnemy;
    }





}
