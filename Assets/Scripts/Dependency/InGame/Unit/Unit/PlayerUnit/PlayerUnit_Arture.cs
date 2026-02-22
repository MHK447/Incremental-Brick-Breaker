using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class PlayerUnit_Arture : PlayerUnit   
{
    [SerializeField]
    private GameObject BulletPrefab;

    [SerializeField]
    private Transform BulletSpawnTr;

    public PrefabPool<Bullet> BulletPool = new PrefabPool<Bullet>();

    private BulletInfo BulletInfo = new BulletInfo();

    // 현재 활성화된 bullet들을 추적
    private List<Bullet> activeBullets = new List<Bullet>();



    public override void Set(int idx, int grade, int order = 0)
    {
        base.Set(idx, grade, order);

        BulletPool.Init(BulletPrefab, transform, 4);

        BulletInfo.AttackDamage = InfoData.Damage;
        BulletInfo.AttackSpeed = InfoData.AttackSpeed;
        BulletInfo.AttackForce = 10;

        


    }




    public override void Attack()
    {
        var bullet = BulletPool.Get();
        if (bullet == null)
        {
            Debug.LogError("Bullet is null!");
            return;
        }

        var findtr = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.EnemyUnitGroup.FindTargetEnemy(transform);
        
        Transform targetTransform = null;
        
        if (findtr != null)
        {
            targetTransform = findtr.transform;
        }
        else
        {
            // 적이 없으면 EnemyBlockSpawner를 타겟으로 사용
            var enemyUnitGroup = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.EnemyUnitGroup;
            var enemyBlockSpawner = enemyUnitGroup.EnemyBlockSpawner;
            
            if (enemyBlockSpawner != null && enemyBlockSpawner.IsSpawn && !enemyBlockSpawner.IsDead)
            {
                targetTransform = enemyBlockSpawner.transform;
            }
        }
        
        if (targetTransform == null)
        {
            Debug.LogWarning("No enemy target found!");
            BulletPool.Return(bullet); // 사용하지 않은 bullet 반환
            return;
        }

        bullet.Set(BulletInfo, BulletSpawnTr, targetTransform, ReturnBullet);
        
        // 활성화된 bullet 리스트에 추가
        activeBullets.Add(bullet);
    }




    public void ReturnBullet(Bullet bullet)
    {
        // 활성화된 bullet 리스트에서 제거
        activeBullets.Remove(bullet);
        BulletPool.Return(bullet);
    }

    // 게임이 reset될 때 모든 활성화된 bullet들을 초기화
    protected override void ResetUnit()
    {
        base.ResetUnit();

        // 현재 활성화된 모든 bullet들을 풀에 반환
        for (int i = activeBullets.Count - 1; i >= 0; i--)
        {
            if (activeBullets[i] != null)
            {
                BulletPool.Return(activeBullets[i]);
            }
        }
        
        // 리스트 초기화
        activeBullets.Clear();
    }
}

