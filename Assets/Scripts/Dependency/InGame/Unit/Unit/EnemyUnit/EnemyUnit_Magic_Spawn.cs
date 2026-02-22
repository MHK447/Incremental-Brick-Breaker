using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class EnemyUnit_Magic_Spawn : EnemyUnit
{
    private float SkillSpawnDelay = 12f;

    private bool IsDeadSpawn = false;

    private EnemyBlockSpawner EnemyBlockSpawner;

    public override void Set(int idx, int unitdmg, int unithp, int deadexpvalue, int order = 0, float landingY = 0)
    {
        base.Set(idx, unitdmg, unithp, deadexpvalue, order, landingY);
        IsDeadSpawn = false;

        Anim.Play("Idle", 0, 0f);
        SetState(StateType.Idle);

        EnemyBlockSpawner = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.EnemyUnitGroup.EnemyBlockSpawner;

        SkillSpawnDelay = 12f;
    }


    public override void Update()
    {
        if (IsDead) return;

        if (!GameRoot.Instance.UserData.Playerdata.IsWaveRestProperty.Value)
        {
            // 스폰 로직은 계속 실행
            SkillSpawnDelay -= Time.deltaTime;
            if (SkillSpawnDelay <= 0f)
            {
                SkillSpawnDelay = 12f;
                SetState(StateType.Attack);
            }
        }
        else
        {
            SetState(StateType.Idle);
        }
    }

    public override void Dead(bool isdirection = false)
    {
        base.Dead(isdirection);

        if (!IsDeadSpawn)
        {
            Attack();
            IsDeadSpawn = true;
        }
    }

    public override void Attack()
    {
        if (!GameRoot.Instance.UserData.Playerdata.IsWaveRestProperty.Value && EnemyBlockSpawner.CurHp > 0)
        {
            // EnemyUnitGroup을 통해 적을 스폰
            var enemyGroup = GameRoot.Instance.InGameSystem.GetInGame<InGameBase>().Stage.EnemyUnitGroup;

            for (int i = 0; i < 2; ++i)
            {
                var randenemyidx = Random.Range(5, 7);

                enemyGroup.EnemySpawn(randenemyidx, 1, (int)InfoData.StartHp / 2);
            }
        }
        SetState(StateType.Idle);

        // 스폰 이펙트
    }

}

