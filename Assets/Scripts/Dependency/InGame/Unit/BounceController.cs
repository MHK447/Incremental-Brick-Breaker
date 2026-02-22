using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class BounceController : MonoBehaviour
{
    [SerializeField]
    private Transform TargetTr;


    [Header("속도 설정")]
    public float minSpeed = 50;
    public float maxSpeed = 60;
    public float bouncePower = 20f;

    [Header("반사 랜덤 각도")]
    public float randomAngleRange = 35f;

    [Header("충돌 처리 간격")]
    public float bounceInterval = 0.2f; // 최소 바운스 간격 (초)

    Rigidbody2D rb;
    private float lastBounceTime = -999f; // 마지막 바운스 시간
    private float stuckCheckTime = 0f; // 끼임 체크 타이머
    private bool isCollidingWithWall = false; // ✅ 벽 충돌 여부 체크

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (TargetTr == null)
        {
            TargetTr = this.transform;
        }
    }

    public void Set(float movespeed)
    {
        minSpeed = movespeed * 0.7f;
        maxSpeed = movespeed;
    }

    void FixedUpdate()
    {
        // ✅ 속도가 너무 느리고 "벽과 충돌 중"일 때만 끼임 방지 로직 실행
        if (isCollidingWithWall && rb.linearVelocity.sqrMagnitude < minSpeed * minSpeed * 0.5f)
        {
            stuckCheckTime += Time.fixedDeltaTime;
            
            if (stuckCheckTime > 0.3f)
            {
                Vector2 randomDir = Random.insideUnitCircle.normalized;
                rb.linearVelocity = randomDir * minSpeed;
                stuckCheckTime = 0f;
            }
        }
        else
        {
            stuckCheckTime = 0f;
        }

        // ✅ 최대 속도 제한
        if (rb.linearVelocity.sqrMagnitude > maxSpeed * maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall") || collision.gameObject.layer == LayerMask.NameToLayer("Enemy") || collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            isCollidingWithWall = true; // ✅ 벽 충돌 시작
            
            // 충돌 간격 체크
            if (Time.time - lastBounceTime >= bounceInterval)
            {
                ForceBounce(collision);
                lastBounceTime = Time.time;
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall") || collision.gameObject.layer == LayerMask.NameToLayer("Enemy") || collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            // ✅ 벽에 끼거나 기어가면 무조건 튀게 (속도가 느리면 interval 무시)
            if (rb.linearVelocity.sqrMagnitude < minSpeed * minSpeed)
            {
                // 속도가 거의 없으면 강제로 랜덤 방향으로 튕겨냄
                Vector2 normal = collision.contacts[0].normal;
                Vector2 randomOffset = Random.insideUnitCircle * 0.3f;
                Vector2 bounceDirection = (normal + randomOffset).normalized;
                
                // 무조건 최소 속도 이상으로 튕기기
                rb.linearVelocity = bounceDirection * minSpeed;
                lastBounceTime = Time.time;
                stuckCheckTime = 0f;
                return;
            }

            // 충돌 간격 체크 - 너무 자주 호출되는 것 방지
            if (Time.time - lastBounceTime < bounceInterval)
                return;

            // ✅ 벽 쪽으로 향하고 있으면 강제로 노멀 방향으로 밀기
            Vector2 n = collision.contacts[0].normal;
            float dot = Vector2.Dot(rb.linearVelocity.normalized, -n);
            if (dot > 0.7f) // 벽쪽으로 계속 밀리면
            {
                rb.linearVelocity = (n + Random.insideUnitCircle * 0.4f).normalized * minSpeed;
                lastBounceTime = Time.time;
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Wall") || collision.gameObject.layer == LayerMask.NameToLayer("Enemy") || collision.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            isCollidingWithWall = false; // ✅ 벽에서 떨어짐
            stuckCheckTime = 0f;
        }
    }

    private void ForceBounce(Collision2D collision)
    {
        Vector2 vin = -collision.relativeVelocity;
        if (vin.sqrMagnitude < 0.001f)
            vin = rb.linearVelocity;

        Vector2 n = collision.contacts[0].normal;
        Vector2 reflect = Vector2.Reflect(vin.normalized, n);

        float rand = Random.Range(-randomAngleRange, randomAngleRange);
        reflect = (Quaternion.Euler(0, 0, rand) * reflect).normalized;

        // ✅ 반사 각도가 너무 평평하면 최소 각도 강제
        if (Mathf.Abs(reflect.y) < 0.2f && Mathf.Abs(n.y) > 0.6f)
            reflect = (reflect + Vector2.up * 1.2f).normalized;
        if (Mathf.Abs(reflect.x) < 0.2f && Mathf.Abs(n.x) > 0.6f)
            reflect = (reflect + Vector2.right * 0.6f).normalized;

        float newSpeed = vin.magnitude * bouncePower;
        newSpeed = Mathf.Clamp(newSpeed, minSpeed, maxSpeed); // ✅ 수정: 최대값을 maxSpeed로

        rb.linearVelocity = reflect * newSpeed;
        stuckCheckTime = 0f; // 바운스 했으니 끼임 타이머 초기화
    }
}
