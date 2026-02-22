using UnityEngine;
using BanpoFri;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

public class Poison_Aliment : AlimentData
{
    private List<Color> originalColors = new List<Color>();
    private bool isApplied = false;

    public override void Set(AlimentType type, float time, EnemyUnit target, double damage, float damagedelay, int maxStackCount = 1)
    {
        base.Set(type, time, target, damage, damagedelay, maxStackCount);

        deltatime = 0f;
        
        // 독 효과 적용
        ApplyPoisonEffect();
    }

    private void ApplyPoisonEffect()
    {
        if (Target == null || isApplied) return;
        
        isApplied = true;
        originalColors.Clear();
        
        // 원래 색상 저장하고 초록색으로 변경
        var spriteList = Target.GetComponent<UnitBase>()?.GetUnitSpriteList();
        if (spriteList != null)
        {
            foreach (var sprite in spriteList)
            {
                if (sprite != null)
                {
                    // 원래 색상 저장
                    originalColors.Add(sprite.color);
                    
                    // 초록색으로 변경 (밝은 초록색)
                    sprite.color = new Color(0.5f, 1f, 0.5f, sprite.color.a);
                }
            }
        }
    }

    public override void Update()
    {
        if (Time <= 0f)
        {
            // 독 효과 해제
            RemovePoisonEffect();
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

    private void RemovePoisonEffect()
    {
        if (Target == null || !isApplied) return;
        
        // 원래 색상으로 복구
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

    public override void OnDamage()
    {
        base.OnDamage();
        // 중첩 수에 따라 데미지 증가
        double stackedDamage = Damage * StackCount;
        Target.Damage(stackedDamage);
    }



}
