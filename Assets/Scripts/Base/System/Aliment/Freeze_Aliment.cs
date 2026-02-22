using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class Freeze_Aliment : AlimentData
{
    private List<Color> originalColors = new List<Color>();
    private bool isApplied = false;
    
    public override void Set(AlimentType type, float time, EnemyUnit target, double damage, float damagedelay, int maxStackCount = 1)
    {
        base.Set(type, time, target, damage, damagedelay, maxStackCount);

        deltatime = 0f;
        
        // 동결 효과 적용
        ApplyFreezeEffect();
    }

    private void ApplyFreezeEffect()
    {
        if (Target == null || isApplied) return;
        
        isApplied = true;
        originalColors.Clear();
        
        // 적 유닛을 스턴 상태로 설정
        Target.SetState(UnitBase.StateType.Sturn);
        
        // 원래 색상 저장하고 파랑색으로 변경
        var spriteList = Target.GetComponent<UnitBase>()?.GetUnitSpriteList();
        if (spriteList != null)
        {
            foreach (var sprite in spriteList)
            {
                if (sprite != null)
                {
                    // 원래 색상 저장
                    originalColors.Add(sprite.color);
                    
                    // 파랑색으로 변경 (밝은 파랑색)
                    sprite.color = new Color(0.5f, 0.7f, 1f, sprite.color.a);
                }
            }
        }
    }

    public override void Update()
    {
        if (Time <= 0f)
        {
            // 동결 해제
            RemoveFreezeEffect();
            return;
        }

        // 시간 감소
        deltatime += UnityEngine.Time.deltaTime;
        if (deltatime >= DamageDelay)
        {
            Time -= DamageDelay;
            deltatime = 0f;
            OnDamage();
        }
    }

    private void RemoveFreezeEffect()
    {
        if (Target == null || !isApplied) return;

        // 스턴 해제는 살아 있는 유닛만 (죽은 유닛은 재활용 시 ResetUnit에서 Idle로 초기화됨)
        if (!Target.IsDead)
            Target.SetState(UnitBase.StateType.Idle);

        // 재활용 시 파란색이 남지 않도록 항상 색상 복구
        var spriteList = Target.GetComponent<UnitBase>()?.GetUnitSpriteList();
        if (spriteList != null && originalColors.Count == spriteList.Count)
        {
            for (int i = 0; i < spriteList.Count; i++)
            {
                if (spriteList[i] != null && i < originalColors.Count)
                {
                    spriteList[i].color = originalColors[i];
                }
            }
        }

        isApplied = false;
        originalColors.Clear();
    }

    public override void OnRemove()
    {
        RemoveFreezeEffect();
    }

    public override void OnDamage()
    {
        base.OnDamage();
        
        // 동결 중에는 데미지를 주지 않음 (선택적)
        // Target.Damage(Damage);
    }

}

