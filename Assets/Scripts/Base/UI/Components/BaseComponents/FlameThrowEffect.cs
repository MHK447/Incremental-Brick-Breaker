using UnityEngine;
using BanpoFri;
using System.Collections.Generic;

[EffectPath("Effect/FlameThrowEffect", false, false)]
public class FlameThrowEffect : Effect
{
    [SerializeField] private int damage = 1;
    [SerializeField] private float damageInterval = 0.5f;

    private readonly Dictionary<Component, float> nextDamageTimeByTarget = new Dictionary<Component, float>();

    public void Init(int damageValue)
    {
        damage = Mathf.Max(0, damageValue);
    }

    public override void Play(Vector3 worldPos, Transform followTrans)
    {
        base.Play(worldPos, followTrans);
        nextDamageTimeByTarget.Clear();
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (damage <= 0 || !TryGetDamageTarget(collision, out var target))
            return;

        float now = Time.time;
        if (!nextDamageTimeByTarget.TryGetValue(target, out float nextDamageTime) || now >= nextDamageTime)
        {
            ApplyDamage(target, damage);
            nextDamageTimeByTarget[target] = now + damageInterval;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (TryGetDamageTarget(collision, out var target))
            nextDamageTimeByTarget.Remove(target);
    }

    private static bool TryGetDamageTarget(Collider2D collision, out Component target)
    {
        target = collision.GetComponent<EnemyUnitBase>();
        if (target != null)
            return true;

        target = collision.GetComponentInParent<EnemyBlockSpawner>();
        return target != null;
    }

    private static void ApplyDamage(Component target, int value)
    {
        if (target is EnemyUnitBase enemy)
        {
            enemy.Damage(value);
            return;
        }

        if (target is EnemyBlockSpawner spawner)
            spawner.Damage(value);
    }
}

