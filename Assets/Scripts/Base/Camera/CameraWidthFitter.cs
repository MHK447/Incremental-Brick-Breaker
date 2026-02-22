using UnityEngine;
using BanpoFri;

public class CameraWidthFitter : MonoBehaviour
{
    //카메라 넓이를 FarLeft에서 FarRight으로 맞추고 중앙 높이를 CenterHeight에 맞춤
    [SerializeField] private Camera TargetCamera;
    [SerializeField] private Transform FarRightPosition;
    [SerializeField] private Transform FarLeftPosition;
    [SerializeField] private Transform CenterHeightPosition;

    private void Awake() => MatchWidth();

    [ContextMenu("Match Width")]
    public void MatchWidth()
    {
        if (FarLeftPosition == null || FarRightPosition == null || CenterHeightPosition == null) return;
        Camera cam = TargetCamera == null ? Camera.main : TargetCamera;

        //set size
        float dist = Vector3.Distance(FarLeftPosition.position, FarRightPosition.position);
        cam.orthographicSize = dist / (cam.aspect * 2);

        //set position
        // Vector3 position = FarLeftPosition.position;
        // position.y = CenterHeightPosition.position.y;
        // position.x += dist * 0.5f;
        // position.z = -2;
        // cam.transform.position = position;
    }
}