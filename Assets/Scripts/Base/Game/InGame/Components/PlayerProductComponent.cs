
using UnityEngine;
using DG.Tweening;
public class PlayerProductComponent : MonoBehaviour
{
    private Vector3 startPosition;

    private Vector3 startLocalPosition;
    private Quaternion startRotation;

    [SerializeField]
    private Rigidbody Rb;

    private Vector3 localscale;


    void Awake()
    {
        // 초기 위치와 회전을 저장 (transform 기준으로)
        startPosition = transform.position;
        startLocalPosition = transform.localPosition;
        startRotation = transform.rotation;
        localscale = transform.localScale;
        // 리지드바디 회전 제약 설정 (X, Y축 회전 고정, Z축만 자유)
        if (Rb != null)
        {
            Rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;
        }
    }

    public void Init()
    {
        if (Rb != null)
        {
            // DOTween 정리 (혹시 실행 중인 트윈이 있다면)
            transform.DOKill();

            // 리지드바디 물리 상태 완전 초기화
            Rb.linearVelocity = Vector3.zero;
            Rb.angularVelocity = Vector3.zero;
            Rb.Sleep(); // 물리 시뮬레이션 일시 정지

            // Transform 위치와 회전 먼저 설정
            transform.localPosition = startLocalPosition;
            transform.rotation = startRotation;
            transform.localScale = localscale;

            // 리지드바디 위치와 회전도 직접 설정
            Rb.position = startPosition;
            Rb.rotation = startRotation;

            // 물리 제약 다시 설정
            Rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;

            // 물리 시뮬레이션 재시작
            Rb.WakeUp();
        }


        transform.localScale = Vector3.zero;
        transform.DOScale(localscale, 0.3f).SetEase(Ease.OutBack);
    }


    public void EndGame(Vector3 force)
    {
        if (Rb != null)
        {
            // 물리 제약 해제 (자유롭게 날아갈 수 있도록)
            Rb.constraints = RigidbodyConstraints.None;

            // 받은 힘을 기반으로 와장창 흩어지는 방향 계산
            Vector3 scatterForce = new Vector3(
                force.x + Random.Range(-3f, 3f), // 좌우로 크게 흩어짐
                Random.Range(1f, 3f), // 위로 적당히 (너무 세지 않게)
                force.z + Random.Range(-3f, 3f)  // 앞뒤로 크게 흩어짐
            );

            // 강한 회전 토크 (와장창 굴러가는 느낌)
            Vector3 randomTorque = new Vector3(
                Random.Range(-300f, 300f),
                Random.Range(-400f, 400f),
                Random.Range(-300f, 300f)
            );

            Rb.AddTorque(randomTorque);
            Rb.AddForce(scatterForce, ForceMode.Impulse);
        }
    }
}
