using UnityEngine;

public class ParticleData : MonoBehaviour
{
    [Header("Particle Data")]
    public int particleID;
    public float damage = 10f;
    public float lifetime = 5f;
    public string particleType = "Default";
    
    [Header("Effects")]
    public GameObject hitEffect;
    public AudioClip hitSound;
    
    private float currentLifetime;
    
    void Start()
    {
        currentLifetime = lifetime;
        
        // 자동으로 파괴되도록 설정
        if (lifetime > 0)
        {
            Destroy(gameObject, lifetime);
        }
    }
    
    void Update()
    {
        // 생명주기 관리
        if (lifetime > 0)
        {
            currentLifetime -= Time.deltaTime;
            if (currentLifetime <= 0)
            {
                DestroyParticle();
            }
        }
    }
    
    // 다른 오브젝트와 충돌 시 호출
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"파티클 {particleID}가 {other.name}과 충돌했습니다!");
        
        // 충돌 효과 재생
        PlayHitEffect();
        
        // 데미지 처리 (예시)
        if (other.CompareTag("Enemy"))
        {
            // 적에게 데미지 주기
            Debug.Log($"{other.name}에게 {damage} 데미지를 입혔습니다!");
        }
    }
    
    void PlayHitEffect()
    {
        // 히트 이펙트 재생
        if (hitEffect != null)
        {
            Instantiate(hitEffect, transform.position, Quaternion.identity);
        }
        
        // 히트 사운드 재생
        if (hitSound != null)
        {
            AudioSource.PlayClipAtPoint(hitSound, transform.position);
        }
    }
    
    void DestroyParticle()
    {
        // 파괴 전 마지막 이펙트
        PlayHitEffect();
        
        // 오브젝트 파괴
        Destroy(gameObject);
    }
    
    // 외부에서 파티클 데이터 설정
    public void SetParticleData(int id, float dmg, float life, string type)
    {
        particleID = id;
        damage = dmg;
        lifetime = life;
        particleType = type;
    }
}
